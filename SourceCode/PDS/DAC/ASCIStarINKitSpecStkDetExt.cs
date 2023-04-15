using PX.Data;
using PX.Objects.IN;
using System;
using ASCISTARCustom.Cost.Descriptor;

namespace ASCISTARCustom
{
    public sealed class ASCIStarINKitSpecStkDetExt : PXCacheExtension<PX.Objects.IN.INKitSpecStkDet>
    {
        public static bool IsActive() => true;

        #region UsrUnitCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Unit Cost")]
        public decimal? UsrUnitCost { get; set; }
        public abstract class usrUnitCost : PX.Data.BQL.BqlDecimal.Field<usrUnitCost> { }
        #endregion

        #region UsrUnitPct
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Unit Pct")]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrUnitPct { get; set; }
        public abstract class usrUnitPct : PX.Data.BQL.BqlDecimal.Field<usrUnitPct> { }
        #endregion

        #region UsrExtCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Ext Cost")]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrUnitCost>), typeof(SumCalc<ASCIStarINKitSpecHdrExt.usrPreciousMetalCost>))]
        public decimal? UsrExtCost { get; set; }
        public abstract class usrExtCost : PX.Data.BQL.BqlDecimal.Field<usrExtCost> { }
        #endregion

        #region UsrCostingType
        [PXDBString(1, IsUnicode = true, InputMask = "")]
        [ASCIStarCostingType.List]
        [PXUIField(DisplayName = "Costing Type")]
        public string UsrCostingType { get; set; }
        public abstract class usrCostingType : PX.Data.BQL.BqlString.Field<usrCostingType> { }
        #endregion

        #region UsrCostRollupType
        [PXString(1, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Rollup Type")]
        [ASCIStarCostRollupType.List]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrCostRollupType>))]
        public string UsrCostRollupType { get; set; }
        public abstract class usrCostRollupType : PX.Data.BQL.BqlString.Field<usrCostRollupType> { }
        #endregion

        #region GoldGrams
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Gold, Grams")]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrActualGRAMGold>))]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrGoldGrams>), typeof(SumCalc<ASCIStarINKitSpecHdrExt.usrTotalGoldGrams>))]
        public decimal? UsrGoldGrams { get; set; }
        public abstract class usrGoldGrams : PX.Data.BQL.BqlDecimal.Field<usrGoldGrams> { }
        #endregion

        #region FineGoldGrams
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fine Gold, Grams")]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrPricingGRAMGold>))]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrFineGoldGrams>), typeof(SumCalc<ASCIStarINKitSpecHdrExt.usrTotalFineGoldGrams>))]
        public decimal? UsrFineGoldGrams { get; set; }
        public abstract class usrFineGoldGrams : PX.Data.BQL.BqlDecimal.Field<usrFineGoldGrams> { }
        #endregion

        #region SilverGrams
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Silver, Grams")]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrActualGRAMSilver>))]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrSilverGrams>), typeof(SumCalc<ASCIStarINKitSpecHdrExt.usrTotalSilverGrams>))]
        public decimal? UsrSilverGrams { get; set; }
        public abstract class usrSilverGrams : PX.Data.BQL.BqlDecimal.Field<usrSilverGrams> { }
        #endregion

        #region FineSilverGrams
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fine Silver, Grams")]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrPricingGRAMSilver>))]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrFineSilverGrams>), typeof(SumCalc<ASCIStarINKitSpecHdrExt.usrTotalFineSilverGrams>))]
        public decimal? UsrFineSilverGrams { get; set; }
        public abstract class usrFineSilverGrams : PX.Data.BQL.BqlDecimal.Field<usrFineSilverGrams> { }
        #endregion

        #region MetalLossPct
        [PXDBDecimal(2, MinValue = 0, MaxValue = 100)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Metal Loss, %")]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrContractLossPct>))]
        public decimal? UsrMetalLossPct { get; set; }
        public abstract class usrMetalLossPct : PX.Data.BQL.BqlDecimal.Field<usrMetalLossPct> { }
        #endregion

        #region SurchargePct
        [PXDBDecimal(2, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Surcharge %", Visible = true)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrContractSurcharge>))]
        public decimal? UsrSurchargePct { get; set; }
        public abstract class usrSurchargePct : PX.Data.BQL.BqlDecimal.Field<usrSurchargePct> { }
        #endregion

        #region Increment
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Increment")]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrContractIncrement>))]
        public decimal? UsrIncrement { get; set; }
        public abstract class usrIncrement : PX.Data.BQL.BqlDecimal.Field<usrIncrement> { }
        #endregion

        #region MatrixStep
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Matrix Step")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrMatrixStep>))]
        public decimal? UsrMatrixStep { get; set; }
        public abstract class usrMatrixStep : PX.Data.BQL.BqlDecimal.Field<usrMatrixStep> { }
        #endregion

        #region UsrSalesPrice
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Vendor Price", Visibility = PXUIVisibility.Visible, Enabled = true)]
        public decimal? UsrSalesPrice { get; set; }
        public abstract class usrSalesPrice : PX.Data.BQL.BqlDecimal.Field<usrSalesPrice> { }
        #endregion

        #region FabricationCost
        [PXDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fabrication/Value Add")]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrFabricationCost>))]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrFabricationCost>), typeof(SumCalc<ASCIStarINKitSpecHdrExt.usrFabricationCost>))]
        public decimal? UsrFabricationCost { get; set; }
        public abstract class usrFabricationCost : PX.Data.BQL.BqlDecimal.Field<usrFabricationCost> { }
        #endregion

        #region PackagingCost
        [PXDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Packaging Cost")]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrPackagingCost>))]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrPackagingCost>), typeof(SumCalc<ASCIStarINKitSpecHdrExt.usrPackagingCost>))]
        public decimal? UsrPackagingCost { get; set; }
        public abstract class usrPackagingCost : PX.Data.BQL.BqlDecimal.Field<usrPackagingCost> { }
        #endregion

        #region LaborCost
        [PXDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "In-house Labor Cost")]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrLaborCost>))]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrLaborCost>), typeof(SumCalc<ASCIStarINKitSpecHdrExt.usrLaborCost>))]
        public decimal? UsrLaborCost { get; set; }
        public abstract class usrLaborCost : PX.Data.BQL.BqlDecimal.Field<usrLaborCost> { }
        #endregion

        #region MaterialCost
        [PXDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Materials Cost")]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrMaterialsCost>))]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrMaterialCost>), typeof(SumCalc<ASCIStarINKitSpecHdrExt.usrMaterialCost>))]
        public decimal? UsrMaterialCost { get; set; }
        public abstract class usrMaterialCost : PX.Data.BQL.BqlDecimal.Field<usrMaterialCost> { }
        #endregion

        #region OtherCost
        [PXDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Other Cost", Visible = false)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrOtherCost>))]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrOtherCost>), typeof(SumCalc<ASCIStarINKitSpecHdrExt.usrOtherCost>))]
        public decimal? UsrOtherCost { get; set; }
        public abstract class usrOtherCost : PX.Data.BQL.BqlDecimal.Field<usrOtherCost> { }
        #endregion
                
        #region FreightCost
        [PXDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Freight Cost")]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrFreightCost>))]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrFreightCost>), typeof(SumCalc<ASCIStarINKitSpecHdrExt.usrFreightCost>))]
        public decimal? UsrFreightCost { get; set; }
        public abstract class usrFreightCost : PX.Data.BQL.BqlDecimal.Field<usrFreightCost> { }
        #endregion

        #region HandlingCost
        [PXDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Handling Cost")]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrHandlingCost>))]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrHandlingCost>), typeof(SumCalc<ASCIStarINKitSpecHdrExt.usrHandlingCost>))]
        public decimal? UsrHandlingCost { get; set; }
        public abstract class usrHandlingCost : PX.Data.BQL.BqlDecimal.Field<usrHandlingCost> { }
        #endregion

        #region DutyCost
        [PXDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Duty Cost")]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrDutyCost>))]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrDutyCost>), typeof(SumCalc<ASCIStarINKitSpecHdrExt.usrDutyCost>))]
        public decimal? UsrDutyCost { get; set; }
        public abstract class usrDutyCost : PX.Data.BQL.BqlDecimal.Field<usrDutyCost> { }
        #endregion
    }
}