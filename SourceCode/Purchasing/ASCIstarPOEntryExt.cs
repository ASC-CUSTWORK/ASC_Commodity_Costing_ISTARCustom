using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.AP;
using PX.Objects.Common;
using PX.Objects.PO;
using ASCISTARCustom.Cost.Descriptor;
using ASCISTARCustom.Common.Builder;
using PX.Data.BQL;

namespace ASCISTARCustom
{
    public class ASCIStarPOOrderEntryExt : PXGraphExtension<POOrderEntry>
    {
        public static bool IsActive() => true;

        #region DataViews
        PXSelect<INKitSpecHdr, Where<INKitSpecHdr.kitInventoryID, Equal<Required<INKitSpecHdr.kitInventoryID>>>> CostItem;

        PXSelect<POVendorInventory,
            Where<POVendorInventory.vendorID, Equal<Current<POOrder.vendorID>>,
                And<POVendorInventory.inventoryID, Equal<Current<POLine.inventoryID>>>>> vendorItemSelect;

        PXSelect<APVendorPrice,
           Where<APVendorPrice.vendorID, Equal<Required<APVendorPrice.vendorID>>,
               And<APVendorPrice.inventoryID, Equal<Required<APVendorPrice.inventoryID>>,
                   And<APVendorPrice.effectiveDate, Equal<Required<APVendorPrice.effectiveDate>>>>>> vendorPrice;

        #endregion

        #region Actions
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Email Purchase Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual IEnumerable EmailPurchaseOrder(PXAdapter adapter, [PXString] string notificationCD = null)
        {
            bool massProcess = adapter.MassProcess;
            // Acuminator disable once PX1008 LongOperationDelegateSynchronousExecution [Justification]
            // TODO: DEV NOTE: This long operation should be modifyed to fit Acumatica standarts 
            PXLongOperation.StartOperation(Base.UID, () =>
            {
                bool flag = false;
                foreach (POOrder currentItem in adapter.Get<POOrder>())
                {
                    if (massProcess)
                        PXProcessing<POOrder>.SetCurrentItem((object)currentItem);
                    try
                    {
                        Base.Document.Cache.Current = (object)currentItem;
                        Dictionary<string, string> parameters = new Dictionary<string, string>();
                        parameters["POOrder.OrderType"] = currentItem.OrderType;
                        parameters["POOrder.OrderNbr"] = currentItem.OrderNbr;
                        using (PXTransactionScope transactionScope = new PXTransactionScope())
                        {
                            var list = PXNoteAttribute.GetFileNotes(Base.Document.Cache, Base.Document.Current).Select(attachment => (Guid?)attachment).ToList();
                            ASCIStarPOOrderExt currentExt = currentItem.GetExtension<ASCIStarPOOrderExt>();
                            NotificationSetup ns = NotificationSetup.PK.Find(Base, currentExt.UsrSetupID);
                            Base.Activity.SendNotification(
                                "Vendor",
                                (ns.NotificationCD ?? "PURCHASE ORDER"),
                                currentItem.BranchID,
                                parameters,
                                list);
                            Base.Save.Press();
                            transactionScope.Complete();
                        }
                        if (massProcess)
                            PXProcessing<POOrder>.SetProcessed();
                    }
                    catch (Exception ex)
                    {
                        if (!massProcess)
                        {
                            throw;
                        }

                        Base.Document.Cache.SetStatus((object)currentItem, PXEntryStatus.Notchanged);
                        Base.Document.Cache.Remove((object)currentItem);
                        PXProcessing<POOrder>.SetError(ex);
                        flag = true;
                    }
                }
                if (flag)
                    throw new PXOperationCompletedWithErrorException("At least one item has not been processed.");
            });
            return adapter.Get<POOrder>();
        }
        #endregion

        #region Event Handlers

        protected virtual void _(Events.FieldVerifying<POOrder, ASCIStarPOOrderExt.usrPricingDate> e)
        {
            var row = e.Row;
            if (row == null) return;

            if ((DateTime?)e.NewValue > DateTime.Today || e.NewValue == null) throw new PXSetPropertyException<ASCIStarPOOrderExt.usrPricingDate>("Pricing date can not be from future or empty!");
        }

        protected virtual void _(Events.FieldUpdated<POOrder, POOrder.vendorID> e)
        {
            var row = e.Row;
            if (row == null) return;

            Vendor vendor = Vendor.PK.Find(Base, row.VendorID);
            if (vendor == null) return;

            e.Cache.SetValueExt<ASCIStarPOOrderExt.usrMarketID>(row, vendor.GetExtension<ASCIStarVendorExt>().UsrMarketID);
        }

        protected virtual void _(Events.FieldUpdated<POOrder, POOrder.orderDate> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetNewUnitCostOnPOLines(this.Base.Transactions.Cache, this.Base.Transactions.Select()?.FirstTableItems.ToList());
        }

        protected virtual void _(Events.FieldUpdated<POOrder, ASCIStarPOOrderExt.usrPricingDate> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetNewUnitCostOnPOLines(this.Base.Transactions.Cache, this.Base.Transactions.Select()?.FirstTableItems.ToList());
        }

        protected virtual void _(Events.FieldUpdated<POLine, POLine.inventoryID> e, PXFieldUpdated baseEvent)
        {
            baseEvent(e.Cache, e.Args);

            var row = e.Row;
            if (row == null || row.InventoryID == null) return;

            SetNewUnitCostOnPOLines(e.Cache, new List<POLine>() { row });
        }

        protected virtual void _(Events.FieldUpdated<POLine, POLine.orderQty> e)
        {
            var row = e.Row;
            if (row == null || row.InventoryID == null) return;

            SetNewUnitCostOnPOLines(e.Cache, new List<POLine>() { row });
        }
        #endregion


        #region Helper Methods
        private void SetNewUnitCostOnPOLines(PXCache cache, List<POLine> poLineList)
        {
            if (this.Base.Document.Current == null || this.Base.Document.Current.VendorID == null) return;

            var poOrderExt = PXCache<POOrder>.GetExtension<ASCIStarPOOrderExt>(this.Base.Document.Current);

            if (poOrderExt == null || poOrderExt.UsrMarketID == null)
            {
                this.Base.Document.Cache.RaiseExceptionHandling<ASCIStarPOOrderExt.usrMarketID>(this.Base.Document.Current, null,
                    new PXSetPropertyException<ASCIStarPOOrderExt.usrMarketID>("Select Market first."));
                return;
            }

            foreach (var poLine in poLineList)
            {
                var inventoryItem = InventoryItem.PK.Find(Base, poLine.InventoryID);
                var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(inventoryItem);

                var poVendorInventory = SelectFrom<POVendorInventory>
                    .Where<POVendorInventory.vendorID.IsEqual<P.AsInt>
                    .And<POVendorInventory.inventoryID.IsEqual<P.AsInt>>>.View.Select(this.Base, this.Base.Document.Current.VendorID, poLine.InventoryID)?.TopFirst;

                var jewelryCostProvider = new ASCIStarCostBuilder(this.Base)
                                .WithInventoryItem(inventoryItem)
                                .WithPOVendorInventory(poVendorInventory)
                                .WithPricingData(poOrderExt.UsrPricingDate ?? PXTimeZoneInfo.Today)
                                .Build();

                poLine.CuryUnitCost = jewelryCostProvider.GetPurchaseUnitCost( 
                    inventoryItemExt?.UsrCostingType == ASCIStarCostingType.StandardCost ? ASCIStarCostingType.StandardCost : ASCIStarCostingType.MarketCost);

                cache.SetValueExt<POLine.curyUnitCost>(poLine, poLine.CuryUnitCost);

                cache.SetValueExt<ASCIStarPOLineExt.usrMarketPrice>(poLine, jewelryCostProvider.PreciousMetalMarketCostPerTOZ);

                SetInventoryItemCustomFields(cache, poLine, jewelryCostProvider);
            }
        }

        private void SetInventoryItemCustomFields(PXCache cache, POLine row, ASCIStarCostBuilder costBuilder)
        {
            cache.SetValueExt<ASCIStarPOLineExt.usrContractIncrement>(row, costBuilder.ItemCostSpecification.Increment);
            cache.SetValueExt<ASCIStarPOLineExt.usrActualGRAMGold>(row, costBuilder.ItemCostSpecification.GoldGrams);
            cache.SetValueExt<ASCIStarPOLineExt.usrPricingGRAMGold>(row, costBuilder.ItemCostSpecification.FineGoldGrams);
            cache.SetValueExt<ASCIStarPOLineExt.usrActualGRAMSilver>(row, costBuilder.ItemCostSpecification.SilverGrams);
            cache.SetValueExt<ASCIStarPOLineExt.usrPricingGRAMSilver>(row, costBuilder.ItemCostSpecification.FineSilverGrams);
        }

        #endregion
    }
}
