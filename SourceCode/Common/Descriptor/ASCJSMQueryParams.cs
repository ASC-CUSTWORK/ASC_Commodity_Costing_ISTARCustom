using System;

namespace ASCJSMCustom.Common.Descriptor
{
    public class ASCJSMQueryParams
    {
        public const string Access_key = "access_key";
        public const string Base = "base";
        public const string Symbols = "symbols";
        public string Historical = DateTime.Now.ToString("YYYY-MM-DD");
        public const string From = "from";
        public const string To = "to";
        public const string Amount = "amount";
        public const string Start_date = "start_date";
        public const string End_date = "end_date";
    }
}
