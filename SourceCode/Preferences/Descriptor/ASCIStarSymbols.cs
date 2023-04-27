using PX.Data;

namespace ASCISTARCustom.Preferences.Descriptor
{
    public class ASCIStarSymbols
    {
        /// <summary>
        /// LBMA Silver Price PM
        /// </summary>
        public const string LBXAG = "LBXAG";

        /// <summary>
        /// LBMA Gold Price AM
        /// </summary>
        public const string LBXAUAM = "LBXAUAM";

        /// <summary>
        /// LBMA Gold Price PM
        /// </summary>
        public const string LBXAUPM = "LBXAUPM";

        /// <summary>
        /// Gold
        /// </summary>
        public const string XAU = "XAU";

        /// <summary>
        /// Silver
        /// </summary>
        public const string XAG = "XAG";

        public class ListAttribute : PXStringListAttribute
        {
            private static string[] _values = new string[]
            {
                LBXAG, LBXAUAM, LBXAUPM, XAU, XAG
            };

            private static string[] _labels = new string[]
            {
                nameof(LBXAG), nameof(LBXAUAM), nameof(LBXAUPM), nameof(XAU), nameof(XAG)
            };

            public ListAttribute() : base(_values, _labels) { }
        }

        public class lBXAG : PX.Data.BQL.BqlString.Constant<lBXAG>
        {
            public lBXAG() : base(LBXAG) { }
        }

        public class lBXAUAM : PX.Data.BQL.BqlString.Constant<lBXAUAM>
        {
            public lBXAUAM() : base(LBXAUAM) { }
        }

        public class lBXAUPM : PX.Data.BQL.BqlString.Constant<lBXAUPM>
        {
            public lBXAUPM() : base(LBXAUPM) { }
        }

        public class xAU : PX.Data.BQL.BqlString.Constant<xAU>
        {
            public xAU() : base(XAU) { }
        }

        public class xAG : PX.Data.BQL.BqlString.Constant<xAG>
        {
            public xAG() : base(XAG) { }
        }
    }
}
