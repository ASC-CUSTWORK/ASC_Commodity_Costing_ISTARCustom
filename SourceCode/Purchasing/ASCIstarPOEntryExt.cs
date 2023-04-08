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

        public SelectFrom<InventoryItem>.Where<InventoryItem.inventoryID.IsEqual<POLine.inventoryID.FromCurrent>>.View InventoryItemView;
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

        protected virtual void _(Events.FieldUpdated<POLine, POLine.orderQty> e)
        {
            var row = e.Row;
            if (row == null || row.InventoryID == null) return;
            InventoryItemView.Current = InventoryItemView.Select().TopFirst;
            if (InventoryItemView.Current == null)
                InventoryItemView.Current = InventoryItem.PK.Find(this.Base, row.InventoryID);
            if (InventoryItemView.Current == null) return;

            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(InventoryItemView.Current);
            if (inventoryItemExt?.UsrCostingType == ASCIStarCostingType.StandardCost) return;

            SetNewCosts(e.Cache, row, InventoryItemView.Current, inventoryItemExt);
        }

        protected virtual void _(Events.FieldUpdated<POOrder, POOrder.orderDate> e)
        {
            var row = e.Row;
            if (row == null ) return;
            InventoryItemView.Current = InventoryItemView.Select().TopFirst;
            if (InventoryItemView.Current == null)
                InventoryItemView.Current = InventoryItem.PK.Find(this.Base, this.Base.Transactions.Current?.InventoryID);
            if (InventoryItemView.Current == null) return;

            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(InventoryItemView.Current);
            if (inventoryItemExt?.UsrCostingType == ASCIStarCostingType.StandardCost) return;

            SetNewCosts(e.Cache, this.Base.Transactions.Current, InventoryItemView.Current, inventoryItemExt);
        }

        protected virtual void _(Events.FieldUpdated<POOrder, ASCIStarPOOrderExt.usrPricingDate> e)
        {
            var row = e.Row;
            if (row == null) return;
            InventoryItemView.Current = InventoryItemView.Select().TopFirst;
            if (InventoryItemView.Current == null)
                InventoryItemView.Current = InventoryItem.PK.Find(this.Base, this.Base.Transactions.Current?.InventoryID);
            if (InventoryItemView.Current == null) return;

            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(InventoryItemView.Current);
            if (inventoryItemExt?.UsrCostingType == ASCIStarCostingType.StandardCost) return;

            SetNewCosts(e.Cache, this.Base.Transactions.Current, InventoryItemView.Current, inventoryItemExt);
        }

        protected virtual void _(Events.FieldUpdated<POLine, POLine.inventoryID> e, PXFieldUpdated baseEvent)
        {
            baseEvent(e.Cache, e.Args);

            var row = e.Row;
            if (row == null || row.InventoryID == null) return;

            InventoryItemView.Current = InventoryItemView.Select().TopFirst;
            if (InventoryItemView.Current == null)
                InventoryItemView.Current = InventoryItem.PK.Find(this.Base, row.InventoryID);
            if (InventoryItemView.Current == null) return;

            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(InventoryItemView.Current);
            if (inventoryItemExt?.UsrCostingType == ASCIStarCostingType.StandardCost) return;

            SetNewCosts(e.Cache, row, InventoryItemView.Current, inventoryItemExt);
            SetInventoryItemCustomFields(e.Cache, row, inventoryItemExt);
        }

        #endregion


        #region Helper Methods
        private void SetNewCosts(PXCache cache, POLine row, InventoryItem inventoryItem, ASCIStarINInventoryItemExt inventoryItemExt)
        {
            if (this.Base.Document.Current == null || this.Base.Document.Current.VendorID == null) return;

            var poOrderExt = PXCache<POOrder>.GetExtension<ASCIStarPOOrderExt>(this.Base.Document.Current);

            if (poOrderExt == null || poOrderExt.UsrMarketID == null)
            {
                this.Base.Document.Cache.RaiseExceptionHandling<ASCIStarPOOrderExt.usrMarketID>(this.Base.Document.Current, null, new PXSetPropertyException<ASCIStarPOOrderExt.usrMarketID>("Select Market first."));
                return;
            }
            var costProvider = new ASCIStarMarketCostProvider.JewelryCost(Base, inventoryItem, 0m, 0m,
                this.Base.Document.Current.VendorID, poOrderExt.UsrMarketID, poOrderExt.UsrPricingDate, row.UOM, this.Base.Document.Current.CuryID);

            row.CuryUnitCost = costProvider.GetPurchaseCost(inventoryItemExt.UsrCostingType);
            cache.SetValueExt<POLine.curyUnitCost>(row, row.CuryUnitCost);

            decimal? marketPrice = decimal.Zero;
            if (costProvider.CostBasisGold?.GoldBasis == null)
                marketPrice = costProvider.CostBasisGold.SilverBasis?.EffectiveMarketPerOz;
            else
                marketPrice = costProvider.CostBasisGold.GoldBasis.EffectiveMarketPerOz;

            cache.SetValueExt<ASCIStarPOLineExt.usrMarketPrice>(row, marketPrice);
        }

        private void SetInventoryItemCustomFields(PXCache cache, POLine row, ASCIStarINInventoryItemExt inventoryItemExt)
        {
            cache.SetValueExt<ASCIStarPOLineExt.usrContractIncrement>(row, inventoryItemExt.UsrContractIncrement);
            cache.SetValueExt<ASCIStarPOLineExt.usrActualGRAMGold>(row, inventoryItemExt.UsrActualGRAMGold);
            cache.SetValueExt<ASCIStarPOLineExt.usrPricingGRAMGold>(row, inventoryItemExt.UsrPricingGRAMGold);
            cache.SetValueExt<ASCIStarPOLineExt.usrActualGRAMSilver>(row, inventoryItemExt.UsrActualGRAMSilver);
            cache.SetValueExt<ASCIStarPOLineExt.usrPricingGRAMSilver>(row, inventoryItemExt.UsrPricingGRAMSilver);
        }

        #endregion
    }
}
