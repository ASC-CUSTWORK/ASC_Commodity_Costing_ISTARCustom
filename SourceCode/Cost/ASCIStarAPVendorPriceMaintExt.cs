using ASCISTARCustom.Common.Builder;
using ASCISTARCustom.Cost.CacheExt;
using ASCISTARCustom.Inventory.Descriptor.Constants;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.PO;
using static ASCISTARCustom.Common.Descriptor.ASCIStarConstants;

namespace ASCISTARCustom
{
    public class ASCIStarAPVendorPriceMaintExt : PXGraphExtension<APVendorPriceMaint>
    {
        public static bool IsActive() => true;

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

            UpdateFloorCeilingFields(e.Cache, row);
        }
        protected virtual void _(Events.FieldUpdated<APVendorPrice, ASCIStarAPVendorPriceExt.usrMatrixStep> e)
        {
            var row = e.Row;
            if (row == null) return;

            UpdateFloorCeilingFields(e.Cache, row);


        }
        #endregion
     
        #region Methods
        private void UpdateFloorCeilingFields(PXCache cache, APVendorPrice row)
        {
            var rowExt = PXCache<APVendorPrice>.GetExtension<ASCIStarAPVendorPriceExt>(row);
            if (rowExt?.UsrCommodity != CommodityType.Silver) return;

            var poVendorInventory = new POVendorInventory() { VendorID = row.VendorID };
            PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(poVendorInventory).UsrMarketID = rowExt.UsrMarketID;

            var inventoryItem = InventoryItem.PK.Find(Base, row.InventoryID);
            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(inventoryItem);
            inventoryItemExt.UsrMatrixStep = rowExt.UsrMatrixStep;

            var jewelryCostProvider = new ASCIStarCostBuilder(this.Base)
                            .WithInventoryItem(inventoryItemExt)
                            .WithPOVendorInventory(poVendorInventory)
                            .Build();
            if (jewelryCostProvider == null) return;

            jewelryCostProvider.CalculatePreciousMetalCost(CostingType.ContractCost);

            cache.SetValueExt<ASCIStarAPVendorPriceExt.usrFloor>(row, jewelryCostProvider.Floor);
            cache.SetValueExt<ASCIStarAPVendorPriceExt.usrCeiling>(row, jewelryCostProvider.Ceiling);
        }
        #endregion
    }
}