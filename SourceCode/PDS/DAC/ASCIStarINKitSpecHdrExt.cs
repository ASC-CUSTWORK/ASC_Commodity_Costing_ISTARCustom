using ASCISTARCustom.Cost.Descriptor;
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

        #region GoldGrams
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Gold, Grams")]
        public decimal? UsrGoldGrams { get; set; }
        public abstract class usrGoldGrams : PX.Data.BQL.BqlDecimal.Field<usrGoldGrams> { }
        #endregion

        #region FineGoldGrams
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fine Gold, Grams")]
        public decimal? UsrFineGoldGrams { get; set; }
        public abstract class usrFineGoldGrams : PX.Data.BQL.BqlDecimal.Field<usrFineGoldGrams> { }
        #endregion

        #region SilverGrams
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Silver, Grams")]
        public decimal? UsrSilverGrams { get; set; }
        public abstract class usrSilverGrams : PX.Data.BQL.BqlDecimal.Field<usrSilverGrams> { }
        #endregion

        #region FineSilverGrams
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fine Silver, Grams")]
        public decimal? UsrFineSilverGrams { get; set; }
        public abstract class usrFineSilverGrams : PX.Data.BQL.BqlDecimal.Field<usrFineSilverGrams> { }
        #endregion

        #region MetalLossPct
        [PXDBDecimal(2, MinValue = 0, MaxValue = 100)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Metal Loss, %")]
        public decimal? UsrMetalLossPct { get; set; }
        public abstract class usrMetalLossPct : PX.Data.BQL.BqlDecimal.Field<usrMetalLossPct> { }
        #endregion

        #region SurchargePct
        [PXDBDecimal(2, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Surcharge %", Visible = true)]
        public decimal? UsrSurchargePct { get; set; }
        public abstract class usrSurchargePct : PX.Data.BQL.BqlDecimal.Field<usrSurchargePct> { }
        #endregion

        #region Increment
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Increment")]
        public decimal? UsrIncrement { get; set; }
        public abstract class usrIncrement : PX.Data.BQL.BqlDecimal.Field<usrIncrement> { }
        #endregion

        #region MatrixStep
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Matrix Step")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrMatrixStep { get; set; }
        public abstract class usrMatrixStep : PX.Data.BQL.BqlDecimal.Field<usrMatrixStep> { }
        #endregion

        #region RollupType
        [PXDBString(1, IsFixed = true, InputMask = "")]
        [ASCIStarCostRollupType.List]
        [PXUIField(DisplayName = "Rollup Type")]
        public string UsrRollupType { get; set; }
        public abstract class usrRollupType : PX.Data.BQL.BqlString.Field<usrRollupType> { }
        #endregion

        #region CostingType
        [PXDBString(1, IsUnicode = true, InputMask = "")]
        [ASCIStarCostingType.List]
        [PXUIField(DisplayName = "Costing Type")]
        public string UsrCostingType { get; set; }
        public abstract class usrCostingType : PX.Data.BQL.BqlString.Field<usrCostingType> { }
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
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Packaging Cost")]
        public decimal? UsrPackagingCost { get; set; }
        public abstract class usrPackagingCost : PX.Data.BQL.BqlDecimal.Field<usrPackagingCost> { }
        #endregion

        #region LaborCost
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "In-house Labor Cost")]
        public decimal? UsrLaborCost { get; set; }
        public abstract class usrLaborCost : PX.Data.BQL.BqlDecimal.Field<usrLaborCost> { }
        #endregion

        #region MaterialCost
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Materials Cost")]
        public decimal? UsrMaterialCost { get; set; }
        public abstract class usrMaterialCost : PX.Data.BQL.BqlDecimal.Field<usrMaterialCost> { }
        #endregion

        #region OtherCost
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Other Cost", Visible = false)]
        public decimal? UsrOtherCost { get; set; }
        public abstract class usrOtherCost : PX.Data.BQL.BqlDecimal.Field<usrOtherCost> { }
        #endregion

        #region UnitCost
        [PXDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Landed Cost", Visibility = PXUIVisibility.Visible, Enabled = false)]
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

        #region UsrIsManualChanged
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public bool? UsrIsManualChanged { get; set; }
        public abstract class usrIsManualChanged : PX.Data.BQL.BqlBool.Field<usrIsManualChanged> { }
        #endregion
    }
}