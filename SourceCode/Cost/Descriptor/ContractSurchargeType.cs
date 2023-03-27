using PX.Data;

namespace ASCISTARCustom
{
    public class ContractSurchargeType
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute() : base(new[] { FixedAmt, PercentageAmt }, new[] { FixedAmt, PercentageAmt }) { }
        }

        public const string FixedAmt = "Fixed";
        public const string PercentageAmt = "Percentage";

        public class fixedAmt : PX.Data.BQL.BqlString.Constant<fixedAmt> { public fixedAmt() : base(FixedAmt) { } }
        public class percentageAmt : PX.Data.BQL.BqlString.Constant<percentageAmt> { public percentageAmt() : base(PercentageAmt) { } }
    }
}

