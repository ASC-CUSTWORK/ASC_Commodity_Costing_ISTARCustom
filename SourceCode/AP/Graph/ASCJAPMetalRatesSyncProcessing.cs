using ASCJewelryLibrary.AP.CacheExt;
using ASCJewelryLibrary.Common.Builder;
using ASCJewelryLibrary.Common.Services.REST.Interfaces;
using ASCJewelryLibrary.AP.DAC.Projections;
using ASCJewelryLibrary.AP.DAC.Unbounds;
using ASCJewelryLibrary.AP.DAC;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.IN;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ASCJewelryLibrary.AP
{
    public class ASCJMetalRatesSyncProcessing : PXGraph<ASCJMetalRatesSyncProcessing>
    {
        #region Constants
        private const string LondonPM = "LONDON PM";
        private const string LondonAM = "LONDON AM";
        private const string NewYork = "NEW YORK";
        private const string Gold24K = "24K";
        private const string Silver = "SSS";
        #endregion

        #region DataViews
        public PXFilter<ASCJMarketVendorFilter> Filter;
        public PXProcessing<ASCJMarketVendor> VandorBasis;
        public PXSetup<ASCJAPMetalRatesSetup> Setup;
        #endregion

        #region Dependency Injection
        [InjectDependency]
        public IASCJMetalsAPILatestRateService _apiRates { get; set; }
        #endregion

        #region ctor
        public ASCJMetalRatesSyncProcessing()
        {
            var graph = this;
            VandorBasis.SetProcessDelegate((List<ASCJMarketVendor> selectedRecors) =>
            {
                var listMessages = new Dictionary<int, PXSetPropertyException>();
                PXLongOperation.SetCustomInfo(listMessages, selectedRecors.Cast<object>().ToArray());
                Processing(selectedRecors, listMessages, graph);
                foreach (var msg in listMessages)
                {
                    if (msg.Value.ErrorLevel == PXErrorLevel.RowError)
                    {
                        PXProcessing.SetError(msg.Key, msg.Value);
                    }
                    else if (msg.Value.ErrorLevel == PXErrorLevel.RowWarning)
                    {
                        PXProcessing.SetWarning(msg.Key, msg.Value);
                    }
                }
            });
        }
        #endregion

        #region DataViewDelegate
        public virtual IEnumerable vandorBasis()
        {
            AppendFilters(VandorBasis, Filter.Current);
            PXView view = new PXView(this, false, VandorBasis.View.BqlSelect);
            int startRow = PXView.StartRow;
            int num = 0;
            IEnumerable results = view.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref num);
            PXView.StartRow = 0;
            return results;
        }
        #endregion

        #region Actions
        public PXCancel<ASCJMarketVendor> Cancel;
        #endregion

        #region ServiceMethods
        private static void Processing(List<ASCJMarketVendor> selectedRecords, Dictionary<int, PXSetPropertyException> listMessages, ASCJMetalRatesSyncProcessing graph)
        {
            var vendors = graph.GetVendorByBAccuntID(graph);
            var inventoryItems = graph.GetInventoryItemByID(graph);
            var vendorPriceMaint = new Lazy<APVendorPriceMaint>(() => PXGraph.CreateInstance<APVendorPriceMaint>());

            foreach (var record in selectedRecords)
            {
                try
                {
                    var vendor = vendors.Select(record.VendorID).RowCast<Vendor>().FirstOrDefault();
                    var item = inventoryItems.Select(record.InventoryID).RowCast<InventoryItem>().FirstOrDefault();
                    graph.CreateOrUpdatePriceRecord(graph, vendorPriceMaint, record, vendor, item);
                }
                catch (Exception exc)
                {
                    listMessages[selectedRecords.IndexOf(record)] = new PXSetPropertyException(exc.Message, PXErrorLevel.RowError);
                }
            }
        }

        public virtual void CreateOrUpdatePriceRecord(ASCJMetalRatesSyncProcessing graph, Lazy<APVendorPriceMaint> vendorPriceMaint, ASCJMarketVendor record, Vendor vendor, InventoryItem item)
        {
            string trimmedAcctCD = vendor.AcctCD.Trim();
            decimal newSalesPRice = decimal.Zero;

            if (trimmedAcctCD == LondonPM)
            {
                newSalesPRice = GetLondonPMPrice(graph, record, item);
            }
            else if (trimmedAcctCD == LondonAM)
            {
                newSalesPRice = GetLondonAMPrice(graph, record, item);
            }
            else if (trimmedAcctCD == NewYork)
            {
                newSalesPRice = GetNewYorkPrice(graph, record, item);
            }

            if (newSalesPRice == decimal.Zero)
            {
                PXProcessing<ASCJMarketVendor>.SetError(string.Format("Market {0} return Zero price for {1} metal, item: {2}", trimmedAcctCD, record.Commodity, record.InventoryID));
            }
            else
            {
                var vendorPrice = ASCJCostBuilder.GetAPVendorPrice(graph, record.VendorID, record.InventoryID, record.UOM, graph.Accessinfo.BusinessDate.Value);

                ProcessAPVendorPrice(vendorPriceMaint, record, vendorPrice, newSalesPRice);
            }
        }

        public virtual decimal GetNewYorkPrice(ASCJMetalRatesSyncProcessing graph, ASCJMarketVendor record, InventoryItem item)
        {
            switch (item.InventoryCD.Trim())
            {
                case Gold24K: return graph._apiRates.GetXAURate(record.CuryID);
                case Silver: return graph._apiRates.GetXAGRate(record.CuryID);
                default: return decimal.Zero;
            }
        }

        public virtual decimal GetLondonAMPrice(ASCJMetalRatesSyncProcessing graph, ASCJMarketVendor record, InventoryItem item)
        {
            switch (item.InventoryCD.Trim())
            {
                case Gold24K: return graph._apiRates.GetLBXAUAMRate(record.CuryID);
                case Silver: return graph._apiRates.GetLBXAGRate(record.CuryID);
                default: return decimal.Zero;
            }
        }

        public virtual decimal GetLondonPMPrice(ASCJMetalRatesSyncProcessing graph, ASCJMarketVendor record, InventoryItem item)
        {
            switch (item.InventoryCD.Trim())
            {
                case Gold24K: return graph._apiRates.GetLBXAUPMRate(record.CuryID);
                case Silver: return graph._apiRates.GetLBXAGRate(record.CuryID);
                default: return decimal.Zero;
            }
        }

        public virtual void ProcessAPVendorPrice(Lazy<APVendorPriceMaint> vendorPriceMaint, ASCJMarketVendor row, APVendorPrice vendorPrice, decimal salesPrice)
        {
            vendorPriceMaint.Value.Clear(PXClearOption.ClearAll);
            using (var transactionScope = new PXTransactionScope())
            {
                vendorPriceMaint.Value.SelectTimeStamp();
                if (vendorPrice == null)
                {
                    CreateAPVendorPriceRecord(vendorPriceMaint, row, salesPrice);
                }
                else
                {
                    if (vendorPrice.EffectiveDate == vendorPriceMaint.Value.Accessinfo.BusinessDate)
                    {
                        vendorPrice.SalesPrice = salesPrice;
                        var vendorPriceExt = PXCache<APVendorPrice>.GetExtension<ASCJAPVendorPriceExt>(vendorPrice);
                        vendorPriceExt.UsrFormAPI = true;
                        vendorPriceMaint.Value.Records.Update(vendorPrice);
                    }
                    else
                    {
                        vendorPrice.ExpirationDate =
                          vendorPriceMaint.Value.Accessinfo.BusinessDate.HasValue
                        ? vendorPriceMaint.Value.Accessinfo.BusinessDate.Value.AddDays(-1)
                        : DateTime.Today.AddDays(-1);
                        vendorPriceMaint.Value.Records.Update(vendorPrice);

                        CreateAPVendorPriceRecord(vendorPriceMaint, row, salesPrice);
                    }
                }
                vendorPriceMaint.Value.Save.Press();
                transactionScope.Complete();
            }
        }

        public virtual void CreateAPVendorPriceRecord(Lazy<APVendorPriceMaint> vendorPriceMaint, ASCJMarketVendor row, decimal salesPrice)
        {
            var vendorPrice = new APVendorPrice();
            vendorPrice = vendorPriceMaint.Value.Records.Insert(vendorPrice);
            vendorPrice.VendorID = row.VendorID;
            vendorPrice.InventoryID = row.InventoryID;
            vendorPrice.SalesPrice = salesPrice;
            vendorPrice.UOM = row.UOM;
            vendorPrice.EffectiveDate = vendorPriceMaint.Value.Accessinfo.BusinessDate;

            var vendorPriceExt = PXCache<APVendorPrice>.GetExtension<ASCJAPVendorPriceExt>(vendorPrice);
            vendorPriceExt.UsrFormAPI = true;

            vendorPriceMaint.Value.Records.Update(vendorPrice);
        }

        private void AppendFilters(PXSelectBase<ASCJMarketVendor> cmd, ASCJMarketVendorFilter filter)
        {
            cmd.OrderByNew<OrderBy<Asc<ASCJMarketVendor.vendorID>>>();

            if (filter.VendorID != null)
            {
                cmd.WhereAnd<Where<ASCJMarketVendor.vendorID, Equal<Current<ASCJMarketVendorFilter.vendorID>>>>();
            }
            if (filter.ItemClassCD != null)
            {
                cmd.WhereAnd<Where<ASCJMarketVendor.itemClassCD, Equal<Current<ASCJMarketVendorFilter.itemClassCD>>>>();
            }
            if (filter.InventoryID != null)
            {
                cmd.WhereAnd<Where<ASCJMarketVendor.inventoryID, Equal<Current<ASCJMarketVendorFilter.inventoryID>>>>();
            }
        }
        #endregion

        #region ServiceQueries
        private PXSelectBase<Vendor> GetVendorByBAccuntID(PXGraph graph)
        {
            return new PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>(graph);
        }
        private PXSelectBase<InventoryItem> GetInventoryItemByID(PXGraph graph)
        {
            return new PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>(graph);
        }
        #endregion
    }
}
