using PX.Data;

namespace ASCISTARCustom.Cost.Descriptor
{
    public class ASCIStarContractSurchargeType
    {
        public const string FixedAmt = "F";
        public const string PercentageAmt = "P";

        public class ListAttribute : PXStringListAttribute
        {
            private static readonly string[] _values = new string[] 
            {
                FixedAmt,
                PercentageAmt
            };

            private static readonly string[] _labels = new string[] 
            {
                "Fixed",
                "Percentage" 
            };

            public ListAttribute() : base(_values, _labels) { }
        }
        
        public class fixedAmt : PX.Data.BQL.BqlString.Constant<fixedAmt> 
        { 
            public fixedAmt() : base(FixedAmt) { } 
        }

        public class percentageAmt : PX.Data.BQL.BqlString.Constant<percentageAmt> 
        { 
            public percentageAmt() : base(PercentageAmt) { } 
        }
    }
}

