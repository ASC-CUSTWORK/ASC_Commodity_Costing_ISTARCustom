using ASCISTARCustom.AP.CacheExt;
using ASCISTARCustom.Common.Builder;
using ASCISTARCustom.Common.Descriptor;
using ASCISTARCustom.Common.Helper;
using ASCISTARCustom.Common.Helper.Extensions;
using ASCISTARCustom.Common.Services.DataProvider.Interfaces;
using ASCISTARCustom.IN.CacheExt;
using ASCISTARCustom.IN.DAC;
using ASCISTARCustom.IN.Descriptor.Constants;
using ASCISTARCustom.INKit.CacheExt;
using ASCISTARCustom.INKit.DAC;
using ASCISTARCustom.INKit.Descriptor;
using ASCISTARCustom.PO.CacheExt;
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

namespace ASCISTARCustom.INKit
{
    public class ASCIStarINKitSpecMaintExt : PXGraphExtension<INKitSpecMaint>
    {
        private const decimal One_Gram = 1m;

        public static bool IsActive() => true;

        #region DataView
        [PXCopyPasteHiddenView]
        public PXSelect<INKitSpecHdr, Where<INKitSpecHdr.kitInventoryID, Equal<Optional<INKitSpecHdr.kitInventoryID>>>> Hdr;

        [PXCopyPasteHiddenView]
        public PXSelect<POVendorInventory, Where<POVendorInventory.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>> VendorItems;

        public SelectFrom<ASCIStarINKitSpecJewelryItem>
                 .Where<ASCIStarINKitSpecJewelryItem.kitInventoryID.IsEqual<INKitSpecHdr.kitInventoryID.FromCurrent>
                    .And<ASCIStarINKitSpecJewelryItem.revisionID.IsEqual<INKitSpecHdr.revisionID.FromCurrent>>>
                        .View JewelryItemView;


        [PXCopyPasteHiddenView]
        public PXSetup<INSetup> ASCIStarINSetup;

        [PXCopyPasteHiddenView]
        public PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>> ASCIStarInventoryItem;

        [PXCopyPasteHiddenView]
        public PXSelect<ASCIStarINJewelryItem, Where<ASCIStarINJewelryItem.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>> ASCIStarJewelryItem;

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
        public IASCIStarInventoryItemDataProvider _itemDataProvider { get; set; }

        [InjectDependency]
        public IASCIStarVendorDataProvider _vendorDataProvider { get; set; }
        #endregion

        #region Overrides
        public override void Initialize()
        {
            base.Initialize();
            var setup = ASCIStarINSetup.Current;
            if (setup != null)
            {
                var setupExt = PXCache<INSetup>.GetExtension<ASCIStarINSetupExt>(setup);
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
                var setupExt = PXCache<INSetup>.GetExtension<ASCIStarINSetupExt>(setup);
                if (setupExt.UsrIsActiveKitVersion == false) // Has to be specified on IN101000 screen
                {
                    CopyJewelryItemFieldsToStockItem(Base.Hdr.Current);
                }
            }
            baseMethod();
        }
        #endregion

        #region CacheAttached

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
                    throw new PXException(ASCIStarINKitMessages.Error.NoDefaultVendor);

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

                    if (ASCIStarMetalType.IsGold(metalType))
                    {
                        var item = _itemDataProvider.GetInventoryItemByCD(ASCIStarConstants.MetalType.Type_24K);
                        SetOrUpdatePreciousMetalCost(row, item, metalType);
                    }
                    else if (ASCIStarMetalType.IsSilver(metalType))
                    {
                        var item = _itemDataProvider.GetInventoryItemByCD(ASCIStarConstants.MetalType.Type_SSS);
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
                    var inSetupExt = setup?.GetExtension<ASCIStarINSetupExt>();
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

        protected virtual void _(Events.FieldSelecting<INKitSpecHdr, ASCIStarINKitSpecHdrExt.usrBasisValue> e)
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

                    if (ASCIStarMetalType.IsGold(metalType) || ASCIStarMetalType.IsSilver(metalType))
                    {
                        var baseItemCd = ASCIStarMetalType.IsGold(metalType) ? ASCIStarConstants.MetalType.Type_24K : ASCIStarConstants.MetalType.Type_SSS;
                        var baseItem = _itemDataProvider.GetInventoryItemByCD(baseItemCd);

                        if (baseItem != null)
                        {
                            var vendorPrice = ASCIStarCostBuilder.GetAPVendorPrice(Base, defaultVendor.VendorID, baseItem.InventoryID, ASCIStarConstants.TOZ.value, PXTimeZoneInfo.Now);

                            if (vendorPrice != null)
                            {
                                if (ASCIStarMetalType.IsGold(metalType))
                                {
                                    value = vendorPrice.SalesPrice;
                                }
                                else
                                {
                                    var roxExt = PXCache<INKitSpecHdr>.GetExtension<ASCIStarINKitSpecHdrExt>(row);
                                    value = (vendorPrice.SalesPrice + (vendorPrice.SalesPrice + (roxExt.UsrMatrixStep ?? 0.5m))) / 2m;
                                }
                            }
                        }
                    }
                    e.ReturnValue = value;
                }
            }
        }

        protected virtual void _(Events.FieldVerifying<INKitSpecHdr, ASCIStarINKitSpecHdrExt.usrBasisValue> e)
        {
            if (e.Row is INKitSpecHdr row)
            {
                if (!IsBaseItemsExists())
                {
                    var rowExt = PXCache<INKitSpecHdr>.GetExtension<ASCIStarINKitSpecHdrExt>(row);
                    e.Cache.RaiseExceptionHandling<ASCIStarINKitSpecHdrExt.usrBasisValue>(row, rowExt.UsrBasisValue,
                        new PXSetPropertyException(ASCIStarINKitMessages.Warning.BaseItemNotSpecifyed, PXErrorLevel.Warning));
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecHdr, ASCIStarINKitSpecHdrExt.usrContractSurcharge> e)
        {
            var row = e.Row;
            if (row == null) return;

            UpdateInKitStkComponents(row);
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecHdr, ASCIStarINKitSpecHdrExt.usrContractLossPct> e)
        {
            var row = e.Row;
            if (row == null) return;

            UpdateInKitStkComponents(row);
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecHdr, ASCIStarINKitSpecHdrExt.usrDutyCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<INKitSpecHdr>.GetExtension<ASCIStarINKitSpecHdrExt>(row);

            if (rowExt.UsrUnitCost == null || rowExt.UsrUnitCost == 0.0m)
            {
                rowExt.UsrDutyCostPct = decimal.Zero;
                return;
            }
            decimal? newCostPctValue = (decimal?)e.NewValue / rowExt.UsrUnitCost * 100.0m;
            if (newCostPctValue == rowExt.UsrDutyCostPct) return;
            rowExt.UsrDutyCostPct = newCostPctValue;
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecHdr, ASCIStarINKitSpecHdrExt.usrDutyCostPct> e)
        {
            var row = e.Row;
            if (row == null) return;

            ASCIStarINKitSpecHdrExt rowExt = PXCache<INKitSpecHdr>.GetExtension<ASCIStarINKitSpecHdrExt>(row);

            decimal? newDutyCostValue = rowExt.UsrUnitCost * (decimal?)e.NewValue / 100.00m;
            if (newDutyCostValue == rowExt.UsrDutyCost) return;
            e.Cache.SetValueExt<ASCIStarINKitSpecHdrExt.usrDutyCost>(row, newDutyCostValue);
        }

        #endregion

        #region INKitSpecStkDet Events
        protected virtual void _(Events.RowSelected<INKitSpecStkDet> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetVisibleINKitSpecStkDet(e.Cache, row);
        }

        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCIStarINKitSpecStkDetExt.usrCostingType> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                var jewelryItem = GetASCIStarINJewelryItem(row.CompInventoryID);
                if (IsCommodityItem(row))
                {
                    e.NewValue = ASCIStarConstants.CostingType.MarketCost;
                }
                else
                {
                    var inventoryItem = _itemDataProvider.GetInventoryItemByID(row.CompInventoryID);
                    if (inventoryItem != null && jewelryItem != null && (ASCIStarMetalType.IsGold(jewelryItem.MetalType) || ASCIStarMetalType.IsSilver(jewelryItem.MetalType)))
                    {

                        var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(inventoryItem);
                        e.NewValue = inventoryItemExt.UsrCostingType;
                    }
                    else
                    {
                        e.NewValue = ASCIStarConstants.CostingType.StandardCost;
                    }
                }
            }
        }

        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCIStarINKitSpecStkDetExt.usrUnitCost> e)
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
                    if (inventoryItem != null && jewelryItem != null && (ASCIStarMetalType.IsGold(jewelryItem?.MetalType) || ASCIStarMetalType.IsSilver(jewelryItem?.MetalType)))
                    {
                        //e.NewValue = ASCIStarCostBuilder.CalculateUnitCost(inventoryItem);
                        var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(inventoryItem);
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

        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCIStarINKitSpecStkDetExt.usrSalesPrice> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                decimal? salesPrice = 0m;
                var defaultVendor = GetItemVendor(row);
                if (defaultVendor != null)
                {
                    var rowExt = PXCache<INKitSpecStkDet>.GetExtension<ASCIStarINKitSpecStkDetExt>(row);
                    if (IsCommodityItem(row))
                    {
                        salesPrice = ASCIStarCostBuilder.GetAPVendorPrice(Base, defaultVendor.VendorID, row.CompInventoryID, ASCIStarConstants.TOZ.value, PXTimeZoneInfo.Now)?.SalesPrice;
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
                            if (rowExt.UsrCostingType == ASCIStarConstants.CostingType.MarketCost)
                            {
                                salesPrice = jewelryCostBuilder.PreciousMetalMarketCostPerTOZ;
                            }
                            else if (rowExt.UsrCostingType == ASCIStarConstants.CostingType.ContractCost)
                            {
                                salesPrice = jewelryCostBuilder.PreciousMetalContractCostPerTOZ;
                            }
                        }
                    }
                }
                e.NewValue = salesPrice;
            }
        }

        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCIStarINKitSpecStkDetExt.usrBasisPrice> e)
        {
            var row = e.Row;
            if (row == null) return;

            var defaultVendor = GetItemVendor(row);

            if (defaultVendor == null) return;
            var defaultVendorExt = PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(defaultVendor);
            e.NewValue = defaultVendorExt?.UsrBasisPrice;
        }

        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCIStarINKitSpecStkDetExt.usrBasisValue> e)
        {
            var row = e.Row;
            if (row == null) return;

            var defaultVendor = GetItemVendor(row);

            if (defaultVendor == null) return;
            var defaultVendorExt = PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(defaultVendor);
            e.NewValue = defaultVendorExt?.UsrBasisValue;
        }

        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCIStarINKitSpecStkDetExt.usrIsMetal> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                var jewelryItem = GetASCIStarINJewelryItem(row.CompInventoryID);
                e.NewValue = jewelryItem != null && (ASCIStarMetalType.IsGold(jewelryItem?.MetalType) || ASCIStarMetalType.IsSilver(jewelryItem?.MetalType));
            }
        }

        protected virtual void _(Events.FieldVerifying<INKitSpecStkDet, INKitSpecStkDet.compInventoryID> e)
        {
            var row = e.Row;
            if (row == null) return;

            var newValue = (int?)e.NewValue;
            if (newValue == null) return;

            if (Hdr.Current?.KitInventoryID == newValue)
            {
                var invItem = _itemDataProvider.GetInventoryItemByID(Hdr.Current?.KitInventoryID);
                e.Cancel = true;
                throw new PXSetPropertyException(ASCIStarINKitMessages.Error.CannotCreateItself, invItem.InventoryCD, invItem.InventoryCD);
            }


            if (JewelryItemView.Current == null)
                JewelryItemView.Current = JewelryItemView.Select()?.TopFirst;



        }

        protected virtual void _(Events.FieldUpdated<INKitSpecStkDet, INKitSpecStkDet.compInventoryID> e)
        {
            var row = e.Row;
            if (row == null) return;

            e.Cache.RaiseFieldDefaulting<ASCIStarINKitSpecStkDetExt.usrCostingType>(row, out object _costType);
            e.Cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrCostingType>(row, _costType);

            e.Cache.RaiseFieldDefaulting<ASCIStarINKitSpecStkDetExt.usrUnitCost>(row, out object _costUnit);
            e.Cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrUnitCost>(row, _costUnit);

            e.Cache.RaiseFieldDefaulting<ASCIStarINKitSpecStkDetExt.usrSalesPrice>(row, out object _salesPrice);
            e.Cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrSalesPrice>(row, _salesPrice);

            e.Cache.RaiseFieldDefaulting<ASCIStarINKitSpecStkDetExt.usrBasisPrice>(row, out object _basisPrice);
            e.Cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrSalesPrice>(row, _basisPrice);

            e.Cache.RaiseFieldDefaulting<ASCIStarINKitSpecStkDetExt.usrBasisValue>(row, out object _basisValue);
            e.Cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrSalesPrice>(row, _basisValue);

            if (IsCommodityItem(row))
            {
                DfltGramsForCommodityItemType(e.Cache, row);

                var inKitHdrExt = PXCache<INKitSpecHdr>.GetExtension<ASCIStarINKitSpecHdrExt>(this.Base.Hdr.Current);
                this.Base.StockDet.Cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrContractLossPct>(row, inKitHdrExt.UsrContractLossPct);
                this.Base.StockDet.Cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrContractSurcharge>(row, inKitHdrExt.UsrContractSurcharge);
            }

            var newValue = (int?)e.NewValue;
            var inJewelryItemDB = GetASCIStarINJewelryItem(newValue);
            if (inJewelryItemDB != null
                    && (inJewelryItemDB.MetalType != null && inJewelryItemDB.MetalType != JewelryItemView.Current?.MetalType
                    && (ASCIStarMetalType.IsGold(inJewelryItemDB.MetalType) || ASCIStarMetalType.IsSilver(inJewelryItemDB.MetalType))))
            {
                JewelryItemView.Current.MetalType = null;
                JewelryItemView.Update(JewelryItemView.Current);
            }

            var itemVendor = GetItemVendor(row);
            if (itemVendor != null && !VendorItems.Select().FirstTableItems.Any(x => x.VendorID == itemVendor.VendorID))
            {
                itemVendor.RecordID = null;
                itemVendor.IsDefault = false;
                itemVendor.InventoryID = row.KitInventoryID;

                var inventoryID = ASCIStarMetalType.GetBaseInventoryID(this.Base, inJewelryItemDB.MetalType);
                if (inventoryID != null)
                {
                    var apVendorPrice = ASCIStarCostBuilder.GetAPVendorPrice(this.Base, itemVendor.VendorID, inventoryID, ASCIStarConstants.TOZ.value, PXTimeZoneInfo.Today);
                    var apVendorPriceExt = PXCache<APVendorPrice>.GetExtension<ASCIStarAPVendorPriceExt>(apVendorPrice);
                    var newVendorExt = PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(itemVendor);
                    newVendorExt.UsrBasisPrice = apVendorPriceExt.UsrBasisValue;
                }

                VendorItems.Insert(itemVendor);
            }
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecStkDet, ASCIStarINKitSpecStkDetExt.usrCostingType> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                var rowExt = PXCache<INKitSpecStkDet>.GetExtension<ASCIStarINKitSpecStkDetExt>(row);
                if (IsCommodityItem(row))
                {
                    var value = GetUnitCostForCommodityItem(row);
                    e.Cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrUnitCost>(row, value);
                }
                else
                {
                    if (rowExt.UsrCostingType == ASCIStarConstants.CostingType.StandardCost)
                    {
                        var result = INItemCost.PK.Find(Base, row.CompInventoryID, Base.Accessinfo.BaseCuryID);
                        e.Cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrUnitCost>(row, result?.AvgCost ?? 0m);
                    }
                    else if (rowExt.UsrCostingType == ASCIStarConstants.CostingType.MarketCost || rowExt.UsrCostingType == ASCIStarConstants.CostingType.ContractCost)
                    {


                        UpdateVendorPrice(e, row, rowExt);
                    }
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecStkDet, ASCIStarINKitSpecStkDetExt.usrCostRollupType> e)
        {
            var row = e.Row;
            if (row == null) return;

            if (!IsCommodityItem(row))
            {
                UpdateTotalSurchargeAndLoss();
            }
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecStkDet, ASCIStarINKitSpecStkDetExt.usrExtCost> e)
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
                var rowExt = PXCache<INKitSpecStkDet>.GetExtension<ASCIStarINKitSpecStkDetExt>(row);
                if (rowExt.UsrCostRollupType == null)
                {
                    e.Cache.RaiseExceptionHandling<ASCIStarINKitSpecStkDetExt.usrCostRollupType>(row, rowExt.UsrCostRollupType,
                        new PXSetPropertyException(ASCIStarINKitMessages.Error.CostRollupTypeNotSet, PXErrorLevel.Error));
                    e.Cancel = true;
                    throw new PXException(ASCIStarINKitMessages.Error.CostRollupTypeNotSet);
                }
            }
        }

        #endregion

        #region INKitSpecNonStkDet Events
        protected virtual void _(Events.FieldDefaulting<INKitSpecNonStkDet, ASCIStarINKitSpecNonStkDetExt.usrUnitCost> e)
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
                var rowExt = PXCache<INKitSpecNonStkDet>.GetExtension<ASCIStarINKitSpecNonStkDetExt>(row);
                if (rowExt.UsrCostRollupType == null)
                {
                    e.Cache.RaiseExceptionHandling<ASCIStarINKitSpecNonStkDetExt.usrCostRollupType>(row, rowExt.UsrCostRollupType,
                        new PXSetPropertyException(ASCIStarINKitMessages.Error.CostRollupTypeNotSet, PXErrorLevel.Error));
                    e.Cancel = true;
                    throw new PXException(ASCIStarINKitMessages.Error.CostRollupTypeNotSet);
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

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(row);
            if (rowExt.UsrMarketID == null)
            {
                e.Cache.RaiseExceptionHandling<ASCIStarPOVendorInventoryExt.usrMarketID>(e.Row, false, new PXSetPropertyException(ASCIStarINConstants.Errors.MarketEmpty, PXErrorLevel.RowError));
            }

            var inventoryID = ASCIStarMetalType.GetBaseInventoryID(this.Base, this.JewelryItemView.Current?.MetalType);

            var apVendorPrice = ASCIStarCostBuilder.GetAPVendorPrice(this.Base, row.VendorID, inventoryID, ASCIStarConstants.TOZ.value, PXTimeZoneInfo.Today);

            if (apVendorPrice == null && PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(row).UsrIsOverrideVendor != true)
            {
                e.Cache.RaiseExceptionHandling<POVendorInventory.isDefault>(row, false,
                    new PXSetPropertyException(ASCIStarMessages.Error.VendorPriceNotFound, PXErrorLevel.RowWarning));
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

        protected virtual void _(Events.FieldVerifying<POVendorInventory, ASCIStarPOVendorInventoryExt.usrBasisPrice> e)
        {
            var row = e.Row;
            if (row == null) return;

            if ((decimal?)e.NewValue == decimal.Zero)
            {
                e.Cache.RaiseExceptionHandling<ASCIStarPOVendorInventoryExt.usrBasisPrice>(row, e.NewValue,
                    new PXSetPropertyException(ASCIStarINConstants.Warnings.BasisOrMarketPriceEmpty, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCIStarPOVendorInventoryExt.usrCommodityID> e)
        {
            var row = e.Row;
            if (row == null || !e.ExternalCall) return;
            var rowExt = row.GetExtension<ASCIStarPOVendorInventoryExt>();
            var inventoryID = (int?)e.NewValue;
            var apVendorPrice = ASCIStarCostBuilder.GetAPVendorPrice(this.Base, rowExt?.UsrMarketID, inventoryID, ASCIStarConstants.TOZ.value, PXTimeZoneInfo.Today);
            var apVendorPriceExt = PXCache<APVendorPrice>.GetExtension<ASCIStarAPVendorPriceExt>(apVendorPrice);
            e.Cache.SetValueExt<ASCIStarPOVendorInventoryExt.usrBasisValue>(row, apVendorPriceExt.UsrBasisValue);
        }

        protected virtual void _(Events.RowPersisting<POVendorInventory> e)
        {
            if (e.Row is POVendorInventory row)
            {
                if (e.Row.IsDefault == true)
                {
                    var rowExt = PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(e.Row);
                    if (rowExt.UsrMarketID == null)
                    {
                        e.Cache.RaiseExceptionHandling<ASCIStarPOVendorInventoryExt.usrMarketID>(row, row.IsDefault, new PXSetPropertyException(ASCIStarINKitMessages.Error.MarketNotFound, PXErrorLevel.Error));
                        e.Cancel = true;
                        throw new PXException(ASCIStarINKitMessages.Error.MarketNotFound);
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

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(e.Row);
            SetBasisValueOnStockComp(rowExt);
        }
        #endregion

        #endregion

        #region ServiceMethods
        protected virtual void SetReadOnlyPOVendorInventoryFields(PXCache cache, POVendorInventory row)
        {
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrContractIncrement>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrContractLossPct>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrContractSurcharge>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrPreciousMetalCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrOtherMaterialsCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrFabricationCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrPackagingCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrLaborCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrPackagingLaborCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrHandlingCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrFreightCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrDutyCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrUnitCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrMatrixStep>(cache, row, true);
        }

        protected virtual void SetVisiblePOVendorInventoryFields(PXCache cache, POVendorInventory row)
        {
            PXUIFieldAttribute.SetVisible<ASCIStarPOVendorInventoryExt.usrContractIncrement>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCIStarPOVendorInventoryExt.usrContractLossPct>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCIStarPOVendorInventoryExt.usrContractSurcharge>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCIStarPOVendorInventoryExt.usrPreciousMetalCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCIStarPOVendorInventoryExt.usrOtherMaterialsCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCIStarPOVendorInventoryExt.usrFabricationCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCIStarPOVendorInventoryExt.usrPackagingCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCIStarPOVendorInventoryExt.usrLaborCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCIStarPOVendorInventoryExt.usrPackagingLaborCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCIStarPOVendorInventoryExt.usrHandlingCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCIStarPOVendorInventoryExt.usrFreightCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCIStarPOVendorInventoryExt.usrDutyCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCIStarPOVendorInventoryExt.usrUnitCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCIStarPOVendorInventoryExt.usrMatrixStep>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCIStarPOVendorInventoryExt.usrEstLandedCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCIStarPOVendorInventoryExt.usrCeiling>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCIStarPOVendorInventoryExt.usrFloor>(cache, null, false);
        }

        protected virtual void SetVisibleItemWeightFields(PXCache cache, INKitSpecHdr row)
        {
            if (JewelryItemView.Current == null)
                JewelryItemView.Current = JewelryItemView.Select()?.TopFirst;

            var mixedType = ASCIStarMetalType.GetMixedTypeValue(JewelryItemView.Current?.MetalType);
            bool isVisibleMixed = mixedType == ASCIStarConstants.MixedMetalType.Type_MixedDefault ||
                JewelryItemView.Current?.MetalType == null;
            PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecHdrExt.usrActualGRAMSilverRight>(cache, row, isVisibleMixed);
            PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecHdrExt.usrPricingGRAMSilverRight>(cache, row, isVisibleMixed);

            bool isVisibleGold = mixedType == ASCIStarConstants.MixedMetalType.Type_MixedGold ||
                ASCIStarMetalType.IsGold(JewelryItemView.Current?.MetalType);
            PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecHdrExt.usrActualGRAMGold>(cache, row, isVisibleMixed || isVisibleGold);
            PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecHdrExt.usrPricingGRAMGold>(cache, row, isVisibleMixed || isVisibleGold);
            PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecHdrExt.usrContractSurcharge>(cache, row, isVisibleMixed || isVisibleGold);

            bool isVisibleSilver = ASCIStarMetalType.IsSilver(JewelryItemView.Current?.MetalType);
            PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecHdrExt.usrActualGRAMSilver>(cache, row, isVisibleSilver);
            PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecHdrExt.usrPricingGRAMSilver>(cache, row, isVisibleSilver);
            PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecHdrExt.usrMatrixStep>(cache, row, isVisibleMixed || isVisibleSilver);
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
            var jewelItem = SelectFrom<ASCIStarINJewelryItem>.Where<ASCIStarINJewelryItem.inventoryID.IsEqual<PX.Data.BQL.P.AsInt>>.View.Select(Base, kitSpecHdr?.KitInventoryID)?.TopFirst;

            if (jewelItem == null) return;

            ASCIStarINKitSpecJewelryItem jewelryKitItem = new ASCIStarINKitSpecJewelryItem()
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
            var jewelItem = SelectFrom<ASCIStarINJewelryItem>.Where<ASCIStarINJewelryItem.inventoryID.IsEqual<PX.Data.BQL.P.AsInt>>.View.Select(Base, kitSpecHdr?.KitInventoryID)?.TopFirst;

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
                var itemExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(item);
                var kitSpecHdrExt = PXCache<INKitSpecHdr>.GetExtension<ASCIStarINKitSpecHdrExt>(kitSpecHdr);
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
                var poVendorInventoryExt = PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(poVendorInventory);
                var kitSpecHdrExt = PXCache<INKitSpecHdr>.GetExtension<ASCIStarINKitSpecHdrExt>(kitSpecHdr);
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

        protected virtual ASCIStarCostBuilder CreateCostBuilder(ASCIStarINKitSpecStkDetExt currentRow, INKitSpecStkDet row)
        {
            var defaultVendor = GetItemVendor(row);

            if (defaultVendor != null)
            {
                return new ASCIStarCostBuilder(Base)
                            .WithInventoryItem(currentRow)
                            .WithPOVendorInventory(defaultVendor)
                            .WithPricingData(PXTimeZoneInfo.Today)
                            .Build();
            }

            throw new PXSetPropertyException(ASCIStarINKitMessages.Error.NoDefaultVendor);
        }

        protected virtual POVendorInventory GetItemVendor(INKitSpecStkDet row) =>
            SelectFrom<POVendorInventory>.
                    Where<POVendorInventory.inventoryID.
                        IsEqual<@P.AsInt>>.View.Select(Base, row.CompInventoryID).FirstTableItems.FirstOrDefault(x => x.IsDefault == true);



        protected virtual void UpdateVendorPrice(Events.FieldUpdated<INKitSpecStkDet, ASCIStarINKitSpecStkDetExt.usrCostingType> e,
            INKitSpecStkDet row, ASCIStarINKitSpecStkDetExt rowExt)
        {
            var inventoryItem = _itemDataProvider.GetInventoryItemByID(row.CompInventoryID);
            var jewelryCostBuilder = CreateCostBuilder(rowExt, row);
            if (jewelryCostBuilder == null) return;

            var result = jewelryCostBuilder.CalculatePreciousMetalCost(e.NewValue?.ToString());
            e.Cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrUnitCost>(row, result);

            var salesPrice = rowExt.UsrCostingType == ASCIStarConstants.CostingType.MarketCost ? jewelryCostBuilder.PreciousMetalMarketCostPerTOZ : jewelryCostBuilder.PreciousMetalContractCostPerTOZ;
            e.Cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrSalesPrice>(row, salesPrice);
            e.Cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrBasisPrice>(row, jewelryCostBuilder.PreciousMetalContractCostPerTOZ);
        }

        protected virtual decimal? GetUnitCostForCommodityItem(INKitSpecStkDet row)
        {
            var value = 0m;
            var defaultVendor = GetItemVendor(row);
            if (defaultVendor == null) return value;

            var defaultVendorExt = PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(defaultVendor);


            if (JewelryItemView.Current == null)
                JewelryItemView.Current = JewelryItemView.Select();

            if (JewelryItemView.Current == null) return value;

            int? metalInventoryID = null;
            decimal multCoef = 24;

            bool isGold = ASCIStarMetalType.IsGold(JewelryItemView.Current.MetalType);
            if (isGold)
            {
                metalInventoryID = _itemDataProvider.GetInventoryItemByCD("24K").InventoryID;
                multCoef = ASCIStarMetalType.GetGoldTypeValue(JewelryItemView.Current.MetalType) / 24;
            }

            bool isSilver = ASCIStarMetalType.IsSilver(JewelryItemView.Current.MetalType);
            if (isSilver)
            {
                metalInventoryID = _itemDataProvider.GetInventoryItemByCD("SSS").InventoryID;
                multCoef = ASCIStarMetalType.GetSilverTypeValue(JewelryItemView.Current.MetalType);
            }

            var vendorPrice = ASCIStarCostBuilder.GetAPVendorPrice(Base, defaultVendorExt.UsrMarketID, metalInventoryID, ASCIStarConstants.TOZ.value, PXTimeZoneInfo.Now);

            if (vendorPrice == null) return value;

            var rowExt = PXCache<INKitSpecStkDet>.GetExtension<ASCIStarINKitSpecStkDetExt>(row);
            var vendorPriceExt = PXCache<APVendorPrice>.GetExtension<ASCIStarAPVendorPriceExt>(vendorPrice);
            if (row.UOM == ASCIStarConstants.GRAM.value)
            {
                if (isSilver)
                {
                    var jewelryCostBuilder = CreateCostBuilder(rowExt, row);
                    if (jewelryCostBuilder == null) return value;

                    var tempValue = jewelryCostBuilder.CalculatePreciousMetalCost(jewelryCostBuilder.ItemCostSpecification.UsrCostingType);
                    value = (jewelryCostBuilder.PreciousMetalAvrSilverMarketCostPerTOZ ?? 0.0m) / ASCIStarConstants.TOZ2GRAM_31_10348.value * multCoef;
                    // return value;
                }
                if (isGold)
                {
                    value = (vendorPriceExt?.UsrCommodityPerGram ?? 0m) * multCoef;
                    //return value;
                }
            }
            else if (row.UOM == ASCIStarConstants.TOZ.value)
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
            return itemClass?.ItemClassCD.NormalizeCD() == ASCIStarConstants.CommodityClass.value;
        }

        protected virtual void DfltGramsForCommodityItemType(PXCache cache, INKitSpecStkDet row)
        {
            var jewelryItem = GetASCIStarINJewelryItem(row.CompInventoryID);

            if (!string.IsNullOrEmpty(jewelryItem?.MetalType))
            {

                if (ASCIStarMetalType.IsGold(jewelryItem?.MetalType))
                {
                    var multFactor = ASCIStarMetalType.GetGoldTypeValue(jewelryItem?.MetalType);
                    var fineGrams = (One_Gram * multFactor) / 24;
                    cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrBaseGoldGrams>(row, One_Gram);
                    cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrBaseFineGoldGrams>(row, fineGrams);
                }
                if (ASCIStarMetalType.IsSilver(jewelryItem?.MetalType))
                {
                    var multFactor = ASCIStarMetalType.GetSilverTypeValue(jewelryItem?.MetalType);
                    var fineGrams = One_Gram * multFactor;
                    cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrBaseSilverGrams>(row, One_Gram);
                    cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrBaseFineSilverGrams>(row, fineGrams);
                }
            }
            else
            {
                cache.RaiseExceptionHandling<INKitSpecStkDet.compInventoryID>(row, row.CompInventoryID,
                    new PXSetPropertyException(ASCIStarINKitMessages.Warning.MissingMetalType, PXErrorLevel.RowWarning));
            }
        }

        protected virtual void UpdateInKitStkComponents(INKitSpecHdr inKitSpecHdr)
        {
            var inKitSpecHdrExt = PXCache<INKitSpecHdr>.GetExtension<ASCIStarINKitSpecHdrExt>(inKitSpecHdr);

            var stkComponets = this.Base.StockDet.Select()?.FirstTableItems.ToList();

            foreach (var stkComponent in stkComponets)
            {
                if (IsCommodityItem(stkComponent))
                {
                    var stkComponentExt = PXCache<INKitSpecStkDet>.GetExtension<ASCIStarINKitSpecStkDetExt>(stkComponent);

                    if (stkComponentExt.UsrCostRollupType == ASCIStarConstants.CostRollupType.PreciousMetal)
                    {
                        stkComponentExt.UsrContractLossPct = inKitSpecHdrExt.UsrContractLossPct;
                        stkComponentExt.UsrContractSurcharge = inKitSpecHdrExt.UsrContractSurcharge;

                        decimal? preciousMetalCost = GetUnitCostForCommodityItem(stkComponent);
                        stkComponentExt.UsrUnitCost = preciousMetalCost;
                        this.Base.StockDet.Cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrUnitCost>(stkComponent, stkComponentExt.UsrUnitCost);
                        this.Base.StockDet.Update(stkComponent);
                    }
                }
            }
        }

        private void SetBasisValueOnStockComp(ASCIStarPOVendorInventoryExt rowExt)
        {
            var stockComponets = this.Base.StockDet.Select()?.FirstTableItems.ToList();
            foreach (var stockComponet in stockComponets)
            {
                var stockComponentExt = PXCache<INKitSpecStkDet>.GetExtension<ASCIStarINKitSpecStkDetExt>(stockComponet);
                if (stockComponentExt.UsrCostRollupType == ASCIStarConstants.CostRollupType.PreciousMetal)
                {
                    stockComponentExt.UsrBasisValue = rowExt.UsrBasisValue;
                    this.Base.StockDet.Update(stockComponet);
                }
            }
        }

        private void SetOrUpdatePreciousMetalCost(POVendorInventory row, InventoryItem item, string metalType)
        {
            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(row);
            var marketID = GetVendorMarketID(row, rowExt);

            var vendorPrice = ASCIStarCostBuilder.GetAPVendorPrice(Base, marketID, item.InventoryID, ASCIStarConstants.TOZ.value, PXTimeZoneInfo.Today);
            if (vendorPrice != null)
            {
                var result = vendorPrice.SalesPrice * ASCIStarMetalType.GetMultFactorConvertTOZtoGram(metalType);
                rowExt.UsrPreciousMetalCost = result;
                VendorItems.Update(row);
            }
        }

        private void UpdateTotalSurchargeAndLoss()
        {
            if (this.Base.Hdr.Current == null) return;

            var newTotalLoss = GetFieldTotalPersentage<ASCIStarINKitSpecStkDetExt.usrContractLossPct>(this.Base.StockDet.Cache);
            var newTotalSurcharge = GetFieldTotalPersentage<ASCIStarINKitSpecStkDetExt.usrContractSurcharge>(this.Base.StockDet.Cache);
            var newIncrement = GetIncrementTotalValue();

            this.Base.Hdr.SetValueExt<ASCIStarINKitSpecHdrExt.usrContractLossPct>(this.Base.Hdr.Current, newTotalLoss);
            this.Base.Hdr.SetValueExt<ASCIStarINKitSpecHdrExt.usrContractSurcharge>(this.Base.Hdr.Current, newTotalSurcharge);
            this.Base.Hdr.SetValueExt<ASCIStarINKitSpecHdrExt.usrContractIncrement>(this.Base.Hdr.Current, newIncrement);
        }

        private decimal? GetFieldTotalPersentage<TField>(PXCache cache) where TField : IBqlField
        {
            List<INKitSpecStkDet> stkLineList = this.Base.StockDet.Select()?.FirstTableItems?
                .Where(x => x.GetExtension<ASCIStarINKitSpecStkDetExt>().UsrCostRollupType == ASCIStarConstants.CostRollupType.PreciousMetal).ToList();

            decimal? totalLossAbsValue = stkLineList.Sum(row =>
            {
                var rowExt = row.GetExtension<ASCIStarINKitSpecStkDetExt>();
                decimal? lineFieldValue = (decimal?)cache.GetValue<TField>(row);
                return rowExt.UsrExtCost * lineFieldValue;
            });

            decimal? totalExtCost = stkLineList.Sum(line => line.GetExtension<ASCIStarINKitSpecStkDetExt>().UsrExtCost);

            return totalExtCost == 0.0m || totalExtCost == null ? decimal.Zero : totalLossAbsValue / totalExtCost;
        }

        private decimal? GetIncrementTotalValue()
        {
            List<INKitSpecStkDet> stkLineList = this.Base.StockDet.Select()?.FirstTableItems?.ToList();

            decimal? totalPerMetalType = decimal.Zero;
            decimal? totalPerPreciousMetalType = decimal.Zero;
            foreach (var row in stkLineList)
            {
                var rowExt = row.GetExtension<ASCIStarINKitSpecStkDetExt>();
                string metalType = GetASCIStarINJewelryItem(row.CompInventoryID)?.MetalType;
                totalPerMetalType += ASCIStarMetalType.GetMultFactorConvertTOZtoGram(metalType) * (rowExt.UsrActualGRAMGold + rowExt.UsrActualGRAMSilver);
                totalPerPreciousMetalType += ASCIStarMetalType.GetMultFactorConvertTOZtoGram(JewelryItemView.Select().TopFirst?.MetalType) * (rowExt.UsrActualGRAMGold + rowExt.UsrActualGRAMSilver); ;
            }


            return totalPerPreciousMetalType == 0.0m || totalPerPreciousMetalType == null
                ? decimal.Zero
                : ASCIStarMetalType.GetMultFactorConvertTOZtoGram(JewelryItemView.Select().TopFirst?.MetalType) * totalPerMetalType / totalPerPreciousMetalType;
        }

        private int? GetVendorMarketID(POVendorInventory row, ASCIStarPOVendorInventoryExt rowExt)
        {
            int? marketID = null;
            if (rowExt.UsrMarketID == null)
            {
                var vendor = _vendorDataProvider.GetVendor(row.VendorID);
                if (vendor != null)
                {
                    marketID = PXCache<Vendor>.GetExtension<ASCIStarVendorExt>(vendor)?.UsrMarketID;
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
            return _itemDataProvider.GetInventoryItemByCD(ASCIStarConstants.MetalType.Type_24K) != null &&
                   _itemDataProvider.GetInventoryItemByCD(ASCIStarConstants.MetalType.Type_SSS) != null;
        }

        private ASCIStarINJewelryItem GetASCIStarINJewelryItem(int? inventoryID) =>
            SelectFrom<ASCIStarINJewelryItem>.Where<ASCIStarINJewelryItem.inventoryID.IsEqual<P.AsInt>>.View.Select(Base, inventoryID)?.TopFirst;

        private POVendorInventory GetDefaultPOVendorInventory() => this.VendorItems.Select()?.FirstTableItems.FirstOrDefault(x => x.IsDefault == true);

        #region Emails Methods
        protected virtual void SendEmailNotification(INKitSpecHdr inKitSpecHdr)
        {

            if (this.VendorItems.Current == null)
                this.VendorItems.Current = this.VendorItems.Select()?.RowCast<POVendorInventory>().FirstOrDefault(x => x.IsDefault == true);
            if (this.VendorItems.Current == null)
                throw new PXException(ASCIStarINKitMessages.Error.NoDefaultVendor);

            var bAccount = BAccount.PK.Find(Base, this.VendorItems.Current.VendorID);

            var inventoryItem = InventoryItem.PK.Find(this.Base, inKitSpecHdr.KitInventoryID);

            var sender = new NotificationGenerator
            {
                To = GetVendorEmail(bAccount),
                Subject = string.Format(ASCIStarINKitMessages.EMailSubject, inventoryItem?.InventoryCD),
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

            string returnBodyString = string.Format(ASCIStarINKitMessages.EMailBody,
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