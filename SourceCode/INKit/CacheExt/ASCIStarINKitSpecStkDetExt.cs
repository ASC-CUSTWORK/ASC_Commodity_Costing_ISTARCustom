using ASCISTARCustom.Common.DTO.Interfaces;
using ASCISTARCustom.IN.CacheExt;
using ASCISTARCustom.INKit.Descriptor;
using ASCISTARCustom.INKit.Interfaces;
using PX.Data;
using PX.Objects.IN;
using System;
using static ASCISTARCustom.Common.Descriptor.ASCIStarConstants;

namespace ASCISTARCustom.INKit.CacheExt
{
    public class ASCIStarINKitSpecStkDetExt : PXCacheExtension<PX.Objects.IN.INKitSpecStkDet>, IASCIStarItemCostSpecDTO, IASCIStarCostRollup
    {
        public static bool IsActive() => true;

        #region UsrUnitCost
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Unit Cost")]
        [PXFormula(typeof(Add<Add<Add<Add<usrPreciousMetalCost, usrOtherMaterialsCost>, usrFabricationCost>, usrPackagingCost>, usrPackagingLaborCost>))]
        public decimal? UsrUnitCost { get; set; }
        public abstract class usrUnitCost : PX.Data.BQL.BqlDecimal.Field<usrUnitCost> { }
        #endregion

        #region UsrExtCost
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Ext Cost", Enabled = false)]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrUnitCost>))]
        [ASCIStarCostAssignment]
        public decimal? UsrExtCost { get; set; }
        public abstract class usrExtCost : PX.Data.BQL.BqlDecimal.Field<usrExtCost> { }
        #endregion

        #region UsrCostingType
        [PXDBString(1, IsUnicode = true, InputMask = "")]
        [CostingType.List]
        [PXUIField(DisplayName = "Costing Type")]
        public string UsrCostingType { get; set; }
        public abstract class usrCostingType : PX.Data.BQL.BqlString.Field<usrCostingType> { }
        #endregion

        #region UsrCostRollupType
        [PXDBString(1, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Rollup Type", Required = true)]
        [PXDefault(CostRollupType.PreciousMetal)]
        [CostRollupType.List]
        //[PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrCostRollupType>))]
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

        #region UsrActualGRAMGold
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Gold, Grams", IsReadOnly = true)]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrBaseGoldGrams>), typeof(SumCalc<ASCIStarINKitSpecHdrExt.usrActualGRAMGold>))]
        public decimal? UsrActualGRAMGold { get; set; }
        public abstract class usrActualGRAMGold : PX.Data.BQL.BqlDecimal.Field<usrActualGRAMGold> { }
        #endregion

        #region UsrBaseFineGoldGrams
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrPricingGRAMGold>))]
        public decimal? UsrBaseFineGoldGrams { get; set; }
        public abstract class usrBaseFineGoldGrams : PX.Data.BQL.BqlDecimal.Field<usrBaseFineGoldGrams> { }
        #endregion

        #region UsrPricingGRAMGold
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fine Gold, Grams", IsReadOnly = true)]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrBaseFineGoldGrams>), typeof(SumCalc<ASCIStarINKitSpecHdrExt.usrPricingGRAMGold>))]
        public decimal? UsrPricingGRAMGold { get; set; }
        public abstract class usrPricingGRAMGold : PX.Data.BQL.BqlDecimal.Field<usrPricingGRAMGold> { }
        #endregion

        #region UsrBaseSilverGrams
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrActualGRAMSilver>))]
        public decimal? UsrBaseSilverGrams { get; set; }
        public abstract class usrBaseSilverGrams : PX.Data.BQL.BqlDecimal.Field<usrBaseSilverGrams> { }
        #endregion

        #region UsrActualGRAMSilver
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Silver, Grams", IsReadOnly = true)]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrBaseSilverGrams>), typeof(SumCalc<ASCIStarINKitSpecHdrExt.usrActualGRAMSilver>))]
        public decimal? UsrActualGRAMSilver { get; set; }
        public abstract class usrActualGRAMSilver : PX.Data.BQL.BqlDecimal.Field<usrActualGRAMSilver> { }
        #endregion

        #region UsrBaseFineSilverGrams
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrPricingGRAMSilver>))]
        public decimal? UsrBaseFineSilverGrams { get; set; }
        public abstract class usrBaseFineSilverGrams : PX.Data.BQL.BqlDecimal.Field<usrBaseFineSilverGrams> { }
        #endregion

        #region UsrPricingGRAMSilver
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fine Silver, Grams", IsReadOnly = true)]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrBaseFineSilverGrams>), typeof(SumCalc<ASCIStarINKitSpecHdrExt.usrPricingGRAMSilver>))]
        public decimal? UsrPricingGRAMSilver { get; set; }
        public abstract class usrPricingGRAMSilver : PX.Data.BQL.BqlDecimal.Field<usrPricingGRAMSilver> { }
        #endregion

        #region UsrContractLossPct
        [PXDBDecimal(2, MinValue = 0, MaxValue = 100)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Metal Loss, %", Enabled = false)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrContractLossPct>))]
        public decimal? UsrContractLossPct { get; set; }
        public abstract class usrContractLossPct : PX.Data.BQL.BqlDecimal.Field<usrContractLossPct> { }
        #endregion

        #region UsrContractSurcharge
        [PXDBDecimal(2, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Surcharge, %", Enabled = false, Visible = true)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrContractSurcharge>))]
        public decimal? UsrContractSurcharge { get; set; }
        public abstract class usrContractSurcharge : PX.Data.BQL.BqlDecimal.Field<usrContractSurcharge> { }
        #endregion

        #region UsrContractIncrement
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Increment", Enabled = false)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrContractIncrement>))]
        public decimal? UsrContractIncrement { get; set; }
        public abstract class usrContractIncrement : PX.Data.BQL.BqlDecimal.Field<usrContractIncrement> { }
        #endregion

        #region UsrMatrixStep
        [PXDBDecimal(2)]
        [PXUIField(DisplayName = "Matrix Step", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrMatrixStep>))]
        public decimal? UsrMatrixStep { get; set; }
        public abstract class usrMatrixStep : PX.Data.BQL.BqlDecimal.Field<usrMatrixStep> { }
        #endregion

        #region UsrSalesPrice
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Vendor Price", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public decimal? UsrSalesPrice { get; set; }
        public abstract class usrSalesPrice : PX.Data.BQL.BqlDecimal.Field<usrSalesPrice> { }
        #endregion

        #region UsrBasisPrice
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Basis Price", IsReadOnly = true)]
        public decimal? UsrBasisPrice { get; set; }
        public abstract class usrBasisPrice : PX.Data.BQL.BqlDecimal.Field<usrBasisPrice> { }
        #endregion

        #region UsrBasisValue
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Price / TOZ @ Basis", IsReadOnly = true)]
        public decimal? UsrBasisValue { get; set; }
        public abstract class usrBasisValue : PX.Data.BQL.BqlDecimal.Field<usrBasisValue> { }
        #endregion

        #region UsrBaseFabricationCost
        [PXDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrFabricationCost>))]
        public decimal? UsrBaseFabricationCost { get; set; }
        public abstract class usrBaseFabricationCost : PX.Data.BQL.BqlDecimal.Field<usrBaseFabricationCost> { }
        #endregion

        #region UsrFabricationCost
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fabrication/Value Add", Enabled = false)]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrBaseFabricationCost>), typeof(SumCalc<ASCIStarINKitSpecHdrExt.usrFabricationCost>))]
        public decimal? UsrFabricationCost { get; set; }
        public abstract class usrFabricationCost : PX.Data.BQL.BqlDecimal.Field<usrFabricationCost> { }
        #endregion

        #region UsrBasePackagingCost
        [PXDecimal(4, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrPackagingCost>))]
        public decimal? UsrBasePackagingCost { get; set; }
        public abstract class usrBasePackagingCost : PX.Data.BQL.BqlDecimal.Field<usrBasePackagingCost> { }
        #endregion

        #region UsrPackagingCost
        [PXDBDecimal(4, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Packaging Cost", Enabled = false)]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrBasePackagingCost>), typeof(SumCalc<ASCIStarINKitSpecHdrExt.usrPackagingCost>))]
        public decimal? UsrPackagingCost { get; set; }
        public abstract class usrPackagingCost : PX.Data.BQL.BqlDecimal.Field<usrPackagingCost> { }
        #endregion

        #region UsrBasePackagingLaborCost
        [PXDecimal(4, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrPackagingLaborCost>))]
        public decimal? UsrBasePackagingLaborCost { get; set; }
        public abstract class usrBasePackagingLaborCost : PX.Data.BQL.BqlDecimal.Field<usrBasePackagingLaborCost> { }
        #endregion

        #region UsrPackagingLaborCost
        [PXDBDecimal(4, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Labor For Packaging")]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrBasePackagingLaborCost>), typeof(SumCalc<ASCIStarINKitSpecHdrExt.usrPackagingLaborCost>))]
        public decimal? UsrPackagingLaborCost { get; set; }
        public abstract class usrPackagingLaborCost : PX.Data.BQL.BqlDecimal.Field<usrPackagingLaborCost> { }
        #endregion

        #region UsrLaborCost
        [PXDBDecimal(4, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "In-house Labor Cost", Enabled = false)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrLaborCost>))]
        public decimal? UsrLaborCost { get; set; }
        public abstract class usrLaborCost : PX.Data.BQL.BqlDecimal.Field<usrLaborCost> { }
        #endregion

        #region UsrBaseMaterialCost
        [PXDecimal(4, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrOtherMaterialsCost>))]
        public decimal? UsrBaseMaterialCost { get; set; }
        public abstract class usrBaseMaterialCost : PX.Data.BQL.BqlDecimal.Field<usrBaseMaterialCost> { }
        #endregion

        #region UsrOtherMaterialsCost
        [PXDBDecimal(4, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Materials Cost", Enabled = false)]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrBaseMaterialCost>), typeof(SumCalc<ASCIStarINKitSpecHdrExt.usrOtherMaterialsCost>))]
        public decimal? UsrOtherMaterialsCost { get; set; }
        public abstract class usrOtherMaterialsCost : PX.Data.BQL.BqlDecimal.Field<usrOtherMaterialsCost> { }
        #endregion

        #region UsrOtherCost
        [PXDBDecimal(4, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Other Cost", Enabled = false, Visible = false)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrOtherCost>))]
        public decimal? UsrOtherCost { get; set; }
        public abstract class usrOtherCost : PX.Data.BQL.BqlDecimal.Field<usrOtherCost> { }
        #endregion

        #region UsrFreightCost
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Freight Cost", Enabled = false)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrFreightCost>))]
        public decimal? UsrFreightCost { get; set; }
        public abstract class usrFreightCost : PX.Data.BQL.BqlDecimal.Field<usrFreightCost> { }
        #endregion

        #region UsrHandlingCost
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Handling Cost", Enabled = false)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrHandlingCost>))]
        public decimal? UsrHandlingCost { get; set; }
        public abstract class usrHandlingCost : PX.Data.BQL.BqlDecimal.Field<usrHandlingCost> { }
        #endregion

        #region UsrDutyCost
        [PXDBDecimal(4, MinValue = 0, MaxValue = 1000)]
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

        #region UsrCommodityType
        [PXString(1)]
        [PXUIField(DisplayName = "Commodity Type")]
        [CommodityType.List]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrCommodityType>))]
        public string UsrCommodityType { get; set; }
        public abstract class usrCommodityType : PX.Data.BQL.BqlBool.Field<usrCommodityType> { }
        #endregion

        #region Implementation Unneeded Interface's fields

        [PXInt]
        [PXFormula(typeof(INKitSpecStkDet.compInventoryID))]
        public int? InventoryID { get; set; }
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        [PXDecimal]
        public decimal? UsrEstLandedCost { get; set; }
        public abstract class usrEstLandedCost : PX.Data.BQL.BqlBool.Field<usrEstLandedCost> { }

        [PXDecimal]
        public decimal? UsrPreciousMetalCost { get; set; }
        public abstract class usrPreciousMetalCost : PX.Data.BQL.BqlDecimal.Field<usrPreciousMetalCost> { }

        [PXDecimal]
        public decimal? UsrDutyCostPct { get; set; }
        public abstract class usrDutyCostPct : PX.Data.BQL.BqlDecimal.Field<usrDutyCostPct> { }

        #endregion
    }
}