using ASCISTARCustom.AP.CacheExt;
using ASCISTARCustom.Common.Builder;
using ASCISTARCustom.IN.CacheExt;
using ASCISTARCustom.IN.DAC;
using ASCISTARCustom.PO.CacheExt;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.PO;
using static ASCISTARCustom.Common.Descriptor.ASCIStarConstants;

namespace ASCISTARCustom.AP.GraphExt
{
    public class ASCIStarAPVendorPriceMaintExt : PXGraphExtension<APVendorPriceMaint>
    {
        public static bool IsActive() => true;

        public override void Initialize()
        {
            base.Initialize();
            Base.Records.WhereAnd<Where
                <Brackets<ASCIStarAPVendorPriceFilterExt.usrOnlyMarkets.FromCurrent.IsEqual<True>
                .And<Vendor.vendorClassID.IsEqual<Common.Descriptor.ASCIStarConstants.MarketClass>>
                .Or<ASCIStarAPVendorPriceFilterExt.usrOnlyMarkets.FromCurrent.IsEqual<False>>>>>();
        }

        #region CacheAttached
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Basis Price", Visibility = PXUIVisibility.Visible)]
        protected virtual void _(Events.CacheAttached<APVendorPrice.salesPrice> e) { }
        #endregion CacheAttached

        #region Event Handlers
        protected virtual void _(Events.FieldUpdated<APVendorPrice, APVendorPrice.vendorID> e)
        {
            var row = e.Row;
            if (row == null) return;
            Vendor vendor = Vendor.PK.Find(this.Base, (int?)e.NewValue);

            e.Cache.SetValueExt<ASCIStarAPVendorPriceExt.usrMarketID>(row, vendor?.GetExtension<ASCIStarVendorExt>().UsrMarketID);
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
        private void UpdateCostProviderFields(PXCache cache, APVendorPrice row)
        {
            if (row.InventoryID == null || row.VendorID == null) return;

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
