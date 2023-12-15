using PX.Common;

namespace ASCJewelryLibrary.PO.Helpers
{
    [PXLocalizable]
    public class ASCJPOMessages
    {
        public static class Constants
        {
            public const string LandedCostCode = "CUSTOMSDUTY";
        }

        public static class Warnings
        {
            public const string DisabledLandedCostVendor = "Landed Cost Vendor check box is disabled, click on Vendor and check Vendor Properties group on Vendors screen.";
            public const string NoDefaultVendorOnItem = "Current Vendor of Purchase Order is not selected as Default for current Item, check Stock Item screen (Vendors tab) and Unit Cost!";
        }

        public static class Errors
        {
            public const string MarketEmpty = "Select Market first.";
            public const string ProcessingWithErrorMessages = "At least one item has not been processed.";
        }
    }
}
