using PX.Data;


namespace ASCISTARCustom.Cost.Descriptor
{
    public class ASCIStarCostRollupType
    {
        public const string PreciousMetal = "C";
        public const string Materials = "M";
        public const string Fabrication = "F";
        public const string Labor = "L";
        public const string Handling = "H";
        public const string Shipping = "S";
        public const string Duty = "D";
        public const string Packaging = "P";
        public const string Other = "O";

        public const string MessagePreciousMetal = "Precious Metal";
        public const string MessageMaterials = "Materials";
        public const string MessageFabrication = "Fabrication";
        public const string MessageLabor = "In-house Labor";
        public const string MessageHandling = "Handling";
        public const string MessageShipping = "Shipping";
        public const string MessageDuty = "Duty";
        public const string MessagePackaging = "Packaging";
        public const string MessageOther = "Other";

        public class ListAttribute : PXStringListAttribute
        {
            private static readonly string[] _values = new string[] 
            { 
                PreciousMetal, 
                Materials, 
                Fabrication,
                Packaging,
                Shipping,
                Duty,
                Labor, 
                Handling,
                Other
            };

            private static readonly string[] _lables = new string[]
            {
                MessagePreciousMetal, 
                MessageMaterials, 
                MessageFabrication,
                MessagePackaging,
                MessageShipping,
                MessageDuty,
                MessageLabor, 
                MessageHandling,
                MessageOther
            };

            public ListAttribute() : base(_values, _lables) { }
        }

        public class commodity : PX.Data.BQL.BqlString.Constant<commodity>
        {
            public commodity() : base(PreciousMetal) { }
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

