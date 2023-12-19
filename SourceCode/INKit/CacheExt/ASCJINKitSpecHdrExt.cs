using ASCJewelryLibrary.Common.DTO.Interfaces;
using ASCJewelryLibrary.IN.CacheExt;
using PX.Data;
using PX.Objects.IN;
using System;

namespace ASCJewelryLibrary.INKit.CacheExt
{
    [Serializable]
    [PXCacheName("IN Kit Spec Hdr Extension")]
    public sealed class ASCJINKitSpecHdrExt : PXCacheExtension<INKitSpecHdr>, IASCJItemCostSpecDTO
    {
        public static bool IsActive() => true;

        #region UsrASCJVQuoteLineCtr
        [PXDBInt]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        public int? UsrASCJVQuoteLineCtr { get; set; }
        public abstract class usrASCJVQuoteLineCtr : PX.Data.BQL.BqlInt.Field<usrASCJVQuoteLineCtr> { }
        #endregion

        #region UsrASCJLegacyID
        [PXDBString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Legacy ID")]
        public string UsrASCJLegacyID { get; set; }
        public abstract class usrASCJLegacyID : PX.Data.BQL.BqlString.Field<usrASCJLegacyID> { }
        #endregion

        #region UsrASCJLegacyShortRef
        [PXDBString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Legacy Short Ref")]
        public string UsrASCJLegacyShortRef { get; set; }
        public abstract class usrASCJlegacyShortRef : PX.Data.BQL.BqlString.Field<usrASCJlegacyShortRef> { }
        #endregion

        #region UsrASCJActualGRAMGold
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Total Gold Grams", Enabled = false)]
        public decimal? UsrASCJActualGRAMGold { get; set; }
        public abstract class usrASCJActualGRAMGold : PX.Data.BQL.BqlDecimal.Field<usrASCJActualGRAMGold> { }
        #endregion

        #region UsrASCJPricingGRAMGold
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Total Fine Gold Grams", Enabled = false)]
        public decimal? UsrASCJPricingGRAMGold { get; set; }
        public abstract class usrASCJPricingGRAMGold : PX.Data.BQL.BqlDecimal.Field<usrASCJPricingGRAMGold> { }
        #endregion

        #region UsrASCJPricingGRAMSilver
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Total Fine Silver Grams", Enabled = false)]
        public decimal? UsrASCJPricingGRAMSilver { get; set; }
        public abstract class usrASCJPricingGRAMSilver : PX.Data.BQL.BqlDecimal.Field<usrASCJPricingGRAMSilver> { }
        #endregion

        #region UsrASCJPricingGRAMSilverRight
        [PXDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Total Fine Silver Grams", Enabled = false)]
        [PXFormula(typeof(usrASCJPricingGRAMSilver))]
        public decimal? UsrASCJPricingGRAMSilverRight { get; set; }
        public abstract class usrASCJPricingGRAMSilverRight : PX.Data.BQL.BqlDecimal.Field<usrASCJPricingGRAMSilverRight> { }
        #endregion

        #region UsrASCJActualGRAMSilver
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Total Silver Grams", Enabled = false)]
        public decimal? UsrASCJActualGRAMSilver { get; set; }
        public abstract class usrASCJActualGRAMSilver : PX.Data.BQL.BqlDecimal.Field<usrASCJActualGRAMSilver> { }
        #endregion

        #region UsrASCJActualGRAMSilverRight
        [PXDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Total Silver Grams", Enabled = false)]
        [PXFormula(typeof(usrASCJActualGRAMSilver))]
        public decimal? UsrASCJActualGRAMSilverRight { get; set; }
        public abstract class usrASCJActualGRAMSilverRight : PX.Data.BQL.BqlDecimal.Field<usrASCJActualGRAMSilverRight> { }
        #endregion

        #region UsrASCJContractIncrement
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Increment", IsReadOnly = true)]
        public decimal? UsrASCJContractIncrement { get; set; }
        public abstract class usrASCJContractIncrement : PX.Data.BQL.BqlDecimal.Field<usrASCJContractIncrement> { }
        #endregion

        #region UsrASCJContractSurcharge
        [PXDBDecimal(2, MinValue = 0, MaxValue = 100)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Total Surcharge / Loss %")]
        public decimal? UsrASCJContractSurcharge { get; set; }
        public abstract class usrASCJContractSurcharge : PX.Data.BQL.BqlDecimal.Field<usrASCJContractSurcharge> { }
        #endregion

        #region UsrASCJContractLossPct
        [PXDBDecimal(2, MinValue = 0, MaxValue = 100)]
        [PXUIField(DisplayName = "Total Metal Loss, %")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrASCJContractLossPct { get; set; }
        public abstract class usrASCJContractLossPct : PX.Data.BQL.BqlDecimal.Field<usrASCJContractLossPct> { }
        #endregion

        #region UsrASCJUnitCost
        [PXDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Unit Cost", Visibility = PXUIVisibility.Visible, Enabled = false)]
        [PXFormula(typeof(Add<Add<Add<Add<usrASCJPreciousMetalCost, usrASCJOtherMaterialsCost>, usrASCJFabricationCost>, usrASCJPackagingCost>, usrASCJPackagingLaborCost>))]
        public decimal? UsrASCJUnitCost { get; set; }
        public abstract class usrASCJUnitCost : PX.Data.BQL.BqlDecimal.Field<usrASCJUnitCost> { }
        #endregion

        #region UsrASCJPreciousMetalCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Precious Metal Cost")]
        public decimal? UsrASCJPreciousMetalCost { get; set; }
        public abstract class usrASCJPreciousMetalCost : PX.Data.BQL.BqlDecimal.Field<usrASCJPreciousMetalCost> { }
        #endregion

        #region UsrASCJFabricationCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fabrication/Value Add")]
        public decimal? UsrASCJFabricationCost { get; set; }
        public abstract class usrASCJFabricationCost : PX.Data.BQL.BqlDecimal.Field<usrASCJFabricationCost> { }
        #endregion

        #region UsrASCJPackagingCost
        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Packaging Cost")]
        public decimal? UsrASCJPackagingCost { get; set; }
        public abstract class usrASCJPackagingCost : PX.Data.BQL.BqlDecimal.Field<usrASCJPackagingCost> { }
        #endregion

        #region UsrASCJLaborCost
        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "In-house Labor Cost")]
        public decimal? UsrASCJLaborCost { get; set; }
        public abstract class usrASCJLaborCost : PX.Data.BQL.BqlDecimal.Field<usrASCJLaborCost> { }
        #endregion

        #region UsrASCJPackagingLaborCost
        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Labor Packaging")]
        public decimal? UsrASCJPackagingLaborCost { get; set; }
        public abstract class usrASCJPackagingLaborCost : PX.Data.BQL.BqlDecimal.Field<usrASCJPackagingLaborCost> { }
        #endregion

        #region UsrASCJOtherMaterialsCost
        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Other Materials Cost")]
        public decimal? UsrASCJOtherMaterialsCost { get; set; }
        public abstract class usrASCJOtherMaterialsCost : PX.Data.BQL.BqlDecimal.Field<usrASCJOtherMaterialsCost> { }
        #endregion

        #region UsrASCJOtherCost
        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Other Cost", Visible = false)]
        public decimal? UsrASCJOtherCost { get; set; }
        public abstract class usrASCJOtherCost : PX.Data.BQL.BqlDecimal.Field<usrASCJOtherCost> { }
        #endregion

        #region UsrASCJEstLandedCost
        [PXDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Est. Landed Cost", Visibility = PXUIVisibility.Visible, Enabled = false)]
        [PXFormula(typeof(Add<Add<Add<Add<usrASCJUnitCost, usrASCJHandlingCost>, usrASCJFreightCost>, usrASCJLaborCost>, usrASCJDutyCost>))]
        public decimal? UsrASCJEstLandedCost { get; set; }
        public abstract class usrASCJEstLandedCost : PX.Data.BQL.BqlDecimal.Field<usrASCJEstLandedCost> { }
        #endregion

        #region UsrASCJFreightCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Freight Cost")]
        public decimal? UsrASCJFreightCost { get; set; }
        public abstract class usrASCJFreightCost : PX.Data.BQL.BqlDecimal.Field<usrASCJFreightCost> { }
        #endregion

        #region UsrASCJHandlingCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Handling Cost")]
        public decimal? UsrASCJHandlingCost { get; set; }
        public abstract class usrASCJHandlingCost : PX.Data.BQL.BqlDecimal.Field<usrASCJHandlingCost> { }
        #endregion

        #region UsrASCJDutyCost
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Duty Cost")]
        public decimal? UsrASCJDutyCost { get; set; }
        public abstract class usrASCJDutyCost : PX.Data.BQL.BqlDecimal.Field<usrASCJDutyCost> { }
        #endregion

        #region UsrASCJDutyCostPct
        [PXDBDecimal(2, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Duty, %")]
        public decimal? UsrASCJDutyCostPct { get; set; }
        public abstract class usrASCJDutyCostPct : PX.Data.BQL.BqlDecimal.Field<usrASCJDutyCostPct> { }
        #endregion

        #region UsrASCJBasisValue
        [PXPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Price / TOZ @ Basis", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public decimal? UsrASCJBasisValue { get; set; }
        public abstract class usrASCJBasisValue : PX.Data.BQL.BqlDecimal.Field<usrASCJBasisValue> { }
        #endregion

        #region UsrASCJMatrixStep
        [PXDBDecimal(6, MinValue = 0, MaxValue = 10)]
        [PXUIField(DisplayName = "Matrix Step")]
        [PXDefault(TypeCode.Decimal, "0.500000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<INKitSpecHdr.kitInventoryID, ASCJINInventoryItemExt.usrASCJMatrixStep>))]
        public decimal? UsrASCJMatrixStep { get; set; }
        public abstract class usrASCJMatrixStep : PX.Data.BQL.BqlDecimal.Field<usrASCJMatrixStep> { }
        #endregion

        #region Implementation Unneeded Interface's fields

        [PXInt]
        public int? UsrASCJInventoryID { get; set; }
        public abstract class usrASCJInventoryID : PX.Data.BQL.BqlInt.Field<usrASCJInventoryID> { }

        [PXString]
        public string UsrASCJCostingType { get; set; }
        public abstract class usrASCJCostingType : PX.Data.BQL.BqlDecimal.Field<usrASCJCostingType> { }

        [PXString]
        public string UsrASCJCommodityType { get; set; }
        public abstract class usrASCJCommodityType : PX.Data.BQL.BqlDecimal.Field<usrASCJCommodityType> { }
        #endregion
    }
}