using ASCJewelryLibrary.Common.Descriptor;
using ASCJewelryLibrary.Common.DTO.Interfaces;
using ASCJewelryLibrary.IN.Descriptor.Constants;
using PX.Data;
using PX.Data.BQL;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using System;
using static ASCJewelryLibrary.Common.Descriptor.ASCJConstants;

namespace ASCJewelryLibrary.IN.CacheExt
{
    [Serializable]
    [PXCacheName("Inventory Item Extension")]
    public class ASCJINInventoryItemExt : PXCacheExtension<InventoryItem>, IASCJItemCostSpecDTO
    {
        public static bool IsActive() => true;

        #region Inventory
        [PXInt]
        public virtual int? UsrASCJInventoryID { get; set; }
        public abstract class usrASCJInventoryID : PX.Data.BQL.BqlInt.Field<usrASCJInventoryID> { }
        #endregion

        #region ItemStatus
        /// The status of the Inventory Item.
        /// </summary>
        /// <value>
        /// Possible values are:
        /// <c>"AC"</c> - Active (can be used in inventory operations, such as issues and receipts),
        /// <c>"NS"</c> - No Sales (cannot be sold),
        /// <c>"NP"</c> - Style (cannot be purchased),
        /// <c>"NR"</c> - No Request (cannot be used on requisition requests),
        /// <c>"IN"</c> - Inactive,
        /// <c>"DE"</c> - Marked for Deletion.
        /// Defaults to Active (<c>"AC"</c>).
        ///// </value>
        [PXDBString(2, IsFixed = true)]
        [PXDefault("NP")]
        [PXUIField(DisplayName = "Item Status", Visibility = PXUIVisibility.SelectorVisible)]
        [ASCJINConstants.InventoryItemStatusExt.ASCJList]
        public virtual String ItemStatus { get; set; }
        public abstract class itemStatus : BqlString.Field<itemStatus> { }
        #endregion

        #region UsrASCJLegacyShortRef
        [PXDBString(30)]
        [PXUIField(DisplayName = "Legacy Short Ref")]
        public string UsrASCJLegacyShortRef { get; set; }
        public abstract class usrASCJLegacyShortRef : PX.Data.BQL.BqlString.Field<usrASCJLegacyShortRef> { }
        #endregion

        #region UsrASCJLegacyID
        [PXDBString(30)]
        [PXUIField(DisplayName = "Legacy ID")]
        public string UsrASCJLegacyID { get; set; }
        public abstract class usrASCJLegacyID : PX.Data.BQL.BqlString.Field<usrASCJLegacyID> { }
        #endregion

        // Hidden field due to logic doesn't need conversion between Commodity Items (logic always use 24K and SSS items)
        #region UsrASCJPriceAsID
        [PXDBInt()]
        [PXUIField(DisplayName = "Price as Item", Visible = false, Enabled = false)]
        [PXSelector(typeof(Search2<InventoryItem.inventoryID, LeftJoin<INItemClass, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>,
            Where<INItemClass.itemClassCD, Equal<ASCJConstants.CommodityClass>>>),
            typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr)
                        , SubstituteKey = typeof(InventoryItem.inventoryCD), DescriptionField = typeof(InventoryItem.descr))]
        public int? UsrASCJPriceAsID { get; set; }
        public abstract class usrASCJPriceAsID : PX.Data.BQL.BqlInt.Field<usrASCJPriceAsID> { }
        #endregion

        // Hidden field due to logic doesn't need conversion between Commodity Items (logic always use 24K and SSS items)
        #region UsrASCJPriceToUnit
        [PXString]
        [INUnit(DisplayName = "Price To", Visibility = PXUIVisibility.SelectorVisible, Visible = false, Enabled = false)]
        [PXDefault("EACH", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXRestrictor(typeof(Where<ASCJINUnitExt.usrASCJCommodity, IsNotNull>), "Market Cost requires that a conversion is selected", typeof(INUnit.fromUnit))]
        public string UsrASCJPriceToUnit { get; set; }
        public abstract class usrASCJPriceToUnit : PX.Data.BQL.BqlString.Field<usrASCJPriceToUnit> { }
        #endregion

        #region UsrASCJPricingGRAMGold
        [PXDBDecimal(28)]
        [PXUIField(DisplayName = "Fine Gold, Grams")]
        public decimal? UsrASCJPricingGRAMGold { get; set; }
        public abstract class usrASCJPricingGRAMGold : PX.Data.BQL.BqlDecimal.Field<usrASCJPricingGRAMGold> { }
        #endregion

        #region UsrASCJPricingGRAMSilver
        [PXDBDecimal(28)]
        [PXUIField(DisplayName = "Fine Silver, Grams")]
        public decimal? UsrASCJPricingGRAMSilver { get; set; }
        public abstract class usrASCJPricingGRAMSilver : PX.Data.BQL.BqlDecimal.Field<usrASCJPricingGRAMSilver> { }
        #endregion

        #region UsrASCJActualGRAMGold
        [PXDBDecimal(28)]
        [PXUIField(DisplayName = "Gold, Grams")]
        public decimal? UsrASCJActualGRAMGold { get; set; }
        public abstract class usrASCJActualGRAMGold : PX.Data.BQL.BqlDecimal.Field<usrASCJActualGRAMGold> { }
        #endregion

        #region UsrASCJActualGRAMSilver
        [PXDBDecimal(28)]
        [PXUIField(DisplayName = "Silver, Grams")]
        public decimal? UsrASCJActualGRAMSilver { get; set; }
        public abstract class usrASCJActualGRAMSilver : PX.Data.BQL.BqlDecimal.Field<usrASCJActualGRAMSilver> { }
        #endregion

        #region UsrASCJProductWeight
        [PXDBDecimal(28)]
        [PXUIField(DisplayName = "Product Weight, Grams")]
        public decimal? UsrASCJProductWeight { get; set; }
        public abstract class usrASCJProductWeight : PX.Data.BQL.BqlDecimal.Field<usrASCJProductWeight> { }
        #endregion

        #region UsrASCJCostingType
        [PXDBString(1, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Costing Type")]
        [CostingType.ASCJList]
        [PXDefault(CostingType.ContractCost, PersistingCheck = PXPersistingCheck.Nothing)]
        public string UsrASCJCostingType { get; set; }
        public abstract class usrASCJCostingType : PX.Data.BQL.BqlString.Field<usrASCJCostingType> { }
        #endregion

        #region UsrASCJBasisValue
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Price / TOZ @ Basis", IsReadOnly = true)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrASCJBasisValue { get; set; }
        public abstract class usrASCJBasisValue : PX.Data.BQL.BqlDecimal.Field<usrASCJBasisValue> { }
        #endregion

        #region UsrASCJMarketPriceGram
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Market Price per Gram", IsReadOnly = true)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrASCJMarketPriceGram { get; set; }
        public abstract class usrASCJMarketPriceGram : PX.Data.BQL.BqlDecimal.Field<usrASCJMarketPriceGram> { }
        #endregion

        #region UsrASCJMarketPriceTOZ
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Market Price per TOZ", IsReadOnly = true)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrASCJMarketPriceTOZ { get; set; }
        public abstract class usrASCJMarketPriceTOZ : PX.Data.BQL.BqlDecimal.Field<usrASCJMarketPriceTOZ> { }
        #endregion

        #region UsrASCJMatrixPriceGram
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Matrix Price per Gram", IsReadOnly = true)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrASCJMatrixPriceGram { get; set; }
        public abstract class usrASCJMatrixPriceGram : PX.Data.BQL.BqlDecimal.Field<usrASCJMatrixPriceGram> { }
        #endregion

        #region UsrASCJMatrixPriceTOZ
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Matrix Price per TOZ", IsReadOnly = true)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrASCJMatrixPriceTOZ { get; set; }
        public abstract class usrASCJMatrixPriceTOZ : PX.Data.BQL.BqlDecimal.Field<usrASCJMatrixPriceTOZ> { }
        #endregion

        #region UsrASCJContractLossPct
        [PXDBDecimal(2, MinValue = 0, MaxValue = 100)]
        [PXUIField(DisplayName = "Metal Loss, %")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrASCJContractLossPct { get; set; }
        public abstract class usrASCJContractLossPct : PX.Data.BQL.BqlDecimal.Field<usrASCJContractLossPct> { }
        #endregion

        #region UsrASCJContractIncrement
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Increment/Dollar")]
        public decimal? UsrASCJContractIncrement { get; set; }
        public abstract class usrASCJContractIncrement : PX.Data.BQL.BqlDecimal.Field<usrASCJContractIncrement> { }
        #endregion

        #region UsrASCJIncrement
        [PXDecimalAttribute(6)]
        [PXUIField(DisplayName = "Increment", IsReadOnly = true)]
        [PXFormula(typeof(Switch<
            Case<Where<usrASCJCommodityType.IsEqual<CommodityType.gold>>, Mult<usrASCJActualGRAMGold, usrASCJContractIncrement>,
            Case<Where<usrASCJCommodityType.IsEqual<CommodityType.silver>>, Mult<usrASCJActualGRAMSilver, usrASCJContractIncrement>>>>))]
        public decimal? UsrASCJIncrement { get; set; }
        public abstract class usrASCJIncrement : PX.Data.BQL.BqlDecimal.Field<usrASCJIncrement> { }
        #endregion

        #region UsrASCJMatrixStep
        [PXDBDecimal(6, MinValue = 0, MaxValue = 10)]
        [PXUIField(DisplayName = "Matrix Step")]
        [PXDefault(TypeCode.Decimal, "0.500000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrASCJMatrixStep { get; set; }
        public abstract class usrASCJMatrixStep : PX.Data.BQL.BqlDecimal.Field<usrASCJMatrixStep> { }
        #endregion

        #region UsrASCJFloor
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Floor", IsReadOnly = true, Visible = false)]
        public decimal? UsrASCJFloor { get; set; }
        public abstract class usrASCJFloor : PX.Data.BQL.BqlDecimal.Field<usrASCJFloor> { }
        #endregion

        #region UsrASCJCeiling
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Ceiling", IsReadOnly = true, Visible = false)]
        public decimal? UsrASCJCeiling { get; set; }
        public abstract class usrASCJCeiling : PX.Data.BQL.BqlDecimal.Field<usrASCJCeiling> { }
        #endregion

        #region UsrASCJContractSurcharge
        [PXDBDecimal(2, MinValue = -100, MaxValue = 100)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Surcharge %")]
        public decimal? UsrASCJContractSurcharge { get; set; }
        public abstract class usrASCJContractSurcharge : PX.Data.BQL.BqlDecimal.Field<usrASCJContractSurcharge> { }
        #endregion

        #region UsrASCJPreciousMetalCost
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Precious Metal Cost")]
        public decimal? UsrASCJPreciousMetalCost { get; set; }
        public abstract class usrASCJPreciousMetalCost : PX.Data.BQL.BqlDecimal.Field<usrASCJPreciousMetalCost> { }
        #endregion

        #region UsrASCJMaterialsCost
        [PXUIField(DisplayName = "Other Materials Cost")]
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrASCJOtherMaterialsCost { get; set; }
        public abstract class usrASCJOtherMaterialsCost : PX.Data.BQL.BqlDecimal.Field<usrASCJOtherMaterialsCost> { }
        #endregion

        #region UsrASCJFabricationCost
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Fabrication/Value Add")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrASCJFabricationCost { get; set; }
        public abstract class usrASCJFabricationCost : PX.Data.BQL.BqlDecimal.Field<usrASCJFabricationCost> { }
        #endregion

        #region UsrASCJLaborCost
        [PXUIField(DisplayName = "In-house Labor Cost")]
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrASCJLaborCost { get; set; }
        public abstract class usrASCJLaborCost : PX.Data.BQL.BqlDecimal.Field<usrASCJLaborCost> { }
        #endregion

        #region UsrASCJHandlingCost
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Handling Cost")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]

        public decimal? UsrASCJHandlingCost { get; set; }
        public abstract class usrASCJHandlingCost : PX.Data.BQL.BqlDecimal.Field<usrASCJHandlingCost> { }
        #endregion

        #region UsrASCJFreightCost
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Freight Cost")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrASCJFreightCost { get; set; }
        public abstract class usrASCJFreightCost : PX.Data.BQL.BqlDecimal.Field<usrASCJFreightCost> { }
        #endregion

        #region UsrASCJDutyCost
        [PXUIField(DisplayName = "Duty Cost")]
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrASCJDutyCost { get; set; }
        public abstract class usrASCJDutyCost : PX.Data.BQL.BqlDecimal.Field<usrASCJDutyCost> { }
        #endregion

        #region UsrASCJDutyCostPct
        [PXUIField(DisplayName = "Duty, %")]
        [PXDBDecimal(2, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrASCJDutyCostPct { get; set; }
        public abstract class usrASCJDutyCostPct : PX.Data.BQL.BqlDecimal.Field<usrASCJDutyCostPct> { }
        #endregion

        #region UsrASCJOtherCost
        [PXUIField(DisplayName = "Other Cost", Visible = false)]
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrASCJOtherCost { get; set; }
        public abstract class usrASCJOtherCost : PX.Data.BQL.BqlDecimal.Field<usrASCJOtherCost> { }
        #endregion

        #region UsrASCJPackagingCost
        [PXUIField(DisplayName = "Packaging Cost")]
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrASCJPackagingCost { get; set; }
        public abstract class usrASCJPackagingCost : PX.Data.BQL.BqlDecimal.Field<usrASCJPackagingCost> { }
        #endregion

        #region UsrASCJPackagingLaborCost
        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Labor Packaging")]
        public decimal? UsrASCJPackagingLaborCost { get; set; }
        public abstract class usrASCJPackagingLaborCost : PX.Data.BQL.BqlDecimal.Field<usrASCJPackagingLaborCost> { }
        #endregion

        #region UsrASCJUnitCost
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Unit Cost", Visibility = PXUIVisibility.Visible, Enabled = false)]
        [PXFormula(typeof(Add<Add<Add<Add<usrASCJPackagingLaborCost, usrASCJOtherMaterialsCost>, usrASCJFabricationCost>, usrASCJPackagingCost>, usrASCJPreciousMetalCost>))]
        public decimal? UsrASCJUnitCost { get; set; }
        public abstract class usrASCJUnitCost : PX.Data.BQL.BqlDecimal.Field<usrASCJUnitCost> { }
        #endregion

        #region UsrASCJEstLandedCost
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Est. Landed Cost", Visibility = PXUIVisibility.Visible, Enabled = false)]
        [PXFormula(typeof(Add<Add<Add<Add<usrASCJDutyCost, usrASCJHandlingCost>, usrASCJFreightCost>, usrASCJLaborCost>, usrASCJUnitCost>))]
        public decimal? UsrASCJEstLandedCost { get; set; }
        public abstract class usrASCJEstLandedCost : PX.Data.BQL.BqlDecimal.Field<usrASCJEstLandedCost> { }
        #endregion

        #region UsrASCJCommodityType
        [PXDBString(1)]
        [PXUIField(DisplayName = "Commodity Type")]
        [CommodityType.ASCJList]
        [PXDefault(CommodityType.Undefined, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string UsrASCJCommodityType { get; set; }
        public abstract class usrASCJCommodityType : PX.Data.BQL.BqlString.Field<usrASCJCommodityType> { }
        #endregion
    }
}