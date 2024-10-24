using ASCJSMCustom.AP.CacheExt;
using ASCJSMCustom.Common.Builder;
using ASCJSMCustom.Common.Services.REST.Interfaces;
using ASCJSMCustom.Cost.DAC;
using ASCJSMCustom.Cost.DAC.Unbounds;
using ASCJSMCustom.Cost.Descriptor;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.IN;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static ASCJSMCustom.Common.Descriptor.ASCJSMConstants;
using ASCJSMCustom.Cost.DAC.Projections;

namespace ASCJSMCustom.Cost
{
    public class ASCJSMMetalRatesSyncProcessing : PXGraph<ASCJSMMetalRatesSyncProcessing>
    {
        #region Constants
        private const string Gold24K = "24K";
        private const string Silver = "SSS";
        #endregion

        public PXCancel<ASCJSMMarketVendor> Cancel;

        #region DataViews
        public PXFilter<ASCJSMMarketVendorFilter> Filter;
        public PXProcessing<ASCJSMMarketVendor> VandorBasis;
        public PXSetup<ASCJSMSetup> Setup;
        #endregion

        #region Dependency Injection
        [InjectDependency]
        public IASCJSMMetalsAPILatestRateService _apiRates { get; set; }
        #endregion

        #region ctor
        public ASCJSMMetalRatesSyncProcessing()
        {
            var graph = this;
            VandorBasis.SetProcessDelegate((List<ASCJSMMarketVendor> selectedRecords) =>
            {
                var listMessages = new Dictionary<int, ASCJSMApiResponseMessage>();
                PXLongOperation.SetCustomInfo(listMessages, selectedRecords.Cast<object>().ToArray());
                Processing(selectedRecords, listMessages, graph);
                foreach (var msg in listMessages)
                {
                    switch (msg.Value.Status)
                    {
                        case ASCJSMApiResponseMessage.Success:
                            PXProcessing<ASCJSMMarketVendor>.SetProcessed();
                            break;
                        case ASCJSMApiResponseMessage.Warning:
                            PXProcessing<ASCJSMMarketVendor>.SetWarning(msg.Key, msg.Value.Message);
                            break;
                        case ASCJSMApiResponseMessage.Error:
                            PXProcessing<ASCJSMMarketVendor>.SetError(msg.Key, msg.Value.Message);
                            break;
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

        #region ServiceMethods
        private static void Processing(List<ASCJSMMarketVendor> selectedRecords, Dictionary<int, ASCJSMApiResponseMessage> listMessages, ASCJSMMetalRatesSyncProcessing graph)
        {
            var vendors = graph.GetVendorByBAccuntID(graph);
            var inventoryItems = graph.GetInventoryItemByID(graph);
            var vendorPriceMaint = new Lazy<APVendorPriceMaint>(() => PXGraph.CreateInstance<APVendorPriceMaint>());

            foreach (var record in selectedRecords)
            {
                var vendor = vendors.Select(record.VendorID).RowCast<Vendor>().FirstOrDefault();
                var item = inventoryItems.Select(record.InventoryID).RowCast<InventoryItem>().FirstOrDefault();
                listMessages[selectedRecords.IndexOf(record)] = graph.CreateOrUpdatePriceRecord(graph, vendorPriceMaint, record, vendor, item);
            }
        }

        public virtual ASCJSMApiResponseMessage CreateOrUpdatePriceRecord(ASCJSMMetalRatesSyncProcessing graph, Lazy<APVendorPriceMaint> vendorPriceMaint, ASCJSMMarketVendor record, Vendor vendor, InventoryItem item)
        {
            string trimmedAcctCD = vendor.AcctCD.Trim();
            decimal newSalesPRice = decimal.Zero;
            var resultMessage = new ASCJSMApiResponseMessage();
            try
            {
                switch (trimmedAcctCD)
                {
                    case MarketList.MessageLondonPM:
                        newSalesPRice = GetLondonPMPrice(graph, record, item);
                        break;
                    case MarketList.MessageLondonAM:
                        newSalesPRice = GetLondonAMPrice(graph, record, item);
                        break;
                    case MarketList.MessageNewYork:
                        newSalesPRice = GetNewYorkPrice(graph, record, item);
                        break;
                    default:
                        newSalesPRice = decimal.Zero;
                        break;
                }
            }

            catch (Exception exc)
            {
                resultMessage.Price = decimal.Zero;
                resultMessage.Status = ASCJSMApiResponseMessage.Error;
                resultMessage.Message = exc.Message;
            }
            if (resultMessage.Status != ASCJSMApiResponseMessage.Error && newSalesPRice == decimal.Zero)
            {
                string message = string.Format("Market {0} return Zero price for {1} metal, item: {2}", trimmedAcctCD, record.Commodity, record.InventoryID);
                resultMessage.Price = decimal.Zero;
                resultMessage.Status = ASCJSMApiResponseMessage.Error;
                resultMessage.Message = message;
            }
            if (newSalesPRice != decimal.Zero)
            {
                resultMessage.Price = newSalesPRice;
                resultMessage.Status = ASCJSMApiResponseMessage.Success;
                resultMessage.Message = string.Empty;
                var vendorPrice = ASCJSMCostBuilder.GetAPVendorPrice(graph, record.VendorID, record.InventoryID, record.UOM, graph.Accessinfo.BusinessDate.Value.AddDays(-1));
                ProcessAPVendorPrice(vendorPriceMaint, record, vendorPrice, newSalesPRice);
            }
            return resultMessage;
        }

        public virtual decimal GetNewYorkPrice(ASCJSMMetalRatesSyncProcessing graph, ASCJSMMarketVendor record, InventoryItem item)
        {
            switch (item.InventoryCD.Trim())
            {
                case Gold24K: return graph._apiRates.GetXAURate(record.CuryID);
                case Silver: return graph._apiRates.GetXAGRate(record.CuryID);
                default: return decimal.Zero;
            }
        }

        public virtual decimal GetLondonAMPrice(ASCJSMMetalRatesSyncProcessing graph, ASCJSMMarketVendor record, InventoryItem item)
        {
            switch (item.InventoryCD.Trim())
            {
                case Gold24K: return graph._apiRates.GetLBXAUAMRate(record.CuryID);
                case Silver: return graph._apiRates.GetLBXAGRate(record.CuryID);
                default: return decimal.Zero;
            }
        }

        public virtual decimal GetLondonPMPrice(ASCJSMMetalRatesSyncProcessing graph, ASCJSMMarketVendor record, InventoryItem item)
        {
            switch (item.InventoryCD.Trim())
            {
                case Gold24K: return graph._apiRates.GetLBXAUPMRate(record.CuryID);
                case Silver: return graph._apiRates.GetLBXAGRate(record.CuryID);
                default: return decimal.Zero;
            }
        }

        public virtual void ProcessAPVendorPrice(Lazy<APVendorPriceMaint> vendorPriceMaint, ASCJSMMarketVendor row, APVendorPrice vendorPrice, decimal salesPrice)
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
                    if (vendorPrice.EffectiveDate == vendorPriceMaint.Value.Accessinfo.BusinessDate.Value.AddDays(-1))
                    {
                        vendorPrice.SalesPrice = salesPrice;
                        var vendorPriceExt = PXCache<APVendorPrice>.GetExtension<ASCJSMAPVendorPriceExt>(vendorPrice);
                        vendorPriceExt.UsrFormAPI = true;
                        vendorPriceMaint.Value.Records.Update(vendorPrice);
                    }
                    else
                    {
                        vendorPrice.ExpirationDate =
                          vendorPriceMaint.Value.Accessinfo.BusinessDate.HasValue
                        ? vendorPriceMaint.Value.Accessinfo.BusinessDate.Value.AddDays(-2)
                        : DateTime.Today.AddDays(-2);
                        vendorPriceMaint.Value.Records.Update(vendorPrice);

                        CreateAPVendorPriceRecord(vendorPriceMaint, row, salesPrice);
                    }
                }
                vendorPriceMaint.Value.Save.Press();
                transactionScope.Complete();
            }
        }

        public virtual void CreateAPVendorPriceRecord(Lazy<APVendorPriceMaint> vendorPriceMaint, ASCJSMMarketVendor row, decimal salesPrice)
        {
            var vendorPrice = new APVendorPrice();
            vendorPrice = vendorPriceMaint.Value.Records.Insert(vendorPrice);
            vendorPrice.VendorID = row.VendorID;
            vendorPrice.InventoryID = row.InventoryID;
            vendorPrice.SalesPrice = salesPrice;
            vendorPrice.UOM = row.UOM;

            vendorPrice.EffectiveDate = vendorPriceMaint.Value.Accessinfo.BusinessDate.Value.AddDays(-1);

            var vendorPriceExt = PXCache<APVendorPrice>.GetExtension<ASCJSMAPVendorPriceExt>(vendorPrice);
            vendorPriceExt.UsrFormAPI = true;

            vendorPriceMaint.Value.Records.Update(vendorPrice);
        }

        private void AppendFilters(PXSelectBase<ASCJSMMarketVendor> cmd, ASCJSMMarketVendorFilter filter)
        {
            cmd.OrderByNew<OrderBy<Asc<ASCJSMMarketVendor.vendorID>>>();

            if (filter.VendorID != null)
            {
                cmd.WhereAnd<Where<ASCJSMMarketVendor.vendorID, Equal<Current<ASCJSMMarketVendorFilter.vendorID>>>>();
            }
            if (filter.InventoryID != null)
            {
                cmd.WhereAnd<Where<ASCJSMMarketVendor.inventoryID, Equal<Current<ASCJSMMarketVendorFilter.inventoryID>>>>();
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
