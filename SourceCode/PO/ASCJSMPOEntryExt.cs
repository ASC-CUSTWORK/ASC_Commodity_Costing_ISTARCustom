using ASCJSMCustom.Common.Builder;
using ASCJSMCustom.PO.DAC;
using ASCJSMCustom.PO.Helpers;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.Common;
using PX.Objects.IN;
using PX.Objects.PO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static ASCJSMCustom.Common.Descriptor.ASCJSMConstants;
using PX.Objects.PO.GraphExtensions.POOrderEntryExt;
using ASCJSMCustom.Common.Descriptor;
using ASCJSMCustom.IN.CacheExt;

namespace ASCJSMCustom.PO
{
    public class ASCJSMPOOrderEntryExt : PXGraphExtension<POOrderEntry>
    {
        public static bool IsActive() => true;

        [PXCopyPasteHiddenView]
        public FbqlSelect<SelectFromBase<InventoryItemCurySettings,
          TypeArrayOf<IFbqlJoin>.Empty>.Where<BqlChainableConditionBase<TypeArrayOf<IBqlBinary>.FilledWith<And<Compare<InventoryItemCurySettings.inventoryID,
              Equal<P.AsInt>>>>>.And<BqlOperand<InventoryItemCurySettings.curyID, IBqlString>.IsEqual<BqlField<AccessInfo.baseCuryID, IBqlString>.AsOptional>>>,
          InventoryItemCurySettings>.View ASCIStarItemCurySettings;

        protected virtual void _(Events.FieldVerifying<POLine, ASCJSMPOLineExt.usrMarketPrice> e)
        {
            if (e.Row == null) return;
            decimal? newValue = (decimal?)e.NewValue;
            if (newValue == decimal.Zero)
            {
                e.Cache.RaiseExceptionHandling<ASCJSMPOLineExt.usrMarketPrice>(e.Row, newValue,
                    new PXSetPropertyException<ASCJSMPOLineExt.usrMarketPrice>(ASCJSMMessages.Error.MarketPriceNotFound, PXErrorLevel.Warning));
            }
        }


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
                            ASCJSMPOOrderExt currentExt = currentItem.GetExtension<ASCJSMPOOrderExt>();
                            var baseExt = Base.GetExtension<POOrderEntry_ActivityDetailsExt>();
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
                if (flag) throw new PXOperationCompletedWithErrorException(ASCJSMPOMessages.Errors.ProcessingWithErrorMessages);
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

        protected virtual void _(Events.FieldUpdated<POOrder, POOrder.orderDate> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetNewUnitCostOnPOLines(this.Base.Transactions.Cache, this.Base.Transactions.Select()?.FirstTableItems.ToList());
        }

        protected virtual void _(Events.FieldUpdated<POOrder, ASCJSMPOOrderExt.usrPricingDate> e)
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

            var poOrderExt = PXCache<POOrder>.GetExtension<ASCJSMPOOrderExt>(this.Base.Document.Current);

            foreach (var poLine in poLineList)
            {
                var inventoryItem = InventoryItem.PK.Find(Base, poLine.InventoryID);
                var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(inventoryItem);

                var poVendorInventory = GetDefaultPOVendorInventory(this.Base.Document.Current.VendorID, poLine.InventoryID);
                if (poVendorInventory == null)
                {
                    cache.RaiseExceptionHandling<POLine.inventoryID>(poLine, poLine.InventoryID,
                        new PXSetPropertyException(ASCJSMPOMessages.Warnings.NoDefaultVendorOnItem, PXErrorLevel.RowWarning));
                    continue;
                }

                var jewelryCostProvider = new ASCJSMCostBuilder(this.Base)
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

        private void SetInventoryItemCustomFields(PXCache cache, POLine row, ASCJSMCostBuilder costBuilder)
        {
            cache.SetValueExt<ASCJSMPOLineExt.usrContractIncrement>(row, costBuilder.ItemCostSpecification.UsrContractIncrement);
            cache.SetValueExt<ASCJSMPOLineExt.usrActualGRAMGold>(row, costBuilder.ItemCostSpecification.UsrActualGRAMGold);
            cache.SetValueExt<ASCJSMPOLineExt.usrPricingGRAMGold>(row, costBuilder.ItemCostSpecification.UsrPricingGRAMGold);
            cache.SetValueExt<ASCJSMPOLineExt.usrActualGRAMSilver>(row, costBuilder.ItemCostSpecification.UsrActualGRAMSilver);
            cache.SetValueExt<ASCJSMPOLineExt.usrPricingGRAMSilver>(row, costBuilder.ItemCostSpecification.UsrPricingGRAMSilver);
            cache.SetValueExt<ASCJSMPOLineExt.usrMarketPrice>(row, costBuilder.PreciousMetalMarketCostPerTOZ);
            cache.SetValueExt<ASCJSMPOLineExt.usrBasisValue>(row, costBuilder.BasisValue);
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
