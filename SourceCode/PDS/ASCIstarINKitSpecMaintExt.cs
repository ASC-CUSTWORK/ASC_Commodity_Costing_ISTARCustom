using ASCISTARCustom.Common.Builder;
using ASCISTARCustom.Common.Descriptor;
using ASCISTARCustom.Common.Helper;
using ASCISTARCustom.Common.Services.DataProvider.Interfaces;
using ASCISTARCustom.Inventory.CacheExt;
using ASCISTARCustom.Inventory.DAC;
using ASCISTARCustom.Inventory.Descriptor.Constants;
using ASCISTARCustom.PDS.CacheExt;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.PO;
using System.Collections;
using System.Linq;

namespace ASCISTARCustom.PDS
{
    public class ASCIStarINKitSpecMaintExt : PXGraphExtension<INKitSpecMaint>
    {
        #region Static Functions
        public static bool IsActive() => true;
        #endregion

        #region DataView
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

        public PXSetup<INSetup> ASCIStarINSetup;
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
            ASCIStarUpdateMetalCost.SetEnabled(false);
            ASCIStarUpdateMetalCost.SetVisible(false);

            var setup = ASCIStarINSetup.Current;
            if (setup != null)
            {
                var setupExt = PXCache<INSetup>.GetExtension<ASCIStarINSetupExt>(setup);
                ASCIStarCreateProdItem.SetVisible(!setupExt.UsrIsPDSTenant ?? false);
                ASCIStarCreateProdItem.SetEnabled(!setupExt.UsrIsPDSTenant ?? false);
            }
        }
        #endregion

        #region CacheAttached
        [PXRemoveBaseAttribute(typeof(PXDBStringAttribute))]
        [PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">##")]
        [PXDefault("01")]
        protected void _(Events.CacheAttached<INKitSpecHdr.revisionID> cacheAttached) { }
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
            Base.StockDet.Select().RowCast<INKitSpecStkDet>().ForEach(currentLine =>
            {
                //TODO create logic for updateing metal costs
            });

            Base.NonStockDet.Select().RowCast<INKitSpecNonStkDet>().ForEach(currentLine =>
            {
                //TODO create logic for updateing metal costs based on non stock component.
                //NOTE! this loop might not be needed
            });

            return adapter.Get();
        }
        #endregion

        #region Event Handlers

        #region INKitSpecHdr Events
        protected virtual void _(Events.RowInserted<INKitSpecHdr> e)
        {
            var row = e.Row;
            if (row == null || this.Base.Hdr.Current == null) return;

            CopyJewelryItemFields(this.Base.Hdr.Current);
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
        #endregion

        #region INKitSpecStkDet Events
        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCIStarINKitSpecStkDetExt.usrCostingType> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                var jewelryItem = GetASCIStarINJewelryItem(row.CompInventoryID);
                if (jewelryItem != null && (ASCIStarMetalType.IsGold(jewelryItem.MetalType) || ASCIStarMetalType.IsSilver(jewelryItem.MetalType)))
                {
                    var inventoryItem = _itemDataProvider.GetInventoryItemByID(row.CompInventoryID);
                    var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(inventoryItem);
                    e.NewValue = inventoryItemExt.UsrCostingType;
                }
                else
                {
                    e.NewValue = ASCIStarCostingType.StandardCost;
                }
            }
        }
        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCIStarINKitSpecStkDetExt.usrUnitCost> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                var jewelryItem = GetASCIStarINJewelryItem(row.CompInventoryID);
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
        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCIStarINKitSpecStkDetExt.usrSalesPrice> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                decimal? salesPrice = 0m;
                var defaultVendor = VendorItems.Select().RowCast<POVendorInventory>().FirstOrDefault(_ => _.IsDefault == true);
                if (defaultVendor != null)
                {
                    var rowExt = PXCache<INKitSpecStkDet>.GetExtension<ASCIStarINKitSpecStkDetExt>(row);
                    if (rowExt.UsrCostingType == ASCIStarCostingType.MarketCost)
                    {
                        var inventoryItem = _itemDataProvider.GetInventoryItemByID(row.CompInventoryID);
                        salesPrice = CreateCostBuilder(row).PreciousMetalMarketCostPerTOZ;
                    }
                    else if (rowExt.UsrCostingType == ASCIStarCostingType.ContractCost)
                    {
                        var inventoryItem = _itemDataProvider.GetInventoryItemByID(row.CompInventoryID);
                        salesPrice = CreateCostBuilder(row).PreciousMetalContractCostPerTOZ;
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
        protected virtual void _(Events.FieldUpdated<INKitSpecStkDet, ASCIStarINKitSpecStkDetExt.usrCostingType> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                var rowExt = PXCache<INKitSpecStkDet>.GetExtension<ASCIStarINKitSpecStkDetExt>(row);
                if (rowExt.UsrCostingType == ASCIStarCostingType.StandardCost)
                {
                    var result = INItemCost.PK.Find(Base, row.CompInventoryID, Base.Accessinfo.BaseCuryID);
                    e.Cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrUnitCost>(row, result?.AvgCost ?? 0m);
                }
                else if (rowExt.UsrCostingType == ASCIStarCostingType.MarketCost || rowExt.UsrCostingType == ASCIStarCostingType.ContractCost)
                {
                    var jewelryCostBuilder = CreateCostBuilder(row);
                    var result = jewelryCostBuilder.CalculatePreciousMetalCost();
                    e.Cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrUnitCost>(row, result ?? 0m);

                    UpdateVendorPrice(e, row, rowExt, jewelryCostBuilder);
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

        #endregion

        #region ServiceMethods
        protected virtual void CopyJewelryItemFields(INKitSpecHdr kitSpecHdr)
        {
            var jewelItem = SelectFrom<ASCIStarINJewelryItem>.Where<ASCIStarINJewelryItem.inventoryID.IsEqual<PX.Data.BQL.P.AsInt>>.View.Select(this.Base, kitSpecHdr.KitInventoryID)?.TopFirst;

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

            this.JewelryItemView.Insert(jewelryKitItem);
        }
        private ASCIStarCostBuilder CreateCostBuilder(INKitSpecStkDet currentRow)
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
        private static void UpdateVendorPrice(Events.FieldUpdated<INKitSpecStkDet, ASCIStarINKitSpecStkDetExt.usrCostingType> e, INKitSpecStkDet row, ASCIStarINKitSpecStkDetExt rowExt, ASCIStarCostBuilder jewelryCostBuilder)
        {
            var salesPrice = rowExt.UsrCostingType == ASCIStarCostingType.MarketCost ? jewelryCostBuilder.PreciousMetalMarketCostPerTOZ : jewelryCostBuilder.PreciousMetalContractCostPerTOZ;
            e.Cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrSalesPrice>(row, salesPrice);
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