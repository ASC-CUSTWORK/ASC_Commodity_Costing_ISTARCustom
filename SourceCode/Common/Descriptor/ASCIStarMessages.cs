using PX.Common;

namespace ASCISTARCustom.Common.Descriptor
{
    public class ASCIStarMessages
    {
        #region Plugin
        [PXLocalizable]
        public class Plugin
        {
            public const string PluginStart = "INFO:......ASC Jewelshop Module Plugin start working...";
            public const string PluginEnd = "INFO:......ASC Jewelshop Module Plugin work completed.";
            public const string PluginCreateConnectionPref = "INFO:......Creating Connection preferences...";
            public const string PluginCreateConnectionPrefSuccess = "SUCCESS:...Connection Preferences created successfully!";
            public const string PluginCreateConnectionPrefError = "ERROR:.....{0}";
        }
        #endregion

        #region StatusCode
        [PXLocalizable]
        public class StatusCode
        {
            public const string StatusCodeError = "Error: Received a {0} status code. Content: {1}";
            public const string RemoteServerError = "The remote server returned an error. For more details open trace.";
        }
        #endregion

        #region Connection
        [PXLocalizable]
        public class Connection
        {
            public const string TestConnectionSuccess = "The connection to the Metals-API was successful.";
            public const string TestConnectionFailed = "Test connection failed. For more details, please refer to the trace log.";
        }
        #endregion

        #region Error
        [PXLocalizable]
        public class Error
        {
            public const string SymbolNotSpecified = "Symbol not provided. Please specify the desired symbol in preferences and try again.";
            public const string ProcError = "Error: {0}";
            public const string MissingMetalType = "The Metal Type on Jewelry tab is missing!";
            public const string VendorDoesNotContainValidPrice = "{0} does not contain a valid price for {1} on {2}";
            public const string VendorRecordNotFound = "Vendor record not found";
            public const string VendorPriceNotFound = "Vendor price record not found, check Vendor Prices screen.";
            public const string MarketPriceNotFound = "Market price record not found, check Vendor Prices screen.";
            public const string UnitConversionNotFound = "Unit conversion record from {0} to {1} not found";
            public const string POVendorInventoryMetalItemEmpty = "Metal Item can not be empty.";
            public const string POVendorInventoryVendorPriceEmpty = "Vendor Price can not be empty.";
            public const string NoDefaultVendor = "To proceed, please add a default vendor or select one on the Vendors tab.";
            public const string CostRollupTypeNotSet = "Cost Rollup Type is not set. Please select Rollup Type before saving.";
            public const string MoreThenOneDefaultVendor = "You have more than one default vendor. Please select one on the Vendors tab.";
        }
        #endregion

    }
}
