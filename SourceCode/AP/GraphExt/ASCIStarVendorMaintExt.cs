using ASCISTARCustom.AP.CacheExt;
using ASCISTARCustom.Common.Builder;
using ASCISTARCustom.Common.Descriptor;
using ASCISTARCustom.Common.Helper.Extensions;
using ASCISTARCustom.IN.CacheExt;
using ASCISTARCustom.IN.DAC;
using ASCISTARCustom.PO.CacheExt;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.PO;
using static ASCISTARCustom.Common.Descriptor.ASCIStarConstants;

namespace ASCISTARCustom.Cost
{
    public class ASCIStarVendorMaintExt : PXGraphExtension<VendorMaint>
    {
        public static bool IsActive() => true;

        #region Selects
        [PXFilterable]
        [PXCopyPasteHiddenView]
        public PXSelectJoin<APVendorPrice,
                           InnerJoin<InventoryItem, On<APVendorPrice.inventoryID, Equal<InventoryItem.inventoryID>>,
                           InnerJoin<INItemClass, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>>,
                                    Where<APVendorPrice.vendorID, Equal<Current<Vendor.bAccountID>>,
                                        And<INItemClass.itemClassCD, Equal<ASCIStarConstants.CommodityClass>>>,
                            OrderBy<Desc<APVendorPrice.effectiveDate>>> VendorPriceBasis;
        #endregion

        #region CacheAttached
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Basis Floor", Visibility = PXUIVisibility.Visible)]
        protected virtual void _(Events.CacheAttached<APVendorPrice.salesPrice> e) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Commodity Type Item")]
        [PXSelector(typeof(SearchFor<InventoryItem.inventoryID>.In<
                            SelectFrom<InventoryItem>.InnerJoin<INItemClass>
                                .On<InventoryItem.itemClassID.IsEqual<INItemClass.itemClassID>>
            .Where<INItemClass.itemClassCD.IsEqual<ASCIStarConstants.CommodityClass>>>), SubstituteKey = typeof(InventoryItem.inventoryCD), DescriptionField = typeof(InventoryItem.descr))]
        protected virtual void _(Events.CacheAttached<APVendorPrice.inventoryID> e) { }
        #endregion

        #region EventHandlers
        protected virtual void _(Events.RowSelected<PX.Objects.CR.Standalone.Location> e)
        {
            if (e.Row == null) return;

            PXUIFieldAttribute.SetRequired<PX.Objects.CR.Standalone.Location.vBranchID>(e.Cache, true);
            PXDefaultAttribute.SetPersistingCheck<PX.Objects.CR.Standalone.Location.vBranchID>(e.Cache, e.Row, PXPersistingCheck.NullOrBlank);
        }
        protected virtual void _(Events.FieldDefaulting<APVendorPrice, APVendorPrice.vendorID> e)
        {
            if (e.Row == null || this.Base.BAccount.Current == null) return;

            if (this.Base.BAccount.Current.BAccountID > 0)
                e.NewValue = this.Base.BAccount.Current.BAccountID;
            else
                throw new PXException("Save Vendor first and then add Costing information.");
        }

        protected virtual void _(Events.RowSelected<VendorR> e)
        {
            if (e.Row is VendorR row)
            {
                PXUIFieldAttribute.SetRequired<ASCIStarVendorExt.usrMarketID>(e.Cache, row.VendorClassID?.NormalizeCD() != "MARKET");
            }
        }

        protected virtual void _(Events.RowPersisting<VendorR> e)
        {
            if (e.Row is VendorR row)
            {
                if (row.VendorClassID?.NormalizeCD() != "MARKET")
                {
                    var rowExt = PXCache<Vendor>.GetExtension<ASCIStarVendorExt>(row);
                    if (rowExt?.UsrMarketID == null)
                        PXUIFieldAttribute.SetError<ASCIStarVendorExt.usrMarketID>(e.Cache, row, "'Market' cannot be empty.");
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<APVendorPrice, APVendorPrice.salesPrice> e)
        {
            var row = e.Row;
            if (row == null) return;

            UpdateCostProviderFields(e.Cache, row);
        }

        protected virtual void _(Events.FieldUpdated<APVendorPrice, ASCIStarAPVendorPriceExt.usrMatrixStep> e)
        {
            var row = e.Row;
            if (row == null) return;

            UpdateCostProviderFields(e.Cache, row);
        }
        #endregion

        #region Methods
        protected virtual void UpdateCostProviderFields(PXCache cache, APVendorPrice row)
        {
            if (row.InventoryID == null) return;
            var rowExt = PXCache<APVendorPrice>.GetExtension<ASCIStarAPVendorPriceExt>(row);

            var poVendorInventory = new POVendorInventory() { VendorID = row.VendorID };
            PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(poVendorInventory).UsrMarketID = rowExt.UsrMarketID;

            var inventoryItem = InventoryItem.PK.Find(Base, row.InventoryID);
            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(inventoryItem);
            inventoryItemExt.UsrMatrixStep = rowExt.UsrMatrixStep;

            var inJewelryAttribute = new ASCIStarINJewelryItem();
            var itemCD = inventoryItem.InventoryCD.Trim();
            if (itemCD == "24K") inJewelryAttribute.MetalType = "24K";
            if (itemCD == "SSS") inJewelryAttribute.MetalType = "SSS";

            var jewelryCostProvider = new ASCIStarCostBuilder(this.Base)
                            .WithInventoryItem(inventoryItemExt)
                            .WithJewelryAttrData(inJewelryAttribute)
                            .WithPOVendorInventory(poVendorInventory)
                            .Build();
            if (jewelryCostProvider == null) return;
            jewelryCostProvider.CalculatePreciousMetalCost(CostingType.ContractCost);

            cache.SetValueExt<ASCIStarAPVendorPriceExt.usrFloor>(row, jewelryCostProvider.Floor);
            cache.SetValueExt<ASCIStarAPVendorPriceExt.usrCeiling>(row, jewelryCostProvider.Ceiling);
            cache.SetValueExt<ASCIStarAPVendorPriceExt.usrBasisValue>(row, jewelryCostProvider.BasisValue);
        }
        #endregion

    }
}