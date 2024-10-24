using ASCJSMCustom.AP.CacheExt;
using ASCJSMCustom.Common.Builder;
using ASCJSMCustom.Common.Descriptor;
using ASCJSMCustom.Common.Helper;
using ASCJSMCustom.Common.Helper.Extensions;
using ASCJSMCustom.Common.Services.DataProvider.Interfaces;
using ASCJSMCustom.IN.DAC;
using ASCJSMCustom.IN.Descriptor.Constants;
using ASCJSMCustom.INKit.CacheExt;
using ASCJSMCustom.INKit.DAC;
using ASCJSMCustom.INKit.Descriptor;
using ASCJSMCustom.PO.CacheExt;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.EP;
using PX.Objects.AP;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.SM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static ASCJSMCustom.Common.Descriptor.ASCJSMConstants;
using ASCJSMCustom.IN.CacheExt;

namespace ASCJSMCustom.INKit
{
    public class ASCJSMINKitSpecMaintExt : PXGraphExtension<INKitSpecMaint>
    {
        private const decimal One_Gram = 1m;

        public static bool IsActive() => true;

        #region DataView
        [PXCopyPasteHiddenView]
        public PXSelect<INKitSpecHdr, Where<INKitSpecHdr.kitInventoryID, Equal<Optional<INKitSpecHdr.kitInventoryID>>>> Hdr;

        [PXCopyPasteHiddenView]
        public PXSelect<POVendorInventory, Where<POVendorInventory.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>> VendorItems;

        public SelectFrom<ASCJSMINKitSpecJewelryItem>
                 .Where<ASCJSMINKitSpecJewelryItem.kitInventoryID.IsEqual<INKitSpecHdr.kitInventoryID.FromCurrent>
                    .And<ASCJSMINKitSpecJewelryItem.revisionID.IsEqual<INKitSpecHdr.revisionID.FromCurrent>>>
                        .View JewelryItemView;


        [PXCopyPasteHiddenView]
        public PXSetup<INSetup> ASCIStarINSetup;

        [PXCopyPasteHiddenView]
        public PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>> ASCIStarInventoryItem;

        [PXCopyPasteHiddenView]
        public PXSelect<ASCJSMINJewelryItem, Where<ASCJSMINJewelryItem.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>> ASCIStarJewelryItem;

        [PXCopyPasteHiddenView]
        public FbqlSelect<SelectFromBase<InventoryItemCurySettings,
            TypeArrayOf<IFbqlJoin>.Empty>.Where<BqlChainableConditionBase<TypeArrayOf<IBqlBinary>.FilledWith<And<Compare<InventoryItemCurySettings.inventoryID,
                Equal<P.AsInt>>>>>.And<BqlOperand<InventoryItemCurySettings.curyID, IBqlString>.IsEqual<BqlField<AccessInfo.baseCuryID, IBqlString>.AsOptional>>>,
            InventoryItemCurySettings>.View ASCIStarItemCurySettings;

        [PXCopyPasteHiddenView]
        public FbqlSelect<SelectFromBase<InventoryItemCurySettings,
            TypeArrayOf<IFbqlJoin>.Empty>.Where<BqlOperand<InventoryItemCurySettings.inventoryID, IBqlInt>.IsEqual<P.AsInt>>,
            InventoryItemCurySettings>.View ASCIStarAllItemCurySettings;
        #endregion

        #region Dependency Injection
        [InjectDependency]
        public IASCJSMInventoryItemDataProvider _itemDataProvider { get; set; }

        [InjectDependency]
        public IASCJSMrVendorDataProvider _vendorDataProvider { get; set; }
        #endregion

        #region Overrides
        public override void Initialize()
        {
            base.Initialize();
            var setup = ASCIStarINSetup.Current;
            if (setup != null)
            {
                var setupExt = PXCache<INSetup>.GetExtension<ASCJSMINSetupExt>(setup);
                ASCIStarCreateProdItem.SetVisible(!setupExt.UsrIsActiveKitVersion ?? false);
                ASCIStarCreateProdItem.SetEnabled(!setupExt.UsrIsActiveKitVersion ?? false);
            }
        }

        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            CopyFieldsValueToStockItem(Base.Hdr.Current);
            CopyFieldsValueToPOVendorInventory(Base.Hdr.Current);

            var setup = ASCIStarINSetup.Current;
            if (setup != null)
            {
                var setupExt = PXCache<INSetup>.GetExtension<ASCJSMINSetupExt>(setup);
                if (setupExt.UsrIsActiveKitVersion == false) // Has to be specified on IN101000 screen
                {
                    CopyJewelryItemFieldsToStockItem(Base.Hdr.Current);
                }
            }
            baseMethod();
        }
        #endregion

        #region CacheAttached

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDefault(true)]
        protected virtual void _(Events.CacheAttached<INKitSpecHdr.allowCompAddition> e) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDefault(true)]
        protected virtual void _(Events.CacheAttached<INKitSpecStkDet.allowSubstitution> e) { }

        [PXRemoveBaseAttribute(typeof(PXDBStringAttribute))]
        [PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">##")]
        [PXDefault("01")]
        [PXUIField(DisplayName = "Variant")]
        protected void _(Events.CacheAttached<INKitSpecHdr.revisionID> cacheAttached) { }

        [PXRemoveBaseAttribute(typeof(PXUIFieldAttribute))]
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXUIField(DisplayName = "Default", Enabled = true)]
        protected virtual void _(Events.CacheAttached<POVendorInventory.isDefault> cacheAttached) { }

        [PXRemoveBaseAttribute(typeof(PXDBDefaultAttribute))]
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDBDefault(typeof(INKitSpecHdr.kitInventoryID))]
        protected virtual void _(Events.CacheAttached<POVendorInventory.inventoryID> cacheAttached) { }

        #endregion

        #region Actions
        public PXAction<INKitSpecHdr> SendEmailToVendor;
        [PXUIField(DisplayName = "Send Email to Vendor", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void sendEmailToVendor()
        {
            if (this.Base.Hdr.Current == null) return;

            PXLongOperation.StartOperation(this.Base, () =>
            {
                var defaultVendorInventory = GetDefaultPOVendorInventory();
                if (defaultVendorInventory == null)
                    throw new PXException(ASCJSMINKitMessages.Error.NoDefaultVendor);

                SendEmailNotification(this.Base.Hdr.Current);
            });
        }

        public PXAction<INKitSpecHdr> ASCIStarCreateProdItem;
        [PXUIField(DisplayName = "Create Production Item", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable aSCIStarCreateProdItem(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXAction<INKitSpecHdr> ASCIStarUpdateMetalCost;
        [PXUIField(DisplayName = "Update Metal Cost", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(CommitChanges = true)]
        public virtual IEnumerable aSCIStarUpdateMetalCost(PXAdapter adapter)
        {
            var currentHdr = Base.Hdr.Current;
            if (currentHdr != null)
            {
                VendorItems.Select().RowCast<POVendorInventory>().ForEach(row =>
                {

                    // var jewelryItem = GetASCIStarINJewelryItem(currentHdr.KitInventoryID);
                    if (JewelryItemView.Current == null)
                        JewelryItemView.Current = JewelryItemView.Select()?.TopFirst;

                    var metalType = JewelryItemView.Current?.MetalType;

                    if (ASCJSMMetalType.IsGold(metalType))
                    {
                        var item = _itemDataProvider.GetInventoryItemByCD(ASCJSMConstants.MetalType.Type_24K);
                        SetOrUpdatePreciousMetalCost(row, item, metalType);
                    }
                    else if (ASCJSMMetalType.IsSilver(metalType))
                    {
                        var item = _itemDataProvider.GetInventoryItemByCD(ASCJSMConstants.MetalType.Type_SSS);
                        SetOrUpdatePreciousMetalCost(row, item, metalType);
                    }
                });
                Base.Save.PressButton();
            }

            return adapter.Get();
        }
        #endregion

        #region Event Handlers

        #region INKitSpecHdr Events
        protected virtual void _(Events.RowInserted<INKitSpecHdr> e)
        {
            var row = e.Row;
            if (row == null || Base.Hdr.Current == null) return;

            CopyJewelryItemFields(Base.Hdr.Current);
            CopyFieldsValueFromStockItem(this.Base.Hdr.Current);
        }

        protected virtual void _(Events.RowSelected<INKitSpecHdr> e)
        {
            if (e.Row is INKitSpecHdr row)
            {
                var setup = ASCIStarINSetup.Current;
                if (setup != null)
                {
                    var inSetupExt = setup?.GetExtension<ASCJSMINSetupExt>();
                    PXUIFieldAttribute.SetVisible<INKitSpecHdr.revisionID>(Base.Hdr.Cache, Base.Hdr.Current, inSetupExt?.UsrIsActiveKitVersion == true);
                }

                SetVisibleItemWeightFields(e.Cache, row);
            }
        }
        //protected virtual void _(Events.FieldSelecting<INKitSpecHdr, ASCIStarINKitSpecHdrExt.usrUnitCost> e)
        //{
        //    if (e.Row is INKitSpecHdr row)
        //    {
        //        var rowExt = PXCache<INKitSpecHdr>.GetExtension<ASCIStarINKitSpecHdrExt>(row);

        //        var result = ASCIStarCostBuilder.CalculateUnitCost(rowExt);
        //        e.ReturnValue = result;
        //    }
        //}
        //protected virtual void _(Events.FieldSelecting<INKitSpecHdr, ASCIStarINKitSpecHdrExt.usrEstLandedCost> e)
        //{
        //    if (e.Row is INKitSpecHdr row)
        //    {
        //        var rowExt = PXCache<INKitSpecHdr>.GetExtension<ASCIStarINKitSpecHdrExt>(row);

        //        var result = ASCIStarCostBuilder.CalculateUnitCost(rowExt) + ASCIStarCostBuilder.CalculateEstLandedCost(rowExt);
        //        e.ReturnValue = result;
        //    }
        //}

        protected virtual void _(Events.FieldSelecting<INKitSpecHdr, ASCJSMINKitSpecHdrExt.usrBasisValue> e)
        {
            if (e.Row is INKitSpecHdr row)
            {
                var defaultVendor = VendorItems.Select().RowCast<POVendorInventory>().FirstOrDefault(_ => _.IsDefault == true);

                if (defaultVendor != null)
                {
                    decimal? value = 0m;
                    // var jewelryItem = GetASCIStarINJewelryItem(row.KitInventoryID);
                    if (JewelryItemView.Current == null)
                        JewelryItemView.Current = JewelryItemView.Select()?.TopFirst;

                    var metalType = JewelryItemView.Current?.MetalType;

                    if (ASCJSMMetalType.IsGold(metalType) || ASCJSMMetalType.IsSilver(metalType))
                    {
                        var baseItemCd = ASCJSMMetalType.IsGold(metalType) ? ASCJSMConstants.MetalType.Type_24K : ASCJSMConstants.MetalType.Type_SSS;
                        var baseItem = _itemDataProvider.GetInventoryItemByCD(baseItemCd);

                        if (baseItem != null)
                        {
                            var vendorPrice = ASCJSMCostBuilder.GetAPVendorPrice(Base, defaultVendor.VendorID, baseItem.InventoryID, ASCJSMConstants.TOZ.value, PXTimeZoneInfo.Now);

                            if (vendorPrice != null)
                            {
                                if (ASCJSMMetalType.IsGold(metalType))
                                {
                                    value = vendorPrice.SalesPrice;
                                }
                                else
                                {
                                    var roxExt = PXCache<INKitSpecHdr>.GetExtension<ASCJSMINKitSpecHdrExt>(row);
                                    value = (vendorPrice.SalesPrice + (vendorPrice.SalesPrice + (roxExt.UsrMatrixStep ?? 0.5m))) / 2m;
                                }
                            }
                        }
                    }
                    e.ReturnValue = value;
                }
            }
        }

        protected virtual void _(Events.FieldVerifying<INKitSpecHdr, ASCJSMINKitSpecHdrExt.usrBasisValue> e)
        {
            if (e.Row is INKitSpecHdr row)
            {
                if (!IsBaseItemsExists())
                {
                    var rowExt = PXCache<INKitSpecHdr>.GetExtension<ASCJSMINKitSpecHdrExt>(row);
                    e.Cache.RaiseExceptionHandling<ASCJSMINKitSpecHdrExt.usrBasisValue>(row, rowExt.UsrBasisValue,
                        new PXSetPropertyException(ASCJSMINKitMessages.Warning.BaseItemNotSpecifyed, PXErrorLevel.Warning));
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecHdr, ASCJSMINKitSpecHdrExt.usrContractSurcharge> e)
        {
            var row = e.Row;
            if (row == null) return;

            UpdateInKitStkComponents(row);
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecHdr, ASCJSMINKitSpecHdrExt.usrContractLossPct> e)
        {
            var row = e.Row;
            if (row == null) return;

            UpdateInKitStkComponents(row);
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecHdr, ASCJSMINKitSpecHdrExt.usrUnitCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<INKitSpecHdr>.GetExtension<ASCJSMINKitSpecHdrExt>(row);
            decimal? newValue = (decimal?)e.NewValue;
            rowExt.UsrDutyCost = rowExt.UsrDutyCostPct * newValue / 100.0m;
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecHdr, ASCJSMINKitSpecHdrExt.usrDutyCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<INKitSpecHdr>.GetExtension<ASCJSMINKitSpecHdrExt>(row);

            if (rowExt.UsrUnitCost == null || rowExt.UsrUnitCost == 0.0m)
            {
                rowExt.UsrDutyCostPct = decimal.Zero;
                return;
            }
            decimal? newCostPctValue = (decimal?)e.NewValue / rowExt.UsrUnitCost * 100.0m;
            if (newCostPctValue == rowExt.UsrDutyCostPct) return;
            rowExt.UsrDutyCostPct = newCostPctValue;
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecHdr, ASCJSMINKitSpecHdrExt.usrDutyCostPct> e)
        {
            var row = e.Row;
            if (row == null) return;

            ASCJSMINKitSpecHdrExt rowExt = PXCache<INKitSpecHdr>.GetExtension<ASCJSMINKitSpecHdrExt>(row);

            decimal? newDutyCostValue = rowExt.UsrUnitCost * (decimal?)e.NewValue / 100.00m;
            if (newDutyCostValue == rowExt.UsrDutyCost) return;
            e.Cache.SetValueExt<ASCJSMINKitSpecHdrExt.usrDutyCost>(row, newDutyCostValue);
        }

        #endregion

        #region INKitSpecStkDet Events
        protected virtual void _(Events.RowSelected<INKitSpecStkDet> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetVisibleINKitSpecStkDet(e.Cache, row);
        }

        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCJSMINKitSpecStkDetExt.usrCostingType> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                var jewelryItem = GetASCIStarINJewelryItem(row.CompInventoryID);
                if (IsCommodityItem(row))
                {
                    e.NewValue = ASCJSMConstants.CostingType.MarketCost;
                }
                else
                {
                    var inventoryItem = _itemDataProvider.GetInventoryItemByID(row.CompInventoryID);
                    if (inventoryItem != null && jewelryItem != null && (ASCJSMMetalType.IsGold(jewelryItem.MetalType) || ASCJSMMetalType.IsSilver(jewelryItem.MetalType)))
                    {

                        var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(inventoryItem);
                        e.NewValue = inventoryItemExt.UsrCostingType;
                    }
                    else
                    {
                        e.NewValue = ASCJSMConstants.CostingType.StandardCost;
                    }
                }
            }
        }

        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCJSMINKitSpecStkDetExt.usrUnitCost> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                var jewelryItem = GetASCIStarINJewelryItem(row.CompInventoryID);
                if (IsCommodityItem(row))
                {
                    e.NewValue = GetUnitCostForCommodityItem(row);

                }
                else
                {
                    var inventoryItem = _itemDataProvider.GetInventoryItemByID(row.CompInventoryID);
                    if (inventoryItem != null && jewelryItem != null && (ASCJSMMetalType.IsGold(jewelryItem?.MetalType) || ASCJSMMetalType.IsSilver(jewelryItem?.MetalType)))
                    {
                        var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(inventoryItem);
                        e.NewValue = inventoryItemExt.UsrPreciousMetalCost;
                    }
                    else
                    {
                        var result = INItemCost.PK.Find(Base, row.CompInventoryID, Base.Accessinfo.BaseCuryID);
                        e.NewValue = result?.AvgCost ?? 0m;
                    }
                }
            }
        }

        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCJSMINKitSpecStkDetExt.usrSalesPrice> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                decimal? salesPrice = 0m;
                var defaultVendor = GetItemVendor(row);
                if (defaultVendor != null)
                {
                    var rowExt = PXCache<INKitSpecStkDet>.GetExtension<ASCJSMINKitSpecStkDetExt>(row);
                    if (IsCommodityItem(row))
                    {
                        salesPrice = ASCJSMCostBuilder.GetAPVendorPrice(Base, defaultVendor.VendorID, row.CompInventoryID, ASCJSMConstants.TOZ.value, PXTimeZoneInfo.Now)?.SalesPrice;
                    }
                    else
                    {
                        var jewelryCostBuilder = CreateCostBuilder(rowExt, row);
                        if (jewelryCostBuilder == null)
                        {
                            e.NewValue = salesPrice;
                            return;
                        }
                        else
                        {
                            if (rowExt.UsrCostingType == ASCJSMConstants.CostingType.MarketCost)
                            {
                                salesPrice = jewelryCostBuilder.PreciousMetalMarketCostPerTOZ;
                            }
                            else if (rowExt.UsrCostingType == ASCJSMConstants.CostingType.ContractCost)
                            {
                                salesPrice = jewelryCostBuilder.PreciousMetalContractCostPerTOZ;
                            }
                        }
                    }
                }
                e.NewValue = salesPrice;
            }
        }

        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCJSMINKitSpecStkDetExt.usrBasisPrice> e)
        {
            var row = e.Row;
            if (row == null) return;

            var defaultVendor = GetItemVendor(row);

            if (defaultVendor == null) return;
            var defaultVendorExt = PXCache<POVendorInventory>.GetExtension<ASCJSMPOVendorInventoryExt>(defaultVendor);
            e.NewValue = defaultVendorExt?.UsrBasisPrice;
        }

        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCJSMINKitSpecStkDetExt.usrBasisValue> e)
        {
            var row = e.Row;
            if (row == null) return;

            var defaultVendor = GetItemVendor(row);

            if (defaultVendor == null) return;
            var defaultVendorExt = PXCache<POVendorInventory>.GetExtension<ASCJSMPOVendorInventoryExt>(defaultVendor);
            e.NewValue = defaultVendorExt?.UsrBasisValue;
        }

        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCJSMINKitSpecStkDetExt.usrIsMetal> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                var jewelryItem = GetASCIStarINJewelryItem(row.CompInventoryID);
                e.NewValue = jewelryItem != null && (ASCJSMMetalType.IsGold(jewelryItem?.MetalType) || ASCJSMMetalType.IsSilver(jewelryItem?.MetalType));
            }
        }

        protected virtual void _(Events.FieldVerifying<INKitSpecStkDet, INKitSpecStkDet.compInventoryID> e)
        {
            var row = e.Row;
            if (row == null || e.NewValue == null) return;

            var newValue = (int?)e.NewValue;

            if (Hdr.Current?.KitInventoryID == newValue)
            {
                var invItem = _itemDataProvider.GetInventoryItemByID(Hdr.Current?.KitInventoryID);
                e.Cancel = true;
                throw new PXSetPropertyException(ASCJSMINKitMessages.Error.CannotCreateItself, invItem.InventoryCD, invItem.InventoryCD);
            }

            if (JewelryItemView.Current == null)
                JewelryItemView.Current = JewelryItemView.Select()?.TopFirst;
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecStkDet, INKitSpecStkDet.compInventoryID> e)
        {
            var row = e.Row;
            if (row == null) return;

            e.Cache.RaiseFieldDefaulting<ASCJSMINKitSpecStkDetExt.usrCostingType>(row, out object _costType);
            e.Cache.SetValueExt<ASCJSMINKitSpecStkDetExt.usrCostingType>(row, _costType);

            e.Cache.RaiseFieldDefaulting<ASCJSMINKitSpecStkDetExt.usrUnitCost>(row, out object _costUnit);
            e.Cache.SetValueExt<ASCJSMINKitSpecStkDetExt.usrUnitCost>(row, _costUnit);

            e.Cache.RaiseFieldDefaulting<ASCJSMINKitSpecStkDetExt.usrSalesPrice>(row, out object _salesPrice);
            e.Cache.SetValueExt<ASCJSMINKitSpecStkDetExt.usrSalesPrice>(row, _salesPrice);

            e.Cache.RaiseFieldDefaulting<ASCJSMINKitSpecStkDetExt.usrBasisPrice>(row, out object _basisPrice);
            e.Cache.SetValueExt<ASCJSMINKitSpecStkDetExt.usrSalesPrice>(row, _basisPrice);

            e.Cache.RaiseFieldDefaulting<ASCJSMINKitSpecStkDetExt.usrBasisValue>(row, out object _basisValue);
            e.Cache.SetValueExt<ASCJSMINKitSpecStkDetExt.usrSalesPrice>(row, _basisValue);

            if (IsCommodityItem(row))
            {
                DfltGramsForCommodityItemType(e.Cache, row);

                var inKitHdrExt = PXCache<INKitSpecHdr>.GetExtension<ASCJSMINKitSpecHdrExt>(this.Base.Hdr.Current);
                this.Base.StockDet.Cache.SetValueExt<ASCJSMINKitSpecStkDetExt.usrContractLossPct>(row, inKitHdrExt.UsrContractLossPct);
                this.Base.StockDet.Cache.SetValueExt<ASCJSMINKitSpecStkDetExt.usrContractSurcharge>(row, inKitHdrExt.UsrContractSurcharge);
            }

            var newValue = (int?)e.NewValue;
            var inJewelryItemDB = GetASCIStarINJewelryItem(newValue);

            if (inJewelryItemDB != null)
            {
                if (inJewelryItemDB.MetalType != null && inJewelryItemDB.MetalType != JewelryItemView.Current?.MetalType
                && (ASCJSMMetalType.IsGold(inJewelryItemDB.MetalType) || ASCJSMMetalType.IsSilver(inJewelryItemDB.MetalType)))
                {
                    JewelryItemView.Current.MetalType = null;
                    JewelryItemView.Update(JewelryItemView.Current);
                }

                InsertNewVendor(row, inJewelryItemDB.MetalType);
            }
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecStkDet, ASCJSMINKitSpecStkDetExt.usrCostingType> e)
        {
            var row = e.Row;
            if (e.Row == null) return;

            var rowExt = PXCache<INKitSpecStkDet>.GetExtension<ASCJSMINKitSpecStkDetExt>(row);
            if (IsCommodityItem(row))
            {
                var value = GetUnitCostForCommodityItem(row);
                e.Cache.SetValueExt<ASCJSMINKitSpecStkDetExt.usrUnitCost>(row, value);
            }
            else
            {
                if (rowExt.UsrCostingType == ASCJSMConstants.CostingType.StandardCost)
                {
                    var result = INItemCost.PK.Find(Base, row.CompInventoryID, Base.Accessinfo.BaseCuryID);
                    e.Cache.SetValueExt<ASCJSMINKitSpecStkDetExt.usrUnitCost>(row, result?.AvgCost ?? 0m);
                }
                else if (rowExt.UsrCostingType == ASCJSMConstants.CostingType.MarketCost || rowExt.UsrCostingType == ASCJSMConstants.CostingType.ContractCost)
                {
                    UpdateUnitCostAndBasisPrice(e.Cache, row, rowExt);
                }
            }

            SetOneTypeForMetalRows(rowExt.UsrCostingType);
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecStkDet, ASCJSMINKitSpecStkDetExt.usrCostRollupType> e)
        {
            var row = e.Row;
            if (row == null) return;

            if (!IsCommodityItem(row))
            {
                UpdateTotalSurchargeAndLoss();
            }
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecStkDet, ASCJSMINKitSpecStkDetExt.usrExtCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            if (!IsCommodityItem(row))
            {
                UpdateTotalSurchargeAndLoss();
            }
        }

        protected virtual void _(Events.RowPersisting<INKitSpecStkDet> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                var rowExt = PXCache<INKitSpecStkDet>.GetExtension<ASCJSMINKitSpecStkDetExt>(row);
                if (rowExt.UsrCostRollupType == null)
                {
                    e.Cache.RaiseExceptionHandling<ASCJSMINKitSpecStkDetExt.usrCostRollupType>(row, rowExt.UsrCostRollupType,
                        new PXSetPropertyException(ASCJSMINKitMessages.Error.CostRollupTypeNotSet, PXErrorLevel.Error));
                    e.Cancel = true;
                    throw new PXException(ASCJSMINKitMessages.Error.CostRollupTypeNotSet);
                }
            }
        }

        #endregion

        #region INKitSpecNonStkDet Events
        protected virtual void _(Events.FieldDefaulting<INKitSpecNonStkDet, ASCJSMINKitSpecNonStkDetExt.usrUnitCost> e)
        {
            if (e.Row is INKitSpecNonStkDet row)
            {
                var result = InventoryItemCurySettings.PK.Find(Base, row.CompInventoryID, Base.Accessinfo.BaseCuryID);
                e.NewValue = result?.StdCost ?? 0m;
            }
        }

        protected virtual void _(Events.RowPersisting<INKitSpecNonStkDet> e)
        {
            if (e.Row is INKitSpecNonStkDet row)
            {
                var rowExt = PXCache<INKitSpecNonStkDet>.GetExtension<ASCJSMINKitSpecNonStkDetExt>(row);
                if (rowExt.UsrCostRollupType == null)
                {
                    e.Cache.RaiseExceptionHandling<ASCJSMINKitSpecNonStkDetExt.usrCostRollupType>(row, rowExt.UsrCostRollupType,
                        new PXSetPropertyException(ASCJSMINKitMessages.Error.CostRollupTypeNotSet, PXErrorLevel.Error));
                    e.Cancel = true;
                    throw new PXException(ASCJSMINKitMessages.Error.CostRollupTypeNotSet);
                }
            }
        }
        #endregion

        #region POVendorInventory Events

        protected virtual void _(Events.RowSelected<POVendorInventory> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetReadOnlyPOVendorInventoryFields(e.Cache, row);
            SetVisiblePOVendorInventoryFields(e.Cache, row);
        }

        protected virtual void _(Events.FieldVerifying<POVendorInventory, POVendorInventory.isDefault> e)
        {
            var row = e.Row;
            if (row == null || (bool)e.NewValue != true) return;

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJSMPOVendorInventoryExt>(row);
            if (rowExt.UsrMarketID == null)
            {
                e.Cache.RaiseExceptionHandling<ASCJSMPOVendorInventoryExt.usrMarketID>(e.Row, false, new PXSetPropertyException(ASCJSMINConstants.Errors.MarketEmpty, PXErrorLevel.RowError));
            }

            var inventoryID = ASCJSMMetalType.GetCommodityInventoryByMetalType(this.Base, this.JewelryItemView.Current?.MetalType);

            var apVendorPrice = ASCJSMCostBuilder.GetAPVendorPrice(this.Base, row.VendorID, inventoryID, ASCJSMConstants.TOZ.value, PXTimeZoneInfo.Today);

            if (apVendorPrice == null && PXCache<POVendorInventory>.GetExtension<ASCJSMPOVendorInventoryExt>(row).UsrIsOverrideVendor != true)
            {
                e.Cache.RaiseExceptionHandling<POVendorInventory.isDefault>(row, false,
                    new PXSetPropertyException(ASCJSMMessages.Error.VendorPriceNotFound, PXErrorLevel.RowWarning));
            }

            List<POVendorInventory> selectPOVendors = VendorItems.Select()?.FirstTableItems.ToList();
            foreach (var vendorInventory in selectPOVendors)
            {
                if (vendorInventory.IsDefault == true && vendorInventory != row)
                {
                    this.VendorItems.Cache.SetValue<POVendorInventory.isDefault>(vendorInventory, false);
                    this.VendorItems.View.RequestRefresh();
                    break;
                }
            }
        }

        protected virtual void _(Events.FieldVerifying<POVendorInventory, ASCJSMPOVendorInventoryExt.usrBasisPrice> e)
        {
            var row = e.Row;
            if (row == null) return;

            if ((decimal?)e.NewValue == decimal.Zero)
            {
                e.Cache.RaiseExceptionHandling<ASCJSMPOVendorInventoryExt.usrBasisPrice>(row, e.NewValue,
                    new PXSetPropertyException(ASCJSMINConstants.Warnings.BasisOrMarketPriceEmpty, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCJSMPOVendorInventoryExt.usrCommodityID> e)
        {
            var row = e.Row;
            if (row == null || !e.ExternalCall) return;
            var rowExt = row.GetExtension<ASCJSMPOVendorInventoryExt>();
            var inventoryID = (int?)e.NewValue;
            var apVendorPrice = ASCJSMCostBuilder.GetAPVendorPrice(this.Base, rowExt?.UsrMarketID, inventoryID, ASCJSMConstants.TOZ.value, PXTimeZoneInfo.Today);
            var apVendorPriceExt = PXCache<APVendorPrice>.GetExtension<ASCJSMAPVendorPriceExt>(apVendorPrice);
            e.Cache.SetValueExt<ASCJSMPOVendorInventoryExt.usrBasisValue>(row, apVendorPriceExt.UsrBasisValue);
        }

        protected virtual void _(Events.RowPersisting<POVendorInventory> e)
        {
            if (e.Row is POVendorInventory row)
            {
                if (e.Row.IsDefault == true)
                {
                    var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJSMPOVendorInventoryExt>(e.Row);
                    if (rowExt.UsrMarketID == null)
                    {
                        e.Cache.RaiseExceptionHandling<ASCJSMPOVendorInventoryExt.usrMarketID>(row, row.IsDefault, new PXSetPropertyException(ASCJSMINKitMessages.Error.MarketNotFound, PXErrorLevel.Error));
                        e.Cancel = true;
                        throw new PXException(ASCJSMINKitMessages.Error.MarketNotFound);
                    }
                }
            }
        }

        protected virtual void _(Events.RowUpdated<POVendorInventory> e)
        {
            if (e.OldRow == null || e.Row == null || !e.Row.VendorID.HasValue)
            {
                return;
            }

            var itemCorySettings = ASCIStarAllItemCurySettings.Select(e.Row.InventoryID).RowCast<InventoryItemCurySettings>();
            var vendor = Vendor.PK.Find(Base, e.Row.VendorID);
            bool flag = false;
            foreach (var item in itemCorySettings)
            {
                var prefferedVendor = Vendor.PK.Find(Base, item.PreferredVendorID);
                if (vendor.BaseCuryID == null || string.Equals(item.CuryID, vendor.BaseCuryID, StringComparison.OrdinalIgnoreCase))
                {
                    var conditionResult = (e.Row.IsDefault == true && (item.PreferredVendorID != e.Row.VendorID || item.PreferredVendorLocationID != e.Row.VendorLocationID)) ||
                                          (e.Row.IsDefault != true && item.PreferredVendorID == e.Row.VendorID && item.PreferredVendorLocationID == e.Row.VendorLocationID);
                    if (conditionResult)
                    {
                        item.PreferredVendorID = ((e.Row.IsDefault == true) ? e.Row.VendorID : null);
                        item.PreferredVendorLocationID = ((e.Row.IsDefault == true) ? e.Row.VendorLocationID : null);
                        ASCIStarItemCurySettings.Update(item);
                        flag = true;
                    }
                }
                else if (prefferedVendor != null && prefferedVendor.BaseCuryID == null)
                {
                    item.PreferredVendorID = null;
                    item.PreferredVendorLocationID = null;
                    ASCIStarItemCurySettings.Update(item);
                }
            }

            if (!(e.Row.IsDefault == true && flag))
            {
                return;
            }

            foreach (var currentRow in VendorItems.Select().RowCast<POVendorInventory>())
            {
                if (currentRow.RecordID != e.Row.RecordID && currentRow.IsDefault == true)
                {
                    VendorItems.Cache.SetValue<POVendorInventory.isDefault>(currentRow, false);
                }
            }

            VendorItems.Cache.ClearQueryCacheObsolete();
            VendorItems.View.RequestRefresh();

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJSMPOVendorInventoryExt>(e.Row);
            SetBasisValueOnStockComp(rowExt);
        }
        #endregion

        #endregion

        #region ServiceMethods
        protected virtual void SetReadOnlyPOVendorInventoryFields(PXCache cache, POVendorInventory row)
        {
            PXUIFieldAttribute.SetReadOnly<ASCJSMPOVendorInventoryExt.usrContractIncrement>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJSMPOVendorInventoryExt.usrContractLossPct>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJSMPOVendorInventoryExt.usrContractSurcharge>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJSMPOVendorInventoryExt.usrPreciousMetalCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJSMPOVendorInventoryExt.usrOtherMaterialsCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJSMPOVendorInventoryExt.usrFabricationCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJSMPOVendorInventoryExt.usrPackagingCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJSMPOVendorInventoryExt.usrLaborCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJSMPOVendorInventoryExt.usrPackagingLaborCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJSMPOVendorInventoryExt.usrHandlingCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJSMPOVendorInventoryExt.usrFreightCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJSMPOVendorInventoryExt.usrDutyCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJSMPOVendorInventoryExt.usrUnitCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJSMPOVendorInventoryExt.usrMatrixStep>(cache, row, true);
        }

        protected virtual void SetVisiblePOVendorInventoryFields(PXCache cache, POVendorInventory row)
        {
            PXUIFieldAttribute.SetVisible<ASCJSMPOVendorInventoryExt.usrContractIncrement>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJSMPOVendorInventoryExt.usrContractLossPct>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJSMPOVendorInventoryExt.usrContractSurcharge>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJSMPOVendorInventoryExt.usrPreciousMetalCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJSMPOVendorInventoryExt.usrOtherMaterialsCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJSMPOVendorInventoryExt.usrFabricationCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJSMPOVendorInventoryExt.usrPackagingCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJSMPOVendorInventoryExt.usrLaborCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJSMPOVendorInventoryExt.usrPackagingLaborCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJSMPOVendorInventoryExt.usrHandlingCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJSMPOVendorInventoryExt.usrFreightCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJSMPOVendorInventoryExt.usrDutyCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJSMPOVendorInventoryExt.usrUnitCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJSMPOVendorInventoryExt.usrMatrixStep>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJSMPOVendorInventoryExt.usrEstLandedCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJSMPOVendorInventoryExt.usrCeiling>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJSMPOVendorInventoryExt.usrFloor>(cache, null, false);
        }

        protected virtual void SetVisibleItemWeightFields(PXCache cache, INKitSpecHdr row)
        {
            if (JewelryItemView.Current == null)
                JewelryItemView.Current = JewelryItemView.Select()?.TopFirst;

            var mixedType = ASCJSMMetalType.GetMixedTypeValue(JewelryItemView.Current?.MetalType);
            bool isVisibleMixed = mixedType == ASCJSMConstants.MixedMetalType.Type_MixedDefault ||
                JewelryItemView.Current?.MetalType == null;
            PXUIFieldAttribute.SetVisible<ASCJSMINKitSpecHdrExt.usrActualGRAMSilverRight>(cache, row, isVisibleMixed);
            PXUIFieldAttribute.SetVisible<ASCJSMINKitSpecHdrExt.usrPricingGRAMSilverRight>(cache, row, isVisibleMixed);

            bool isVisibleGold = mixedType == ASCJSMConstants.MixedMetalType.Type_MixedGold ||
                ASCJSMMetalType.IsGold(JewelryItemView.Current?.MetalType);
            PXUIFieldAttribute.SetVisible<ASCJSMINKitSpecHdrExt.usrActualGRAMGold>(cache, row, isVisibleMixed || isVisibleGold);
            PXUIFieldAttribute.SetVisible<ASCJSMINKitSpecHdrExt.usrPricingGRAMGold>(cache, row, isVisibleMixed || isVisibleGold);
            PXUIFieldAttribute.SetVisible<ASCJSMINKitSpecHdrExt.usrContractSurcharge>(cache, row, isVisibleMixed || isVisibleGold);

            bool isVisibleSilver = ASCJSMMetalType.IsSilver(JewelryItemView.Current?.MetalType);
            PXUIFieldAttribute.SetVisible<ASCJSMINKitSpecHdrExt.usrActualGRAMSilver>(cache, row, isVisibleSilver);
            PXUIFieldAttribute.SetVisible<ASCJSMINKitSpecHdrExt.usrPricingGRAMSilver>(cache, row, isVisibleSilver);
            PXUIFieldAttribute.SetVisible<ASCJSMINKitSpecHdrExt.usrMatrixStep>(cache, row, isVisibleMixed || isVisibleSilver);
        }

        protected virtual void SetVisibleINKitSpecStkDet(PXCache cache, INKitSpecStkDet row)
        {
            //if (JewelryItemView.Current == null)
            //    JewelryItemView.Current = JewelryItemView.Select()?.TopFirst;

            //var mixedType = ASCIStarMetalType.GetMixedTypeValue(JewelryItemView.Current?.MetalType);
            //bool isVisibleMixed = mixedType == ASCIStarConstants.MixedMetalType.Type_MixedDefault ||
            //    JewelryItemView.Current?.MetalType == null;

            //bool isVisibleGold = mixedType == ASCIStarConstants.MixedMetalType.Type_MixedGold ||
            //    ASCIStarMetalType.IsGold(JewelryItemView.Current?.MetalType);
            //PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecStkDetExt.usrActualGRAMGold>(cache, null, isVisibleMixed || isVisibleGold);
            //PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecStkDetExt.usrPricingGRAMGold>(cache, null, isVisibleMixed || isVisibleGold);

            //bool isVisibleSilver = ASCIStarMetalType.IsSilver(JewelryItemView.Current?.MetalType);
            //PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecStkDetExt.usrActualGRAMSilver>(cache, null, isVisibleMixed || isVisibleSilver);
            //PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecStkDetExt.usrPricingGRAMSilver>(cache, null, isVisibleMixed || isVisibleSilver);
            //PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecStkDetExt.usrMatrixStep>(cache, null, isVisibleMixed || isVisibleSilver);
        }

        protected virtual void CopyJewelryItemFields(INKitSpecHdr kitSpecHdr)
        {
            var jewelItem = SelectFrom<ASCJSMINJewelryItem>.Where<ASCJSMINJewelryItem.inventoryID.IsEqual<PX.Data.BQL.P.AsInt>>.View.Select(Base, kitSpecHdr?.KitInventoryID)?.TopFirst;

            if (jewelItem == null) return;

            ASCJSMINKitSpecJewelryItem jewelryKitItem = new ASCJSMINKitSpecJewelryItem()
            {
                KitInventoryID = kitSpecHdr.KitInventoryID,
                RevisionID = kitSpecHdr.RevisionID,
                ShortDesc = jewelItem.ShortDesc,
                LongDesc = jewelItem.LongDesc,
                StyleStatus = jewelItem.StyleStatus,
                CustomerCode = jewelItem.CustomerCode,
                InvCategory = jewelItem?.InvCategory,
                ItemType = jewelItem?.ItemType,
                ItemSubType = jewelItem?.ItemSubType,
                Collection = jewelItem.Collection,
                MetalType = jewelItem?.MetalType,
                MetalNote = jewelItem.MetalNote,
                MetalColor = jewelItem?.MetalColor,
                Plating = jewelItem.Plating,
                Finishes = jewelItem.Finishes,
                VendorMaker = jewelItem.VendorMaker,
                OrgCountry = jewelItem?.OrgCountry,
                StoneType = jewelItem.StoneType,
                WebNotesComment = jewelItem.WebNotesComment,
                StoneComment = jewelItem.StoneComment,
                StoneColor = jewelItem?.StoneColor,
                StoneShape = jewelItem.StoneShape,
                StoneCreation = jewelItem.StoneCreation,
                GemstoneTreatment = jewelItem.GemstoneTreatment,
                SettingType = jewelItem.SettingType,
                Findings = jewelItem.Findings,
                FindingsSubType = jewelItem.FindingsSubType,
                ChainType = jewelItem.ChainType,
                RingLength = jewelItem.RingLength,
                RingSize = jewelItem.RingSize,
                OD = jewelItem.OD,
            };

            JewelryItemView.Insert(jewelryKitItem);
        }

        protected virtual void CopyJewelryItemFieldsToStockItem(INKitSpecHdr kitSpecHdr)
        {
            var jewelItem = SelectFrom<ASCJSMINJewelryItem>.Where<ASCJSMINJewelryItem.inventoryID.IsEqual<PX.Data.BQL.P.AsInt>>.View.Select(Base, kitSpecHdr?.KitInventoryID)?.TopFirst;

            if (jewelItem == null) return;

            jewelItem.ShortDesc = JewelryItemView.Current?.ShortDesc;
            jewelItem.LongDesc = JewelryItemView.Current?.LongDesc;
            jewelItem.StyleStatus = JewelryItemView.Current?.StyleStatus;
            jewelItem.CustomerCode = JewelryItemView.Current?.CustomerCode;
            jewelItem.InvCategory = JewelryItemView.Current?.InvCategory;
            jewelItem.ItemType = JewelryItemView.Current?.ItemType;
            jewelItem.ItemSubType = JewelryItemView.Current?.ItemSubType;
            jewelItem.Collection = JewelryItemView.Current?.Collection;
            jewelItem.MetalType = JewelryItemView.Current?.MetalType;
            jewelItem.MetalNote = JewelryItemView.Current?.MetalNote;
            jewelItem.MetalColor = JewelryItemView.Current?.MetalColor;
            jewelItem.Plating = JewelryItemView.Current?.Plating;
            jewelItem.Finishes = JewelryItemView.Current?.Finishes;
            jewelItem.VendorMaker = JewelryItemView.Current?.VendorMaker;
            jewelItem.OrgCountry = JewelryItemView.Current?.OrgCountry;
            jewelItem.StoneType = JewelryItemView.Current?.StoneType;
            jewelItem.WebNotesComment = JewelryItemView.Current?.WebNotesComment;
            jewelItem.StoneComment = JewelryItemView.Current?.StoneComment;
            jewelItem.StoneColor = JewelryItemView.Current?.StoneColor;
            jewelItem.StoneShape = JewelryItemView.Current?.StoneShape;
            jewelItem.StoneCreation = JewelryItemView.Current?.StoneCreation;
            jewelItem.GemstoneTreatment = JewelryItemView.Current?.GemstoneTreatment;
            jewelItem.SettingType = JewelryItemView.Current?.SettingType;
            jewelItem.Findings = JewelryItemView.Current?.Findings;
            jewelItem.FindingsSubType = JewelryItemView.Current?.FindingsSubType;
            jewelItem.ChainType = JewelryItemView.Current?.ChainType;
            jewelItem.RingLength = JewelryItemView.Current?.RingLength;
            jewelItem.RingSize = JewelryItemView.Current?.RingSize;
            jewelItem.OD = JewelryItemView.Current?.OD;
            ASCIStarJewelryItem.Update(jewelItem);
        }

        protected virtual void CopyFieldsValueFromStockItem(INKitSpecHdr kitSpecHdr)
        {
            var item = _itemDataProvider.GetInventoryItemByID(kitSpecHdr?.KitInventoryID);
            if (item != null && kitSpecHdr != null)
            {
                //        kitSpecHdrExt.UsrTotalGoldGrams = itemExt.UsrActualGRAMGold;
                //        kitSpecHdrExt.UsrTotalFineGoldGrams = itemExt.UsrPricingGRAMGold;
                //        kitSpecHdrExt.UsrTotalSilverGrams = itemExt.UsrActualGRAMSilver;
                //        kitSpecHdrExt.UsrTotalFineSilverGrams = itemExt.UsrPricingGRAMSilver;
                //        kitSpecHdrExt.UsrPreciousMetalCost = itemExt.UsrCommodityCost;
                //        kitSpecHdrExt.UsrFabricationCost = itemExt.UsrFabricationCost;
                //        kitSpecHdrExt.UsrOtherCost = itemExt.UsrOtherCost;
                //        kitSpecHdrExt.UsrPackagingCost = itemExt.UsrPackagingCost;
                //        kitSpecHdrExt.UsrLaborCost = itemExt.UsrLaborCost;
                //        kitSpecHdrExt.UsrHandlingCost = itemExt.UsrHandlingCost;
                //        kitSpecHdrExt.UsrFreightCost = itemExt.UsrFreightCost;
                //        kitSpecHdrExt.UsrDutyCost = itemExt.UsrDutyCost;
                //        kitSpecHdrExt.UsrDutyCostPct = itemExt.UsrDutyCostPct;
                //        kitSpecHdrExt.UsrLegacyID = itemExt.UsrLegacyID;
                //        kitSpecHdrExt.UsrLegacyShortRef = itemExt.UsrLegacyShortRef;
                kitSpecHdr.Descr = item.Descr;
                Base.Hdr.Update(kitSpecHdr);
            }
        }

        protected virtual void CopyFieldsValueToStockItem(INKitSpecHdr kitSpecHdr)
        {
            var item = _itemDataProvider.GetInventoryItemByID(kitSpecHdr?.KitInventoryID);
            if (item != null && kitSpecHdr != null)
            {
                var itemExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(item);
                var kitSpecHdrExt = PXCache<INKitSpecHdr>.GetExtension<ASCJSMINKitSpecHdrExt>(kitSpecHdr);
                itemExt.UsrActualGRAMGold = kitSpecHdrExt.UsrActualGRAMGold;
                itemExt.UsrPricingGRAMGold = kitSpecHdrExt.UsrPricingGRAMGold;
                itemExt.UsrActualGRAMSilver = kitSpecHdrExt.UsrActualGRAMSilver;
                itemExt.UsrPricingGRAMSilver = kitSpecHdrExt.UsrPricingGRAMSilver;
                itemExt.UsrPreciousMetalCost = kitSpecHdrExt.UsrPreciousMetalCost;
                itemExt.UsrContractLossPct = kitSpecHdrExt.UsrContractLossPct;
                itemExt.UsrContractSurcharge = kitSpecHdrExt.UsrContractSurcharge;
                itemExt.UsrContractIncrement = kitSpecHdrExt.UsrContractIncrement;
                itemExt.UsrFabricationCost = kitSpecHdrExt.UsrFabricationCost;
                itemExt.UsrOtherCost = kitSpecHdrExt.UsrOtherCost;
                itemExt.UsrOtherMaterialsCost = kitSpecHdrExt.UsrOtherMaterialsCost;
                itemExt.UsrPackagingCost = kitSpecHdrExt.UsrPackagingCost;
                itemExt.UsrPackagingLaborCost = kitSpecHdrExt.UsrPackagingLaborCost;
                itemExt.UsrLaborCost = kitSpecHdrExt.UsrLaborCost;
                itemExt.UsrHandlingCost = kitSpecHdrExt.UsrHandlingCost;
                itemExt.UsrFreightCost = kitSpecHdrExt.UsrFreightCost;
                itemExt.UsrDutyCost = kitSpecHdrExt.UsrDutyCost;
                itemExt.UsrDutyCostPct = kitSpecHdrExt.UsrDutyCostPct;
                itemExt.UsrLegacyID = kitSpecHdrExt.UsrLegacyID;
                itemExt.UsrLegacyShortRef = kitSpecHdrExt.UsrLegacyShortRef;
                ASCIStarInventoryItem.Update(item);
            }
        }

        protected virtual void CopyFieldsValueToPOVendorInventory(INKitSpecHdr kitSpecHdr)
        {
            var poVendorInventory = VendorItems.Select().RowCast<POVendorInventory>().FirstOrDefault(_ => _.IsDefault == true);
            if (poVendorInventory != null && kitSpecHdr != null)
            {
                var poVendorInventoryExt = PXCache<POVendorInventory>.GetExtension<ASCJSMPOVendorInventoryExt>(poVendorInventory);
                var kitSpecHdrExt = PXCache<INKitSpecHdr>.GetExtension<ASCJSMINKitSpecHdrExt>(kitSpecHdr);
                poVendorInventoryExt.UsrPreciousMetalCost = kitSpecHdrExt.UsrPreciousMetalCost;
                poVendorInventoryExt.UsrContractLossPct = kitSpecHdrExt.UsrContractLossPct;
                poVendorInventoryExt.UsrContractSurcharge = kitSpecHdrExt.UsrContractSurcharge;
                poVendorInventoryExt.UsrContractIncrement = kitSpecHdrExt.UsrContractIncrement;
                poVendorInventoryExt.UsrFabricationCost = kitSpecHdrExt.UsrFabricationCost;
                poVendorInventoryExt.UsrOtherCost = kitSpecHdrExt.UsrOtherCost;
                poVendorInventoryExt.UsrOtherMaterialsCost = kitSpecHdrExt.UsrOtherMaterialsCost;
                poVendorInventoryExt.UsrPackagingCost = kitSpecHdrExt.UsrPackagingCost;
                poVendorInventoryExt.UsrPackagingLaborCost = kitSpecHdrExt.UsrPackagingLaborCost;
                poVendorInventoryExt.UsrLaborCost = kitSpecHdrExt.UsrLaborCost;
                poVendorInventoryExt.UsrHandlingCost = kitSpecHdrExt.UsrHandlingCost;
                poVendorInventoryExt.UsrFreightCost = kitSpecHdrExt.UsrFreightCost;
                poVendorInventoryExt.UsrDutyCost = kitSpecHdrExt.UsrDutyCost;
                poVendorInventoryExt.UsrDutyCostPct = kitSpecHdrExt.UsrDutyCostPct;
                VendorItems.Update(poVendorInventory);
            }
        }

        protected virtual ASCJSMCostBuilder CreateCostBuilder(ASCJSMINKitSpecStkDetExt currentRow, INKitSpecStkDet row)
        {
            var defaultVendor = GetItemVendor(row);

            if (defaultVendor != null)
            {
                return new ASCJSMCostBuilder(Base)
                            .WithInventoryItem(currentRow)
                            .WithPOVendorInventory(defaultVendor)
                            .WithPricingData(PXTimeZoneInfo.Today)
                            .Build();
            }

            throw new PXSetPropertyException(ASCJSMINKitMessages.Error.NoDefaultVendor);
        }

        protected virtual POVendorInventory GetItemVendor(INKitSpecStkDet row) =>
            SelectFrom<POVendorInventory>.
                    Where<POVendorInventory.inventoryID.
                        IsEqual<@P.AsInt>>.View.Select(Base, row.CompInventoryID).FirstTableItems.FirstOrDefault(x => x.IsDefault == true);

        protected virtual void UpdateUnitCostAndBasisPrice(PXCache cache, INKitSpecStkDet row, ASCJSMINKitSpecStkDetExt rowExt)
        {
            if (row.DfltCompQty == decimal.Zero)
            {
                rowExt.UsrActualGRAMGold = rowExt.UsrBaseGoldGrams;
                rowExt.UsrActualGRAMSilver = rowExt.UsrBaseSilverGrams;
            }

            var jewelryCostBuilder = CreateCostBuilder(rowExt, row);
            if (jewelryCostBuilder == null) return;

            var resultPerStkComp = jewelryCostBuilder.CalculatePreciousMetalCost(rowExt.UsrCostingType) ?? decimal.Zero;

            decimal? unitCost = row.DfltCompQty == decimal.Zero ? resultPerStkComp : resultPerStkComp / row.DfltCompQty;

            cache.SetValueExt<ASCJSMINKitSpecStkDetExt.usrUnitCost>(row, unitCost);

            var salesPrice = rowExt.UsrCostingType == ASCJSMConstants.CostingType.MarketCost ? jewelryCostBuilder.PreciousMetalMarketCostPerTOZ : jewelryCostBuilder.PreciousMetalContractCostPerTOZ;
            cache.SetValueExt<ASCJSMINKitSpecStkDetExt.usrSalesPrice>(row, salesPrice);
            cache.SetValueExt<ASCJSMINKitSpecStkDetExt.usrBasisPrice>(row, jewelryCostBuilder.PreciousMetalContractCostPerTOZ);
        }

        protected virtual void SetOneTypeForMetalRows(string usrCostingType)
        {
            var rows = this.Base.StockDet.Select().FirstTableItems.Where(x => x.GetExtension<ASCJSMINKitSpecStkDetExt>().UsrCostRollupType == CostRollupType.PreciousMetal);

            bool needToRefresh = false;
            foreach (INKitSpecStkDet row in rows)
            {
                var rowExt = row.GetExtension<ASCJSMINKitSpecStkDetExt>();
                if (rowExt.UsrCostingType == usrCostingType) continue;

                this.Base.StockDet.Cache.SetValueExt<ASCJSMINKitSpecStkDetExt.usrCostingType>(row, usrCostingType);
                this.Base.StockDet.Cache.Update(row);
                needToRefresh = true;
            }
            if (needToRefresh) this.Base.StockDet.View.RequestRefresh();
        }

        protected virtual decimal? GetUnitCostForCommodityItem(INKitSpecStkDet row)
        {
            var value = 0m;
            var defaultVendor = GetItemVendor(row);
            if (defaultVendor == null) return value;

            var defaultVendorExt = PXCache<POVendorInventory>.GetExtension<ASCJSMPOVendorInventoryExt>(defaultVendor);


            if (JewelryItemView.Current == null)
                JewelryItemView.Current = JewelryItemView.Select();

            if (JewelryItemView.Current == null) return value;

            int? metalInventoryID = null;
            decimal multCoef = 24;

            bool isGold = ASCJSMMetalType.IsGold(JewelryItemView.Current.MetalType);
            if (isGold)
            {
                metalInventoryID = _itemDataProvider.GetInventoryItemByCD("24K").InventoryID;
                multCoef = ASCJSMMetalType.GetGoldTypeValue(JewelryItemView.Current.MetalType) / 24;
            }

            bool isSilver = ASCJSMMetalType.IsSilver(JewelryItemView.Current.MetalType);
            if (isSilver)
            {
                metalInventoryID = _itemDataProvider.GetInventoryItemByCD("SSS").InventoryID;
                multCoef = ASCJSMMetalType.GetSilverTypeValue(JewelryItemView.Current.MetalType);
            }

            var vendorPrice = ASCJSMCostBuilder.GetAPVendorPrice(Base, defaultVendorExt.UsrMarketID, metalInventoryID, ASCJSMConstants.TOZ.value, PXTimeZoneInfo.Now);

            if (vendorPrice == null) return value;

            var rowExt = PXCache<INKitSpecStkDet>.GetExtension<ASCJSMINKitSpecStkDetExt>(row);
            var vendorPriceExt = PXCache<APVendorPrice>.GetExtension<ASCJSMAPVendorPriceExt>(vendorPrice);
            if (row.UOM == ASCJSMConstants.GRAM.value)
            {
                if (isSilver)
                {
                    var jewelryCostBuilder = CreateCostBuilder(rowExt, row);
                    if (jewelryCostBuilder == null) return value;

                    var tempValue = jewelryCostBuilder.CalculatePreciousMetalCost(jewelryCostBuilder.ItemCostSpecification.UsrCostingType);
                    value = (jewelryCostBuilder.PreciousMetalAvrSilverMarketCostPerTOZ ?? 0.0m) / ASCJSMConstants.TOZ2GRAM_31_10348.value * multCoef;
                    // return value;
                }
                if (isGold)
                {
                    value = (vendorPriceExt?.UsrCommodityPerGram ?? 0m) * multCoef;
                    //return value;
                }
            }
            else if (row.UOM == ASCJSMConstants.TOZ.value)
            {
                return vendorPrice?.SalesPrice ?? 0m;
            }
            decimal? surchargeValue = (100.0m + (rowExt.UsrContractSurcharge ?? 0.0m)) / 100.0m;
            decimal? metalLossValue = (100.0m + (rowExt.UsrContractLossPct ?? 0.0m)) / 100.0m;
            return value * surchargeValue * metalLossValue;
        }

        protected virtual bool IsCommodityItem(INKitSpecStkDet row)
        {
            var inventoryItem = _itemDataProvider.GetInventoryItemByID(row.CompInventoryID);
            var itemClass = _itemDataProvider.GetItemClassByID(inventoryItem?.ItemClassID);
            return itemClass?.ItemClassCD.NormalizeCD() == ASCJSMConstants.CommodityClass.value;
        }

        protected virtual void DfltGramsForCommodityItemType(PXCache cache, INKitSpecStkDet row)
        {
            var jewelryItem = GetASCIStarINJewelryItem(row.CompInventoryID);

            if (string.IsNullOrEmpty(jewelryItem?.MetalType))
            {
                cache.RaiseExceptionHandling<INKitSpecStkDet.compInventoryID>(row, row.CompInventoryID,
                   new PXSetPropertyException(ASCJSMINKitMessages.Warning.MissingMetalType, PXErrorLevel.RowWarning));
            }
            else
            {
                if (ASCJSMMetalType.IsGold(jewelryItem?.MetalType))
                {
                    var multFactor = ASCJSMMetalType.GetGoldTypeValue(jewelryItem?.MetalType);
                    var fineGrams = (One_Gram * multFactor) / 24;
                    cache.SetValueExt<ASCJSMINKitSpecStkDetExt.usrBaseGoldGrams>(row, One_Gram);
                    cache.SetValueExt<ASCJSMINKitSpecStkDetExt.usrBaseFineGoldGrams>(row, fineGrams);
                }
                if (ASCJSMMetalType.IsSilver(jewelryItem?.MetalType))
                {
                    var multFactor = ASCJSMMetalType.GetSilverTypeValue(jewelryItem?.MetalType);
                    var fineGrams = One_Gram * multFactor;
                    cache.SetValueExt<ASCJSMINKitSpecStkDetExt.usrBaseSilverGrams>(row, One_Gram);
                    cache.SetValueExt<ASCJSMINKitSpecStkDetExt.usrBaseFineSilverGrams>(row, fineGrams);
                }
            }
        }

        protected virtual void InsertNewVendor(INKitSpecStkDet row, string metalType)
        {
            var itemVendor = GetItemVendor(row);
            if (itemVendor != null && !VendorItems.Select().FirstTableItems.Any(x => x.VendorID == itemVendor.VendorID))
            {
                itemVendor.RecordID = null;
                itemVendor.IsDefault = false;
                itemVendor.InventoryID = row.KitInventoryID;
                itemVendor.NoteID = null;

                var inventoryID = ASCJSMMetalType.GetCommodityInventoryByMetalType(this.Base, metalType);
                if (inventoryID != null)
                {
                    var apVendorPrice = ASCJSMCostBuilder.GetAPVendorPrice(this.Base, itemVendor.VendorID, inventoryID, ASCJSMConstants.TOZ.value, PXTimeZoneInfo.Today);
                    var apVendorPriceExt = PXCache<APVendorPrice>.GetExtension<ASCJSMAPVendorPriceExt>(apVendorPrice);
                    var newVendorExt = PXCache<POVendorInventory>.GetExtension<ASCJSMPOVendorInventoryExt>(itemVendor);
                    newVendorExt.UsrBasisPrice = apVendorPriceExt.UsrBasisValue;
                }

                VendorItems.Insert(itemVendor);
            }
        }

        protected virtual void UpdateInKitStkComponents(INKitSpecHdr inKitSpecHdr)
        {
            var inKitSpecHdrExt = PXCache<INKitSpecHdr>.GetExtension<ASCJSMINKitSpecHdrExt>(inKitSpecHdr);

            var stkComponets = this.Base.StockDet.Select()?.FirstTableItems.ToList();

            foreach (var stkComponent in stkComponets)
            {
                if (IsCommodityItem(stkComponent))
                {
                    var stkComponentExt = PXCache<INKitSpecStkDet>.GetExtension<ASCJSMINKitSpecStkDetExt>(stkComponent);

                    if (stkComponentExt.UsrCostRollupType == ASCJSMConstants.CostRollupType.PreciousMetal)
                    {
                        stkComponentExt.UsrContractLossPct = inKitSpecHdrExt.UsrContractLossPct;
                        stkComponentExt.UsrContractSurcharge = inKitSpecHdrExt.UsrContractSurcharge;

                        decimal? preciousMetalCost = GetUnitCostForCommodityItem(stkComponent);
                        stkComponentExt.UsrUnitCost = preciousMetalCost;
                        this.Base.StockDet.Cache.SetValueExt<ASCJSMINKitSpecStkDetExt.usrUnitCost>(stkComponent, stkComponentExt.UsrUnitCost);
                        this.Base.StockDet.Update(stkComponent);
                    }
                }
            }
        }

        private void SetBasisValueOnStockComp(ASCJSMPOVendorInventoryExt rowExt)
        {
            var stockComponets = this.Base.StockDet.Select()?.FirstTableItems.ToList();
            foreach (var stockComponet in stockComponets)
            {
                var stockComponentExt = PXCache<INKitSpecStkDet>.GetExtension<ASCJSMINKitSpecStkDetExt>(stockComponet);
                if (stockComponentExt.UsrCostRollupType == ASCJSMConstants.CostRollupType.PreciousMetal)
                {
                    stockComponentExt.UsrBasisValue = rowExt.UsrBasisValue;
                    this.Base.StockDet.Update(stockComponet);
                }
            }
        }

        private void SetOrUpdatePreciousMetalCost(POVendorInventory row, InventoryItem item, string metalType)
        {
            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJSMPOVendorInventoryExt>(row);
            var marketID = GetVendorMarketID(row, rowExt);

            var vendorPrice = ASCJSMCostBuilder.GetAPVendorPrice(Base, marketID, item.InventoryID, ASCJSMConstants.TOZ.value, PXTimeZoneInfo.Today);
            if (vendorPrice != null)
            {
                var result = vendorPrice.SalesPrice * ASCJSMMetalType.GetMultFactorConvertTOZtoGram(metalType);
                rowExt.UsrPreciousMetalCost = result;
                VendorItems.Update(row);
            }
        }

        private void UpdateTotalSurchargeAndLoss()
        {
            if (this.Base.Hdr.Current == null) return;

            var newTotalLoss = GetFieldTotalPersentage<ASCJSMINKitSpecStkDetExt.usrContractLossPct, ASCJSMINKitSpecStkDetExt.usrContractSurcharge>(this.Base.StockDet.Cache);
            var newTotalSurcharge = GetFieldTotalPersentage<ASCJSMINKitSpecStkDetExt.usrContractSurcharge, ASCJSMINKitSpecStkDetExt.usrContractLossPct>(this.Base.StockDet.Cache);
            var newIncrement = GetIncrementTotalValue();

            this.Base.Hdr.SetValueExt<ASCJSMINKitSpecHdrExt.usrContractLossPct>(this.Base.Hdr.Current, newTotalLoss);
            this.Base.Hdr.SetValueExt<ASCJSMINKitSpecHdrExt.usrContractSurcharge>(this.Base.Hdr.Current, newTotalSurcharge);
            this.Base.Hdr.SetValueExt<ASCJSMINKitSpecHdrExt.usrContractIncrement>(this.Base.Hdr.Current, newIncrement);
        }

        private decimal? GetFieldTotalPersentage<TField, THField>(PXCache cache) where TField : IBqlField where THField : IBqlField
        {
            List<INKitSpecStkDet> stkLineList = this.Base.StockDet.Select()?.FirstTableItems?
                .Where(x => x.GetExtension<ASCJSMINKitSpecStkDetExt>().UsrCostRollupType == ASCJSMConstants.CostRollupType.PreciousMetal).ToList();

            decimal? totalAbsoluteValue = decimal.Zero;
            decimal? totalFieldValue = decimal.Zero;

            foreach (var stkLine in stkLineList)
            {
                var rowExt = stkLine.GetExtension<ASCJSMINKitSpecStkDetExt>();
                decimal? lineBaseValue = (decimal?)cache.GetValue<TField>(stkLine);
                decimal? lineHelperValue = (decimal?)cache.GetValue<THField>(stkLine);
                decimal? temp = rowExt.UsrExtCost / (1 + lineBaseValue / 100) / (1 + lineHelperValue / 100);
                totalAbsoluteValue += temp;
                totalFieldValue += temp * lineBaseValue / 100;
            }

            decimal? returnvalue = totalAbsoluteValue == 0.0m || totalAbsoluteValue == null ? decimal.Zero : totalFieldValue / totalAbsoluteValue * 100;
            return returnvalue;
        }

        private decimal? GetIncrementTotalValue()
        {
            List<INKitSpecStkDet> stkLineList = this.Base.StockDet.Select()?.FirstTableItems?.ToList();

            decimal? totalPerMetalType = decimal.Zero;
            decimal? totalPerPreciousMetalType = decimal.Zero;
            var basicMetalType = JewelryItemView.Select().TopFirst?.MetalType;
            var basiMultFactor = ASCJSMMetalType.GetMultFactorConvertTOZtoGram(basicMetalType);
            foreach (var row in stkLineList)
            {
                var rowExt = row.GetExtension<ASCJSMINKitSpecStkDetExt>();
                string metalType = GetASCIStarINJewelryItem(row.CompInventoryID)?.MetalType;
                totalPerMetalType += ASCJSMMetalType.GetMultFactorConvertTOZtoGram(metalType) * (rowExt.UsrActualGRAMGold + rowExt.UsrActualGRAMSilver);
                totalPerPreciousMetalType += basiMultFactor * (rowExt.UsrActualGRAMGold + rowExt.UsrActualGRAMSilver);
            }


            return totalPerPreciousMetalType == 0.0m || totalPerPreciousMetalType == null
                        ? decimal.Zero
                        : ASCJSMMetalType.GetMultFactorConvertTOZtoGram(basicMetalType) * totalPerMetalType / totalPerPreciousMetalType;
        }

        private int? GetVendorMarketID(POVendorInventory row, ASCJSMPOVendorInventoryExt rowExt)
        {
            int? marketID = null;
            if (rowExt.UsrMarketID == null)
            {
                var vendor = _vendorDataProvider.GetVendor(row.VendorID);
                if (vendor != null)
                {
                    marketID = PXCache<Vendor>.GetExtension<ASCJSMVendorExt>(vendor)?.UsrMarketID;
                }
            }
            else
            {
                marketID = rowExt.UsrMarketID;
            }

            return marketID;
        }

        private bool IsBaseItemsExists()
        {
            return _itemDataProvider.GetInventoryItemByCD(ASCJSMConstants.MetalType.Type_24K) != null &&
                   _itemDataProvider.GetInventoryItemByCD(ASCJSMConstants.MetalType.Type_SSS) != null;
        }

        private ASCJSMINJewelryItem GetASCIStarINJewelryItem(int? inventoryID) =>
            SelectFrom<ASCJSMINJewelryItem>.Where<ASCJSMINJewelryItem.inventoryID.IsEqual<P.AsInt>>.View.Select(Base, inventoryID)?.TopFirst;

        private POVendorInventory GetDefaultPOVendorInventory() => this.VendorItems.Select()?.FirstTableItems.FirstOrDefault(x => x.IsDefault == true);

        #region Emails Methods
        protected virtual void SendEmailNotification(INKitSpecHdr inKitSpecHdr)
        {

            if (this.VendorItems.Current == null)
                this.VendorItems.Current = this.VendorItems.Select()?.RowCast<POVendorInventory>().FirstOrDefault(x => x.IsDefault == true);
            if (this.VendorItems.Current == null)
                throw new PXException(ASCJSMINKitMessages.Error.NoDefaultVendor);

            var bAccount = BAccount.PK.Find(Base, this.VendorItems.Current.VendorID);

            var inventoryItem = InventoryItem.PK.Find(this.Base, inKitSpecHdr.KitInventoryID);

            var sender = new NotificationGenerator
            {
                To = GetVendorEmail(bAccount),
                Subject = string.Format(ASCJSMINKitMessages.EMailSubject, inventoryItem?.InventoryCD),
                Body = CreateEmailBody(inventoryItem, bAccount),
                BodyFormat = EmailFormatListAttribute.Html,
            };

            AddAttachmentsToEmail(sender);

            sender.Send();
        }

        private string GetVendorEmail(BAccount bAccount) =>
           SelectFrom<Contact>.Where<Contact.contactID.IsEqual<P.AsInt>>.View.Select(Base, bAccount.PrimaryContactID)?.TopFirst?.EMail;

        private string CreateEmailBody(InventoryItem inventoryItem, BAccount bAccount)
        {
            Location location = SelectFrom<Location>.Where<Location.bAccountID.IsEqual<P.AsInt>>.View.Select(Base, bAccount.BAccountID)?.TopFirst;

            string companyName = PX.Data.PXLogin.ExtractCompany(PX.Common.PXContext.PXIdentity.IdentityName);

            string returnBodyString = string.Format(ASCJSMINKitMessages.EMailBody,
                bAccount.AcctName, inventoryItem.InventoryCD, inventoryItem.Descr, location?.VLeadTime?.ToString(), companyName, PXAccess.GetUserDisplayName());

            return returnBodyString;
        }

        private void AddAttachmentsToEmail(NotificationGenerator sender)
        {
            UploadFileMaintenance fileUpload = PXGraph.CreateInstance<UploadFileMaintenance>();
            Guid[] savedFileOnScreenGuids = PXNoteAttribute.GetFileNotes(Base.Hdr.Cache, Base.Hdr.Current);

            foreach (var eachGuid in savedFileOnScreenGuids)
            {
                FileInfo file = fileUpload.GetFile(eachGuid);

                sender.AddAttachment(file.Name, file.BinData);
            }
        }
        #endregion

        #endregion
    }
}