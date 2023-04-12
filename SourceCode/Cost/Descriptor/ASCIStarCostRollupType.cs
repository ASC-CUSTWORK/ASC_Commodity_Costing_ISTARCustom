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
        public const string Freight = "S";
        public const string Duty = "D";
        public const string Packaging = "P";
        public const string Blank = "";

        public const string MessagePreciousMetal = "Precious Metal";
        public const string MessageMaterials = "Other Materials";
        public const string MessageFabrication = "Value Add Fabrication";
        public const string MessageLabor = "In-house Labor";
        public const string MessageHandling = "Handling";
        public const string MessageFreight = "Freight";
        public const string MessageDuty = "Duty";
        public const string MessagePackaging = "Packaging";
        public const string MessageBlank = "<Blank>";

        public class ListAttribute : PXStringListAttribute
        {
            private static readonly string[] _values = new string[]
            {
                PreciousMetal,
                Fabrication,
                Materials,
                Packaging,
                Freight,
                Handling,
                Duty,
             //   Labor, 
           //     Blank
            };

            private static readonly string[] _lables = new string[]
            {
                MessagePreciousMetal,
                MessageFabrication,
                MessageMaterials,
                MessagePackaging,
                MessageFreight,
                MessageHandling,
                MessageDuty,
             //   MessageLabor, 
             //   MessageBlank
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

        public class freight : PX.Data.BQL.BqlString.Constant<freight>
        {
            public freight() : base(Freight) { }
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
            public other() : base(Blank) { }
        }
    }
}

