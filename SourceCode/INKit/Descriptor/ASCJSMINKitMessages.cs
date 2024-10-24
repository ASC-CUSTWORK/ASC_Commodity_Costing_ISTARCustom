using PX.Common;

namespace ASCJSMCustom.INKit.Descriptor
{
    public class ASCJSMINKitMessages
    {
        public const string EMailSubject = "Purchase Item request: {0}";
        public const string EMailBody = "Dear supplier, {0}\r\n" +
                                        "Please, find in attachment specification for Item Style: {1} - {2} \r\n" +
                                        "Provide {0}'s most favorable quote for its manufacture. \r\n" +
                                        "Expectation delivery time frame is {3} days from Purcahse Order date. \r\n" +
                                        "\r\nBest regards, \r\n" +
                                        "{4}\r\n" + "{5}";

        [PXLocalizable]
        public class Warning
        {
            public const string BaseItemNotSpecifyed = "System is missing the base items. Please ensure 'SSS' and '24K' items are created before proceeding.";
            public const string MissingMetalType = "The Metal Type is missing!";
        }

        [PXLocalizable]
        public class Error
        {
            public const string ItemWrongMetalType = "Kit has another Metal Type from selected item!";
            public const string NoDefaultVendor = "To proceed, please add a default vendor or select one on the Vendors tab.";
            public const string CostRollupTypeNotSet = "Cost Rollup Type is not set. Please select Rollup Type before saving.";
            public const string MarketNotFound = "Market field cannot be empty";
            public const string CannotCreateItself = "Unable to create {0} using {0} as a source.";
        }
    }
}
