using ASCJSMCustom.AP.CacheExt;
using ASCJSMCustom.Common.Builder;
using ASCJSMCustom.IN.DAC;
using ASCJSMCustom.PO.CacheExt;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.PO;
using static ASCJSMCustom.Common.Descriptor.ASCJSMConstants;
using ASCJSMCustom.IN.CacheExt;

namespace ASCJSMCustom.AP.GraphExt
{
    public class ASCJSMAPVendorPriceMaintExt : PXGraphExtension<APVendorPriceMaint>
    {
        public static bool IsActive() => true;

        public override void Initialize()
        {
            base.Initialize();
            Base.Records.WhereAnd<Where
                <Brackets<ASCJSMAPVendorPriceFilterExt.usrOnlyMarkets.FromCurrent.IsEqual<True>
                .And<Vendor.vendorClassID.IsEqual<Common.Descriptor.ASCJSMConstants.MarketClass>>
                .Or<ASCJSMAPVendorPriceFilterExt.usrOnlyMarkets.FromCurrent.IsEqual<False>>>>>();
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

            e.Cache.SetValueExt<ASCJSMAPVendorPriceExt.usrMarketID>(row, vendor?.GetExtension<ASCJSMVendorExt>().UsrMarketID);
        }
        protected virtual void _(Events.FieldUpdated<APVendorPrice, APVendorPrice.salesPrice> e)
        {
            var row = e.Row;
            if (row == null) return;

            UpdateCostProviderFields(e.Cache, row);
        }
        protected virtual void _(Events.FieldUpdated<APVendorPrice, ASCJSMAPVendorPriceExt.usrMatrixStep> e)
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

            var rowExt = PXCache<APVendorPrice>.GetExtension<ASCJSMAPVendorPriceExt>(row);

            var poVendorInventory = new POVendorInventory() { VendorID = row.VendorID };
            PXCache<POVendorInventory>.GetExtension<ASCJSMPOVendorInventoryExt>(poVendorInventory).UsrMarketID = rowExt.UsrMarketID;

            var inventoryItem = InventoryItem.PK.Find(Base, row.InventoryID);
            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(inventoryItem);
            inventoryItemExt.UsrMatrixStep = rowExt.UsrMatrixStep;

            var inJewelryAttribute = new ASCJSMINJewelryItem();
            var itemCD = inventoryItem.InventoryCD.Trim();
            if (itemCD == "24K") inJewelryAttribute.MetalType = "24K";
            if (itemCD == "SSS") inJewelryAttribute.MetalType = "SSS";

            var jewelryCostProvider = new ASCJSMCostBuilder(this.Base)
                            .WithInventoryItem(inventoryItemExt)
                            .WithJewelryAttrData(inJewelryAttribute)
                            .WithPOVendorInventory(poVendorInventory)
                            .Build();
            if (jewelryCostProvider == null) return;

            jewelryCostProvider.CalculatePreciousMetalCost(CostingType.ContractCost);

            cache.SetValueExt<ASCJSMAPVendorPriceExt.usrFloor>(row, jewelryCostProvider.Floor);
            cache.SetValueExt<ASCJSMAPVendorPriceExt.usrCeiling>(row, jewelryCostProvider.Ceiling);
            cache.SetValueExt<ASCJSMAPVendorPriceExt.usrBasisValue>(row, jewelryCostProvider.BasisValue);
        }
        #endregion

    }
}
