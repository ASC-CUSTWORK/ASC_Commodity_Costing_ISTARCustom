using ASCISTARCustom.Common.Builder;
using ASCISTARCustom.AP.CacheExt;
using ASCISTARCustom.PO.DAC;
using ASCISTARCustom.PO.Helpers;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static ASCISTARCustom.Common.Descriptor.ASCIStarConstants;
using ASCISTARCustom.IN.CacheExt;
using PX.Objects.PO.GraphExtensions.POOrderEntryExt;

namespace ASCISTARCustom.PO
{
    public class ASCIStarPOOrderEntryExt : PXGraphExtension<POOrderEntry>
    {
        public static bool IsActive() => true;

        [PXCopyPasteHiddenView]
        public FbqlSelect<SelectFromBase<InventoryItemCurySettings,
          TypeArrayOf<IFbqlJoin>.Empty>.Where<BqlChainableConditionBase<TypeArrayOf<IBqlBinary>.FilledWith<And<Compare<InventoryItemCurySettings.inventoryID,
              Equal<P.AsInt>>>>>.And<BqlOperand<InventoryItemCurySettings.curyID, IBqlString>.IsEqual<BqlField<AccessInfo.baseCuryID, IBqlString>.AsOptional>>>,
          InventoryItemCurySettings>.View ASCIStarItemCurySettings;

        #region Actions
        public PXAction<POOrder> emailPurchaseOrder;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Email Purchase Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual IEnumerable EmailPurchaseOrder(PXAdapter adapter, [PXString] string notificationCD = null)
        {
            bool massProcess = adapter.MassProcess;

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
                            var baseExt = Base.GetExtension<POOrderEntry_ActivityDetailsExt>();
                            baseExt?.SendNotification("Vendor", (ns.NotificationCD ?? "PURCHASE ORDER"), currentItem.BranchID, parameters, false, list);
                            Base.Save.Press();
                            transactionScope.Complete();
                        }
                        if (massProcess) PXProcessing<POOrder>.SetProcessed();
                    }
                    catch (Exception ex)
                    {
                        if (!massProcess) throw;

                        Base.Document.Cache.SetStatus((object)currentItem, PXEntryStatus.Notchanged);
                        Base.Document.Cache.Remove((object)currentItem);
                        PXProcessing<POOrder>.SetError(ex);
                        flag = true;
                    }
                }
                if (flag) throw new PXOperationCompletedWithErrorException(ASCIStarPOMessages.Errors.ProcessingWithErrorMessages);
            });
            return adapter.Get<POOrder>();
        }

        public PXAction<POOrder> updateUnitCost;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Update Unit Cost", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        public virtual IEnumerable UpdateUnitCost(PXAdapter adapter)
        {
            var poLines = this.Base.Transactions.Select().FirstTableItems.ToList();
            SetNewUnitCostOnPOLines(this.Base.Transactions.Cache, poLines, true);
            this.Base.Document.View.RequestRefresh();
            return adapter.Get<POOrder>();
        }
        #endregion

        #region Event Handlers

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

            SetNewUnitCostOnPOLines(this.Base.Transactions.Cache, this.Base.Transactions.Select()?.FirstTableItems.ToList(), true);
        }

        protected virtual void _(Events.FieldUpdated<POLine, POLine.inventoryID> e, PXFieldUpdated baseEvent)
        {
            var row = e.Row;
            if (row == null || row.InventoryID == null) return;
            baseEvent.Invoke(e.Cache, e.Args);

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
        private void SetNewUnitCostOnPOLines(PXCache cache, List<POLine> poLineList, bool toUpdate = false)
        {
            if (!poLineList.Any() || this.Base.Document.Current?.VendorID == null) return;

            var poOrderExt = PXCache<POOrder>.GetExtension<ASCIStarPOOrderExt>(this.Base.Document.Current);

            if (poOrderExt == null || poOrderExt.UsrMarketID == null)
            {
                this.Base.Document.Cache.RaiseExceptionHandling<ASCIStarPOOrderExt.usrMarketID>(this.Base.Document.Current, null,
                    new PXSetPropertyException<ASCIStarPOOrderExt.usrMarketID>(ASCIStarPOMessages.Errors.MarketEmpty, PXErrorLevel.RowError));
                return;
            }

            foreach (var poLine in poLineList)
            {
                var inventoryItem = InventoryItem.PK.Find(Base, poLine.InventoryID);
                var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(inventoryItem);

                var poVendorInventory = GetDefaultPOVendorInventory(this.Base.Document.Current.VendorID, poLine.InventoryID);
                if (poVendorInventory == null)
                {
                    cache.RaiseExceptionHandling<POLine.inventoryID>(poLine, poLine.InventoryID,
                        new PXSetPropertyException(ASCIStarPOMessages.Warnings.NoDefaultVendorOnItem, PXErrorLevel.RowWarning));
                    continue;
                }

                var jewelryCostProvider = new ASCIStarCostBuilder(this.Base)
                                .WithInventoryItem(inventoryItemExt)
                                .WithPOVendorInventory(poVendorInventory)
                                .WithPricingData(poOrderExt.UsrPricingDate ?? PXTimeZoneInfo.Today)
                                .Build();

                if (jewelryCostProvider != null)
                {
                    var newUnitCost = jewelryCostProvider.GetPurchaseUnitCost(inventoryItemExt?.UsrCostingType == CostingType.StandardCost ? CostingType.StandardCost : CostingType.MarketCost);

                    cache.SetValueExt<POLine.curyUnitCost>(poLine, newUnitCost);
                    cache.SetValueExt<POLine.unitCost>(poLine, newUnitCost);

                    SetInventoryItemCustomFields(cache, poLine, jewelryCostProvider);

                    if (toUpdate) this.Base.Transactions.Update(poLine);
                }
            }
        }

        private void SetInventoryItemCustomFields(PXCache cache, POLine row, ASCIStarCostBuilder costBuilder)
        {
            cache.SetValueExt<ASCIStarPOLineExt.usrContractIncrement>(row, costBuilder.ItemCostSpecification.UsrContractIncrement);
            cache.SetValueExt<ASCIStarPOLineExt.usrActualGRAMGold>(row, costBuilder.ItemCostSpecification.UsrActualGRAMGold);
            cache.SetValueExt<ASCIStarPOLineExt.usrPricingGRAMGold>(row, costBuilder.ItemCostSpecification.UsrPricingGRAMGold);
            cache.SetValueExt<ASCIStarPOLineExt.usrActualGRAMSilver>(row, costBuilder.ItemCostSpecification.UsrActualGRAMSilver);
            cache.SetValueExt<ASCIStarPOLineExt.usrPricingGRAMSilver>(row, costBuilder.ItemCostSpecification.UsrPricingGRAMSilver);
            cache.SetValueExt<ASCIStarPOLineExt.usrMarketPrice>(row, costBuilder.PreciousMetalMarketCostPerTOZ);
            cache.SetValueExt<ASCIStarPOLineExt.usrBasisValue>(row, costBuilder.BasisValue);
        }

        private POVendorInventory GetDefaultPOVendorInventory(int? vendorID, int? inventoryID)
        {
            var inventoryCurySettings = this.ASCIStarItemCurySettings.Select(inventoryID)?.FirstTableItems?.ToList();
            foreach (var line in inventoryCurySettings)
            {
                if (line.PreferredVendorID == null) return null;

                var prefferedVendor = Vendor.PK.Find(Base, line.PreferredVendorID);

                var poVendorInventory = SelectFrom<POVendorInventory>
                    .Where<POVendorInventory.vendorID.IsEqual<P.AsInt>.And<POVendorInventory.inventoryID.IsEqual<P.AsInt>>>
                    .View.Select(Base, prefferedVendor.BAccountID, inventoryID)?.TopFirst;

                return poVendorInventory;
            }
            return null;
        }
        #endregion
    }
}
