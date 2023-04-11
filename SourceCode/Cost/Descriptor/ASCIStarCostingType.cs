using PX.Data;

namespace ASCISTARCustom.Cost.Descriptor
{
    public class ASCIStarCostingType
    {
        public const string StandardCost = "S";
        public const string MarketCost = "M"; // GRAMS * Market Price from Matrix * (1 + Surcharge) * (1 + Loss Percentage)
        public const string ContractCost = "C"; // Get Fixed Price from Assembly
        public const string PercentageCost = "P"; // SUM(All Non PercentageCost) * 
                                                  // public const string WeightCost = "W"; // GRAMS * Unit

        public const string MessageStandard = "ERP Valuation";
        public const string MessageMarket = "Market";
        public const string MessageContract = "Contract";
        public const string MessagePercentage = "Percentage";
        //   public const string MessageWeight = "By Weight";

        public class ListAttribute : PXStringListAttribute
        {
            private static readonly string[] values = new string[] { MarketCost, ContractCost, StandardCost, PercentageCost/*, WeightCost*/ };
            private static readonly string[] lables = new string[] { MessageMarket, MessageContract, MessageStandard, MessagePercentage/*, MessageWeight */};
            public ListAttribute() : base(values, lables) { }
        }

        public class standardCost : PX.Data.BQL.BqlString.Constant<standardCost> { public standardCost() : base(StandardCost) { } }
        public class marketCost : PX.Data.BQL.BqlString.Constant<marketCost> { public marketCost() : base(MarketCost) { } }
        public class contractCost : PX.Data.BQL.BqlString.Constant<contractCost> { public contractCost() : base(ContractCost) { } }
        public class percentageCost : PX.Data.BQL.BqlString.Constant<percentageCost> { public percentageCost() : base(PercentageCost) { } }
        //  public class weightCost : PX.Data.BQL.BqlString.Constant<weightCost> { public weightCost() : base(WeightCost) { } }
    }
}


