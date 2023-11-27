using PX.Common;
using PX.Data;

namespace ASCISTARCustom.Common.Descriptor
{
    [PXLocalizable]
    public static class ASCIStarConstants
    {
        public class MetalType
        {
            public const string Type_24K = "24K";
            public const string Type_23K = "23K";
            public const string Type_22K = "22K";
            public const string Type_21K = "21K";
            public const string Type_20K = "20K";
            public const string Type_19K = "19K";
            public const string Type_18K = "18K";
            public const string Type_17K = "17K";
            public const string Type_16K = "16K";
            public const string Type_15K = "15K";
            public const string Type_14K = "14K";
            public const string Type_13K = "13K";
            public const string Type_12K = "12K";
            public const string Type_11K = "11K";
            public const string Type_10K = "10K";
            public const string Type_09K = "09K";
            public const string Type_08K = "08K";
            public const string Type_07K = "07K";
            public const string Type_06K = "06K";

            public const string Type_24F = "24F";
            public const string Type_23F = "23F";
            public const string Type_22F = "22F";
            public const string Type_21F = "21F";
            public const string Type_20F = "20F";
            public const string Type_19F = "19F";
            public const string Type_18F = "18F";
            public const string Type_17F = "17F";
            public const string Type_16F = "16F";
            public const string Type_15F = "15F";
            public const string Type_14F = "14F";
            public const string Type_13F = "13F";
            public const string Type_12F = "12F";
            public const string Type_11F = "11F";
            public const string Type_10F = "10F";
            public const string Type_09F = "09F";
            public const string Type_08F = "08F";
            public const string Type_07F = "07F";
            public const string Type_06F = "06F";

            public const string Type_FSS = "FSS";
            public const string Type_SSS = "SSS";
        }

        public class MixedMetalType
        {
            public const string Type_MixedGold = "MG";
            public const string Type_MixedDefault = "MD";
            public const string Type_MixedUndefined = "MU";
        }

        public class CommodityClass : PX.Data.BQL.BqlString.Constant<CommodityClass>
        {
            public static readonly string value = "COMMODITY";
            public CommodityClass() : base(value) { }
        }

        public class MarketClass : PX.Data.BQL.BqlString.Constant<MarketClass>
        {
            public static readonly string value = "MARKET";
            public MarketClass() : base(value) { }
        }

        public class TOZ : PX.Data.BQL.BqlString.Constant<TOZ>
        {
            public static readonly string value = "TOZ";
            public TOZ() : base(value) { }
        }

        public class GRAM : PX.Data.BQL.BqlString.Constant<GRAM>
        {
            public static readonly string value = "GRAM";
            public GRAM() : base(value) { }
        }

        public class TOZ2GRAM_31_10348 : PX.Data.BQL.BqlDecimal.Constant<TOZ2GRAM_31_10348>
        {
            public static readonly decimal value = 31.10348m;
            public TOZ2GRAM_31_10348() : base(value) { }
        }

        public class DecimalOne : PX.Data.BQL.BqlDecimal.Constant<DecimalOne>
        {
            public static readonly decimal value = 1.0m;
            public DecimalOne() : base(value) { }
        }

        public class DecimalTwo : PX.Data.BQL.BqlDecimal.Constant<DecimalTwo>
        {
            public static readonly decimal value = 2.0m;
            public DecimalTwo() : base(value) { }
        }

        public class DecimalOneHundred : PX.Data.BQL.BqlDecimal.Constant<DecimalOneHundred>
        {
            public static readonly decimal value = 100.0m;
            public DecimalOneHundred() : base(value) { }
        }

        public class CommodityType
        {
            public class ListAttribute : PXStringListAttribute
            {
                public ListAttribute() : base(
                    new[] { Undefined, Gold, Silver, Platinum, Costume, Brass },
                    new[] { MessageUndefined, MessageGold, MessageSilver, MessagePlatinum, MessageCostume, MessageBrass })
                { }
            }

            //ADD BRASS HERE
            public const string Undefined = "U";    // (U)ndefined
            public const string Gold = "G";         // (G)mporting
            public const string Silver = "S";       // (S)ilver
            public const string Platinum = "P";     // (P)latinum
            public const string Costume = "C";      // (C)ostume
            public const string Brass = "B";        // (C)ostume

            public const string MessageUndefined = "  ";
            public const string MessageGold = "Gold";
            public const string MessageSilver = "Silver";
            public const string MessagePlatinum = "Platinum";
            public const string MessageCostume = "Costume";
            public const string MessageBrass = "Brass";

            public class undefined : PX.Data.BQL.BqlString.Constant<undefined> { public undefined() : base(Undefined) { } }
            public class gold : PX.Data.BQL.BqlString.Constant<gold> { public gold() : base(Gold) { } }
            public class silver : PX.Data.BQL.BqlString.Constant<silver> { public silver() : base(Silver) { } }
            public class platinum : PX.Data.BQL.BqlString.Constant<platinum> { public platinum() : base(Platinum) { } }
            public class costume : PX.Data.BQL.BqlString.Constant<costume> { public costume() : base(Costume) { } }
            public class brass : PX.Data.BQL.BqlString.Constant<brass> { public brass() : base(Brass) { } }
        }

        public class CostRollupType
        {
            public const string PreciousMetal = "C";
            public const string Materials = "M";
            public const string Fabrication = "F";
            public const string Labor = "L";
            public const string Handling = "H";
            public const string Freight = "S";
            public const string Duty = "D";
            public const string Packaging = "P";
            public const string PackagingForLabor = "B";
            public const string Blank = "";

            public const string MessagePreciousMetal = "Precious Metal";
            public const string MessageMaterials = "Other Materials";
            public const string MessageFabrication = "Fabrication/Value Add";
            public const string MessageLabor = "In-house Labor";
            public const string MessageHandling = "Handling";
            public const string MessageFreight = "Freight";
            public const string MessageDuty = "Duty";
            public const string MessagePackaging = "Packaging";
            public const string MessagePackagingForLabor = "Packaging Labor";
            public const string MessageBlank = "<Blank>";

            public class ListAttribute : PXStringListAttribute
            {
                private static readonly string[] _values = new string[]
                {
                PreciousMetal,
                Fabrication,
                Materials,
                Packaging,
                PackagingForLabor,
                Labor,
                Freight,
                Handling,
                Duty,

                    //     Blank
                };

                private static readonly string[] _lables = new string[]
                {
                MessagePreciousMetal,
                MessageFabrication,
                MessageMaterials,
                MessagePackaging,
                MessagePackagingForLabor,
                MessageLabor,
                MessageFreight,
                MessageHandling,
                MessageDuty,
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

            public class packagingForLabor : PX.Data.BQL.BqlString.Constant<packagingForLabor>
            {
                public packagingForLabor() : base(PackagingForLabor) { }
            }

            public class other : PX.Data.BQL.BqlString.Constant<other>
            {
                public other() : base(Blank) { }
            }
        }

        public class CostingType
        {
            public const string StandardCost = "S";
            public const string MarketCost = "M"; // GRAMS * Market Price from Matrix * (1 + Surcharge) * (1 + Loss Percentage)
            public const string ContractCost = "C"; // Get Fixed Price from Assembly
            public const string PercentageCost = "P"; // SUM(All Non PercentageCost) * 
                                                      // public const string WeightCost = "W"; // GRAMS * Unit

            public const string MessageStandard = "Avg. Actual Valuation";
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

        public class MarketList
        {
            public const string NewYork = "NY";
            public const string LondonAM = "LA";
            public const string LondonPM = "LP";

            public const string MessageNewYork = "NEW YORK";
            public const string MessageLondonAM = "LONDON AM";
            public const string MessageLondonPM = "LONDON PM";

            public class ListAttribute : PXStringListAttribute
            {
                public ListAttribute() : base(
                    new[] { NewYork, LondonAM, LondonPM },
                    new[] { MessageNewYork, MessageLondonAM, MessageLondonPM })
                { }
            }

            public class newYork : PX.Data.BQL.BqlString.Constant<newYork> { public newYork() : base(NewYork) { } }
            public class londonAM : PX.Data.BQL.BqlString.Constant<londonAM> { public londonAM() : base(LondonAM) { } }
            public class londonPM : PX.Data.BQL.BqlString.Constant<londonPM> { public londonPM() : base(LondonPM) { } }
            public class defaultMarket : PX.Data.BQL.BqlString.Constant<defaultMarket> { public defaultMarket() : base(MessageLondonPM) { } }
        }
    }
}
