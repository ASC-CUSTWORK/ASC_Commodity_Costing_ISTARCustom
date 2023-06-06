using PX.Common;
using PX.Data;
using PX.Data.BQL;
using static ASCISTARCustom.IN.Helpers.INConstants;

namespace ASCISTARCustom.IN.Helpers
{
    [PXLocalizable]
    public static class INConstants
    {
        public class INTestType
        {
            public const string Material = "Material";
            public const string Assay = "Assay";
            public const string Plating = "Plating";
            public const string TarnishTesting = "Tarnish Testing";
            public const string PreProduction = "Pre-Production";
            public const string Production = "Production";
            public const string InStore = "In-Store";
            public const string InFactoryInspection = "In-Factory Inspection";
            public const string SpecialTest = "Special Test";
        }
        public class INPassStatus
        {
            public const string Pass = "Pass";
            public const string Fail = "Fail";
            public const string ConditionalFailed = "Failed w/Conditional Approval";

            public const string P = "P";
            public const string F = "F";
            public const string C = "C";
        }

        public class INAttributesID
        {
            public const string CPTESTTYPE = "CPTESTTYPE";
            public const string CPPROTOCOL = "CPPROTOCOL";
            public const string REASONCODE = "REASONCODE";

            public const string StyleStatus = "JSSTYLST";
            public const string CustomerCode = "JSPERIOD";
            public const string InventoryCategory = "JSINVCAT";
            public const string ItemType = "JSITMTYPE";
            public const string ItemSubType = "JSITSUBTYP";
            public const string Collections = "JSCOLETN";
            public const string MetalType = "JSMETAL";
            public const string MetalColor = "JSMETALCLR";
            public const string Plating = "JSPLATING";
            public const string Finishes = "JSFINISHES";
            public const string VendorMaker = "JSMAKERS";
            public const string CountryOfOrigion = "JSORGCNTRY";
            public const string StoneType = "JSWRSHAPE";
            public const string StoneColor = "JSGEMTYP";
            public const string StoneShapes = "JSGEMSHP";
            public const string StoneCreations = "JSSTNMTD";
            public const string GemstoneTreatment = "JSGENTRT";
            public const string SettingType = "JSWRSETTYP";
            public const string Findings = "JSCLOSRTYP";
            public const string FindingsSubType = "JSFIDSTYP";
            public const string ChainType = "JSERTYPE";
            public const string RingLength = "JSLENGTH";
            public const string RingSize = "JSRINGSIZE";
            public const string OD = "JSOD";

            public class styleStatus : BqlString.Constant<styleStatus> { public styleStatus() : base(StyleStatus) { } }
            public class customerCode : BqlString.Constant<customerCode> { public customerCode() : base(CustomerCode) { } }
            public class inventoryCategory : BqlString.Constant<inventoryCategory> { public inventoryCategory() : base(InventoryCategory) { } }
            public class itemType : BqlString.Constant<itemType> { public itemType() : base(ItemType) { } }
            public class itemSubType : BqlString.Constant<itemSubType> { public itemSubType() : base(ItemSubType) { } }
            public class collections : BqlString.Constant<collections> { public collections() : base(Collections) { } }
            public class metalType : BqlString.Constant<metalType> { public metalType() : base(MetalType) { } }
            public class metalColor : BqlString.Constant<metalColor> { public metalColor() : base(MetalColor) { } }
            public class plating : BqlString.Constant<plating> { public plating() : base(Plating) { } }
            public class finishes : BqlString.Constant<finishes> { public finishes() : base(Finishes) { } }
            public class vendorMaker : BqlString.Constant<vendorMaker> { public vendorMaker() : base(VendorMaker) { } }
            public class countryOfOrigion : BqlString.Constant<countryOfOrigion> { public countryOfOrigion() : base(CountryOfOrigion) { } }
            public class stoneType : BqlString.Constant<stoneType> { public stoneType() : base(StoneType) { } }
            public class stoneColor : BqlString.Constant<stoneColor> { public stoneColor() : base(StoneColor) { } }
            public class stoneShapes : BqlString.Constant<stoneShapes> { public stoneShapes() : base(StoneShapes) { } }
            public class stoneCreations : BqlString.Constant<stoneCreations> { public stoneCreations() : base(StoneCreations) { } }
            public class gemstoneTreatment : BqlString.Constant<gemstoneTreatment> { public gemstoneTreatment() : base(GemstoneTreatment) { } }
            public class settingType : BqlString.Constant<settingType> { public settingType() : base(SettingType) { } }
            public class findings : BqlString.Constant<findings> { public findings() : base(Findings) { } }
            public class findingsSubType : BqlString.Constant<findingsSubType> { public findingsSubType() : base(FindingsSubType) { } }
            public class chainType : BqlString.Constant<chainType> { public chainType() : base(ChainType) { } }
            public class ringLength : BqlString.Constant<ringLength> { public ringLength() : base(RingLength) { } }
            public class ringSize : BqlString.Constant<ringSize> { public ringSize() : base(RingSize) { } }
            public class od : BqlString.Constant<od> { public od() : base(OD) { } }
        }
    }

    public class INComplianceType
    {
        public static readonly string[] Values = { INTestType.Material, INTestType.Assay, INTestType.Plating,
                                                   INTestType.TarnishTesting, INTestType.PreProduction, INTestType.Production,
                                                   INTestType.InStore, INTestType.InFactoryInspection, INTestType.SpecialTest };

        public static readonly string[] Labels =  { INTestType.Material, INTestType.Assay, INTestType.Plating,
                                                   INTestType.TarnishTesting, INTestType.PreProduction, INTestType.Production,
                                                   INTestType.InStore, INTestType.InFactoryInspection, INTestType.SpecialTest };
        public class TestTypeListAttribute : PXStringListAttribute
        {
            public TestTypeListAttribute() : base(Values, Labels) { }
        }
    }

    public class INCompliancePassStatus
    {
        public static readonly string[] Labels = { INPassStatus.Pass, INPassStatus.Fail, INPassStatus.ConditionalFailed };
        public static readonly string[] Values = { INPassStatus.P, INPassStatus.F, INPassStatus.C };

        public class PassStatusListAttribute : PXStringListAttribute
        {
            public PassStatusListAttribute() : base(Values, Labels) { }
        }
    }
}
