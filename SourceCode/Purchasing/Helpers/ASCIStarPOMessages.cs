using PX.Common;

namespace ASCISTARCustom.Purchasing.Helpers
{
    [PXLocalizable]
    public class ASCIStarPOMessages
    {
        public static class Constants
        {
            public const string LandedCostCode = "CUSTOMSDUTY";
        }

        public static class Warnings
        {
            public const string DisabledLandedCostVendor = "Landed Cost Vendor check box is disabled, click on Vendor and check Vendor Properties group on Vendors screen.";
        }

        public static class Errors
        {
            public const string MarketEmpty = "Select Market first.";
            public const string ProcessingWithErrorMessages = "At least one item has not been processed.";
        }
    }
}
