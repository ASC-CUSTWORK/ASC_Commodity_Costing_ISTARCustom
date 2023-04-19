using ASCISTARCustom.Common.Descriptor;
using ASCISTARCustom.Inventory.Descriptor.Constants;
using PX.Data;
using PX.Objects.IN;
using System;
using static ASCISTARCustom.Common.Descriptor.ASCIStarConstants;

namespace ASCISTARCustom
{
    public class ASCIStarINInventoryItemExt : PXCacheExtension<InventoryItem>
    {
        public static bool IsActive() => true;

        #region UsrLegacyShortRef
        [PXDBString(30)]
        [PXUIField(DisplayName = "Legacy Short Ref")]
        public string UsrLegacyShortRef { get; set; }
        public abstract class usrLegacyShortRef : PX.Data.BQL.BqlString.Field<usrLegacyShortRef> { }
        #endregion

        #region UsrLegacyID
        [PXDBString(30)]
        [PXUIField(DisplayName = "Legacy ID")]
        public string UsrLegacyID { get; set; }
        public abstract class usrLegacyID : PX.Data.BQL.BqlString.Field<usrLegacyID> { }
        #endregion

        //#region UsrCommodity
        //[PXDBString(1)]
        //[PXUIField(DisplayName = "Metal")]
        //[ASCIStarConstants.CommodityType.List]
        //[PXDefault(ASCIStarConstants.CommodityType.Undefined, PersistingCheck = PXPersistingCheck.Nothing)]
        //public string UsrCommodity { get; set; }
        //public abstract class usrCommodity : PX.Data.BQL.BqlString.Field<usrCommodity> { }
        //#endregion

        #region UsrPriceAsID
        [PXDBInt()]
        [PXUIField(DisplayName = "Price as Item")]
        [PXSelector(typeof(Search2<InventoryItem.inventoryID, LeftJoin<INItemClass, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>, Where<INItemClass.itemClassCD, Equal<ASCIStarConstants.CommodityClass>>>),
            typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr)
                        , SubstituteKey = typeof(InventoryItem.inventoryCD), DescriptionField = typeof(InventoryItem.descr))]
        public int? UsrPriceAsID { get; set; }
        public abstract class usrPriceAsID : PX.Data.BQL.BqlInt.Field<usrPriceAsID> { }
        #endregion

        #region ToUnit
        // Acuminator disable once PX1023 MultipleTypeAttributesOnProperty [Justification]
        [INUnit(DisplayName = "Price To", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault("EACH", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXRestrictor(typeof(Where<ASCIStarINUnitExt.usrCommodity, IsNotNull>), "Market Cost requires that a conversion is selected", typeof(INUnit.fromUnit))]
        public string UsrPriceToUnit { get; set; }
        public abstract class usrPriceToUnit : PX.Data.BQL.BqlString.Field<usrPriceToUnit> { }

        #endregion

        #region UsrPricingGRAMGold
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Fine Gold, Grams")]
        public Decimal? UsrPricingGRAMGold { get; set; }
        public abstract class usrPricingGRAMGold : PX.Data.BQL.BqlDecimal.Field<usrPricingGRAMGold> { }
        #endregion

        #region UsrPricingGRAMSilver
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Fine Silver, Grams")]
        public decimal? UsrPricingGRAMSilver { get; set; }
        public abstract class usrPricingGRAMSilver : PX.Data.BQL.BqlDecimal.Field<usrPricingGRAMSilver> { }
        #endregion

        #region UsrActualGRAMGold
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Gold, Grams")]
        public Decimal? UsrActualGRAMGold { get; set; }
        public abstract class usrActualGRAMGold : PX.Data.BQL.BqlDecimal.Field<usrActualGRAMGold> { }
        #endregion

        #region UsrActualGRAMSilver
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Silver, Grams")]
        public Decimal? UsrActualGRAMSilver { get; set; }
        public abstract class usrActualGRAMSilver : PX.Data.BQL.BqlDecimal.Field<usrActualGRAMSilver> { }
        #endregion

        #region UsrContractWgt
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Contract Wgt (g)")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrContractWgt { get; set; }
        public abstract class usrContractWgt : PX.Data.BQL.BqlDecimal.Field<usrContractWgt> { }
        #endregion

        #region UsrCostingType
        [PXDBString(1, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Costing Type")]
        [ASCIStarCostingType.List]
        [PXDefault(ASCIStarCostingType.ContractCost, PersistingCheck = PXPersistingCheck.Nothing)]
        public string UsrCostingType { get; set; }
        public abstract class usrCostingType : PX.Data.BQL.BqlString.Field<usrCostingType> { }
        #endregion

        #region UsrCostRollupType
        [PXDBString(1, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Rollup Type")]
        [ASCIStarCostRollupType.List]
        [PXDefault(ASCIStarCostRollupType.Blank, PersistingCheck = PXPersistingCheck.Nothing)]
        public string UsrCostRollupType { get; set; }
        public abstract class usrCostRollupType : PX.Data.BQL.BqlString.Field<usrCostRollupType> { }
        #endregion

        #region UsrContractPrice
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Contract Price")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrContractPrice { get; set; }
        public abstract class usrContractPrice : PX.Data.BQL.BqlDecimal.Field<usrContractPrice> { }
        #endregion

        #region UsrContractLossPct
        [PXDBDecimal(4, MinValue = 0, MaxValue = 100)]
        [PXUIField(DisplayName = "Metal Loss, %")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrContractLossPct { get; set; }
        public abstract class usrContractLossPct : PX.Data.BQL.BqlDecimal.Field<usrContractLossPct> { }
        #endregion

        #region UsrContractIncrement
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Increment")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrContractIncrement { get; set; }
        public abstract class usrContractIncrement : PX.Data.BQL.BqlDecimal.Field<usrContractIncrement> { }
        #endregion

        #region UsrMatrixStep
        [PXDBDecimal(6, MinValue = 0, MaxValue = 10)]
        [PXUIField(DisplayName = "Matrix Step")]
        [PXDefault(TypeCode.Decimal, "0.500000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrMatrixStep { get; set; }
        public abstract class usrMatrixStep : PX.Data.BQL.BqlDecimal.Field<usrMatrixStep> { }
        #endregion

        #region UsrFloor
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Floor", IsReadOnly = true)]
        public decimal? UsrFloor { get; set; }
        public abstract class usrFloor : PX.Data.BQL.BqlDecimal.Field<usrFloor> { }
        #endregion

        #region UsrCeiling
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Ceiling", IsReadOnly = true)]
        public decimal? UsrCeiling { get; set; }
        public abstract class usrCeiling : PX.Data.BQL.BqlDecimal.Field<usrCeiling> { }
        #endregion

        #region UsrContractSurcharge
        [PXDBDecimal(6, MinValue = 0, MaxValue = 100)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Surcharge %", Visible = true)]
        public decimal? UsrContractSurcharge { get; set; }
        public abstract class usrContractSurcharge : PX.Data.BQL.BqlDecimal.Field<usrContractSurcharge> { }
        #endregion

        #region UsrCommodityCost
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Precious Metal Cost")]
        public Decimal? UsrCommodityCost { get; set; }
        public abstract class usrCommodityCost : PX.Data.BQL.BqlDecimal.Field<usrCommodityCost> { }
        #endregion

        #region UsrMaterialsCost
        [PXUIField(DisplayName = "Other Materials Cost")]
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public Decimal? UsrMaterialsCost { get; set; }
        public abstract class usrMaterialsCost : PX.Data.BQL.BqlDecimal.Field<usrMaterialsCost> { }
        #endregion

        #region UsrFabricationCost
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Fabrication/Value Add")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public Decimal? UsrFabricationCost { get; set; }
        public abstract class usrFabricationCost : PX.Data.BQL.BqlDecimal.Field<usrFabricationCost> { }
        #endregion

        #region UsrLaborCost
        [PXUIField(DisplayName = "In-house Labor Cost")]
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]

        public Decimal? UsrLaborCost { get; set; }
        public abstract class usrLaborCost : PX.Data.BQL.BqlDecimal.Field<usrLaborCost> { }
        #endregion

        #region UsrHandlingCost
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Handling Cost")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]

        public Decimal? UsrHandlingCost { get; set; }
        public abstract class usrHandlingCost : PX.Data.BQL.BqlDecimal.Field<usrHandlingCost> { }
        #endregion

        #region UsrFreightCost
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Freight Cost")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public Decimal? UsrFreightCost { get; set; }
        public abstract class usrFreightCost : PX.Data.BQL.BqlDecimal.Field<usrFreightCost> { }
        #endregion

        #region UsrDutyCost
        [PXUIField(DisplayName = "Duty Cost")]
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public Decimal? UsrDutyCost { get; set; }
        public abstract class usrDutyCost : PX.Data.BQL.BqlDecimal.Field<usrDutyCost> { }
        #endregion

        #region UsrDutyCostPct

        [PXUIField(DisplayName = "Duty %")]
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public Decimal? UsrDutyCostPct { get; set; }
        public abstract class usrDutyCostPct : PX.Data.BQL.BqlDecimal.Field<usrDutyCostPct> { }
        #endregion

        #region UsrOtherCost
        [PXUIField(DisplayName = "Other Cost", Visible = false)]
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public Decimal? UsrOtherCost { get; set; }
        public abstract class usrOtherCost : PX.Data.BQL.BqlDecimal.Field<usrOtherCost> { }
        #endregion

        #region UsrPackagingCost

        [PXUIField(DisplayName = "Packaging Cost")]
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public Decimal? UsrPackagingCost { get; set; }
        public abstract class usrPackagingCost : PX.Data.BQL.BqlDecimal.Field<usrPackagingCost> { }
        #endregion

        #region UsrContractCost
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Unit Cost", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public Decimal? UsrContractCost { get; set; }
        public abstract class usrContractCost : PX.Data.BQL.BqlDecimal.Field<usrContractCost> { }
        #endregion

        #region UsrUnitCost
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Est. Landed Cost", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public Decimal? UsrUnitCost { get; set; }
        public abstract class usrUnitCost : PX.Data.BQL.BqlDecimal.Field<usrUnitCost> { }
        #endregion

        #region UsrCommodityType
        [PXDBString(1)]
        [PXUIField(DisplayName = "Commodity Type")]
        [CommodityType.List]
        [PXDefault(CommodityType.Undefined, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string UsrCommodityType { get; set; }
        public abstract class usrCommodityType : PX.Data.BQL.BqlString.Field<usrCommodityType> { }
        #endregion
    }
}