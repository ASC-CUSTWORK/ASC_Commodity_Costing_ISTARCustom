using ASCJSMCustom.IN.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.PO;
using System;
using System.Linq;

namespace ASCJSMCustom.PO
{
    public class ASCJSMPOLandedCostDocEntryExt : PXGraphExtension<POLandedCostDocEntry>
    {
        public static bool IsActive() => true;

        #region Methods

        [PXOverride]
        public virtual void AllocateLandedCosts(Action baseMethod)
        {
            baseMethod();

            RecalculateLinesCuryAllocatedLCAmt();
        }

        private void RecalculateLinesCuryAllocatedLCAmt()
        {
            var items = Base.ReceiptLines.Select()?.FirstTableItems?.ToList();
            foreach (var item in items)
            {
                var vendorDuty = GetVendorDuty(item.InventoryID, Base.Document.Current.VendorID);
                if (vendorDuty == null) continue;
                var curyAllocatedLCAmt = (vendorDuty?.DutyPct ?? decimal.Zero) / 100.00m * item.LineAmt;
                item.CuryAllocatedLCAmt = curyAllocatedLCAmt;

                Base.ReceiptLines.Update(item);
            }
        }

        private ASCJSMINVendorDuty GetVendorDuty(int? inventoryID, int? vendorID) => SelectFrom<ASCJSMINVendorDuty>
                                        .Where<ASCJSMINVendorDuty.inventoryID.IsEqual<P.AsInt>
                                        .And<ASCJSMINVendorDuty.vendorID.IsEqual<P.AsInt>>>
                                        .View.Select(this.Base, inventoryID, vendorID)?.TopFirst;

        #endregion
    }

}
