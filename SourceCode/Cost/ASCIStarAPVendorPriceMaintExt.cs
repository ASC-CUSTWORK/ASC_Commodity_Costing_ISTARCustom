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
        #region CacheAttached

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Basis Price", Visibility = PXUIVisibility.Visible)]
        protected virtual void _(Events.CacheAttached<APVendorPrice.salesPrice> e) { }
        //[PXMergeAttributes(Method = MergeMethod.Merge)]
        //[PXFormula(typeof(Switch<Case<Where<Current<APVendorPrice.uOM>, NotEqual<TOZ>>, Null>, Div<APVendorPrice.salesPrice, TOZ2GRAM_31_10348>>))]
        //   protected void APVendorPrice_UsrCommodityPerGram_CacheAttached(PXCache sender) { }

        //[PXMergeAttributes(Method = MergeMethod.Append)]
        //[PXFormula(typeof(Switch<Case<Where<Current<APVendorPrice.uOM>, NotEqual<TOZ>>, Null>, Div<APVendorPrice.salesPrice, ASCIStarAPVendorPriceExt.usrCommodityPrice>>))]
        //protected void APVendorPrice_UsrIncrement_CacheAttached(PXCache sender) { }

        //[PXMergeAttributes(Method = MergeMethod.Append)]
        //[PXFormula(typeof(Switch<Case<Where<Current<APVendorPrice.uOM>, NotEqual<TOZ>>, Null>, Div<Div<APVendorPrice.salesPrice, ASCIStarAPVendorPriceExt.usrCommodityPrice>, TOZ2GRAM>>))]
        //protected void APVendorPrice_UsrIncrementPerGram_CacheAttached(PXCache sender) { }

        //SWITCH TO ACTUAL LOOKUP?

        #endregion CacheAttached

        #region Event Handlers
        protected virtual void _(Events.FieldUpdating<APVendorPrice, APVendorPrice.vendorID> e)
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
        //protected void APVendorPrice_VendorID_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e, PXFieldUpdating InvokeBaseHandler)
        //{
        //    if (InvokeBaseHandler != null)
        //        InvokeBaseHandler(cache, e);
        //    var row = (APVendorPrice)e.Row;
        //    if (row == null)
        //        return;

        //    PXTrace.WriteInformation($"");
        //    Vendor vendor = Vendor.PK.Find(cache.Graph, row.VendorID);

        //    if (vendor.AcctCD.Trim().Equals("LONDON AM") || vendor.AcctCD.Trim().Equals("LONDON PM") || vendor.AcctCD.Trim().Equals("NEW YORK"))
        //    {
        //        ASCIStarAPVendorPriceExt ext = cache.GetExtension<ASCIStarAPVendorPriceExt>(row);
        //        if (ext == null)
        //            return;
        //        switch (vendor.AcctCD.Trim())
        //        {
        //            case "LONDON AM":
        //                ext.UsrMarket = MarketList.LondonAM;
        //                ext.UsrCommodityPerGram = row.SalesPrice / 31.1m;
        //                break;
        //            case "LONDON PM":
        //                ext.UsrMarket = MarketList.LondonAM;
        //                ext.UsrCommodityPerGram = row.SalesPrice / 31.1m;
        //                break;
        //            case "NEW YORK":
        //                ext.UsrMarket = MarketList.LondonAM;
        //                ext.UsrCommodityPerGram = row.SalesPrice / 31.1m;
        //                break;
        //            default:
        //                ext.UsrMarket = "";
        //                ext.UsrCommodityPerGram = null;
        //                break;

        //        }

        //    }

        //}



        //protected void APVendorPrice_SalesPrice_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e, PXFieldUpdating InvokeBaseHandler)
        //{
        //    PXTrace.WriteInformation($"APVendorPrice_SalesPrice_FieldUpdating");

        //    if (InvokeBaseHandler != null)
        //        InvokeBaseHandler(cache, e);
        //    var row = (APVendorPrice)e.Row;
        //    if (row == null)
        //        return;
        //    PXTrace.WriteInformation($"row.VendorID:{row.VendorID}");
        //    Vendor vendor = Vendor.PK.Find(cache.Graph, row.VendorID);

        //    PXTrace.WriteInformation($"vendor.AcctCD:'{vendor.AcctCD}'");

        //    if (vendor.AcctCD.Trim() == "LONDON AM" || vendor.AcctCD.Trim() == "LONDON PM" || vendor.AcctCD.Trim() == "NEW YORK")
        //    {
        //        PXTrace.WriteInformation($"Changing UsrCommodityIncrement");
        //        ASCIStarAPVendorPriceExt ext = cache.GetExtension<ASCIStarAPVendorPriceExt>(row);
        //        //Add Actual UOM Lookup here
        //        decimal factor = 31.1m;
        //        decimal? before = ext.UsrCommodityPerGram ?? 0.00m;
        //        ext.UsrCommodityPerGram = (Decimal)e.NewValue / factor;

        //        PXTrace.WriteInformation($"UsrCommodityIncrement:{before} to {ext.UsrCommodityPerGram}");

        //    }

        //}


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

            jewelryCostProvider.CalculatePreciousMetalCost(ASCIStarCostingType.ContractCost);

            cache.SetValueExt<ASCIStarAPVendorPriceExt.usrFloor>(row, jewelryCostProvider.Floor);
            cache.SetValueExt<ASCIStarAPVendorPriceExt.usrCeiling>(row, jewelryCostProvider.Ceiling);
        }
        #endregion
    }
}