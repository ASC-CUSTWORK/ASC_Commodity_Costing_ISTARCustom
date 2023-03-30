using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.AP;
using PX.Objects.Common;
using PX.Objects.PO;

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

        PXSelect<InventoryItemCurySettings,
            Where<InventoryItemCurySettings.inventoryID, Equal<Required<InventoryItemCurySettings.inventoryID>>>> itemCurySettings;

        PXSelect<InventoryItem,
            Where<InventoryItem.inventoryID, Equal<Required<InventoryItemCurySettings.inventoryID>>>> inventoryItem;
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
        protected virtual void _(Events.FieldUpdated<POLine, ASCIStarPOLineExt.usrCostingType> args)
        {
            if (args.Row is POLine row)
            {
                args.Cache.RaiseFieldDefaulting<POLine.curyUnitCost>(row, out object value);
                args.Cache.SetValueExt<POLine.curyUnitCost>(row, value);
            }
        }
        protected virtual void POLine_CuryUnitCost_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e, PXFieldDefaulting InvokeBaseHandler)
        {
            InvokeBaseHandler?.Invoke(sender, e);
            if (e.Row is POLine row)
            {
                var doc = Base.Document.Current;
                var docExt = PXCache<POOrder>.GetExtension<ASCIStarPOOrderExt>(doc);
                var rowExt = PXCache<POLine>.GetExtension<ASCIStarPOLineExt>(row);
                var inventoryItem = PXSelectorAttribute.Select<InventoryItem.inventoryID>(sender, row, row.InventoryID) as InventoryItem;
                if (inventoryItem != null)
                {
                    var costHelper = new ASCIStarMarketCostHelper.JewelryCost(Base, inventoryItem, 0m, 0m, doc.VendorID, docExt.UsrMarketID, docExt.UsrEstArrivalDate, row.UOM, doc.CuryID);
                    e.NewValue = CuryUnitCost(rowExt, costHelper);
                }
            }
        }
        #endregion

        #region Service Methods
        private decimal CuryUnitCost(ASCIStarPOLineExt rowExt, ASCIStarMarketCostHelper.JewelryCost costHelper)
        {
            return rowExt.UsrCostingType == CostingType.MarketCost ? costHelper.GetMarketCost() : 
                   rowExt.UsrCostingType == CostingType.ContractCost ? costHelper.GetContractCost() : 
                   rowExt.UsrCostingType == CostingType.WeightCost ? costHelper.GetWeightCost() : 0m;
        }
        #endregion
    }
}