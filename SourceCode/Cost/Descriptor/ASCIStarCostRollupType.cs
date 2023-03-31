using PX.Data;


namespace ASCISTARCustom.Cost.Descriptor
{
    public class ASCIStarCostRollupType
    {
        public const string Commodity = "C";
        public const string Materials = "M";
        public const string Fabrication = "F";
        public const string Labor = "L";
        public const string Handling = "H";
        public const string Shipping = "S";
        public const string Duty = "D";
        public const string Packaging = "P";
        public const string Other = "O";

        public const string MessageCommodity = "Commodity";
        public const string MessageMaterials = "Materials";
        public const string MessageFabrication = "Fabrication";
        public const string MessageLabor = "Labor";
        public const string MessageHandling = "Handling";
        public const string MessageShipping = "Shipping";
        public const string MessageDuty = "Duty";
        public const string MessagePackaging = "Packaging";
        public const string MessageOther = "Other";

        public class ListAttribute : PXStringListAttribute
        {
            private static readonly string[] _values = new string[] 
            { 
                Commodity, 
                Materials, 
                Fabrication, 
                Labor, 
                Handling, 
                Shipping, 
                Duty, 
                Packaging, 
                Other 
            };

            private static readonly string[] _lables = new string[]
            {
                MessageCommodity, 
                MessageMaterials, 
                MessageFabrication, 
                MessageLabor, 
                MessageHandling, 
                MessageShipping, 
                MessageDuty, 
                MessagePackaging, 
                MessageOther 
            };

            public ListAttribute() : base(_values, _lables) { }
        }

        public class commodity : PX.Data.BQL.BqlString.Constant<commodity>
        {
            public commodity() : base(Commodity) { }
        }

        public class materials : PX.Data.BQL.BqlString.Constant<materials>
        {
            public materials() : base(Materials) { }
        }

        public class fabrication : PX.Data.BQL.BqlString.Constant<fabrication>
        {
            public fabrication() : base(Fabrication) { }
        }

        public class labor : PX.Data.BQL.BqlString.Constant<labor>
        {
            public labor() : base(Labor) { }
        }

        public class handling : PX.Data.BQL.BqlString.Constant<handling>
        {
            public handling() : base(Handling) { }
        }

        public class shipping : PX.Data.BQL.BqlString.Constant<shipping>
        {
            public shipping() : base(Shipping) { }
        }

        public class duty : PX.Data.BQL.BqlString.Constant<duty>
        {
            public duty() : base(Duty) { }
        }

        public class packaging : PX.Data.BQL.BqlString.Constant<packaging>
        {
            public packaging() : base(Packaging) { }
        }

        public class other : PX.Data.BQL.BqlString.Constant<other>
        {
            public other() : base(Other) { }
        }
    }
}

