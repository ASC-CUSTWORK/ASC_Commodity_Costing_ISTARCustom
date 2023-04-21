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
using System.Linq;
using static ASCISTARCustom.Common.Descriptor.ASCIStarConstants;

namespace ASCISTARCustom.PDS
{
    public class ASCIStarINKitSpecMaintExt : PXGraphExtension<INKitSpecMaint>
    {
        #region Constants
        private const decimal One_Gram = 1m;
        private const decimal One_Ounce = 31.1034768m;
        #endregion

        #region Static Functions
        public static bool IsActive() => true;
        #endregion

        #region DataView
        [PXCopyPasteHiddenView]
        public PXSelect<INKitSpecHdr, Where<INKitSpecHdr.kitInventoryID, Equal<Optional<INKitSpecHdr.kitInventoryID>>>> Hdr;

        [PXCopyPasteHiddenView]
        public PXSelect<POVendorInventory, Where<POVendorInventory.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>> VendorItems;

        public SelectFrom<ASCIStarINKitSpecJewelryItem>
                 .Where<ASCIStarINKitSpecJewelryItem.kitInventoryID.IsEqual<INKitSpecHdr.kitInventoryID.FromCurrent>
                    .And<ASCIStarINKitSpecJewelryItem.revisionID.IsEqual<INKitSpecHdr.revisionID.FromCurrent>>>
                        .View JewelryItemView;

        [PXFilterable]
        public PXSelectJoin<
            APVendorPrice,
            InnerJoin<POVendorInventory,
                On<POVendorInventory.vendorID, Equal<Current<APVendorPrice.vendorID>>,
                And<POVendorInventory.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>>,
            InnerJoin<InventoryItemCurySettings,
                On<InventoryItemCurySettings.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
                And<InventoryItemCurySettings.preferredVendorID, Equal<POVendorInventory.vendorID>>>,
            InnerJoin<InventoryItem,
                On<APVendorPrice.inventoryID, Equal<InventoryItem.inventoryID>>,
            InnerJoin<INItemClass,
                On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>>>>,
            Where<APVendorPrice.vendorID, Equal<InventoryItemCurySettings.preferredVendorID>,
                And<INItemClass.itemClassCD, Equal<ASCIStarConstants.CommodityClass>,
                And<APVendorPrice.effectiveDate, LessEqual<AccessInfo.businessDate>,
                And<APVendorPrice.expirationDate, GreaterEqual<AccessInfo.businessDate>>>>>,
            OrderBy<
                Desc<APVendorPrice.effectiveDate>>> VendorPriceBasis;

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
            CopyJewelryItemFieldsToStockItem(Base.Hdr.Current);
            
            baseMethod();

            //{
            //    using (var transactionScope = new PXTransactionScope())
            //    {
            //        var inventoryItemMaint = PXGraph.CreateInstance<InventoryItemMaint>();
            //        inventoryItemMaint.Item.Current = inventoryItemMaint.Item.Search<InventoryItem.inventoryID>(Hdr.Current.KitInventoryID);
            //        inventoryItemMaint.Item.UpdateCurrent();

            //        var kitVendorItem = VendorItems.Select().RowCast<POVendorInventory>().FirstOrDefault(_ => _.IsDefault == true);
            //        if (kitVendorItem != null)
            //        {
            //            var invVendorItem = inventoryItemMaint.VendorItems.Select().RowCast<POVendorInventory>().FirstOrDefault(_ => _.RecordID == kitVendorItem.RecordID);
            //            if (invVendorItem != null)
            //            {
            //                inventoryItemMaint.VendorItems.Current = invVendorItem;
            //                inventoryItemMaint.VendorItems.Current.IsDefault = true;
            //                inventoryItemMaint.VendorItems.Update(inventoryItemMaint.VendorItems.Current);
            //                inventoryItemMaint.Save.PressButton();
            //                transactionScope.Complete();
            //            }
            //        }
            //    }
            //}
        }
        #endregion

        #region CacheAttached

        #region INKitSpecHdrCacheAttaches
        [PXRemoveBaseAttribute(typeof(PXDBStringAttribute))]
        [PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">##")]
        [PXDefault("01")]
        [PXUIField(DisplayName = "Variant")]
        protected void _(Events.CacheAttached<INKitSpecHdr.revisionID> cacheAttached) { }
        #endregion

        #region POVendorInventoryCacheAttaches
        [PXRemoveBaseAttribute(typeof(PXUIFieldAttribute))]
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXUIField(DisplayName = "Default", Enabled = true)]
        protected virtual void _(Events.CacheAttached<POVendorInventory.isDefault> cacheAttached) { }

        [PXRemoveBaseAttribute(typeof(PXDBDefaultAttribute))]
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDBDefault(typeof(INKitSpecHdr.kitInventoryID))]
        protected virtual void _(Events.CacheAttached<POVendorInventory.inventoryID> cacheAttached) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXFormula(typeof(Add<Add<Add<
            ASCIStarPOVendorInventoryExt.usrCommodityCost,
            ASCIStarPOVendorInventoryExt.usrOtherMaterialCost>, 
            ASCIStarPOVendorInventoryExt.usrFabricationCost>, 
            ASCIStarPOVendorInventoryExt.usrPackagingCost>))]
        protected virtual void _(Events.CacheAttached<ASCIStarPOVendorInventoryExt.usrUnitCost> cacheAttached) { }
        #endregion

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
            //CopyFieldsValueFromStockItem(this.Base.Hdr.Current);
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
        protected virtual void _(Events.FieldSelecting<INKitSpecHdr, ASCIStarINKitSpecHdrExt.usrUnitCost> e)
        {
            if (e.Row is INKitSpecHdr row)
            {
                var result = ASCIStarCostBuilder.CalculateUnitCost(row);
                e.ReturnValue = result;
            }
        }
        protected virtual void _(Events.FieldSelecting<INKitSpecHdr, ASCIStarINKitSpecHdrExt.usrLandedCost> e)
        {
            if (e.Row is INKitSpecHdr row)
            {
                var result = ASCIStarCostBuilder.CalculateUnitCost(row) + ASCIStarCostBuilder.CalculateLandedCost(row);
                e.ReturnValue = result;
            }
        }
        protected virtual void _(Events.FieldSelecting<INKitSpecHdr, ASCIStarINKitSpecHdrExt.usrSalesPrice> e)
        {
            if (e.Row is INKitSpecHdr row)
            {
                var defaultVendor = VendorItems.Select().RowCast<POVendorInventory>().FirstOrDefault(_ => _.IsDefault == true);
                if (defaultVendor != null)
                {
                    decimal? value = 0m;
                    var jewelryItem = GetASCIStarINJewelryItem(row.KitInventoryID);
                    if (ASCIStarMetalType.IsGold(jewelryItem?.MetalType))
                    {
                        var baseItem = _itemDataProvider.GetInventoryItemByCD(MetalType.Type_24K);
                        if (baseItem != null)
                        {
                            value = ASCIStarCostBuilder.GetAPVendorPrice(Base, defaultVendor.GetExtension<ASCIStarPOVendorInventoryExt>().UsrMarketID, baseItem.InventoryID, TOZ.value, PXTimeZoneInfo.Now)?.SalesPrice;
                        }
                    }
                    else if (ASCIStarMetalType.IsSilver(jewelryItem?.MetalType))
                    {
                        var baseItem = _itemDataProvider.GetInventoryItemByCD(MetalType.Type_SSS);
                        if (baseItem != null)
                        {
                            value = ASCIStarCostBuilder.GetAPVendorPrice(Base, defaultVendor.GetExtension<ASCIStarPOVendorInventoryExt>().UsrMarketID, baseItem.InventoryID, TOZ.value, PXTimeZoneInfo.Now)?.SalesPrice;
                        }
                    }
                    e.ReturnValue = value;
                }
            }
        }
        protected virtual void _(Events.FieldVerifying<INKitSpecHdr, ASCIStarINKitSpecHdrExt.usrSalesPrice> e)
        {
            if (e.Row is INKitSpecHdr row)
            {
                var rowExt = PXCache<INKitSpecHdr>.GetExtension<ASCIStarINKitSpecHdrExt>(row);
                if (!IsBaseItemsExists())
                {
                    e.Cache.RaiseExceptionHandling<ASCIStarINKitSpecHdrExt.usrSalesPrice>(row, rowExt.UsrSalesPrice, new PXSetPropertyException(ASCIStarMessages.Error.BaseItemNotSpecifyed, PXErrorLevel.Warning));
                }
            }
        }
        #endregion

        #region INKitSpecStkDet Events
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
                    if (jewelryItem != null && (ASCIStarMetalType.IsGold(jewelryItem.MetalType) || ASCIStarMetalType.IsSilver(jewelryItem.MetalType)))
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
                    e.NewValue = SetCostForCommodityClassItem(row);
                }
                else
                {
                    var inventoryItem = _itemDataProvider.GetInventoryItemByID(row.CompInventoryID);
                    if (jewelryItem != null && (ASCIStarMetalType.IsGold(jewelryItem?.MetalType) || ASCIStarMetalType.IsSilver(jewelryItem?.MetalType)))
                    {
                        e.NewValue = ASCIStarCostBuilder.CalculateUnitCost(inventoryItem);
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
                    var value = SetCostForCommodityClassItem(row);
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
                        var result = CalculateUnitCost(jewelryCostBuilder.CalculatePreciousMetalCost(), row.CompInventoryID);
                        e.Cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrUnitCost>(row, result);

                        UpdateVendorPrice(e, row, rowExt, jewelryCostBuilder);
                    }
                }
            }
        }
        protected virtual void _(Events.RowPersisting<INKitSpecStkDet> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                var rowExt = PXCache<INKitSpecStkDet>.GetExtension<ASCIStarINKitSpecStkDetExt>(row);
                if (rowExt.UsrCostRollupType == null)
                {
                    e.Cache.RaiseExceptionHandling<ASCIStarINKitSpecStkDetExt.usrCostRollupType>(row, rowExt.UsrCostRollupType, new PXSetPropertyException(ASCIStarMessages.Error.CostRollupTypeNotSet, PXErrorLevel.Error));
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
                    e.Cache.RaiseExceptionHandling<ASCIStarINKitSpecNonStkDetExt.usrCostRollupType>(row, rowExt.UsrCostRollupType, new PXSetPropertyException(ASCIStarMessages.Error.CostRollupTypeNotSet, PXErrorLevel.Error));
                    e.Cancel = true;
                    throw new PXException(ASCIStarMessages.Error.CostRollupTypeNotSet);
                }
            }
        }
        #endregion

        #region POVendorInventory Events
        protected virtual void _(Events.RowPersisting<POVendorInventory> e)
        {
            if (e.Row is POVendorInventory row)
            {
                var result = VendorItems.Select().RowCast<POVendorInventory>();
                if (result.Any(_ => _.GetExtension<ASCIStarPOVendorInventoryExt>().UsrMarketID == null))
                {
                    e.Cache.RaiseExceptionHandling<ASCIStarPOVendorInventoryExt.usrMarketID>(row, row.IsDefault, new PXSetPropertyException(ASCIStarMessages.Error.MarketNotFound, PXErrorLevel.Error));
                    e.Cancel = true;
                    throw new PXException(ASCIStarMessages.Error.MarketNotFound);
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
        //protected virtual void CopyFieldsValueFromStockItem(INKitSpecHdr kitSpecHdr)
        //{
        //    var item = _itemDataProvider.GetInventoryItemByID(kitSpecHdr?.KitInventoryID);
        //    if (item != null && kitSpecHdr != null)
        //    {
        //        var itemExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(item);
        //        var kitSpecHdrExt = PXCache<INKitSpecHdr>.GetExtension<ASCIStarINKitSpecHdrExt>(kitSpecHdr);
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
        //        Base.Hdr.Update(kitSpecHdr);
        //    }
        //}
        protected virtual void CopyFieldsValueToStockItem(INKitSpecHdr kitSpecHdr)
        {
            var item = _itemDataProvider.GetInventoryItemByID(kitSpecHdr?.KitInventoryID);
            if (item != null && kitSpecHdr != null)
            {
                var itemExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(item);
                var kitSpecHdrExt = PXCache<INKitSpecHdr>.GetExtension<ASCIStarINKitSpecHdrExt>(kitSpecHdr);
                itemExt.UsrActualGRAMGold = kitSpecHdrExt.UsrTotalGoldGrams;
                itemExt.UsrPricingGRAMGold = kitSpecHdrExt.UsrTotalFineGoldGrams;
                itemExt.UsrActualGRAMSilver = kitSpecHdrExt.UsrTotalSilverGrams;
                itemExt.UsrPricingGRAMSilver = kitSpecHdrExt.UsrTotalFineSilverGrams;
                itemExt.UsrCommodityCost = kitSpecHdrExt.UsrPreciousMetalCost;
                itemExt.UsrFabricationCost = kitSpecHdrExt.UsrFabricationCost;
                itemExt.UsrOtherCost = kitSpecHdrExt.UsrOtherCost;
                itemExt.UsrPackagingCost = kitSpecHdrExt.UsrPackagingCost;
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
        protected virtual ASCIStarCostBuilder CreateCostBuilder(INKitSpecStkDet currentRow)
        {
            var defaultVendor = VendorItems.Select().RowCast<POVendorInventory>().FirstOrDefault(_ => _.IsDefault == true);
            if (defaultVendor != null)
            {
                return new ASCIStarCostBuilder(Base)
                            .WithInventoryItem(currentRow)
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
        }
        protected virtual decimal SetCostForCommodityClassItem(INKitSpecStkDet row)
        {
            var value = 0m;
            var defaultVendor = VendorItems.Select().RowCast<POVendorInventory>().FirstOrDefault(_ => _.IsDefault == true);
            if (defaultVendor != null)
            {
                var vendorPrice = ASCIStarCostBuilder.GetAPVendorPrice(Base, defaultVendor.VendorID, row.CompInventoryID, TOZ.value, PXTimeZoneInfo.Now);
                if (vendorPrice != null)
                {
                    var vendorPriceExt = PXCache<APVendorPrice>.GetExtension<ASCIStarAPVendorPriceExt>(vendorPrice);
                    if (row.UOM == GRAM.value)
                    {
                        value = vendorPriceExt?.UsrCommodityPerGram ?? 0m;
                    }
                    else if (row.UOM == TOZ.value)
                    {
                        value = vendorPrice?.SalesPrice ?? 0m;
                    }
                }
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
                    var multFactor = ASCIStarMetalType.GetSilverTypeValue(jewelryItem?.MetalType);
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
        private decimal CalculateUnitCost(decimal? preciousMetalCost, int? inventoryID) //<-- this is a alternate way to calculate unit cost
        {
            decimal? value = 0m;
            var inventoryItem = _itemDataProvider.GetInventoryItemByID(inventoryID);
            if (inventoryItem != null)
            {
                var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(inventoryItem);
                value = preciousMetalCost 
                        + inventoryItemExt.UsrMaterialsCost 
                        + inventoryItemExt.UsrFabricationCost 
                        + inventoryItemExt.UsrPackagingCost 
                        + inventoryItemExt.UsrPackagingLaborCost;
            }
            return value ?? 0m;
        }
        private void SetOrUpdatePreciousMetalCost(POVendorInventory row, InventoryItem item, ASCIStarINJewelryItem jewelryItem)
        {
            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(row);
            var marketID = GetVendorMarketID(row, rowExt);

            var vendorPrice = ASCIStarCostBuilder.GetAPVendorPrice(Base, marketID, item.InventoryID, TOZ.value, PXTimeZoneInfo.Today);
            if (vendorPrice != null)
            {
                var result = vendorPrice.SalesPrice * ASCIStarMetalType.GetMultFactorConvertTOZtoGram(jewelryItem.MetalType);
                rowExt.UsrCommodityCost = result;
                VendorItems.Update(row);
            }
        }
        private int? GetVendorMarketID(POVendorInventory row,  ASCIStarPOVendorInventoryExt rowExt)
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
        public virtual InventoryItemCurySettings GetCurySettings(int? inventoryID, string curyID = null)
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
            PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecHdrExt.usrTotalGoldGrams>(cache, row, isVisibleGold);
            PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecHdrExt.usrTotalFineGoldGrams>(cache, row, isVisibleGold);

            bool isVisibleSilver = ASCIStarMetalType.IsSilver(jewelryItem?.MetalType);
            PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecHdrExt.usrTotalSilverGrams>(cache, row, isVisibleSilver);
            PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecHdrExt.usrTotalFineSilverGrams>(cache, row, isVisibleSilver);
            PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecHdrExt.usrMatrixStep>(cache, row, isVisibleSilver);
        }
        private bool IsBaseItemsExists()
        {
            return _itemDataProvider.GetInventoryItemByCD(MetalType.Type_24K) != null &&
                   _itemDataProvider.GetInventoryItemByCD(MetalType.Type_SSS) != null;
        }
        #endregion

        #region ServiceQueries
        private ASCIStarINJewelryItem GetASCIStarINJewelryItem(int? inventoryID)
        {
            return PXSelect<
                ASCIStarINJewelryItem,
                Where<ASCIStarINJewelryItem.inventoryID, Equal<Required<ASCIStarINJewelryItem.inventoryID>>>>
                .Select(Base, inventoryID);
        }
        #endregion
    }
}