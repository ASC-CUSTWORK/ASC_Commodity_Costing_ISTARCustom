using ASCISTARCustom.Common.Builder;
using ASCISTARCustom.Common.Descriptor;
using ASCISTARCustom.Common.Helper;
using ASCISTARCustom.Common.Helper.Extensions;
using ASCISTARCustom.Common.Services.DataProvider.Interfaces;
using ASCISTARCustom.Cost.CacheExt;
using ASCISTARCustom.Inventory.CacheExt;
using ASCISTARCustom.Inventory.DAC;
using ASCISTARCustom.Inventory.Descriptor.Constants;
using ASCISTARCustom.PDS.CacheExt;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.PO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static ASCISTARCustom.Common.Descriptor.ASCIStarConstants;

namespace ASCISTARCustom.PDS
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

        //[PXFilterable]
        //public PXSelectJoin<
        //    APVendorPrice,
        //    InnerJoin<POVendorInventory,
        //        On<POVendorInventory.vendorID, Equal<Current<APVendorPrice.vendorID>>,
        //        And<POVendorInventory.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>>,
        //    InnerJoin<InventoryItemCurySettings,
        //        On<InventoryItemCurySettings.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
        //        And<InventoryItemCurySettings.preferredVendorID, Equal<POVendorInventory.vendorID>>>,
        //    InnerJoin<InventoryItem,
        //        On<APVendorPrice.inventoryID, Equal<InventoryItem.inventoryID>>,
        //    InnerJoin<INItemClass,
        //        On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>>>>,
        //    Where<APVendorPrice.vendorID, Equal<InventoryItemCurySettings.preferredVendorID>,
        //        And<INItemClass.itemClassCD, Equal<ASCIStarConstants.CommodityClass>,
        //        And<APVendorPrice.effectiveDate, LessEqual<AccessInfo.businessDate>,
        //        And<APVendorPrice.expirationDate, GreaterEqual<AccessInfo.businessDate>>>>>,
        //    OrderBy<
        //        Desc<APVendorPrice.effectiveDate>>> VendorPriceBasis;

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
                ASCIStarCreateProdItem.SetVisible(!setupExt.UsrIsPDSTenant ?? false);
                ASCIStarCreateProdItem.SetEnabled(!setupExt.UsrIsPDSTenant ?? false);
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
                if (setupExt.UsrIsPDSTenant == false)
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

                    var jewelryItem = GetASCIStarINJewelryItem(currentHdr.KitInventoryID);
                    if (ASCIStarMetalType.IsGold(jewelryItem?.MetalType))
                    {
                        var item = _itemDataProvider.GetInventoryItemByCD(MetalType.Type_24K);
                        SetOrUpdatePreciousMetalCost(row, item, jewelryItem);
                    }
                    else if (ASCIStarMetalType.IsSilver(jewelryItem?.MetalType))
                    {
                        var item = _itemDataProvider.GetInventoryItemByCD(MetalType.Type_SSS);
                        SetOrUpdatePreciousMetalCost(row, item, jewelryItem);
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
                    PXUIFieldAttribute.SetVisible<INKitSpecHdr.revisionID>(Base.Hdr.Cache, Base.Hdr.Current, inSetupExt?.UsrIsPDSTenant == true);
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
                    var jewelryItem = GetASCIStarINJewelryItem(row.KitInventoryID);
                    var metalType = jewelryItem?.MetalType;

                    if (ASCIStarMetalType.IsGold(metalType) || ASCIStarMetalType.IsSilver(metalType))
                    {
                        var baseItemCd = ASCIStarMetalType.IsGold(metalType) ? MetalType.Type_24K : MetalType.Type_SSS;
                        var baseItem = _itemDataProvider.GetInventoryItemByCD(baseItemCd);

                        if (baseItem != null)
                        {
                            var vendorPrice = ASCIStarCostBuilder.GetAPVendorPrice(Base, defaultVendor.VendorID, baseItem.InventoryID, TOZ.value, PXTimeZoneInfo.Now);

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
                var rowExt = PXCache<INKitSpecHdr>.GetExtension<ASCIStarINKitSpecHdrExt>(row);
                if (!IsBaseItemsExists())
                {
                    e.Cache.RaiseExceptionHandling<ASCIStarINKitSpecHdrExt.usrBasisValue>(row, rowExt.UsrBasisValue, new PXSetPropertyException(ASCIStarMessages.Error.BaseItemNotSpecifyed, PXErrorLevel.Warning));
                }
            }
        }
        #endregion

        #region INKitSpecStkDet Events
        protected virtual void _(Events.RowSelecting<INKitSpecStkDet> e)
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
                    e.NewValue = ASCIStarCostingType.MarketCost;
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
                        e.NewValue = ASCIStarCostingType.StandardCost;
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
                var defaultVendor = VendorItems.Select().RowCast<POVendorInventory>().FirstOrDefault(_ => _.IsDefault == true);
                if (defaultVendor != null)
                {
                    var rowExt = PXCache<INKitSpecStkDet>.GetExtension<ASCIStarINKitSpecStkDetExt>(row);
                    if (IsCommodityItem(row))
                    {
                        salesPrice = ASCIStarCostBuilder.GetAPVendorPrice(Base, defaultVendor.VendorID, row.CompInventoryID, TOZ.value, PXTimeZoneInfo.Now)?.SalesPrice;
                    }
                    else
                    {
                        if (rowExt.UsrCostingType == ASCIStarCostingType.MarketCost)
                        {
                            salesPrice = CreateCostBuilder(row).PreciousMetalMarketCostPerTOZ;
                        }
                        else if (rowExt.UsrCostingType == ASCIStarCostingType.ContractCost)
                        {
                            salesPrice = CreateCostBuilder(row).PreciousMetalContractCostPerTOZ;
                        }
                    }
                }
                e.NewValue = salesPrice;
            }
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
            if (e.Row is INKitSpecStkDet row)
            {
                if (Hdr.Current?.KitInventoryID == (int)e.NewValue)
                {
                    var invItem = _itemDataProvider.GetInventoryItemByID(Hdr.Current?.KitInventoryID);
                    e.Cancel = true;
                    throw new PXSetPropertyException(ASCIStarMessages.Error.CannotCreateItself, invItem.InventoryCD, invItem.InventoryCD);
                }
            }
        }
        protected virtual void _(Events.FieldUpdated<INKitSpecStkDet, INKitSpecStkDet.compInventoryID> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                e.Cache.RaiseFieldDefaulting<ASCIStarINKitSpecStkDetExt.usrCostingType>(row, out object _costType);
                e.Cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrCostingType>(row, _costType);

                e.Cache.RaiseFieldDefaulting<ASCIStarINKitSpecStkDetExt.usrUnitCost>(row, out object _costUnit);
                e.Cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrUnitCost>(row, _costUnit);

                e.Cache.RaiseFieldDefaulting<ASCIStarINKitSpecStkDetExt.usrSalesPrice>(row, out object _salesPrice);
                e.Cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrSalesPrice>(row, _salesPrice);

                if (IsCommodityItem(row))
                {
                    DfltGramsForCommodityItemType(e.Cache, row);
                }
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
                    if (rowExt.UsrCostingType == ASCIStarCostingType.StandardCost)
                    {
                        var result = INItemCost.PK.Find(Base, row.CompInventoryID, Base.Accessinfo.BaseCuryID);
                        e.Cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrUnitCost>(row, result?.AvgCost ?? 0m);
                    }
                    else if (rowExt.UsrCostingType == ASCIStarCostingType.MarketCost || rowExt.UsrCostingType == ASCIStarCostingType.ContractCost)
                    {
                        var jewelryCostBuilder = CreateCostBuilder(row);
                        //var result = CalculateUnitCost(jewelryCostBuilder.CalculatePreciousMetalCost(), row.CompInventoryID);
                        var result = jewelryCostBuilder.CalculatePreciousMetalCost(e.NewValue?.ToString());
                        e.Cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrUnitCost>(row, result);

                        UpdateVendorPrice(e, row, rowExt, jewelryCostBuilder);
                    }
                }
            }
        }
        protected virtual void _(Events.FieldUpdated<INKitSpecStkDet, ASCIStarINKitSpecHdrExt.usrExtCost> e)
        {
            var row = e.Row;
            if (row == null || this.Base.Hdr.Current == null) return;

            var newTotalLoss = GetFieldTotalPersentage<ASCIStarINKitSpecStkDetExt.usrContractLossPct>(e.Cache);
            var newTotalSurcharge = GetFieldTotalPersentage<ASCIStarINKitSpecStkDetExt.usrContractSurcharge>(e.Cache);
            var newIncrement = GetIncrementTotalValue();

            this.Base.Hdr.SetValueExt<ASCIStarINKitSpecHdrExt.usrContractLossPct>(this.Base.Hdr.Current, newTotalLoss);
            this.Base.Hdr.SetValueExt<ASCIStarINKitSpecHdrExt.usrContractSurcharge>(this.Base.Hdr.Current, newTotalSurcharge);
            this.Base.Hdr.SetValueExt<ASCIStarINKitSpecHdrExt.usrContractIncrement>(this.Base.Hdr.Current, newIncrement);
        }

        protected virtual void _(Events.RowPersisting<INKitSpecStkDet> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                var rowExt = PXCache<INKitSpecStkDet>.GetExtension<ASCIStarINKitSpecStkDetExt>(row);
                if (rowExt.UsrCostRollupType == null)
                {
                    e.Cache.RaiseExceptionHandling<ASCIStarINKitSpecStkDetExt.usrCostRollupType>(row, rowExt.UsrCostRollupType,
                        new PXSetPropertyException(ASCIStarMessages.Error.CostRollupTypeNotSet, PXErrorLevel.Error));
                    e.Cancel = true;
                    throw new PXException(ASCIStarMessages.Error.CostRollupTypeNotSet);
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
                        new PXSetPropertyException(ASCIStarMessages.Error.CostRollupTypeNotSet, PXErrorLevel.Error));
                    e.Cancel = true;
                    throw new PXException(ASCIStarMessages.Error.CostRollupTypeNotSet);
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

            var inventoryCD = ASCIStarMetalType.GetBoolableMetalType(this.JewelryItemView.Current?.MetalType) == true ? "24K" : "SSS";
            var inventoryID = SelectFrom<InventoryItem>.Where<InventoryItem.inventoryCD.IsEqual<P.AsString>>.View.Select(Base, inventoryCD)?.TopFirst.InventoryID;

            var apVendorPrice = ASCIStarCostBuilder.GetAPVendorPrice(this.Base, row.VendorID, inventoryID, TOZ.value, PXTimeZoneInfo.Today);

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

        protected virtual void _(Events.RowPersisting<POVendorInventory> e)
        {
            if (e.Row is POVendorInventory row)
            {
                if (e.Row.IsDefault == true)
                {
                    var rowExt = PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(e.Row);
                    if (rowExt.UsrMarketID == null)
                    {
                        e.Cache.RaiseExceptionHandling<ASCIStarPOVendorInventoryExt.usrMarketID>(row, row.IsDefault, new PXSetPropertyException(ASCIStarMessages.Error.MarketNotFound, PXErrorLevel.Error));
                        e.Cancel = true;
                        throw new PXException(ASCIStarMessages.Error.MarketNotFound);
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

            GetCurySettings(e.Row.InventoryID);
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

        protected virtual ASCIStarCostBuilder CreateCostBuilder(INKitSpecStkDet currentRow)
        {
            var defaultVendor = VendorItems.Select().RowCast<POVendorInventory>().FirstOrDefault(_ => _.IsDefault == true);
            if (defaultVendor != null)
            {
                return new ASCIStarCostBuilder(Base)
                            .WithInventoryItem(PXCache<INKitSpecStkDet>.GetExtension<ASCIStarINKitSpecStkDetExt>(currentRow))
                            .WithPOVendorInventory(defaultVendor)
                            .WithPricingData(PXTimeZoneInfo.Today)
                            .Build();
            }

            throw new PXSetPropertyException(ASCIStarMessages.Error.NoDefaultVendor);
        }

        public static void UpdateVendorPrice(Events.FieldUpdated<INKitSpecStkDet, ASCIStarINKitSpecStkDetExt.usrCostingType> e, INKitSpecStkDet row, ASCIStarINKitSpecStkDetExt rowExt, ASCIStarCostBuilder jewelryCostBuilder)
        {
            var salesPrice = rowExt.UsrCostingType == ASCIStarCostingType.MarketCost ? jewelryCostBuilder.PreciousMetalMarketCostPerTOZ : jewelryCostBuilder.PreciousMetalContractCostPerTOZ;
            e.Cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrSalesPrice>(row, salesPrice);
            e.Cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrBasisPrice>(row, jewelryCostBuilder.PreciousMetalContractCostPerTOZ);
        }

        protected virtual decimal GetUnitCostForCommodityItem(INKitSpecStkDet row)
        {
            var value = 0m;
            var defaultVendor = VendorItems.Select().RowCast<POVendorInventory>().FirstOrDefault(_ => _.IsDefault == true);
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

            var vendorPrice = ASCIStarCostBuilder.GetAPVendorPrice(Base, defaultVendorExt.UsrMarketID, metalInventoryID, TOZ.value, PXTimeZoneInfo.Now);

            if (vendorPrice == null) return value;


            var vendorPriceExt = PXCache<APVendorPrice>.GetExtension<ASCIStarAPVendorPriceExt>(vendorPrice);
            if (row.UOM == GRAM.value)
            {
                if (isSilver)
                {
                    var jewelryCostBuilder = CreateCostBuilder(row);
                    if (jewelryCostBuilder == null) return value;

                    var tempValue = jewelryCostBuilder.CalculatePreciousMetalCost(jewelryCostBuilder.ItemCostSpecification.UsrCostingType);
                    value = (jewelryCostBuilder.PreciousMetalAvrSilverMarketCostPerTOZ ?? 0.0m) / TOZ2GRAM_31_10348.value * multCoef;
                    return value;
                }
                if (isGold)
                {
                    value = (vendorPriceExt?.UsrCommodityPerGram ?? 0m) * multCoef;
                    return value;
                }
            }
            else if (row.UOM == TOZ.value)
            {
                return vendorPrice?.SalesPrice ?? 0m;
            }

            return value;
        }

        protected virtual bool IsCommodityItem(INKitSpecStkDet row)
        {
            var inventoryItem = _itemDataProvider.GetInventoryItemByID(row.CompInventoryID);
            var itemClass = _itemDataProvider.GetItemClassByID(inventoryItem?.ItemClassID);
            return itemClass?.ItemClassCD.NormalizeCD() == CommodityClass.value;
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
                else if (ASCIStarMetalType.IsSilver(jewelryItem?.MetalType))
                {
                    var multFactor = ASCIStarMetalType.GetSilverTypeValue(jewelryItem?.MetalType);
                    var fineGrams = One_Gram * multFactor;
                    cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrBaseSilverGrams>(row, One_Gram);
                    cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrBaseFineSilverGrams>(row, fineGrams);
                }
            }
            else
            {
                cache.RaiseExceptionHandling<INKitSpecStkDet.compInventoryID>(row, row.CompInventoryID, new PXSetPropertyException(ASCIStarMessages.Error.MissingMetalType, PXErrorLevel.RowWarning));
            }
        }

        private void SetOrUpdatePreciousMetalCost(POVendorInventory row, InventoryItem item, ASCIStarINJewelryItem jewelryItem)
        {
            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(row);
            var marketID = GetVendorMarketID(row, rowExt);

            var vendorPrice = ASCIStarCostBuilder.GetAPVendorPrice(Base, marketID, item.InventoryID, TOZ.value, PXTimeZoneInfo.Today);
            if (vendorPrice != null)
            {
                var result = vendorPrice.SalesPrice * ASCIStarMetalType.GetMultFactorConvertTOZtoGram(jewelryItem.MetalType);
                rowExt.UsrPreciousMetalCost = result;
                VendorItems.Update(row);
            }
        }

        private decimal? GetFieldTotalPersentage<TField>(PXCache cache) where TField : IBqlField
        {
            List<INKitSpecStkDet> stkLineList = this.Base.StockDet.Select()?.FirstTableItems?.ToList();

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

        protected virtual InventoryItemCurySettings GetCurySettings(int? inventoryID, string curyID = null)
        {
            if (curyID == null)
            {
                curyID = Base.Accessinfo.BaseCuryID;
            }

            return ASCIStarItemCurySettings.SelectSingle(inventoryID, curyID) ?? ASCIStarItemCurySettings.Insert(new InventoryItemCurySettings
            {
                InventoryID = inventoryID,
                CuryID = curyID
            });
        }

        protected virtual void SetVisibleItemWeightFields(PXCache cache, INKitSpecHdr row)
        {
            var jewelryItem = GetASCIStarINJewelryItem(row.KitInventoryID);

            bool isVisibleGold = ASCIStarMetalType.IsGold(jewelryItem?.MetalType);
            PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecHdrExt.usrActualGRAMGold>(cache, row, isVisibleGold);
            PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecHdrExt.usrPricingGRAMGold>(cache, row, isVisibleGold);

            bool isVisibleSilver = ASCIStarMetalType.IsSilver(jewelryItem?.MetalType);
            PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecHdrExt.usrActualGRAMSilver>(cache, row, isVisibleSilver);
            PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecHdrExt.usrPricingGRAMSilver>(cache, row, isVisibleSilver);
            PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecHdrExt.usrMatrixStep>(cache, row, isVisibleSilver);
        }

        protected virtual void SetVisibleINKitSpecStkDet(PXCache cache, INKitSpecStkDet inKitSpecStkDet)
        {
            var jewelryItem = GetASCIStarINJewelryItem(inKitSpecStkDet.KitInventoryID);
            bool isVisibleGold = ASCIStarMetalType.IsGold(jewelryItem?.MetalType);
            PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecStkDetExt.usrActualGRAMGold>(cache, null, isVisibleGold);
            PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecStkDetExt.usrPricingGRAMGold>(cache, null, isVisibleGold);
            PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecStkDetExt.usrContractIncrement>(cache, null, isVisibleGold);

            bool isVisibleSilver = ASCIStarMetalType.IsSilver(jewelryItem?.MetalType);
            PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecStkDetExt.usrActualGRAMSilver>(cache, null, isVisibleSilver);
            PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecStkDetExt.usrPricingGRAMSilver>(cache, null, isVisibleSilver);
            PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecStkDetExt.usrMatrixStep>(cache, null, isVisibleSilver);
        }

        private bool IsBaseItemsExists()
        {
            return _itemDataProvider.GetInventoryItemByCD(MetalType.Type_24K) != null &&
                   _itemDataProvider.GetInventoryItemByCD(MetalType.Type_SSS) != null;
        }

        private ASCIStarINJewelryItem GetASCIStarINJewelryItem(int? inventoryID) =>
            SelectFrom<ASCIStarINJewelryItem>.Where<ASCIStarINJewelryItem.inventoryID.IsEqual<ASCIStarINJewelryItem.inventoryID>>.View.Select(Base, inventoryID)?.TopFirst;

        #endregion
    }
}