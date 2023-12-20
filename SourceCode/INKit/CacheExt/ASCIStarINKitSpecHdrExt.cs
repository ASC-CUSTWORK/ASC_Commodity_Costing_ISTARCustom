using ASCISTARCustom.Common.DTO.Interfaces;
using ASCISTARCustom.IN.CacheExt;
using PX.Data;
using PX.Objects.IN;
using System;

namespace ASCISTARCustom.INKit.CacheExt
{
    public sealed class ASCIStarINKitSpecHdrExt : PXCacheExtension<INKitSpecHdr>, IASCIStarItemCostSpecDTO
    {
        public static bool IsActive() => true;

        #region UsrVQuoteLineCtr
        [PXDBInt]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        public int? UsrVQuoteLineCtr { get; set; }
        public abstract class usrVQuoteLineCtr : PX.Data.BQL.BqlInt.Field<usrVQuoteLineCtr> { }
        #endregion

        #region UsrLegacyID
        [PXDBString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Legacy ID")]
        public string UsrLegacyID { get; set; }
        public abstract class usrLegacyID : PX.Data.BQL.BqlString.Field<usrLegacyID> { }
        #endregion

        #region UsrLegacyShortRef
        [PXDBString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Legacy Short Ref")]
        public string UsrLegacyShortRef { get; set; }
        public abstract class usrlegacyShortRef : PX.Data.BQL.BqlString.Field<usrlegacyShortRef> { }
        #endregion

        #region UsrActualGRAMGold
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Total Gold Grams", Enabled = false)]
        public decimal? UsrActualGRAMGold { get; set; }
        public abstract class usrActualGRAMGold : PX.Data.BQL.BqlDecimal.Field<usrActualGRAMGold> { }
        #endregion

        #region UsrPricingGRAMGold
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Total Fine Gold Grams", Enabled = false)]
        public decimal? UsrPricingGRAMGold { get; set; }
        public abstract class usrPricingGRAMGold : PX.Data.BQL.BqlDecimal.Field<usrPricingGRAMGold> { }
        #endregion

        #region UsrPricingGRAMSilver
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Total Fine Silver Grams", Enabled = false)]
        public decimal? UsrPricingGRAMSilver { get; set; }
        public abstract class usrPricingGRAMSilver : PX.Data.BQL.BqlDecimal.Field<usrPricingGRAMSilver> { }
        #endregion

        #region UsrPricingGRAMSilverRight
        [PXDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Total Fine Silver Grams", Enabled = false)]
        [PXFormula(typeof(usrPricingGRAMSilver))]
        public decimal? UsrPricingGRAMSilverRight { get; set; }
        public abstract class usrPricingGRAMSilverRight : PX.Data.BQL.BqlDecimal.Field<usrPricingGRAMSilverRight> { }
        #endregion

        #region UsrActualGRAMSilver
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Total Silver Grams", Enabled = false)]
        public decimal? UsrActualGRAMSilver { get; set; }
        public abstract class usrActualGRAMSilver : PX.Data.BQL.BqlDecimal.Field<usrActualGRAMSilver> { }
        #endregion

        #region UsrActualGRAMSilverRight
        [PXDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Total Silver Grams", Enabled = false)]
        [PXFormula(typeof(usrActualGRAMSilver))]
        public decimal? UsrActualGRAMSilverRight { get; set; }
        public abstract class usrActualGRAMSilverRight : PX.Data.BQL.BqlDecimal.Field<usrActualGRAMSilverRight> { }
        #endregion

        #region UsrContractIncrement
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Increment", IsReadOnly = true)]
        public decimal? UsrContractIncrement { get; set; }
        public abstract class usrContractIncrement : PX.Data.BQL.BqlDecimal.Field<usrContractIncrement> { }
        #endregion

        #region UsrContractSurcharge
        [PXDBDecimal(2, MinValue = 0, MaxValue = 100)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Total Surcharge %")]
        public decimal? UsrContractSurcharge { get; set; }
        public abstract class usrContractSurcharge : PX.Data.BQL.BqlDecimal.Field<usrContractSurcharge> { }
        #endregion

        #region UsrContractLossPct
        [PXDBDecimal(2, MinValue = 0, MaxValue = 100)]
        [PXUIField(DisplayName = "Total Metal Loss, %")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrContractLossPct { get; set; }
        public abstract class usrContractLossPct : PX.Data.BQL.BqlDecimal.Field<usrContractLossPct> { }
        #endregion

        #region UsrUnitCost
        [PXDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Unit Cost", Visibility = PXUIVisibility.Visible, Enabled = false)]
        [PXFormula(typeof(Add<Add<Add<Add<usrPreciousMetalCost, usrOtherMaterialsCost>, usrFabricationCost>, usrPackagingCost>, usrPackagingLaborCost>))]
        public decimal? UsrUnitCost { get; set; }
        public abstract class usrUnitCost : PX.Data.BQL.BqlDecimal.Field<usrUnitCost> { }
        #endregion

        #region UsrPreciousMetalCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Precious Metal Cost")]
        public decimal? UsrPreciousMetalCost { get; set; }
        public abstract class usrPreciousMetalCost : PX.Data.BQL.BqlDecimal.Field<usrPreciousMetalCost> { }
        #endregion

        #region UsrFabricationCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fabrication/Value Add")]
        public decimal? UsrFabricationCost { get; set; }
        public abstract class usrFabricationCost : PX.Data.BQL.BqlDecimal.Field<usrFabricationCost> { }
        #endregion

        #region UsrPackagingCost
        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Packaging Cost")]
        public decimal? UsrPackagingCost { get; set; }
        public abstract class usrPackagingCost : PX.Data.BQL.BqlDecimal.Field<usrPackagingCost> { }
        #endregion

        #region UsrLaborCost
        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "In-house Labor Cost")]
        public decimal? UsrLaborCost { get; set; }
        public abstract class usrLaborCost : PX.Data.BQL.BqlDecimal.Field<usrLaborCost> { }
        #endregion

        #region UsrPackagingLaborCost
        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Labor Packaging")]
        public decimal? UsrPackagingLaborCost { get; set; }
        public abstract class usrPackagingLaborCost : PX.Data.BQL.BqlDecimal.Field<usrPackagingLaborCost> { }
        #endregion

        #region UsrOtherMaterialsCost
        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Other Materials Cost")]
        public decimal? UsrOtherMaterialsCost { get; set; }
        public abstract class usrOtherMaterialsCost : PX.Data.BQL.BqlDecimal.Field<usrOtherMaterialsCost> { }
        #endregion

        #region UsrOtherCost
        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Other Cost", Visible = false)]
        public decimal? UsrOtherCost { get; set; }
        public abstract class usrOtherCost : PX.Data.BQL.BqlDecimal.Field<usrOtherCost> { }
        #endregion

        #region UsrEstLandedCost
        [PXDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Est. Landed Cost", Visibility = PXUIVisibility.Visible, Enabled = false)]
        [PXFormula(typeof(Add<Add<Add<Add<usrUnitCost, usrHandlingCost>, usrFreightCost>, usrLaborCost>, usrDutyCost>))]
        public decimal? UsrEstLandedCost { get; set; }
        public abstract class usrEstLandedCost : PX.Data.BQL.BqlDecimal.Field<usrEstLandedCost> { }
        #endregion

        #region UsrFreightCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Freight Cost")]
        public decimal? UsrFreightCost { get; set; }
        public abstract class usrFreightCost : PX.Data.BQL.BqlDecimal.Field<usrFreightCost> { }
        #endregion

        #region UsrHandlingCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Handling Cost")]
        public decimal? UsrHandlingCost { get; set; }
        public abstract class usrHandlingCost : PX.Data.BQL.BqlDecimal.Field<usrHandlingCost> { }
        #endregion

        #region UsrDutyCost
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Duty Cost")]
        public decimal? UsrDutyCost { get; set; }
        public abstract class usrDutyCost : PX.Data.BQL.BqlDecimal.Field<usrDutyCost> { }
        #endregion

        #region UsrDutyCostPct
        [PXDBDecimal(2, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Duty, %")]
        public decimal? UsrDutyCostPct { get; set; }
        public abstract class usrDutyCostPct : PX.Data.BQL.BqlDecimal.Field<usrDutyCostPct> { }
        #endregion

        #region UsrBasisValue
        [PXPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Price / TOZ @ Basis", Visibility = PXUIVisibility.Visible, Enabled = false)]
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

        #region Implementation Unneeded Interface's fields

        [PXInt]
        public int? InventoryID { get; set; }
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        [PXString]
        public string UsrCostingType { get; set; }
        public abstract class usrCostingType : PX.Data.BQL.BqlDecimal.Field<usrCostingType> { }

        [PXString]
        public string UsrCommodityType { get; set; }
        public abstract class usrCommodityType : PX.Data.BQL.BqlDecimal.Field<usrCommodityType> { }
        #endregion
    }
}