using ASCISTARCustom.Inventory.Descriptor.Constants;
using ASCISTARCustom.PDS.Descriptor;
using ASCISTARCustom.PDS.Interfaces;
using PX.Data;
using PX.Objects.IN;
using System;

namespace ASCISTARCustom
{
    public sealed class ASCIStarINKitSpecStkDetExt : PXCacheExtension<PX.Objects.IN.INKitSpecStkDet>, IASCIStarCostRollup
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
        [PXUIField(DisplayName = "Ext Cost", Enabled = false)]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrUnitCost>))]
        [ASCIStarCostAssignment]
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
        [PXDBString(1, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Rollup Type", Required = true)]
        [ASCIStarCostRollupType.List]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrCostRollupType>))]
        public string UsrCostRollupType { get; set; }
        public abstract class usrCostRollupType : PX.Data.BQL.BqlString.Field<usrCostRollupType> { }
        #endregion

        #region UsrBaseGoldGrams
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrActualGRAMGold>))]
        public decimal? UsrBaseGoldGrams { get; set; }
        public abstract class usrBaseGoldGrams : PX.Data.BQL.BqlDecimal.Field<usrBaseGoldGrams> { }
        #endregion

        #region GoldGrams
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Gold, Grams", Enabled = false)]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrBaseGoldGrams>), typeof(SumCalc<ASCIStarINKitSpecHdrExt.usrTotalGoldGrams>))]
        public decimal? UsrGoldGrams { get; set; }
        public abstract class usrGoldGrams : PX.Data.BQL.BqlDecimal.Field<usrGoldGrams> { }
        #endregion

        #region FineGoldGrams
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrPricingGRAMGold>))]
        public decimal? UsrBaseFineGoldGrams { get; set; }
        public abstract class usrBaseFineGoldGrams : PX.Data.BQL.BqlDecimal.Field<usrBaseFineGoldGrams> { }
        #endregion

        #region UsrFineGoldGrams
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fine Gold, Grams", Enabled = false)]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrBaseFineGoldGrams>), typeof(SumCalc<ASCIStarINKitSpecHdrExt.usrTotalFineGoldGrams>))]
        public decimal? UsrFineGoldGrams { get; set; }
        public abstract class usrFineGoldGrams : PX.Data.BQL.BqlDecimal.Field<usrFineGoldGrams> { }
        #endregion

        #region UsrBaseSilverGrams
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrActualGRAMSilver>))]
        public decimal? UsrBaseSilverGrams { get; set; }
        public abstract class usrBaseSilverGrams : PX.Data.BQL.BqlDecimal.Field<usrBaseSilverGrams> { }
        #endregion

        #region UsrSilverGrams
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Silver, Grams", Enabled = false)]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrBaseSilverGrams>), typeof(SumCalc<ASCIStarINKitSpecHdrExt.usrTotalSilverGrams>))]
        public decimal? UsrSilverGrams { get; set; }
        public abstract class usrSilverGrams : PX.Data.BQL.BqlDecimal.Field<usrSilverGrams> { }
        #endregion

        #region UsrBaseFineSilverGrams
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrPricingGRAMSilver>))]
        public decimal? UsrBaseFineSilverGrams { get; set; }
        public abstract class usrBaseFineSilverGrams : PX.Data.BQL.BqlDecimal.Field<usrBaseFineSilverGrams> { }
        #endregion

        #region UsrFineSilverGrams
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fine Silver, Grams", Enabled = false)]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrBaseFineSilverGrams>), typeof(SumCalc<ASCIStarINKitSpecHdrExt.usrTotalFineSilverGrams>))]
        public decimal? UsrFineSilverGrams { get; set; }
        public abstract class usrFineSilverGrams : PX.Data.BQL.BqlDecimal.Field<usrFineSilverGrams> { }
        #endregion

        #region MetalLossPct
        [PXDBDecimal(2, MinValue = 0, MaxValue = 100)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Metal Loss, %", Enabled = false)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrContractLossPct>))]
        public decimal? UsrMetalLossPct { get; set; }
        public abstract class usrMetalLossPct : PX.Data.BQL.BqlDecimal.Field<usrMetalLossPct> { }
        #endregion

        #region SurchargePct
        [PXDBDecimal(2, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Surcharge %", Enabled = false, Visible = true)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrContractSurcharge>))]
        public decimal? UsrSurchargePct { get; set; }
        public abstract class usrSurchargePct : PX.Data.BQL.BqlDecimal.Field<usrSurchargePct> { }
        #endregion

        #region Increment
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Increment", Enabled = false)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrContractIncrement>))]
        public decimal? UsrIncrement { get; set; }
        public abstract class usrIncrement : PX.Data.BQL.BqlDecimal.Field<usrIncrement> { }
        #endregion

        #region MatrixStep
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Matrix Step", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrMatrixStep>))]
        public decimal? UsrMatrixStep { get; set; }
        public abstract class usrMatrixStep : PX.Data.BQL.BqlDecimal.Field<usrMatrixStep> { }
        #endregion

        #region UsrSalesPrice
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Basis Value", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public decimal? UsrSalesPrice { get; set; }
        public abstract class usrSalesPrice : PX.Data.BQL.BqlDecimal.Field<usrSalesPrice> { }
        #endregion

        #region UsrBaseFabricationCost
        [PXDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrFabricationCost>))]
        public decimal? UsrBaseFabricationCost { get; set; }
        public abstract class usrBaseFabricationCost : PX.Data.BQL.BqlDecimal.Field<usrBaseFabricationCost> { }
        #endregion

        #region UsrFabricationCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fabrication/Value Add", Enabled = false)]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrBaseFabricationCost>), typeof(SumCalc<ASCIStarINKitSpecHdrExt.usrFabricationCost>))]
        public decimal? UsrFabricationCost { get; set; }
        public abstract class usrFabricationCost : PX.Data.BQL.BqlDecimal.Field<usrFabricationCost> { }
        #endregion

        #region PackagingCost
        [PXDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrPackagingCost>))]
        public decimal? UsrBasePackagingCost { get; set; }
        public abstract class usrBasePackagingCost : PX.Data.BQL.BqlDecimal.Field<usrBasePackagingCost> { }
        #endregion

        #region PackagingCost
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Packaging Cost", Enabled = false)]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrBasePackagingCost>), typeof(SumCalc<ASCIStarINKitSpecHdrExt.usrPackagingCost>))]
        public decimal? UsrPackagingCost { get; set; }
        public abstract class usrPackagingCost : PX.Data.BQL.BqlDecimal.Field<usrPackagingCost> { }
        #endregion

        #region LaborCost
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "In-house Labor Cost", Enabled = false)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrLaborCost>))]
        public decimal? UsrLaborCost { get; set; }
        public abstract class usrLaborCost : PX.Data.BQL.BqlDecimal.Field<usrLaborCost> { }
        #endregion

        #region UsrBaseMaterialCost
        [PXDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrMaterialsCost>))]
        public decimal? UsrBaseMaterialCost { get; set; }
        public abstract class usrBaseMaterialCost : PX.Data.BQL.BqlDecimal.Field<usrBaseMaterialCost> { }
        #endregion

        #region UsrMaterialCost
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Materials Cost", Enabled = false)]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrBaseMaterialCost>), typeof(SumCalc<ASCIStarINKitSpecHdrExt.usrMaterialCost>))]
        public decimal? UsrMaterialCost { get; set; }
        public abstract class usrMaterialCost : PX.Data.BQL.BqlDecimal.Field<usrMaterialCost> { }
        #endregion

        #region OtherCost
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Other Cost", Enabled = false, Visible = false)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrOtherCost>))]
        public decimal? UsrOtherCost { get; set; }
        public abstract class usrOtherCost : PX.Data.BQL.BqlDecimal.Field<usrOtherCost> { }
        #endregion
                
        #region FreightCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Freight Cost", Enabled = false)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrFreightCost>))]
        public decimal? UsrFreightCost { get; set; }
        public abstract class usrFreightCost : PX.Data.BQL.BqlDecimal.Field<usrFreightCost> { }
        #endregion

        #region HandlingCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Handling Cost", Enabled = false)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrHandlingCost>))]
        public decimal? UsrHandlingCost { get; set; }
        public abstract class usrHandlingCost : PX.Data.BQL.BqlDecimal.Field<usrHandlingCost> { }
        #endregion

        #region DutyCost
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Duty Cost", Enabled = false)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrDutyCost>))]
        public decimal? UsrDutyCost { get; set; }
        public abstract class usrDutyCost : PX.Data.BQL.BqlDecimal.Field<usrDutyCost> { }
        #endregion

        #region UsrIsMetal 
        [PXDBBool]
        [PXUIField(DisplayName = "Is Metal", Visible = false, Enabled = false)]
        public bool? UsrIsMetal { get; set; }
        public abstract class usrIsMetal : PX.Data.BQL.BqlBool.Field<usrIsMetal> { }
        #endregion
    }
}