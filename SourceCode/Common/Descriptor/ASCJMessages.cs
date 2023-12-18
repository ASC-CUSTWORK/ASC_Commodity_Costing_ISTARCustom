using PX.Common;

namespace ASCJewelryLibrary.Common.Descriptor
{
    public class ASCJMessages
    {
        #region Plugin
        [PXLocalizable]
        public class ASCJPlugin
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
        public class ASCJStatusCode
        {
            public const string StatusCodeError = "Error: Received a {0} status code. Content: {1}";
            public const string RemoteServerError = "The remote server returned an error. For more details open trace.";
        }
        #endregion

        #region Connection
        [PXLocalizable]
        public class ASCJConnection
        {
            public const string TestConnectionSuccess = "The connection to the Metals-API was successful.";
            public const string TestConnectionFailed = "Test connection failed. For more details, please refer to the trace log.";
        }
        #endregion

        #region Error
        [PXLocalizable]
        public class ASCJError
        {
            public const string SymbolNotSpecified = "Symbol not provided. Please specify the desired symbol in preferences and try again.";
            public const string ProcError = "Error: {0}";
            public const string VendorRecordNotFound = "Vendor record not found";
            public const string VendorPriceNotFound = "Vendor price record not found, check Vendor Prices screen.";
            public const string MarketPriceNotFound = "Market price record not found, check Vendor Prices screen.";
            public const string POVendorInventoryVendorPriceEmpty = "Vendor Price can not be empty.";
        }
        #endregion
    }
}
