using PX.Common;

namespace ASCISTARCustom.INKit.Descriptor
{
    public class ASCIStarINKitMessages
    {
        public const string EMailSubject = "Purchase Item request: {0}";
        public const string EMailBody = "Dear supplier, {0}\r\n" +
                                        "Please, find in attachment specification for Item Style: {1} - {2} \r\n" +
                                        "Provide {0}'s most favorable quote for its manufacture. \r\n" +
                                        "Expectation delivery time frame is {3} days from Purcahse Order date. \r\n" +
                                        "\r\nBest regards, \r\n" +
                                        "{4}\r\n" + "{5}";


        [PXLocalizable]
        public class Error
        {
            public const string ItemWrongMetalType = "Kit has another Metal Type from selected item!";
            public const string NoDefaultVendor = "Select default Vendor on Vendor tab.";
        }
    }
}
