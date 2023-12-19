using ASCJewelryLibrary.Common.DTO.Interfaces;
using ASCJewelryLibrary.IN.CacheExt;
using ASCJewelryLibrary.INKit.Descriptor;
using ASCJewelryLibrary.INKit.Interfaces;
using PX.Data;
using PX.Objects.IN;
using System;
using static ASCJewelryLibrary.Common.Descriptor.ASCJConstants;

namespace ASCJewelryLibrary.INKit.CacheExt
{
    [Serializable]
    [PXCacheName("IN Kit Spec Stock Extension")]
    public class ASCJINKitSpecStkDetExt : PXCacheExtension<PX.Objects.IN.INKitSpecStkDet>, IASCJItemCostSpecDTO, IASCJCostRollup
    {
        public static bool IsActive() => true;

        #region UsrASCJUnitCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Unit Cost")]
        [PXFormula(typeof(Add<Add<Add<Add<usrASCJPreciousMetalCost, usrASCJOtherMaterialsCost>, usrASCJFabricationCost>, usrASCJPackagingCost>, usrASCJPackagingLaborCost>))]
        public decimal? UsrASCJUnitCost { get; set; }
        public abstract class usrASCJUnitCost : PX.Data.BQL.BqlDecimal.Field<usrASCJUnitCost> { }
        #endregion

        #region UsrASCJExtCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Ext Cost", Enabled = false)]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrASCJUnitCost>))]
        [ASCJCostAssignment]
        public decimal? UsrASCJExtCost { get; set; }
        public abstract class usrASCJExtCost : PX.Data.BQL.BqlDecimal.Field<usrASCJExtCost> { }
        #endregion

        #region UsrASCJCostingType
        [PXDBString(1, IsUnicode = true, InputMask = "")]
        [CostingType.ASCJList]
        [PXUIField(DisplayName = "Costing Type")]
        public string UsrASCJCostingType { get; set; }
        public abstract class usrASCJCostingType : PX.Data.BQL.BqlString.Field<usrASCJCostingType> { }
        #endregion

        #region UsrASCJCostRollupType
        [PXDBString(1, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Rollup Type", Required = true)]
        [PXDefault(CostRollupType.PreciousMetal)]
        [CostRollupType.ASCJList]
        //[PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCJINInventoryItemExt.usrASCJCostRollupType>))]
        public string UsrASCJCostRollupType { get; set; }
        public abstract class usrASCJCostRollupType : PX.Data.BQL.BqlString.Field<usrASCJCostRollupType> { }
        #endregion

        #region UsrASCJBaseGoldGrams
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCJINInventoryItemExt.usrASCJActualGRAMGold>))]
        public decimal? UsrASCJBaseGoldGrams { get; set; }
        public abstract class usrASCJBaseGoldGrams : PX.Data.BQL.BqlDecimal.Field<usrASCJBaseGoldGrams> { }
        #endregion

        #region UsrASCJActualGRAMGold
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Gold, Grams", IsReadOnly = true)]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrASCJBaseGoldGrams>), typeof(SumCalc<ASCJINKitSpecHdrExt.usrASCJActualGRAMGold>))]
        public decimal? UsrASCJActualGRAMGold { get; set; }
        public abstract class usrASCJActualGRAMGold : PX.Data.BQL.BqlDecimal.Field<usrASCJActualGRAMGold> { }
        #endregion

        #region UsrASCJBaseFineGoldGrams
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCJINInventoryItemExt.usrASCJPricingGRAMGold>))]
        public decimal? UsrASCJBaseFineGoldGrams { get; set; }
        public abstract class usrASCJBaseFineGoldGrams : PX.Data.BQL.BqlDecimal.Field<usrASCJBaseFineGoldGrams> { }
        #endregion

        #region UsrASCJPricingGRAMGold
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fine Gold, Grams", IsReadOnly = true)]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrASCJBaseFineGoldGrams>), typeof(SumCalc<ASCJINKitSpecHdrExt.usrASCJPricingGRAMGold>))]
        public decimal? UsrASCJPricingGRAMGold { get; set; }
        public abstract class usrASCJPricingGRAMGold : PX.Data.BQL.BqlDecimal.Field<usrASCJPricingGRAMGold> { }
        #endregion

        #region UsrASCJBaseSilverGrams
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCJINInventoryItemExt.usrASCJActualGRAMSilver>))]
        public decimal? UsrASCJBaseSilverGrams { get; set; }
        public abstract class usrASCJBaseSilverGrams : PX.Data.BQL.BqlDecimal.Field<usrASCJBaseSilverGrams> { }
        #endregion

        #region UsrASCJActualGRAMSilver
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Silver, Grams", IsReadOnly =true)]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrASCJBaseSilverGrams>), typeof(SumCalc<ASCJINKitSpecHdrExt.usrASCJActualGRAMSilver>))]
        public decimal? UsrASCJActualGRAMSilver { get; set; }
        public abstract class usrASCJActualGRAMSilver : PX.Data.BQL.BqlDecimal.Field<usrASCJActualGRAMSilver> { }
        #endregion

        #region UsrASCJBaseFineSilverGrams
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCJINInventoryItemExt.usrASCJPricingGRAMSilver>))]
        public decimal? UsrASCJBaseFineSilverGrams { get; set; }
        public abstract class usrASCJBaseFineSilverGrams : PX.Data.BQL.BqlDecimal.Field<usrASCJBaseFineSilverGrams> { }
        #endregion

        #region UsrASCJPricingGRAMSilver
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fine Silver, Grams", IsReadOnly = true)]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrASCJBaseFineSilverGrams>), typeof(SumCalc<ASCJINKitSpecHdrExt.usrASCJPricingGRAMSilver>))]
        public decimal? UsrASCJPricingGRAMSilver { get; set; }
        public abstract class usrASCJPricingGRAMSilver : PX.Data.BQL.BqlDecimal.Field<usrASCJPricingGRAMSilver> { }
        #endregion

        #region UsrASCJContractLossPct
        [PXDBDecimal(2, MinValue = 0, MaxValue = 100)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Metal Loss, %", Enabled = false)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCJINInventoryItemExt.usrASCJContractLossPct>))]
        public decimal? UsrASCJContractLossPct { get; set; }
        public abstract class usrASCJContractLossPct : PX.Data.BQL.BqlDecimal.Field<usrASCJContractLossPct> { }
        #endregion

        #region UsrASCJContractSurcharge
        [PXDBDecimal(2, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Surcharge / Loss, %", Enabled = false, Visible = true)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCJINInventoryItemExt.usrASCJContractSurcharge>))]
        public decimal? UsrASCJContractSurcharge { get; set; }
        public abstract class usrASCJContractSurcharge : PX.Data.BQL.BqlDecimal.Field<usrASCJContractSurcharge> { }
        #endregion

        #region UsrASCJContractIncrement
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Increment", Enabled = false)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCJINInventoryItemExt.usrASCJContractIncrement>))]
        public decimal? UsrASCJContractIncrement { get; set; }
        public abstract class usrASCJContractIncrement : PX.Data.BQL.BqlDecimal.Field<usrASCJContractIncrement> { }
        #endregion

        #region UsrASCJMatrixStep
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Matrix Step", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCJINInventoryItemExt.usrASCJMatrixStep>))]
        public decimal? UsrASCJMatrixStep { get; set; }
        public abstract class usrASCJMatrixStep : PX.Data.BQL.BqlDecimal.Field<usrASCJMatrixStep> { }
        #endregion

        #region UsrASCJSalesPrice
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Vendor Price", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public decimal? UsrASCJSalesPrice { get; set; }
        public abstract class usrASCJSalesPrice : PX.Data.BQL.BqlDecimal.Field<usrASCJSalesPrice> { }
        #endregion

        #region UsrASCJBasisPrice
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Basis Price", IsReadOnly = true)]
        public decimal? UsrASCJBasisPrice { get; set; }
        public abstract class usrASCJBasisPrice : PX.Data.BQL.BqlDecimal.Field<usrASCJBasisPrice> { }
        #endregion

        #region UsrASCJBasisValue
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Price / TOZ @ Basis", IsReadOnly = true)]
        public decimal? UsrASCJBasisValue { get; set; }
        public abstract class usrASCJBasisValue : PX.Data.BQL.BqlDecimal.Field<usrASCJBasisValue> { }
        #endregion

        #region UsrASCJBaseFabricationCost
        [PXDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCJINInventoryItemExt.usrASCJFabricationCost>))]
        public decimal? UsrASCJBaseFabricationCost { get; set; }
        public abstract class usrASCJBaseFabricationCost : PX.Data.BQL.BqlDecimal.Field<usrASCJBaseFabricationCost> { }
        #endregion

        #region UsrASCJFabricationCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fabrication/Value Add", Enabled = false)]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrASCJBaseFabricationCost>), typeof(SumCalc<ASCJINKitSpecHdrExt.usrASCJFabricationCost>))]
        public decimal? UsrASCJFabricationCost { get; set; }
        public abstract class usrASCJFabricationCost : PX.Data.BQL.BqlDecimal.Field<usrASCJFabricationCost> { }
        #endregion

        #region UsrASCJBasePackagingCost
        [PXDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCJINInventoryItemExt.usrASCJPackagingCost>))]
        public decimal? UsrASCJBasePackagingCost { get; set; }
        public abstract class usrASCJBasePackagingCost : PX.Data.BQL.BqlDecimal.Field<usrASCJBasePackagingCost> { }
        #endregion

        #region UsrASCJPackagingCost
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Packaging Cost", Enabled = false)]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrASCJBasePackagingCost>), typeof(SumCalc<ASCJINKitSpecHdrExt.usrASCJPackagingCost>))]
        public decimal? UsrASCJPackagingCost { get; set; }
        public abstract class usrASCJPackagingCost : PX.Data.BQL.BqlDecimal.Field<usrASCJPackagingCost> { }
        #endregion

        #region UsrASCJBasePackagingLaborCost
        [PXDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCJINInventoryItemExt.usrASCJPackagingLaborCost>))]
        public decimal? UsrASCJBasePackagingLaborCost { get; set; }
        public abstract class usrASCJBasePackagingLaborCost : PX.Data.BQL.BqlDecimal.Field<usrASCJBasePackagingLaborCost> { }
        #endregion

        #region UsrASCJPackagingLaborCost
        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Labor For Packaging")]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrASCJBasePackagingLaborCost>), typeof(SumCalc<ASCJINKitSpecHdrExt.usrASCJPackagingLaborCost>))]
        public decimal? UsrASCJPackagingLaborCost { get; set; }
        public abstract class usrASCJPackagingLaborCost : PX.Data.BQL.BqlDecimal.Field<usrASCJPackagingLaborCost> { }
        #endregion

        #region UsrASCJLaborCost
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "In-house Labor Cost", Enabled = false)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCJINInventoryItemExt.usrASCJLaborCost>))]
        public decimal? UsrASCJLaborCost { get; set; }
        public abstract class usrASCJLaborCost : PX.Data.BQL.BqlDecimal.Field<usrASCJLaborCost> { }
        #endregion

        #region UsrASCJBaseMaterialCost
        [PXDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCJINInventoryItemExt.usrASCJOtherMaterialsCost>))]
        public decimal? UsrASCJBaseMaterialCost { get; set; }
        public abstract class usrASCJBaseMaterialCost : PX.Data.BQL.BqlDecimal.Field<usrASCJBaseMaterialCost> { }
        #endregion

        #region UsrASCJOtherMaterialsCost
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Materials Cost", Enabled = false)]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrASCJBaseMaterialCost>), typeof(SumCalc<ASCJINKitSpecHdrExt.usrASCJOtherMaterialsCost>))]
        public decimal? UsrASCJOtherMaterialsCost { get; set; }
        public abstract class usrASCJOtherMaterialsCost : PX.Data.BQL.BqlDecimal.Field<usrASCJOtherMaterialsCost> { }
        #endregion

        #region UsrASCJOtherCost
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Other Cost", Enabled = false, Visible = false)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCJINInventoryItemExt.usrASCJOtherCost>))]
        public decimal? UsrASCJOtherCost { get; set; }
        public abstract class usrASCJOtherCost : PX.Data.BQL.BqlDecimal.Field<usrASCJOtherCost> { }
        #endregion

        #region UsrASCJFreightCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Freight Cost", Enabled = false)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCJINInventoryItemExt.usrASCJFreightCost>))]
        public decimal? UsrASCJFreightCost { get; set; }
        public abstract class usrASCJFreightCost : PX.Data.BQL.BqlDecimal.Field<usrASCJFreightCost> { }
        #endregion

        #region UsrASCJHandlingCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Handling Cost", Enabled = false)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCJINInventoryItemExt.usrASCJHandlingCost>))]
        public decimal? UsrASCJHandlingCost { get; set; }
        public abstract class usrASCJHandlingCost : PX.Data.BQL.BqlDecimal.Field<usrASCJHandlingCost> { }
        #endregion

        #region UsrASCJDutyCost
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Duty Cost", Enabled = false)]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCJINInventoryItemExt.usrASCJDutyCost>))]
        public decimal? UsrASCJDutyCost { get; set; }
        public abstract class usrASCJDutyCost : PX.Data.BQL.BqlDecimal.Field<usrASCJDutyCost> { }
        #endregion

        #region UsrASCJIsMetal 
        [PXDBBool]
        [PXUIField(DisplayName = "Is Metal", Visible = false, Enabled = false)]
        public bool? UsrASCJIsMetal { get; set; }
        public abstract class usrASCJIsMetal : PX.Data.BQL.BqlBool.Field<usrASCJIsMetal> { }
        #endregion

        #region Implementation Unneeded Interface's fields

        [PXInt]
        [PXFormula(typeof(INKitSpecStkDet.compInventoryID))]
        public int? UsrASCJInventoryID { get; set; }
        public abstract class usrASCJInventoryID : PX.Data.BQL.BqlInt.Field<usrASCJInventoryID> { }

        [PXDecimal]
        public decimal? UsrASCJEstLandedCost { get; set; }
        public abstract class usrASCJEstLandedCost : PX.Data.BQL.BqlBool.Field<usrASCJEstLandedCost> { }

        [PXDecimal]
        public decimal? UsrASCJPreciousMetalCost { get; set; }
        public abstract class usrASCJPreciousMetalCost : PX.Data.BQL.BqlBool.Field<usrASCJPreciousMetalCost> { }

        [PXDecimal]
        public decimal? UsrASCJDutyCostPct { get; set; }
        public abstract class usrASCJDutyCostPct : PX.Data.BQL.BqlBool.Field<usrASCJDutyCostPct> { }

        [PXString]
        public string UsrASCJCommodityType { get; set; }
        public abstract class usrASCJCommodityType : PX.Data.BQL.BqlBool.Field<usrASCJCommodityType> { }
        #endregion
    }
}