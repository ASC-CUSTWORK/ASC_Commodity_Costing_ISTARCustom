using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Common;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.Common.Bql;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.PO;
using PX.Objects;
using PX.SM;
using PX.TM;


namespace ASCISTARCustom
{
    public class CostingType
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute() : base(
                new[] { StandardCost, MarketCost, ContractCost,/* PercentageCost,*/ WeightCost },
                new[] { MessageStandard, MessageMarket, MessageContract,/* MessagePercentage,*/ MessageWeight }
                )
            { }
        }

        public const string StandardCost = "S";
        public const string MarketCost = "M"; // GRAMS * Market Price from Matrix * (1 + Surcharge) * (1 + Loss Percentage)
        public const string ContractCost = "C"; // Get Fixed Price from Assembly
       // public const string PercentageCost = "P"; // SUM(All Non PercentageCost) * 
        public const string WeightCost = "W"; // GRAMS * Unit

        public const string MessageStandard = "Standard";
        public const string MessageMarket = "Market";
        public const string MessageContract = "Contract";
   //     public const string MessagePercentage = "Percentage";
        public const string MessageWeight = "By Weight";


        public class standardCost : PX.Data.BQL.BqlString.Constant<standardCost>
        {
            public standardCost() : base(StandardCost) { }

        }
        public class marketCost : PX.Data.BQL.BqlString.Constant<marketCost>
        {
            public marketCost() : base(MarketCost) { }

        }
        public class contractCost : PX.Data.BQL.BqlString.Constant<contractCost>
        {
            public contractCost() : base(ContractCost) { }

        }
        //public class percentageCost : PX.Data.BQL.BqlString.Constant<percentageCost>
        //{
        //    public percentageCost() : base(PercentageCost) { }

        //}
        public class weightCost : PX.Data.BQL.BqlString.Constant<weightCost>
        {
            public weightCost() : base(WeightCost) { }

        }

    }
}


