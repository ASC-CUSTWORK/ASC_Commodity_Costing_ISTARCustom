using ASCJewelryLibrary.AP.CacheExt;
using ASCJewelryLibrary.Common.Builder;
using ASCJewelryLibrary.Common.Descriptor;
using ASCJewelryLibrary.Common.Helper;
using ASCJewelryLibrary.Common.Helper.Extensions;
using ASCJewelryLibrary.Common.Services.DataProvider.Interfaces;
using ASCJewelryLibrary.IN.CacheExt;
using ASCJewelryLibrary.IN.DAC;
using ASCJewelryLibrary.IN.Descriptor.Constants;
using ASCJewelryLibrary.INKit.CacheExt;
using ASCJewelryLibrary.INKit.DAC;
using ASCJewelryLibrary.INKit.Descriptor;
using ASCJewelryLibrary.PO.CacheExt;
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

namespace ASCJewelryLibrary.INKit
{
    public class ASCJINKitSpecMaintExt : PXGraphExtension<INKitSpecMaint>
    {
        private const decimal One_Gram = 1m;

        public static bool IsActive() => true;

        #region DataView
        [PXCopyPasteHiddenView]
        public PXSelect<INKitSpecHdr, Where<INKitSpecHdr.kitInventoryID, Equal<Optional<INKitSpecHdr.kitInventoryID>>>> Hdr;

        [PXCopyPasteHiddenView]
        public PXSelect<POVendorInventory, Where<POVendorInventory.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>> VendorItems;

        public SelectFrom<ASCJINKitSpecJewelryItem>
                 .Where<ASCJINKitSpecJewelryItem.kitInventoryID.IsEqual<INKitSpecHdr.kitInventoryID.FromCurrent>
                    .And<ASCJINKitSpecJewelryItem.revisionID.IsEqual<INKitSpecHdr.revisionID.FromCurrent>>>
                        .View JewelryItemView;


        [PXCopyPasteHiddenView]
        public PXSetup<INSetup> ASCJINSetup;

        [PXCopyPasteHiddenView]
        public PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>> ASCJInventoryItem;

        [PXCopyPasteHiddenView]
        public PXSelect<ASCJINJewelryItem, Where<ASCJINJewelryItem.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>> ASCJJewelryItem;

        [PXCopyPasteHiddenView]
        public FbqlSelect<SelectFromBase<InventoryItemCurySettings,
            TypeArrayOf<IFbqlJoin>.Empty>.Where<BqlChainableConditionBase<TypeArrayOf<IBqlBinary>.FilledWith<And<Compare<InventoryItemCurySettings.inventoryID,
                Equal<P.AsInt>>>>>.And<BqlOperand<InventoryItemCurySettings.curyID, IBqlString>.IsEqual<BqlField<AccessInfo.baseCuryID, IBqlString>.AsOptional>>>,
            InventoryItemCurySettings>.View ASCJItemCurySettings;

        [PXCopyPasteHiddenView]
        public FbqlSelect<SelectFromBase<InventoryItemCurySettings,
            TypeArrayOf<IFbqlJoin>.Empty>.Where<BqlOperand<InventoryItemCurySettings.inventoryID, IBqlInt>.IsEqual<P.AsInt>>,
            InventoryItemCurySettings>.View ASCJAllItemCurySettings;
        #endregion

        #region Dependency Injection
        [InjectDependency]
        public IASCJInventoryItemDataProvider _itemDataProvider { get; set; }

        [InjectDependency]
        public IASCJVendorDataProvider _vendorDataProvider { get; set; }
        #endregion

        #region Overrides
        public override void Initialize()
        {
            base.Initialize();
            var setup = ASCJINSetup.Current;
            if (setup != null)
            {
                var setupExt = PXCache<INSetup>.GetExtension<ASCJINSetupExt>(setup);
                ASCJCreateProdItem.SetVisible(!setupExt.UsrIsActiveKitVersion ?? false);
                ASCJCreateProdItem.SetEnabled(!setupExt.UsrIsActiveKitVersion ?? false);
            }
        }

        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            CopyFieldsValueToStockItem(Base.Hdr.Current);
            CopyFieldsValueToPOVendorInventory(Base.Hdr.Current);

            var setup = ASCJINSetup.Current;
            if (setup != null)
            {
                var setupExt = PXCache<INSetup>.GetExtension<ASCJINSetupExt>(setup);
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
                    throw new PXException(ASCJINKitMessages.Error.NoDefaultVendor);

                SendEmailNotification(this.Base.Hdr.Current);
            });
        }

        public PXAction<INKitSpecHdr> ASCJCreateProdItem;
        [PXUIField(DisplayName = "Create Production Item", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable aSCIStarCreateProdItem(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXAction<INKitSpecHdr> ASCJUpdateMetalCost;
        [PXUIField(DisplayName = "Update Metal Cost", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(CommitChanges = true)]
        public virtual IEnumerable aSCIStarUpdateMetalCost(PXAdapter adapter)
        {
            var currentHdr = Base.Hdr.Current;
            if (currentHdr != null)
            {
                VendorItems.Select().RowCast<POVendorInventory>().ForEach(row =>
                {

                    // var jewelryItem = GetASCJINJewelryItem(currentHdr.KitInventoryID);
                    if (JewelryItemView.Current == null)
                        JewelryItemView.Current = JewelryItemView.Select()?.TopFirst;

                    var metalType = JewelryItemView.Current?.MetalType;

                    if (ASCJMetalType.IsGold(metalType))
                    {
                        var item = _itemDataProvider.GetInventoryItemByCD(ASCJConstants.MetalType.Type_24K);
                        SetOrUpdatePreciousMetalCost(row, item, metalType);
                    }
                    else if (ASCJMetalType.IsSilver(metalType))
                    {
                        var item = _itemDataProvider.GetInventoryItemByCD(ASCJConstants.MetalType.Type_SSS);
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
                var setup = ASCJINSetup.Current;
                if (setup != null)
                {
                    var inSetupExt = setup?.GetExtension<ASCJINSetupExt>();
                    PXUIFieldAttribute.SetVisible<INKitSpecHdr.revisionID>(Base.Hdr.Cache, Base.Hdr.Current, inSetupExt?.UsrIsActiveKitVersion == true);
                }

                SetVisibleItemWeightFields(e.Cache, row);
            }
        }
        //protected virtual void _(Events.FieldSelecting<INKitSpecHdr, ASCJINKitSpecHdrExt.usrUnitCost> e)
        //{
        //    if (e.Row is INKitSpecHdr row)
        //    {
        //        var rowExt = PXCache<INKitSpecHdr>.GetExtension<ASCJINKitSpecHdrExt>(row);

        //        var result = ASCJCostBuilder.CalculateUnitCost(rowExt);
        //        e.ReturnValue = result;
        //    }
        //}
        //protected virtual void _(Events.FieldSelecting<INKitSpecHdr, ASCJINKitSpecHdrExt.usrEstLandedCost> e)
        //{
        //    if (e.Row is INKitSpecHdr row)
        //    {
        //        var rowExt = PXCache<INKitSpecHdr>.GetExtension<ASCJINKitSpecHdrExt>(row);

        //        var result = ASCJCostBuilder.CalculateUnitCost(rowExt) + ASCJCostBuilder.CalculateEstLandedCost(rowExt);
        //        e.ReturnValue = result;
        //    }
        //}

        protected virtual void _(Events.FieldSelecting<INKitSpecHdr, ASCJINKitSpecHdrExt.usrBasisValue> e)
        {
            if (e.Row is INKitSpecHdr row)
            {
                var defaultVendor = VendorItems.Select().RowCast<POVendorInventory>().FirstOrDefault(_ => _.IsDefault == true);

                if (defaultVendor != null)
                {
                    decimal? value = 0m;
                    // var jewelryItem = GetASCJINJewelryItem(row.KitInventoryID);
                    if (JewelryItemView.Current == null)
                        JewelryItemView.Current = JewelryItemView.Select()?.TopFirst;

                    var metalType = JewelryItemView.Current?.MetalType;

                    if (ASCJMetalType.IsGold(metalType) || ASCJMetalType.IsSilver(metalType))
                    {
                        var baseItemCd = ASCJMetalType.IsGold(metalType) ? ASCJConstants.MetalType.Type_24K : ASCJConstants.MetalType.Type_SSS;
                        var baseItem = _itemDataProvider.GetInventoryItemByCD(baseItemCd);

                        if (baseItem != null)
                        {
                            var vendorPrice = ASCJCostBuilder.GetAPVendorPrice(Base, defaultVendor.VendorID, baseItem.InventoryID, ASCJConstants.TOZ.value, PXTimeZoneInfo.Now);

                            if (vendorPrice != null)
                            {
                                if (ASCJMetalType.IsGold(metalType))
                                {
                                    value = vendorPrice.SalesPrice;
                                }
                                else
                                {
                                    var roxExt = PXCache<INKitSpecHdr>.GetExtension<ASCJINKitSpecHdrExt>(row);
                                    value = (vendorPrice.SalesPrice + (vendorPrice.SalesPrice + (roxExt.UsrMatrixStep ?? 0.5m))) / 2m;
                                }
                            }
                        }
                    }
                    e.ReturnValue = value;
                }
            }
        }

        protected virtual void _(Events.FieldVerifying<INKitSpecHdr, ASCJINKitSpecHdrExt.usrBasisValue> e)
        {
            if (e.Row is INKitSpecHdr row)
            {
                if (!IsBaseItemsExists())
                {
                    var rowExt = PXCache<INKitSpecHdr>.GetExtension<ASCJINKitSpecHdrExt>(row);
                    e.Cache.RaiseExceptionHandling<ASCJINKitSpecHdrExt.usrBasisValue>(row, rowExt.UsrBasisValue,
                        new PXSetPropertyException(ASCJINKitMessages.Warning.BaseItemNotSpecifyed, PXErrorLevel.Warning));
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecHdr, ASCJINKitSpecHdrExt.usrContractSurcharge> e)
        {
            var row = e.Row;
            if (row == null) return;

            UpdateInKitStkComponents(row);
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecHdr, ASCJINKitSpecHdrExt.usrContractLossPct> e)
        {
            var row = e.Row;
            if (row == null) return;

            UpdateInKitStkComponents(row);
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecHdr, ASCJINKitSpecHdrExt.usrDutyCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<INKitSpecHdr>.GetExtension<ASCJINKitSpecHdrExt>(row);

            if (rowExt.UsrUnitCost == null || rowExt.UsrUnitCost == 0.0m)
            {
                rowExt.UsrDutyCostPct = decimal.Zero;
                return;
            }
            decimal? newCostPctValue = (decimal?)e.NewValue / rowExt.UsrUnitCost * 100.0m;
            if (newCostPctValue == rowExt.UsrDutyCostPct) return;
            rowExt.UsrDutyCostPct = newCostPctValue;
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecHdr, ASCJINKitSpecHdrExt.usrDutyCostPct> e)
        {
            var row = e.Row;
            if (row == null) return;

            ASCJINKitSpecHdrExt rowExt = PXCache<INKitSpecHdr>.GetExtension<ASCJINKitSpecHdrExt>(row);

            decimal? newDutyCostValue = rowExt.UsrUnitCost * (decimal?)e.NewValue / 100.00m;
            if (newDutyCostValue == rowExt.UsrDutyCost) return;
            e.Cache.SetValueExt<ASCJINKitSpecHdrExt.usrDutyCost>(row, newDutyCostValue);
        }

        #endregion

        #region INKitSpecStkDet Events
        protected virtual void _(Events.RowSelected<INKitSpecStkDet> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetVisibleINKitSpecStkDet(e.Cache, row);
        }

        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCJINKitSpecStkDetExt.usrCostingType> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                var jewelryItem = GetASCJINJewelryItem(row.CompInventoryID);
                if (IsCommodityItem(row))
                {
                    e.NewValue = ASCJConstants.CostingType.MarketCost;
                }
                else
                {
                    var inventoryItem = _itemDataProvider.GetInventoryItemByID(row.CompInventoryID);
                    if (inventoryItem != null && jewelryItem != null && (ASCJMetalType.IsGold(jewelryItem.MetalType) || ASCJMetalType.IsSilver(jewelryItem.MetalType)))
                    {

                        var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(inventoryItem);
                        e.NewValue = inventoryItemExt.UsrCostingType;
                    }
                    else
                    {
                        e.NewValue = ASCJConstants.CostingType.StandardCost;
                    }
                }
            }
        }

        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCJINKitSpecStkDetExt.usrUnitCost> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                var jewelryItem = GetASCJINJewelryItem(row.CompInventoryID);
                if (IsCommodityItem(row))
                {
                    e.NewValue = GetUnitCostForCommodityItem(row);

                }
                else
                {
                    var inventoryItem = _itemDataProvider.GetInventoryItemByID(row.CompInventoryID);
                    if (inventoryItem != null && jewelryItem != null && (ASCJMetalType.IsGold(jewelryItem?.MetalType) || ASCJMetalType.IsSilver(jewelryItem?.MetalType)))
                    {
                        //e.NewValue = ASCJCostBuilder.CalculateUnitCost(inventoryItem);
                        var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(inventoryItem);
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

        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCJINKitSpecStkDetExt.usrSalesPrice> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                decimal? salesPrice = 0m;
                var defaultVendor = GetItemVendor(row);
                if (defaultVendor != null)
                {
                    var rowExt = PXCache<INKitSpecStkDet>.GetExtension<ASCJINKitSpecStkDetExt>(row);
                    if (IsCommodityItem(row))
                    {
                        salesPrice = ASCJCostBuilder.GetAPVendorPrice(Base, defaultVendor.VendorID, row.CompInventoryID, ASCJConstants.TOZ.value, PXTimeZoneInfo.Now)?.SalesPrice;
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
                            if (rowExt.UsrCostingType == ASCJConstants.CostingType.MarketCost)
                            {
                                salesPrice = jewelryCostBuilder.PreciousMetalMarketCostPerTOZ;
                            }
                            else if (rowExt.UsrCostingType == ASCJConstants.CostingType.ContractCost)
                            {
                                salesPrice = jewelryCostBuilder.PreciousMetalContractCostPerTOZ;
                            }
                        }
                    }
                }
                e.NewValue = salesPrice;
            }
        }

        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCJINKitSpecStkDetExt.usrBasisPrice> e)
        {
            var row = e.Row;
            if (row == null) return;

            var defaultVendor = GetItemVendor(row);

            if (defaultVendor == null) return;
            var defaultVendorExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(defaultVendor);
            e.NewValue = defaultVendorExt?.UsrBasisPrice;
        }

        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCJINKitSpecStkDetExt.usrBasisValue> e)
        {
            var row = e.Row;
            if (row == null) return;

            var defaultVendor = GetItemVendor(row);

            if (defaultVendor == null) return;
            var defaultVendorExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(defaultVendor);
            e.NewValue = defaultVendorExt?.UsrBasisValue;
        }

        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCJINKitSpecStkDetExt.usrIsMetal> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                var jewelryItem = GetASCJINJewelryItem(row.CompInventoryID);
                e.NewValue = jewelryItem != null && (ASCJMetalType.IsGold(jewelryItem?.MetalType) || ASCJMetalType.IsSilver(jewelryItem?.MetalType));
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
                throw new PXSetPropertyException(ASCJINKitMessages.Error.CannotCreateItself, invItem.InventoryCD, invItem.InventoryCD);
            }


            if (JewelryItemView.Current == null)
                JewelryItemView.Current = JewelryItemView.Select()?.TopFirst;



        }

        protected virtual void _(Events.FieldUpdated<INKitSpecStkDet, INKitSpecStkDet.compInventoryID> e)
        {
            var row = e.Row;
            if (row == null) return;

            e.Cache.RaiseFieldDefaulting<ASCJINKitSpecStkDetExt.usrCostingType>(row, out object _costType);
            e.Cache.SetValueExt<ASCJINKitSpecStkDetExt.usrCostingType>(row, _costType);

            e.Cache.RaiseFieldDefaulting<ASCJINKitSpecStkDetExt.usrUnitCost>(row, out object _costUnit);
            e.Cache.SetValueExt<ASCJINKitSpecStkDetExt.usrUnitCost>(row, _costUnit);

            e.Cache.RaiseFieldDefaulting<ASCJINKitSpecStkDetExt.usrSalesPrice>(row, out object _salesPrice);
            e.Cache.SetValueExt<ASCJINKitSpecStkDetExt.usrSalesPrice>(row, _salesPrice);

            e.Cache.RaiseFieldDefaulting<ASCJINKitSpecStkDetExt.usrBasisPrice>(row, out object _basisPrice);
            e.Cache.SetValueExt<ASCJINKitSpecStkDetExt.usrSalesPrice>(row, _basisPrice);

            e.Cache.RaiseFieldDefaulting<ASCJINKitSpecStkDetExt.usrBasisValue>(row, out object _basisValue);
            e.Cache.SetValueExt<ASCJINKitSpecStkDetExt.usrSalesPrice>(row, _basisValue);

            if (IsCommodityItem(row))
            {
                DfltGramsForCommodityItemType(e.Cache, row);

                var inKitHdrExt = PXCache<INKitSpecHdr>.GetExtension<ASCJINKitSpecHdrExt>(this.Base.Hdr.Current);
                this.Base.StockDet.Cache.SetValueExt<ASCJINKitSpecStkDetExt.usrContractLossPct>(row, inKitHdrExt.UsrContractLossPct);
                this.Base.StockDet.Cache.SetValueExt<ASCJINKitSpecStkDetExt.usrContractSurcharge>(row, inKitHdrExt.UsrContractSurcharge);
            }

            var newValue = (int?)e.NewValue;
            var inJewelryItemDB = GetASCJINJewelryItem(newValue);
            if (inJewelryItemDB != null
                    && (inJewelryItemDB.MetalType != null && inJewelryItemDB.MetalType != JewelryItemView.Current?.MetalType
                    && (ASCJMetalType.IsGold(inJewelryItemDB.MetalType) || ASCJMetalType.IsSilver(inJewelryItemDB.MetalType))))
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

                var inventoryID = ASCJMetalType.GetBaseInventoryID(this.Base, inJewelryItemDB.MetalType);
                if (inventoryID != null)
                {
                    var apVendorPrice = ASCJCostBuilder.GetAPVendorPrice(this.Base, itemVendor.VendorID, inventoryID, ASCJConstants.TOZ.value, PXTimeZoneInfo.Today);
                    var apVendorPriceExt = PXCache<APVendorPrice>.GetExtension<ASCJAPVendorPriceExt>(apVendorPrice);
                    var newVendorExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(itemVendor);
                    newVendorExt.UsrBasisPrice = apVendorPriceExt.UsrBasisValue;
                }

                VendorItems.Insert(itemVendor);
            }
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecStkDet, ASCJINKitSpecStkDetExt.usrCostingType> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                var rowExt = PXCache<INKitSpecStkDet>.GetExtension<ASCJINKitSpecStkDetExt>(row);
                if (IsCommodityItem(row))
                {
                    var value = GetUnitCostForCommodityItem(row);
                    e.Cache.SetValueExt<ASCJINKitSpecStkDetExt.usrUnitCost>(row, value);
                }
                else
                {
                    if (rowExt.UsrCostingType == ASCJConstants.CostingType.StandardCost)
                    {
                        var result = INItemCost.PK.Find(Base, row.CompInventoryID, Base.Accessinfo.BaseCuryID);
                        e.Cache.SetValueExt<ASCJINKitSpecStkDetExt.usrUnitCost>(row, result?.AvgCost ?? 0m);
                    }
                    else if (rowExt.UsrCostingType == ASCJConstants.CostingType.MarketCost || rowExt.UsrCostingType == ASCJConstants.CostingType.ContractCost)
                    {


                        UpdateVendorPrice(e, row, rowExt);
                    }
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecStkDet, ASCJINKitSpecStkDetExt.usrCostRollupType> e)
        {
            var row = e.Row;
            if (row == null) return;

            if (!IsCommodityItem(row))
            {
                UpdateTotalSurchargeAndLoss();
            }
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecStkDet, ASCJINKitSpecStkDetExt.usrExtCost> e)
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
                var rowExt = PXCache<INKitSpecStkDet>.GetExtension<ASCJINKitSpecStkDetExt>(row);
                if (rowExt.UsrCostRollupType == null)
                {
                    e.Cache.RaiseExceptionHandling<ASCJINKitSpecStkDetExt.usrCostRollupType>(row, rowExt.UsrCostRollupType,
                        new PXSetPropertyException(ASCJINKitMessages.Error.CostRollupTypeNotSet, PXErrorLevel.Error));
                    e.Cancel = true;
                    throw new PXException(ASCJINKitMessages.Error.CostRollupTypeNotSet);
                }
            }
        }

        #endregion

        #region INKitSpecNonStkDet Events
        protected virtual void _(Events.FieldDefaulting<INKitSpecNonStkDet, ASCJINKitSpecNonStkDetExt.usrUnitCost> e)
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
                var rowExt = PXCache<INKitSpecNonStkDet>.GetExtension<ASCJINKitSpecNonStkDetExt>(row);
                if (rowExt.UsrCostRollupType == null)
                {
                    e.Cache.RaiseExceptionHandling<ASCJINKitSpecNonStkDetExt.usrCostRollupType>(row, rowExt.UsrCostRollupType,
                        new PXSetPropertyException(ASCJINKitMessages.Error.CostRollupTypeNotSet, PXErrorLevel.Error));
                    e.Cancel = true;
                    throw new PXException(ASCJINKitMessages.Error.CostRollupTypeNotSet);
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

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(row);
            if (rowExt.UsrMarketID == null)
            {
                e.Cache.RaiseExceptionHandling<ASCJPOVendorInventoryExt.usrMarketID>(e.Row, false, new PXSetPropertyException(ASCJINConstants.Errors.MarketEmpty, PXErrorLevel.RowError));
            }

            var inventoryID = ASCJMetalType.GetBaseInventoryID(this.Base, this.JewelryItemView.Current?.MetalType);

            var apVendorPrice = ASCJCostBuilder.GetAPVendorPrice(this.Base, row.VendorID, inventoryID, ASCJConstants.TOZ.value, PXTimeZoneInfo.Today);

            if (apVendorPrice == null && PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(row).UsrIsOverrideVendor != true)
            {
                e.Cache.RaiseExceptionHandling<POVendorInventory.isDefault>(row, false,
                    new PXSetPropertyException(ASCJMessages.Error.VendorPriceNotFound, PXErrorLevel.RowWarning));
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

        protected virtual void _(Events.FieldVerifying<POVendorInventory, ASCJPOVendorInventoryExt.usrBasisPrice> e)
        {
            var row = e.Row;
            if (row == null) return;

            if ((decimal?)e.NewValue == decimal.Zero)
            {
                e.Cache.RaiseExceptionHandling<ASCJPOVendorInventoryExt.usrBasisPrice>(row, e.NewValue,
                    new PXSetPropertyException(ASCJINConstants.Warnings.BasisOrMarketPriceEmpty, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCJPOVendorInventoryExt.usrCommodityID> e)
        {
            var row = e.Row;
            if (row == null || !e.ExternalCall) return;
            var rowExt = row.GetExtension<ASCJPOVendorInventoryExt>();
            var inventoryID = (int?)e.NewValue;
            var apVendorPrice = ASCJCostBuilder.GetAPVendorPrice(this.Base, rowExt?.UsrMarketID, inventoryID, ASCJConstants.TOZ.value, PXTimeZoneInfo.Today);
            var apVendorPriceExt = PXCache<APVendorPrice>.GetExtension<ASCJAPVendorPriceExt>(apVendorPrice);
            e.Cache.SetValueExt<ASCJPOVendorInventoryExt.usrBasisValue>(row, apVendorPriceExt.UsrBasisValue);
        }

        protected virtual void _(Events.RowPersisting<POVendorInventory> e)
        {
            if (e.Row is POVendorInventory row)
            {
                if (e.Row.IsDefault == true)
                {
                    var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(e.Row);
                    if (rowExt.UsrMarketID == null)
                    {
                        e.Cache.RaiseExceptionHandling<ASCJPOVendorInventoryExt.usrMarketID>(row, row.IsDefault, new PXSetPropertyException(ASCJINKitMessages.Error.MarketNotFound, PXErrorLevel.Error));
                        e.Cancel = true;
                        throw new PXException(ASCJINKitMessages.Error.MarketNotFound);
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

            var itemCorySettings = ASCJAllItemCurySettings.Select(e.Row.InventoryID).RowCast<InventoryItemCurySettings>();
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
                        ASCJItemCurySettings.Update(item);
                        flag = true;
                    }
                }
                else if (prefferedVendor != null && prefferedVendor.BaseCuryID == null)
                {
                    item.PreferredVendorID = null;
                    item.PreferredVendorLocationID = null;
                    ASCJItemCurySettings.Update(item);
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

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(e.Row);
            SetBasisValueOnStockComp(rowExt);
        }
        #endregion

        #endregion

        #region ServiceMethods
        protected virtual void SetReadOnlyPOVendorInventoryFields(PXCache cache, POVendorInventory row)
        {
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrContractIncrement>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrContractLossPct>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrContractSurcharge>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrPreciousMetalCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrOtherMaterialsCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrFabricationCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrPackagingCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrLaborCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrPackagingLaborCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrHandlingCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrFreightCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrDutyCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrUnitCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrMatrixStep>(cache, row, true);
        }

        protected virtual void SetVisiblePOVendorInventoryFields(PXCache cache, POVendorInventory row)
        {
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrContractIncrement>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrContractLossPct>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrContractSurcharge>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrPreciousMetalCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrOtherMaterialsCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrFabricationCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrPackagingCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrLaborCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrPackagingLaborCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrHandlingCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrFreightCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrDutyCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrUnitCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrMatrixStep>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrEstLandedCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrCeiling>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrFloor>(cache, null, false);
        }

        protected virtual void SetVisibleItemWeightFields(PXCache cache, INKitSpecHdr row)
        {
            //if (JewelryItemView.Current == null)
            //    JewelryItemView.Current = JewelryItemView.Select()?.TopFirst;

            //var mixedType = ASCJMetalType.GetMixedTypeValue(JewelryItemView.Current?.MetalType);
            //bool isVisibleMixed = mixedType == ASCJConstants.MixedMetalType.Type_MixedDefault || 
            //    JewelryItemView.Current?.MetalType == null;
            //PXUIFieldAttribute.SetVisible<ASCJINKitSpecHdrExt.usrActualGRAMSilverRight>(cache, row, isVisibleMixed);
            //PXUIFieldAttribute.SetVisible<ASCJINKitSpecHdrExt.usrPricingGRAMSilverRight>(cache, row, isVisibleMixed);

            //bool isVisibleGold = mixedType == ASCJConstants.MixedMetalType.Type_MixedGold || 
            //    ASCJMetalType.IsGold(JewelryItemView.Current?.MetalType);
            //PXUIFieldAttribute.SetVisible<ASCJINKitSpecHdrExt.usrActualGRAMGold>(cache, row, isVisibleMixed || isVisibleGold);
            //PXUIFieldAttribute.SetVisible<ASCJINKitSpecHdrExt.usrPricingGRAMGold>(cache, row, isVisibleMixed || isVisibleGold);
            //PXUIFieldAttribute.SetVisible<ASCJINKitSpecHdrExt.usrContractSurcharge>(cache, row, isVisibleMixed || isVisibleGold);

            //bool isVisibleSilver = ASCJMetalType.IsSilver(JewelryItemView.Current?.MetalType);
            //PXUIFieldAttribute.SetVisible<ASCJINKitSpecHdrExt.usrActualGRAMSilver>(cache, row, isVisibleSilver);
            //PXUIFieldAttribute.SetVisible<ASCJINKitSpecHdrExt.usrPricingGRAMSilver>(cache, row, isVisibleSilver);
            //PXUIFieldAttribute.SetVisible<ASCJINKitSpecHdrExt.usrMatrixStep>(cache, row, isVisibleMixed || isVisibleSilver);
        }

        protected virtual void SetVisibleINKitSpecStkDet(PXCache cache, INKitSpecStkDet row)
        {
            //if (JewelryItemView.Current == null)
            //    JewelryItemView.Current = JewelryItemView.Select()?.TopFirst;

            //var mixedType = ASCJMetalType.GetMixedTypeValue(JewelryItemView.Current?.MetalType);
            //bool isVisibleMixed = mixedType == ASCJConstants.MixedMetalType.Type_MixedDefault ||
            //    JewelryItemView.Current?.MetalType == null;

            //bool isVisibleGold = mixedType == ASCJConstants.MixedMetalType.Type_MixedGold ||
            //    ASCJMetalType.IsGold(JewelryItemView.Current?.MetalType);
            //PXUIFieldAttribute.SetVisible<ASCJINKitSpecStkDetExt.usrActualGRAMGold>(cache, null, isVisibleMixed || isVisibleGold);
            //PXUIFieldAttribute.SetVisible<ASCJINKitSpecStkDetExt.usrPricingGRAMGold>(cache, null, isVisibleMixed || isVisibleGold);

            //bool isVisibleSilver = ASCJMetalType.IsSilver(JewelryItemView.Current?.MetalType);
            //PXUIFieldAttribute.SetVisible<ASCJINKitSpecStkDetExt.usrActualGRAMSilver>(cache, null, isVisibleMixed || isVisibleSilver);
            //PXUIFieldAttribute.SetVisible<ASCJINKitSpecStkDetExt.usrPricingGRAMSilver>(cache, null, isVisibleMixed || isVisibleSilver);
            //PXUIFieldAttribute.SetVisible<ASCJINKitSpecStkDetExt.usrMatrixStep>(cache, null, isVisibleMixed || isVisibleSilver);
        }

        protected virtual void CopyJewelryItemFields(INKitSpecHdr kitSpecHdr)
        {
            var jewelItem = SelectFrom<ASCJINJewelryItem>.Where<ASCJINJewelryItem.inventoryID.IsEqual<PX.Data.BQL.P.AsInt>>.View.Select(Base, kitSpecHdr?.KitInventoryID)?.TopFirst;

            if (jewelItem == null) return;

            ASCJINKitSpecJewelryItem jewelryKitItem = new ASCJINKitSpecJewelryItem()
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
            var jewelItem = SelectFrom<ASCJINJewelryItem>.Where<ASCJINJewelryItem.inventoryID.IsEqual<PX.Data.BQL.P.AsInt>>.View.Select(Base, kitSpecHdr?.KitInventoryID)?.TopFirst;

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
            ASCJJewelryItem.Update(jewelItem);
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
                var itemExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(item);
                var kitSpecHdrExt = PXCache<INKitSpecHdr>.GetExtension<ASCJINKitSpecHdrExt>(kitSpecHdr);
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
                ASCJInventoryItem.Update(item);
            }
        }

        protected virtual void CopyFieldsValueToPOVendorInventory(INKitSpecHdr kitSpecHdr)
        {
            var poVendorInventory = VendorItems.Select().RowCast<POVendorInventory>().FirstOrDefault(_ => _.IsDefault == true);
            if (poVendorInventory != null && kitSpecHdr != null)
            {
                var poVendorInventoryExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(poVendorInventory);
                var kitSpecHdrExt = PXCache<INKitSpecHdr>.GetExtension<ASCJINKitSpecHdrExt>(kitSpecHdr);
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

        protected virtual ASCJCostBuilder CreateCostBuilder(ASCJINKitSpecStkDetExt currentRow, INKitSpecStkDet row)
        {
            var defaultVendor = GetItemVendor(row);

            if (defaultVendor != null)
            {
                return new ASCJCostBuilder(Base)
                            .WithInventoryItem(currentRow)
                            .WithPOVendorInventory(defaultVendor)
                            .WithPricingData(PXTimeZoneInfo.Today)
                            .Build();
            }

            throw new PXSetPropertyException(ASCJINKitMessages.Error.NoDefaultVendor);
        }

        protected virtual POVendorInventory GetItemVendor(INKitSpecStkDet row) =>
            SelectFrom<POVendorInventory>.
                    Where<POVendorInventory.inventoryID.
                        IsEqual<@P.AsInt>>.View.Select(Base, row.CompInventoryID).FirstTableItems.FirstOrDefault(x => x.IsDefault == true);



        protected virtual void UpdateVendorPrice(Events.FieldUpdated<INKitSpecStkDet, ASCJINKitSpecStkDetExt.usrCostingType> e,
            INKitSpecStkDet row, ASCJINKitSpecStkDetExt rowExt)
        {
            var inventoryItem = _itemDataProvider.GetInventoryItemByID(row.CompInventoryID);
            var jewelryCostBuilder = CreateCostBuilder(rowExt, row);
            if (jewelryCostBuilder == null) return;

            var result = jewelryCostBuilder.CalculatePreciousMetalCost(e.NewValue?.ToString());
            e.Cache.SetValueExt<ASCJINKitSpecStkDetExt.usrUnitCost>(row, result);

            var salesPrice = rowExt.UsrCostingType == ASCJConstants.CostingType.MarketCost ? jewelryCostBuilder.PreciousMetalMarketCostPerTOZ : jewelryCostBuilder.PreciousMetalContractCostPerTOZ;
            e.Cache.SetValueExt<ASCJINKitSpecStkDetExt.usrSalesPrice>(row, salesPrice);
            e.Cache.SetValueExt<ASCJINKitSpecStkDetExt.usrBasisPrice>(row, jewelryCostBuilder.PreciousMetalContractCostPerTOZ);
        }

        protected virtual decimal? GetUnitCostForCommodityItem(INKitSpecStkDet row)
        {
            var value = 0m;
            var defaultVendor = GetItemVendor(row);
            if (defaultVendor == null) return value;

            var defaultVendorExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(defaultVendor);


            if (JewelryItemView.Current == null)
                JewelryItemView.Current = JewelryItemView.Select();

            if (JewelryItemView.Current == null) return value;

            int? metalInventoryID = null;
            decimal multCoef = 24;

            bool isGold = ASCJMetalType.IsGold(JewelryItemView.Current.MetalType);
            if (isGold)
            {
                metalInventoryID = _itemDataProvider.GetInventoryItemByCD("24K").InventoryID;
                multCoef = ASCJMetalType.GetGoldTypeValue(JewelryItemView.Current.MetalType) / 24;
            }

            bool isSilver = ASCJMetalType.IsSilver(JewelryItemView.Current.MetalType);
            if (isSilver)
            {
                metalInventoryID = _itemDataProvider.GetInventoryItemByCD("SSS").InventoryID;
                multCoef = ASCJMetalType.GetSilverTypeValue(JewelryItemView.Current.MetalType);
            }

            var vendorPrice = ASCJCostBuilder.GetAPVendorPrice(Base, defaultVendorExt.UsrMarketID, metalInventoryID, ASCJConstants.TOZ.value, PXTimeZoneInfo.Now);

            if (vendorPrice == null) return value;

            var rowExt = PXCache<INKitSpecStkDet>.GetExtension<ASCJINKitSpecStkDetExt>(row);
            var vendorPriceExt = PXCache<APVendorPrice>.GetExtension<ASCJAPVendorPriceExt>(vendorPrice);
            if (row.UOM == ASCJConstants.GRAM.value)
            {
                if (isSilver)
                {
                    var jewelryCostBuilder = CreateCostBuilder(rowExt, row);
                    if (jewelryCostBuilder == null) return value;

                    var tempValue = jewelryCostBuilder.CalculatePreciousMetalCost(jewelryCostBuilder.ItemCostSpecification.UsrCostingType);
                    value = (jewelryCostBuilder.PreciousMetalAvrSilverMarketCostPerTOZ ?? 0.0m) / ASCJConstants.TOZ2GRAM_31_10348.value * multCoef;
                    // return value;
                }
                if (isGold)
                {
                    value = (vendorPriceExt?.UsrCommodityPerGram ?? 0m) * multCoef;
                    //return value;
                }
            }
            else if (row.UOM == ASCJConstants.TOZ.value)
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
            return itemClass?.ItemClassCD.NormalizeCD() == ASCJConstants.CommodityClass.value;
        }

        protected virtual void DfltGramsForCommodityItemType(PXCache cache, INKitSpecStkDet row)
        {
            var jewelryItem = GetASCJINJewelryItem(row.CompInventoryID);

            if (!string.IsNullOrEmpty(jewelryItem?.MetalType))
            {

                if (ASCJMetalType.IsGold(jewelryItem?.MetalType))
                {
                    var multFactor = ASCJMetalType.GetGoldTypeValue(jewelryItem?.MetalType);
                    var fineGrams = (One_Gram * multFactor) / 24;
                    cache.SetValueExt<ASCJINKitSpecStkDetExt.usrBaseGoldGrams>(row, One_Gram);
                    cache.SetValueExt<ASCJINKitSpecStkDetExt.usrBaseFineGoldGrams>(row, fineGrams);
                }
                if (ASCJMetalType.IsSilver(jewelryItem?.MetalType))
                {
                    var multFactor = ASCJMetalType.GetSilverTypeValue(jewelryItem?.MetalType);
                    var fineGrams = One_Gram * multFactor;
                    cache.SetValueExt<ASCJINKitSpecStkDetExt.usrBaseSilverGrams>(row, One_Gram);
                    cache.SetValueExt<ASCJINKitSpecStkDetExt.usrBaseFineSilverGrams>(row, fineGrams);
                }
            }
            else
            {
                cache.RaiseExceptionHandling<INKitSpecStkDet.compInventoryID>(row, row.CompInventoryID,
                    new PXSetPropertyException(ASCJINKitMessages.Warning.MissingMetalType, PXErrorLevel.RowWarning));
            }
        }

        protected virtual void UpdateInKitStkComponents(INKitSpecHdr inKitSpecHdr)
        {
            var inKitSpecHdrExt = PXCache<INKitSpecHdr>.GetExtension<ASCJINKitSpecHdrExt>(inKitSpecHdr);

            var stkComponets = this.Base.StockDet.Select()?.FirstTableItems.ToList();

            foreach (var stkComponent in stkComponets)
            {
                if (IsCommodityItem(stkComponent))
                {
                    var stkComponentExt = PXCache<INKitSpecStkDet>.GetExtension<ASCJINKitSpecStkDetExt>(stkComponent);

                    if (stkComponentExt.UsrCostRollupType == ASCJConstants.CostRollupType.PreciousMetal)
                    {
                        stkComponentExt.UsrContractLossPct = inKitSpecHdrExt.UsrContractLossPct;
                        stkComponentExt.UsrContractSurcharge = inKitSpecHdrExt.UsrContractSurcharge;

                        decimal? preciousMetalCost = GetUnitCostForCommodityItem(stkComponent);
                        stkComponentExt.UsrUnitCost = preciousMetalCost;
                        this.Base.StockDet.Cache.SetValueExt<ASCJINKitSpecStkDetExt.usrUnitCost>(stkComponent, stkComponentExt.UsrUnitCost);
                        this.Base.StockDet.Update(stkComponent);
                    }
                }
            }
        }

        private void SetBasisValueOnStockComp(ASCJPOVendorInventoryExt rowExt)
        {
            var stockComponets = this.Base.StockDet.Select()?.FirstTableItems.ToList();
            foreach (var stockComponet in stockComponets)
            {
                var stockComponentExt = PXCache<INKitSpecStkDet>.GetExtension<ASCJINKitSpecStkDetExt>(stockComponet);
                if (stockComponentExt.UsrCostRollupType == ASCJConstants.CostRollupType.PreciousMetal)
                {
                    stockComponentExt.UsrBasisValue = rowExt.UsrBasisValue;
                    this.Base.StockDet.Update(stockComponet);
                }
            }
        }

        private void SetOrUpdatePreciousMetalCost(POVendorInventory row, InventoryItem item, string metalType)
        {
            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(row);
            var marketID = GetVendorMarketID(row, rowExt);

            var vendorPrice = ASCJCostBuilder.GetAPVendorPrice(Base, marketID, item.InventoryID, ASCJConstants.TOZ.value, PXTimeZoneInfo.Today);
            if (vendorPrice != null)
            {
                var result = vendorPrice.SalesPrice * ASCJMetalType.GetMultFactorConvertTOZtoGram(metalType);
                rowExt.UsrPreciousMetalCost = result;
                VendorItems.Update(row);
            }
        }

        private void UpdateTotalSurchargeAndLoss()
        {
            if (this.Base.Hdr.Current == null) return;

            var newTotalLoss = GetFieldTotalPersentage<ASCJINKitSpecStkDetExt.usrContractLossPct>(this.Base.StockDet.Cache);
            var newTotalSurcharge = GetFieldTotalPersentage<ASCJINKitSpecStkDetExt.usrContractSurcharge>(this.Base.StockDet.Cache);
            var newIncrement = GetIncrementTotalValue();

            this.Base.Hdr.SetValueExt<ASCJINKitSpecHdrExt.usrContractLossPct>(this.Base.Hdr.Current, newTotalLoss);
            this.Base.Hdr.SetValueExt<ASCJINKitSpecHdrExt.usrContractSurcharge>(this.Base.Hdr.Current, newTotalSurcharge);
            this.Base.Hdr.SetValueExt<ASCJINKitSpecHdrExt.usrContractIncrement>(this.Base.Hdr.Current, newIncrement);
        }

        private decimal? GetFieldTotalPersentage<TField>(PXCache cache) where TField : IBqlField
        {
            List<INKitSpecStkDet> stkLineList = this.Base.StockDet.Select()?.FirstTableItems?
                .Where(x => x.GetExtension<ASCJINKitSpecStkDetExt>().UsrCostRollupType == ASCJConstants.CostRollupType.PreciousMetal).ToList();

            decimal? totalLossAbsValue = stkLineList.Sum(row =>
            {
                var rowExt = row.GetExtension<ASCJINKitSpecStkDetExt>();
                decimal? lineFieldValue = (decimal?)cache.GetValue<TField>(row);
                return rowExt.UsrExtCost * lineFieldValue;
            });

            decimal? totalExtCost = stkLineList.Sum(line => line.GetExtension<ASCJINKitSpecStkDetExt>().UsrExtCost);

            return totalExtCost == 0.0m || totalExtCost == null ? decimal.Zero : totalLossAbsValue / totalExtCost;
        }

        private decimal? GetIncrementTotalValue()
        {
            List<INKitSpecStkDet> stkLineList = this.Base.StockDet.Select()?.FirstTableItems?.ToList();

            decimal? totalPerMetalType = decimal.Zero;
            decimal? totalPerPreciousMetalType = decimal.Zero;
            foreach (var row in stkLineList)
            {
                var rowExt = row.GetExtension<ASCJINKitSpecStkDetExt>();
                string metalType = GetASCJINJewelryItem(row.CompInventoryID)?.MetalType;
                totalPerMetalType += ASCJMetalType.GetMultFactorConvertTOZtoGram(metalType) * (rowExt.UsrActualGRAMGold + rowExt.UsrActualGRAMSilver);
                totalPerPreciousMetalType += ASCJMetalType.GetMultFactorConvertTOZtoGram(JewelryItemView.Select().TopFirst?.MetalType) * (rowExt.UsrActualGRAMGold + rowExt.UsrActualGRAMSilver); ;
            }


            return totalPerPreciousMetalType == 0.0m || totalPerPreciousMetalType == null
                ? decimal.Zero
                : ASCJMetalType.GetMultFactorConvertTOZtoGram(JewelryItemView.Select().TopFirst?.MetalType) * totalPerMetalType / totalPerPreciousMetalType;
        }

        private int? GetVendorMarketID(POVendorInventory row, ASCJPOVendorInventoryExt rowExt)
        {
            int? marketID = null;
            if (rowExt.UsrMarketID == null)
            {
                var vendor = _vendorDataProvider.GetVendor(row.VendorID);
                if (vendor != null)
                {
                    marketID = PXCache<Vendor>.GetExtension<ASCJVendorExt>(vendor)?.UsrMarketID;
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
            return _itemDataProvider.GetInventoryItemByCD(ASCJConstants.MetalType.Type_24K) != null &&
                   _itemDataProvider.GetInventoryItemByCD(ASCJConstants.MetalType.Type_SSS) != null;
        }

        private ASCJINJewelryItem GetASCJINJewelryItem(int? inventoryID) =>
            SelectFrom<ASCJINJewelryItem>.Where<ASCJINJewelryItem.inventoryID.IsEqual<P.AsInt>>.View.Select(Base, inventoryID)?.TopFirst;

        private POVendorInventory GetDefaultPOVendorInventory() => this.VendorItems.Select()?.FirstTableItems.FirstOrDefault(x => x.IsDefault == true);

        #region Emails Methods
        protected virtual void SendEmailNotification(INKitSpecHdr inKitSpecHdr)
        {

            if (this.VendorItems.Current == null)
                this.VendorItems.Current = this.VendorItems.Select()?.RowCast<POVendorInventory>().FirstOrDefault(x => x.IsDefault == true);
            if (this.VendorItems.Current == null)
                throw new PXException(ASCJINKitMessages.Error.NoDefaultVendor);

            var bAccount = BAccount.PK.Find(Base, this.VendorItems.Current.VendorID);

            var inventoryItem = InventoryItem.PK.Find(this.Base, inKitSpecHdr.KitInventoryID);

            var sender = new NotificationGenerator
            {
                To = GetVendorEmail(bAccount),
                Subject = string.Format(ASCJINKitMessages.EMailSubject, inventoryItem?.InventoryCD),
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

            string returnBodyString = string.Format(ASCJINKitMessages.EMailBody,
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