using ASCISTARCustom.IN.DAC;
using ASCISTARCustom.PO.DAC;
using ASCISTARCustom.PO.Helpers;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.PO.LandedCosts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASCISTARCustom.PO
{
    public class ASCIStarPOReceiptEntryExt : PXGraphExtension<POReceiptEntry>
    {
        public static bool IsActive() => true;
        [PXOverride]
        public virtual void ReleaseReceipt(
            INReceiptEntry docgraph, PX.Objects.AP.APInvoiceEntry invoiceGraph, POReceipt aDoc, DocumentList<INRegister> aINCreated, DocumentList<PX.Objects.AP.APInvoice> aAPCreated, bool aIsMassProcess,
            Action<INReceiptEntry, PX.Objects.AP.APInvoiceEntry, POReceipt, DocumentList<INRegister>, DocumentList<PX.Objects.AP.APInvoice>, bool> baseMethod)
        {
            baseMethod(docgraph, invoiceGraph, aDoc, aINCreated, aAPCreated, aIsMassProcess);

            CreateLandedCost(aDoc);
        }

        #region Events
        protected virtual void _(Events.FieldVerifying<POReceipt, ASCIStarPOReceiptExt.usrAccrualLandedCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            bool? newValue = (bool?)e.NewValue;
            if (newValue != true) return;

            Vendor vendor = Vendor.PK.Find(this.Base, row.VendorID);
            if (vendor?.LandedCostVendor != true)
            {
                e.Cache.RaiseExceptionHandling<ASCIStarPOReceiptExt.usrAccrualLandedCost>(row, e.NewValue,
                                            new PXSetPropertyException(ASCIStarPOMessages.Warnings.DisabledLandedCostVendor, PXErrorLevel.Warning));
            }
        }

        #endregion

        #region Helper Methods
        public virtual void CreateLandedCost(POReceipt poReceipt)
        {
            if (poReceipt.GetExtension<ASCIStarPOReceiptExt>().UsrAccrualLandedCost == null || poReceipt.GetExtension<ASCIStarPOReceiptExt>().UsrAccrualLandedCost == false)
            {
                return;
            }

            var graph = PXGraph.CreateInstance<POLandedCostDocEntry>();
            var customDuty = SelectFrom<LandedCostCode>.Where<LandedCostCode.landedCostCodeID.IsEqual<@P.AsString>>.View.Select(this.Base, ASCIStarPOMessages.Constants.LandedCostCode).TopFirst;
            var freitnin = SelectFrom<LandedCostCode>.Where<LandedCostCode.landedCostCodeID.IsEqual<@P.AsString>>.View.Select(this.Base, ASCIStarPOMessages.Constants.LandedCostCodeFreitnin).TopFirst;
            var poReceiptLines = this.Base.transactions.Select()?.FirstTableItems.ToList();


            try
            {
               graph.Document.Current = graph.Document.Insert(
               new POLandedCostDoc()
               {
                   VendorID = customDuty.VendorID,
                   VendorLocationID = customDuty.VendorLocationID
               });
            }
            catch (PXFieldValueProcessingException ex)
            {

                if (ex.Message.Contains(PX.Objects.PO.Messages.VendorIsNotLandedCostVendor))
                    return;
            }

            foreach(var line in poReceiptLines)
            {
                var landedCostAmount = GetCostAmountValue(line);
                graph.Details.Current = graph.Details.Insert(
                new POLandedCostDetail()
                {
                    LandedCostCodeID = ASCIStarPOMessages.Constants.LandedCostCode,
                    CuryLineAmt = landedCostAmount
                });
            }

            var receiptLinesAdd = GetPOReceiptLineAddList(poReceipt.ReceiptType, poReceipt.ReceiptNbr);

            graph.AddPurchaseReceiptLines(receiptLinesAdd);

            graph.Save.Press();
            graph.Clear();

            try
            {
                graph.Document.Current = graph.Document.Insert(
                new POLandedCostDoc()
                {
                    VendorID = freitnin.VendorID,
                    VendorLocationID = freitnin.VendorLocationID
                });
            }
            catch (PXFieldValueProcessingException ex)
            {

                if (ex.Message.Contains(PX.Objects.PO.Messages.VendorIsNotLandedCostVendor))
                    return;
            }

            foreach(var line in poReceiptLines)
            {
                var landedCostAmount = GetFreintninAmountValue(line);


                graph.Details.Current = graph.Details.Insert(
                    new POLandedCostDetail()
                    {
                        LandedCostCodeID = ASCIStarPOMessages.Constants.LandedCostCodeFreitnin,
                        CuryLineAmt = landedCostAmount
                    });
            }
            receiptLinesAdd = GetPOReceiptLineAddList(poReceipt.ReceiptType, poReceipt.ReceiptNbr);
            graph.AddPurchaseReceiptLines(receiptLinesAdd);


            graph.AddPurchaseReceiptLines(receiptLinesAdd);
            graph.Save.Press();
            graph.Clear();
        }

        private decimal? GetCostAmountValue(POReceiptLine poReceiptLine)
        {
            decimal? amountValue = decimal.Zero;

            //var poReceiptLines = this.Base.transactions.Select()?.FirstTableItems.ToList();
            //foreach (var line in poReceiptLines)
            //{

            var inventoryItem = SelectFrom<InventoryItem>.Where<InventoryItem.inventoryID.IsEqual<@P.AsInt>>.View.Select(Base, poReceiptLine.InventoryID).TopFirst;
            //var vendorDuty = GetVendorDuty(poReceiptLine.InventoryID, poReceiptLine.VendorID);

            var vendor = SelectFrom<Address>.Where<Address.bAccountID.IsEqual<@P.AsInt>>.View.Select(this.Base, poReceiptLine.VendorID).TopFirst;
            var landedCostDuty = SelectFrom<ASCiStarDutyFreight>.Where<ASCiStarDutyFreight.hSTariffCode.IsEqual<@P.AsString>.And<ASCiStarDutyFreight.countryCode.IsEqual<@P.AsString>>>.OrderBy<ASCiStarDutyFreight.effectiveDate.Desc>.View.Select(this.Base, inventoryItem.HSTariffCode, vendor.CountryID).TopFirst;
                
            amountValue += landedCostDuty?.DutyPercent / 100.00m * poReceiptLine.CuryExtCost;
            //}
            return amountValue;
        }

        private decimal? GetFreintninAmountValue(POReceiptLine poReceiptLine)
        {
            decimal? amountValue = decimal.Zero;
            //var poReceiptLines = this.Base.transactions.Select()?.FirstTableItems.ToList();

            //foreach (var line in poReceiptLines)
            //{
                //var vendorDuty = GetVendorDuty(line.InventoryID, poReceipt.VendorID);
                //var vendor = SelectFrom<Address>.Where<Address.bAccountID.IsEqual<@P.AsInt>>.View.Select(this.Base, line.VendorID).TopFirst;
                //var landedCostDuty = SelectFrom<ASCiStarDutyFreight>.Where<ASCiStarDutyFreight.hSTariffCode.IsEqual<@P.AsString>.And<ASCiStarDutyFreight.countryCode.IsEqual<@P.AsString>>>.View.Select(this.Base, vendorDuty.HSTariffCode, vendor.CountryID).TopFirst;
                //amountValue += landedCostDuty?.DutyPercent / 100.00m * line.CuryExtCost;

            var vendorDuty = SelectFrom<ASCiStarVendorDuty>.Where<ASCiStarVendorDuty.branch.IsEqual<@P.AsInt>.And<ASCiStarVendorDuty.uOM.IsEqual<@P.AsString>>>.OrderBy<ASCiStarVendorDuty.effectiveDate.Desc>.View.Select(this.Base, poReceiptLine.BranchID, poReceiptLine.UOM).TopFirst;

            amountValue += vendorDuty?.Freight * poReceiptLine.Qty;
            //}
            return amountValue;

        }

        private IEnumerable<POReceiptLineAdd> GetPOReceiptLineAddList(string receiptType, string receiptNbr) => SelectFrom<POReceiptLineAdd>
                                         .Where<POReceiptLineAdd.receiptType.IsEqual<P.AsString>
                                         .And<POReceiptLineAdd.receiptNbr.IsEqual<P.AsString>>>
                                         .View.Select(this.Base, receiptType, receiptNbr)?.FirstTableItems;

        private ASCIStarINVendorDuty GetVendorDuty(int? inventoryID, int? vendorID) => SelectFrom<ASCIStarINVendorDuty>
                                        .Where<ASCIStarINVendorDuty.inventoryID.IsEqual<P.AsInt>
                                        .And<ASCIStarINVendorDuty.vendorID.IsEqual<P.AsInt>>>
                                        .View.Select(this.Base, inventoryID, vendorID)?.TopFirst;

        #endregion
    }
}
