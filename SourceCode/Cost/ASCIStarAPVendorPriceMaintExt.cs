using PX.Data;
using PX.Objects.AP;
using static ASCISTARCustom.Common.Descriptor.ASCIStarConstants;

namespace ASCISTARCustom
{
    public class ASCIStarAPVendorPriceMaintExt : PXGraphExtension<APVendorPriceMaint>
    {
        #region CacheAttached

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXFormula(typeof(Switch<Case<Where<Current<APVendorPrice.uOM>, NotEqual<TOZ>>, Null>, Div<APVendorPrice.salesPrice, TOZ2GRAM_31_10348>>))]
        protected void APVendorPrice_UsrCommodityPerGram_CacheAttached(PXCache sender) { }

        //[PXMergeAttributes(Method = MergeMethod.Append)]
        //[PXFormula(typeof(Switch<Case<Where<Current<APVendorPrice.uOM>, NotEqual<TOZ>>, Null>, Div<APVendorPrice.salesPrice, ASCIStarAPVendorPriceExt.usrCommodityPrice>>))]
        //protected void APVendorPrice_UsrIncrement_CacheAttached(PXCache sender) { }

        //[PXMergeAttributes(Method = MergeMethod.Append)]
        //[PXFormula(typeof(Switch<Case<Where<Current<APVendorPrice.uOM>, NotEqual<TOZ>>, Null>, Div<Div<APVendorPrice.salesPrice, ASCIStarAPVendorPriceExt.usrCommodityPrice>, TOZ2GRAM>>))]
        //protected void APVendorPrice_UsrIncrementPerGram_CacheAttached(PXCache sender) { }

        //SWITCH TO ACTUAL LOOKUP?

        #endregion CacheAttached

        #region Event Handlers

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
    }
}