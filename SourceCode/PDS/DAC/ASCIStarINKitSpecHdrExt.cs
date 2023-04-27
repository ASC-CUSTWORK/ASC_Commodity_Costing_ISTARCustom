using PX.Data;
using PX.Objects.IN;
using System;

namespace ASCISTARCustom
{
    public sealed class ASCIStarINKitSpecHdrExt : PXCacheExtension<INKitSpecHdr>
    {
        public static bool IsActive() => true;

        #region UsrVQuoteLineCtr
        [PXDBInt]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        public int? UsrVQuoteLineCtr { get; set; }
        public abstract class usrVQuoteLineCtr : PX.Data.BQL.BqlInt.Field<usrVQuoteLineCtr> { }
        #endregion

        #region LegacyID
        [PXDBString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Legacy ID")]
        public string UsrLegacyID { get; set; }
        public abstract class usrLegacyID : PX.Data.BQL.BqlString.Field<usrLegacyID> { }
        #endregion

        #region LegacyShortRef
        [PXDBString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Legacy Short Ref")]
        public string UsrLegacyShortRef { get; set; }
        public abstract class usrlegacyShortRef : PX.Data.BQL.BqlString.Field<usrlegacyShortRef> { }
        #endregion

        #region UsrTotalGoldGrams
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Total Gold Grams", Enabled = false)]
        public decimal? UsrTotalGoldGrams { get; set; }
        public abstract class usrTotalGoldGrams : PX.Data.BQL.BqlDecimal.Field<usrTotalGoldGrams> { }
        #endregion

        #region UsrTotalFineGoldGrams
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Total Fine Gold Grams", Enabled = false)]
        public decimal? UsrTotalFineGoldGrams { get; set; }
        public abstract class usrTotalFineGoldGrams : PX.Data.BQL.BqlDecimal.Field<usrTotalFineGoldGrams> { }
        #endregion

        #region UsrTotalSilverGrams
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Total Silver Grams", Enabled = false)]
        public decimal? UsrTotalSilverGrams { get; set; }
        public abstract class usrTotalSilverGrams : PX.Data.BQL.BqlDecimal.Field<usrTotalSilverGrams> { }
        #endregion

        #region FineSilverGrams
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Total Fine Silver Grams", Enabled = false)]
        public decimal? UsrTotalFineSilverGrams { get; set; }
        public abstract class usrTotalFineSilverGrams : PX.Data.BQL.BqlDecimal.Field<usrTotalFineSilverGrams> { }
        #endregion

        #region UnitCost
        [PXDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Unit Cost", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public decimal? UsrUnitCost { get; set; }
        public abstract class usrUnitCost : PX.Data.BQL.BqlDecimal.Field<usrUnitCost> { }
        #endregion

        #region CommodityCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Precious Metal Cost")]
        public decimal? UsrPreciousMetalCost { get; set; }
        public abstract class usrPreciousMetalCost : PX.Data.BQL.BqlDecimal.Field<usrPreciousMetalCost> { }
        #endregion

        #region FabricationCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fabrication/Value Add")]
        public decimal? UsrFabricationCost { get; set; }
        public abstract class usrFabricationCost : PX.Data.BQL.BqlDecimal.Field<usrFabricationCost> { }
        #endregion

        #region PackagingCost
        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Packaging Cost")]
        public decimal? UsrPackagingCost { get; set; }
        public abstract class usrPackagingCost : PX.Data.BQL.BqlDecimal.Field<usrPackagingCost> { }
        #endregion

        #region LaborCost
        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "In-house Labor Cost")]
        public decimal? UsrLaborCost { get; set; }
        public abstract class usrLaborCost : PX.Data.BQL.BqlDecimal.Field<usrLaborCost> { }
        #endregion

        #region UsrPackagingLaborCost
        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Labor For Packaging")]
        public decimal? UsrPackagingLaborCost { get; set; }
        public abstract class usrPackagingLaborCost : PX.Data.BQL.BqlDecimal.Field<usrPackagingLaborCost> { }
        #endregion

        #region MaterialCost
        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Other Materials Cost")]
        public decimal? UsrMaterialCost { get; set; }
        public abstract class usrMaterialCost : PX.Data.BQL.BqlDecimal.Field<usrMaterialCost> { }
        #endregion

        #region OtherCost
        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Other Cost", Visible = false)]
        public decimal? UsrOtherCost { get; set; }
        public abstract class usrOtherCost : PX.Data.BQL.BqlDecimal.Field<usrOtherCost> { }
        #endregion

        #region UsrLandedCost
        [PXDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Est. Landed Cost", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public decimal? UsrLandedCost { get; set; }
        public abstract class usrLandedCost : PX.Data.BQL.BqlDecimal.Field<usrLandedCost> { }
        #endregion

        #region FreightCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Freight Cost")]
        public decimal? UsrFreightCost { get; set; }
        public abstract class usrFreightCost : PX.Data.BQL.BqlDecimal.Field<usrFreightCost> { }
        #endregion

        #region HandlingCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Handling Cost")]
        public decimal? UsrHandlingCost { get; set; }
        public abstract class usrHandlingCost : PX.Data.BQL.BqlDecimal.Field<usrHandlingCost> { }
        #endregion

        #region DutyCost
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Duty Cost")]
        public decimal? UsrDutyCost { get; set; }
        public abstract class usrDutyCost : PX.Data.BQL.BqlDecimal.Field<usrDutyCost> { }
        #endregion

        #region DutyCostPct
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Duty %")]
        public decimal? UsrDutyCostPct { get; set; }
        public abstract class usrDutyCostPct : PX.Data.BQL.BqlDecimal.Field<usrDutyCostPct> { }
        #endregion

        #region UsrSalesPrice
        [PXPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Basis Value", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public decimal? UsrBasisValue { get; set; }
        public abstract class usrBasisValue : PX.Data.BQL.BqlDecimal.Field<usrBasisValue> { }
        #endregion

        #region UsrMatrixStep
        [PXDBDecimal(6, MinValue = 0, MaxValue = 10)]
        [PXUIField(DisplayName = "Matrix Step")]
        [PXDefault(TypeCode.Decimal, "0.500000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<INKitSpecHdr.kitInventoryID, ASCIStarINInventoryItemExt.usrMatrixStep>))]
        public decimal? UsrMatrixStep { get; set; }
        public abstract class usrMatrixStep : PX.Data.BQL.BqlDecimal.Field<usrMatrixStep> { }
        #endregion
    }
}