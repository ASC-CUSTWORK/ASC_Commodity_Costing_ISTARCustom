using ASCJewelryLibrary.IN.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.PO;
using System;
using System.Linq;

namespace ASCJewelryLibrary.PO
{
    public class ASCJPOLandedCostDocEntryExt : PXGraphExtension<POLandedCostDocEntry>
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

        private ASCJINVendorDuty GetVendorDuty(int? inventoryID, int? vendorID) => SelectFrom<ASCJINVendorDuty>
                                        .Where<ASCJINVendorDuty.inventoryID.IsEqual<P.AsInt>
                                        .And<ASCJINVendorDuty.vendorID.IsEqual<P.AsInt>>>
                                        .View.Select(this.Base, inventoryID, vendorID)?.TopFirst;

        #endregion
    }

}
