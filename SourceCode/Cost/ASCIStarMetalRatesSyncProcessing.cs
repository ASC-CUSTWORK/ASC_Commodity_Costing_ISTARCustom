using ASCISTARCustom.Common.Descriptor;
using ASCISTARCustom.Common.Services.REST.Interfaces;
using ASCISTARCustom.Cost.DAC.Projections;
using ASCISTARCustom.Cost.DAC.Unbounds;
using ASCISTARCustom.Preferences.DAC;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.IN;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ASCISTARCustom.Cost
{
    public class ASCIStarMetalRatesSyncProcessing : PXGraph<ASCIStarMetalRatesSyncProcessing>
    {
        #region Constants
        private const string LondonPM = "LONDON PM";
        private const string LondonAM = "LONDON AM";
        private const string NewYork = "NEW YORK";
        private const string Gold24K = "24K";
        private const string Silver = "SSS";
        #endregion

        #region DataViews
        public PXFilter<ASCIStarMarketVendorFilter> Filter;
        public PXProcessing<ASCIStarMarketVendor> VandorBasis;
        public PXSetup<ASCIStarSetup> Setup;
        #endregion

        #region Dependency Injection
        [InjectDependency]
        public IASCIStarMetalsAPILatestRateService _apiRates { get; set; }
        #endregion

        #region ctor
        public ASCIStarMetalRatesSyncProcessing()
        {
            var graph = this;
            VandorBasis.SetProcessDelegate((List<ASCIStarMarketVendor> selectedRecors) =>
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
            AppendFilters<ASCIStarMarketVendor>(VandorBasis, Filter.Current);
            PXView view = new PXView(this, false, VandorBasis.View.BqlSelect);
            int startRow = PXView.StartRow;
            int num = 0;
            IEnumerable results = view.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref num);
            PXView.StartRow = 0;
            return results;
        }
        #endregion

        #region Actions
        public PXCancel<ASCIStarMarketVendor> Cancel;
        #endregion

        #region ServiceMethods
        private static void Processing(List<ASCIStarMarketVendor> selectedRecords, Dictionary<int, PXSetPropertyException> listMessages, ASCIStarMetalRatesSyncProcessing graph)
        {
            var vendors = graph.GetVendorByBAccuntID(graph);
            var inventoryItems = graph.GetInventoryItemByID(graph);
            var vendorPrices = graph.GetAPVendorPrice(graph);
            var vendorPriceMaint = new Lazy<APVendorPriceMaint>(() => PXGraph.CreateInstance<APVendorPriceMaint>());

            foreach (var record in selectedRecords)
            {
                try
                {
                    var vendor = vendors.Select(record.VendorID).RowCast<Vendor>().FirstOrDefault();
                    var item = inventoryItems.Select(record.InventoryID).RowCast<InventoryItem>().FirstOrDefault();
                    var vendorPrice = vendorPrices.Select(record.VendorID, record.InventoryID, graph.Accessinfo.BusinessDate, graph.Accessinfo.BusinessDate).RowCast<APVendorPrice>().FirstOrDefault();

                    graph.CreateOrUpdatePriceRecord(graph, vendorPriceMaint, record, vendor, item, vendorPrice);
                }
                catch (Exception exc)
                {
                    // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                    // Acuminator disable once PX1051 NonLocalizableString [Justification]
                    listMessages[selectedRecords.IndexOf(record)] = new PXSetPropertyException(exc.Message, PXErrorLevel.RowError);
                }
            }
        }

        public virtual void CreateOrUpdatePriceRecord(ASCIStarMetalRatesSyncProcessing graph, Lazy<APVendorPriceMaint> vendorPriceMaint, ASCIStarMarketVendor record, Vendor vendor, InventoryItem item, APVendorPrice vendorPrice)
        {
            string trimmedAcctCD = vendor.AcctCD.Trim();

            if (trimmedAcctCD == LondonPM)
            {
                SetLondonPMVendorPrice(graph, vendorPriceMaint, record, item, vendorPrice);
            }
            else if (trimmedAcctCD == LondonAM)
            {
                SetLondonAMVendorPrice(graph, vendorPriceMaint, record, item, vendorPrice);
            }
            else if (trimmedAcctCD == NewYork)
            {
                SetNewYorkVendorPrice(graph, vendorPriceMaint, record, item, vendorPrice);
            }
        }

        public virtual void SetNewYorkVendorPrice(ASCIStarMetalRatesSyncProcessing graph, Lazy<APVendorPriceMaint> vendorPriceMaint, ASCIStarMarketVendor record, InventoryItem item, APVendorPrice vendorPrice)
        {
            switch (item.InventoryCD.Trim())
            {
                case Gold24K:
                    {
                        var salesPrice = graph._apiRates.GetXAURate(record.CuryID);
                        var _ = vendorPrice == null ? CreateRecord(vendorPriceMaint, record, salesPrice) : UpdateRecord(vendorPriceMaint, vendorPrice, salesPrice);
                    }
                    break;
                case Silver:
                    {
                        var salesPrice = graph._apiRates.GetXAGRate(record.CuryID);
                        var _ = vendorPrice == null ? CreateRecord(vendorPriceMaint, record, salesPrice) : UpdateRecord(vendorPriceMaint, vendorPrice, salesPrice);
                    }
                    break;
                default:
                    break;
            }
        }

        public virtual void SetLondonAMVendorPrice(ASCIStarMetalRatesSyncProcessing graph, Lazy<APVendorPriceMaint> vendorPriceMaint, ASCIStarMarketVendor record, InventoryItem item, APVendorPrice vendorPrice)
        {
            switch (item.InventoryCD.Trim())
            {
                case Gold24K:
                    {
                        var salesPrice = graph._apiRates.GetLBXAUAMRate(record.CuryID);
                        var _ = vendorPrice == null ? CreateRecord(vendorPriceMaint, record, salesPrice) : UpdateRecord(vendorPriceMaint, vendorPrice, salesPrice);
                    }
                    break;
                case Silver:
                    {
                        var salesPrice = graph._apiRates.GetLBXAGRate(record.CuryID);
                        var _ = vendorPrice == null ? CreateRecord(vendorPriceMaint, record, salesPrice) : UpdateRecord(vendorPriceMaint, vendorPrice, salesPrice);
                    }
                    break;
                default:
                    break;
            }
        }

        public virtual void SetLondonPMVendorPrice(ASCIStarMetalRatesSyncProcessing graph, Lazy<APVendorPriceMaint> vendorPriceMaint, ASCIStarMarketVendor record, InventoryItem item, APVendorPrice vendorPrice)
        {
            switch (item.InventoryCD.Trim())
            {
                case Gold24K:
                    {
                        var salesPrice = graph._apiRates.GetLBXAUPMRate(record.CuryID);
                        var _ = vendorPrice == null ? CreateRecord(vendorPriceMaint, record, salesPrice) : UpdateRecord(vendorPriceMaint, vendorPrice, salesPrice);
                    }
                    break;
                case Silver:
                    {
                        var salesPrice = graph._apiRates.GetLBXAGRate(record.CuryID);
                        var _ = vendorPrice == null ? CreateRecord(vendorPriceMaint, record, salesPrice) : UpdateRecord(vendorPriceMaint, vendorPrice, salesPrice);
                    }
                    break;
                default:
                    break;
            }
        }

        public virtual APVendorPrice UpdateRecord(Lazy<APVendorPriceMaint> vendorPriceMaint, APVendorPrice record, decimal salesPrice)
        {
            APVendorPrice vendorPrice = null;
            vendorPriceMaint.Value.Clear(PXClearOption.ClearAll);
            using (var transactionScope = new PXTransactionScope())
            {
                record.SalesPrice = salesPrice;
                vendorPrice = vendorPriceMaint.Value.Records.Update(record);
                vendorPriceMaint.Value.Save.Press();
                transactionScope.Complete();
                return vendorPrice;
            }
        }

        public virtual APVendorPrice CreateRecord(Lazy<APVendorPriceMaint> vendorPriceMaint, ASCIStarMarketVendor record, decimal salesPrice)
        {
            vendorPriceMaint.Value.Clear(PXClearOption.ClearAll);
            using (var transactionScope = new PXTransactionScope())
            {
                var vendorPrice = new APVendorPrice();
                vendorPrice = vendorPriceMaint.Value.Records.Insert(vendorPrice);
                vendorPrice.VendorID = record.VendorID;
                vendorPrice.InventoryID = record.InventoryID;
                vendorPrice.SalesPrice = salesPrice;
                vendorPrice.UOM = record.UOM;
                vendorPrice.EffectiveDate = vendorPriceMaint.Value.Accessinfo.BusinessDate;
                vendorPrice.ExpirationDate = vendorPriceMaint.Value.Accessinfo.BusinessDate;

                var vendorPriceExt = PXCache<APVendorPrice>.GetExtension<ASCIStarAPVendorPriceExt>(vendorPrice);
                vendorPriceExt.UsrFormAPI = true;

                vendorPriceMaint.Value.Records.Update(vendorPrice);
                vendorPriceMaint.Value.Save.PressButton();
                transactionScope.Complete();
                return vendorPrice;
            }
        }

        private void AppendFilters<T>(PXSelectBase<T> cmd, ASCIStarMarketVendorFilter filter) where T : class, IBqlTable, new()
        {
            if (filter.VendorID != null)
            {
                cmd.WhereAnd<Where<ASCIStarMarketVendor.vendorID, Equal<Current<ASCIStarMarketVendorFilter.vendorID>>>>();
            }
            if (filter.ItemClassCD != null)
            {
                cmd.WhereAnd<Where<ASCIStarMarketVendor.itemClassCD, Equal<Current<ASCIStarMarketVendorFilter.itemClassCD>>>>();
            }
            if (filter.InventoryID != null)
            {
                cmd.WhereAnd<Where<ASCIStarMarketVendor.inventoryID, Equal<Current<ASCIStarMarketVendorFilter.inventoryID>>>>();
            }
        }
        #endregion

        #region ServiceQueries
        public PXSelectBase<Vendor> GetVendorByBAccuntID(PXGraph graph)
        {
            return new PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>(graph);
        }
        public PXSelectBase<InventoryItem> GetInventoryItemByID(PXGraph graph)
        {
            return new PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>(graph);
        }
        public PXSelectBase<APVendorPrice> GetAPVendorPrice(PXGraph graph)
        {
            return new PXSelect<APVendorPrice, 
                Where<APVendorPrice.vendorID, Equal<Required<APVendorPrice.vendorID>>, 
                    And<APVendorPrice.inventoryID, Equal<Required<APVendorPrice.inventoryID>>, 
                    And<APVendorPrice.effectiveDate, Equal<Required<APVendorPrice.effectiveDate>>, 
                    And<APVendorPrice.expirationDate, Equal<Required<APVendorPrice.expirationDate>>>>>>>(graph);
        }
        #endregion
    }
}
