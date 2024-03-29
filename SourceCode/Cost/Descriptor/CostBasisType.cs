﻿using PX.Data;


namespace ASCISTARCustom
{
    public class CostBasisType
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute() : base(
                new[] { Market.Substring(0, 1), "X", Item.Substring(0, 1), Vendor.Substring(0, 1) },
                new[] { Market, Matrix, Item, Vendor }
                )
            { }
        }

        public const string Market = "Market";
        public const string Matrix = "Matrix";
        public const string Item = "Item"; // GRAMS * Market Price from Matrix * (1 + Surcharge) * (1 + Loss Percentage)
        public const string Vendor = "Vendor"; // Get Fixed Price from Assembly


        public class market : PX.Data.BQL.BqlString.Constant<market>
        {
            public market() : base(Market) { }

        }

        public class matrix : PX.Data.BQL.BqlString.Constant<matrix>
        {
            public matrix() : base("X") { }

        }
        public class item : PX.Data.BQL.BqlString.Constant<item>
        {
            public item() : base(Item) { }

        }
        public class vendor : PX.Data.BQL.BqlString.Constant<vendor>
        {
            public vendor() : base(Vendor) { }

        }


    }
}

