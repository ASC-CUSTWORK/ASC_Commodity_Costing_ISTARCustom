using ASCISTARCustom.Inventory.DAC;
using ASCISTARCustom.Purchasing.DAC;
using ASCISTARCustom.Purchasing.Helpers;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.PO;
using PX.Objects.PO.LandedCosts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Messages = PX.Objects.PO.Messages;

namespace ASCISTARCustom.Purchasing
{
    public class ASCIStarPOReceiptEntryExt : PXGraphExtension<POReceiptEntry>
    {
        public static bool IsActive() => true;

        [PXOverride]
        public virtual IEnumerable Release(PXAdapter adapter, Func<PXAdapter, IEnumerable> baseMethod)
        {
            PXCache cache = this.Base.Document.Cache;
            List<POReceipt> list = new List<POReceipt>();
            foreach (POReceipt indoc in adapter.Get<POReceipt>())
            {
                if (indoc.Hold == false && indoc.Released == false)
                {
                    cache.Update(indoc);
                    list.Add(indoc);
                }
            }

            if (list.Count == 0)
            {
                throw new PXException(Messages.Document_Status_Invalid);
            }
            this.Base.Save.Press();
            PXLongOperation.StartOperation(this.Base, delegate ()
            {
                POReleaseReceipt.ReleaseDoc(list, false);


                CreateLandedCost(list);
            });
            return list;
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
        public virtual void CreateLandedCost(List<POReceipt> poReceiptList)
        {
            var graph = PXGraph.CreateInstance<POLandedCostDocEntry>();
            foreach (POReceipt poReceipt in poReceiptList)
            {
                graph.Document.Current = graph.Document.Insert(
                    new POLandedCostDoc()
                    {
                        VendorID = poReceipt.VendorID,
                        VendorLocationID = poReceipt.VendorLocationID
                    });

                var landedCostAmount = GetCostAmountValue(poReceipt);

                graph.Details.Current = graph.Details.Insert(
                    new POLandedCostDetail()
                    {
                        LandedCostCodeID = ASCIStarPOMessages.Constants.LandedCostCode,
                        CuryLineAmt = landedCostAmount
                    });

                var receiptLinesAdd = GetPOReceiptLineAddList(poReceipt.ReceiptType, poReceipt.ReceiptNbr);

                graph.AddPurchaseReceiptLines(receiptLinesAdd);

                graph.Save.Press();
            }
            graph.Clear();
        }

        private decimal? GetCostAmountValue(POReceipt poReceipt)
        {
            decimal? amountValue = decimal.Zero;

            var poReceiptLines = this.Base.transactions.Select()?.FirstTableItems.ToList();
            foreach (var line in poReceiptLines)
            {
                var vendorDuty = GetVendorDuty(line.InventoryID, poReceipt.VendorID);
                amountValue += vendorDuty?.DutyPct / 100.00m * line.CuryExtCost;
            }
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
