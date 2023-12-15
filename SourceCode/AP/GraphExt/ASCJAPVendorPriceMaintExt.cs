using ASCJewelryLibrary.AP.CacheExt;
using ASCJewelryLibrary.Common.Builder;
using ASCJewelryLibrary.IN.CacheExt;
using ASCJewelryLibrary.PO.CacheExt;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.PO;
using static ASCJewelryLibrary.Common.Descriptor.ASCJConstants;

namespace ASCJewelryLibrary.AP.GraphExt
{
    public class ASCJAPVendorPriceMaintExt : PXGraphExtension<APVendorPriceMaint>
    {
        public static bool IsActive() => true;

        public override void Initialize()
        {
            base.Initialize();
            Base.Records.WhereAnd<Where
                <Brackets<ASCJAPVendorPriceFilterExt.usrASCJOnlyMarkets.FromCurrent.IsEqual<True>
                .And<Vendor.vendorClassID.IsEqual<Common.Descriptor.ASCJConstants.MarketClass>>
                .Or<ASCJAPVendorPriceFilterExt.usrASCJOnlyMarkets.FromCurrent.IsEqual<False>>>>>();
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

            e.Cache.SetValueExt<ASCJAPVendorPriceExt.usrASCJMarketID>(row, vendor?.GetExtension<ASCJVendorExt>().UsrASCJMarketID);
        }
        protected virtual void _(Events.FieldUpdated<APVendorPrice, APVendorPrice.salesPrice> e)
        {
            var row = e.Row;
            if (row == null) return;

            UpdateFloorCeilingFields(e.Cache, row);
        }
        protected virtual void _(Events.FieldUpdated<APVendorPrice, ASCJAPVendorPriceExt.usrASCJMatrixStep> e)
        {
            var row = e.Row;
            if (row == null) return;

            UpdateFloorCeilingFields(e.Cache, row);


        }
        #endregion

        #region Methods
        private void UpdateFloorCeilingFields(PXCache cache, APVendorPrice row)
        {
            var rowExt = PXCache<APVendorPrice>.GetExtension<ASCJAPVendorPriceExt>(row);
            if (rowExt?.UsrASCJCommodity != CommodityType.Silver) return;

            var poVendorInventory = new POVendorInventory() { VendorID = row.VendorID };
            PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(poVendorInventory).UsrASCJMarketID = rowExt.UsrASCJMarketID;

            var inventoryItem = InventoryItem.PK.Find(Base, row.InventoryID);
            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(inventoryItem);
            inventoryItemExt.UsrASCJMatrixStep = rowExt.UsrASCJMatrixStep;

            var jewelryCostProvider = new ASCJCostBuilder(this.Base)
                            .WithInventoryItem(inventoryItemExt)
                            .WithPOVendorInventory(poVendorInventory)
                            .Build();
            if (jewelryCostProvider == null) return;

            jewelryCostProvider.CalculatePreciousMetalCost(CostingType.ContractCost);

            cache.SetValueExt<ASCJAPVendorPriceExt.usrASCJFloor>(row, jewelryCostProvider.Floor);
            cache.SetValueExt<ASCJAPVendorPriceExt.usrASCJCeiling>(row, jewelryCostProvider.Ceiling);
        }
        #endregion

    }
}
