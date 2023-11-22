using ASCISTARCustom.Common.Descriptor;
using ASCISTARCustom.Common.DTO.Interfaces;
using ASCISTARCustom.Cost.CacheExt;
using ASCISTARCustom.Inventory.Descriptor.Constants;
using PX.Data;
using PX.Data.BQL;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using System;
using static ASCISTARCustom.Common.Descriptor.ASCIStarConstants;

namespace ASCISTARCustom
{
    public class ASCIStarINInventoryItemExt : PXCacheExtension<InventoryItem>, IASCIStarItemCostSpecDTO
    {
        public static bool IsActive() => true;

        #region Inventory
        [PXDBIdentity]
        [PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible, Visible = false)]
        [PXReferentialIntegrityCheck]
        public virtual int? InventoryID { get; set; }
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
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
        [ASCIStarINConstants.InventoryItemStatusExt.List]
        public virtual String ItemStatus { get; set; }
        public abstract class itemStatus : BqlString.Field<itemStatus> { }
        #endregion

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

        // Hidden field due to logic doesn't need conversion between Commodity Items (logic always use 24K and SSS items)
        #region UsrPriceAsID
        [PXDBInt()]
        [PXUIField(DisplayName = "Price as Item", Visible = false, Enabled = false)]
        [PXSelector(typeof(Search2<InventoryItem.inventoryID, LeftJoin<INItemClass, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>, 
            Where<INItemClass.itemClassCD, Equal<ASCIStarConstants.CommodityClass>>>),
            typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr)
                        , SubstituteKey = typeof(InventoryItem.inventoryCD), DescriptionField = typeof(InventoryItem.descr))]
        public int? UsrPriceAsID { get; set; }
        public abstract class usrPriceAsID : PX.Data.BQL.BqlInt.Field<usrPriceAsID> { }
        #endregion

        // Hidden field due to logic doesn't need conversion between Commodity Items (logic always use 24K and SSS items)
        #region UsrPriceToUnit
        [PXString]
        //[INUnit(DisplayName = "Price To", Visibility = PXUIVisibility.SelectorVisible, Visible = false, Enabled = false)]
        //[PXDefault("EACH", PersistingCheck = PXPersistingCheck.Nothing)]
        //  [PXRestrictor(typeof(Where<ASCIStarINUnitExt.usrCommodity, IsNotNull>), "Market Cost requires that a conversion is selected", typeof(INUnit.fromUnit))]
        public string UsrPriceToUnit { get; set; }
        public abstract class usrPriceToUnit : PX.Data.BQL.BqlString.Field<usrPriceToUnit> { }
        #endregion

        #region UsrPricingGRAMGold
        [PXDBDecimal(28)]
        [PXUIField(DisplayName = "Fine Gold, Grams")]
        public decimal? UsrPricingGRAMGold { get; set; }
        public abstract class usrPricingGRAMGold : PX.Data.BQL.BqlDecimal.Field<usrPricingGRAMGold> { }
        #endregion

        #region UsrPricingGRAMSilver
        [PXDBDecimal(28)]
        [PXUIField(DisplayName = "Fine Silver, Grams")]
        public decimal? UsrPricingGRAMSilver { get; set; }
        public abstract class usrPricingGRAMSilver : PX.Data.BQL.BqlDecimal.Field<usrPricingGRAMSilver> { }
        #endregion

        #region UsrActualGRAMGold
        [PXDBDecimal(28)]
        [PXUIField(DisplayName = "Gold, Grams")]
        public decimal? UsrActualGRAMGold { get; set; }
        public abstract class usrActualGRAMGold : PX.Data.BQL.BqlDecimal.Field<usrActualGRAMGold> { }
        #endregion

        #region UsrActualGRAMSilver
        [PXDBDecimal(28)]
        [PXUIField(DisplayName = "Silver, Grams")]
        public decimal? UsrActualGRAMSilver { get; set; }
        public abstract class usrActualGRAMSilver : PX.Data.BQL.BqlDecimal.Field<usrActualGRAMSilver> { }
        #endregion

        #region UsrActualGRAMBrass
        [PXDBDecimal(28)]
        [PXUIField(DisplayName = "Brass Weight, Grams")]
        public decimal? UsrActualGRAMBrass { get; set; }
        public abstract class usrActualGRAMBrass : PX.Data.BQL.BqlDecimal.Field<usrActualGRAMBrass> { }
        #endregion

        #region UsrCostingType
        [PXDBString(1, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Costing Type")]
        [CostingType.List]
        [PXDefault(CostingType.ContractCost, PersistingCheck = PXPersistingCheck.Nothing)]
        public string UsrCostingType { get; set; }
        public abstract class usrCostingType : PX.Data.BQL.BqlString.Field<usrCostingType> { }
        #endregion

        

        #region UsrBasisValue
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Price / TOZ @ Basis", IsReadOnly = true)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrBasisValue { get; set; }
        public abstract class usrBasisValue : PX.Data.BQL.BqlDecimal.Field<usrBasisValue> { }
        #endregion

        #region UsrMarketPriceGram
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Market Price per Gram", IsReadOnly = true)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrMarketPriceGram { get; set; }
        public abstract class usrMarketPriceGram : PX.Data.BQL.BqlDecimal.Field<usrMarketPriceGram> { }
        #endregion

        #region UsrMarketPriceTOZ
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Market Price per TOZ", IsReadOnly = true)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrMarketPriceTOZ { get; set; }
        public abstract class usrMarketPriceTOZ : PX.Data.BQL.BqlDecimal.Field<usrMarketPriceTOZ> { }
        #endregion

        #region UsrMatrixPriceGram
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Matrix Price per Gram", IsReadOnly = true)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrMatrixPriceGram { get; set; }
        public abstract class usrMatrixPriceGram : PX.Data.BQL.BqlDecimal.Field<usrMatrixPriceGram> { }
        #endregion

        #region UsrMatrixPriceTOZ
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Matrix Price per TOZ", IsReadOnly = true)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrMatrixPriceTOZ { get; set; }
        public abstract class usrMatrixPriceTOZ : PX.Data.BQL.BqlDecimal.Field<usrMatrixPriceTOZ> { }
        #endregion

        #region UsrContractLossPct
        [PXDBDecimal(2, MinValue = 0, MaxValue = 100)]
        [PXUIField(DisplayName = "Metal Loss, %")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrContractLossPct { get; set; }
        public abstract class usrContractLossPct : PX.Data.BQL.BqlDecimal.Field<usrContractLossPct> { }
        #endregion

        #region UsrContractIncrement
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Increment")]
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
        [PXUIField(DisplayName = "Floor", IsReadOnly = true, Visible = false)]
        public decimal? UsrFloor { get; set; }
        public abstract class usrFloor : PX.Data.BQL.BqlDecimal.Field<usrFloor> { }
        #endregion

        #region UsrCeiling
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Ceiling", IsReadOnly = true, Visible = false)]
        public decimal? UsrCeiling { get; set; }
        public abstract class usrCeiling : PX.Data.BQL.BqlDecimal.Field<usrCeiling> { }
        #endregion

        #region UsrContractSurcharge
        [PXDBDecimal(2, MinValue = -100, MaxValue = 100)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Surcharge, %")]
        public decimal? UsrContractSurcharge { get; set; }
        public abstract class usrContractSurcharge : PX.Data.BQL.BqlDecimal.Field<usrContractSurcharge> { }
        #endregion

        #region UsrPreciousMetalCost
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Precious Metal Cost")]
        public decimal? UsrPreciousMetalCost { get; set; }
        public abstract class usrPreciousMetalCost : PX.Data.BQL.BqlDecimal.Field<usrPreciousMetalCost> { }
        #endregion

        #region UsrMaterialsCost
        [PXUIField(DisplayName = "Other Materials Cost")]
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrOtherMaterialsCost { get; set; }
        public abstract class usrOtherMaterialsCost : PX.Data.BQL.BqlDecimal.Field<usrOtherMaterialsCost> { }
        #endregion

        #region UsrFabricationCost
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Fabrication/Value Add")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrFabricationCost { get; set; }
        public abstract class usrFabricationCost : PX.Data.BQL.BqlDecimal.Field<usrFabricationCost> { }
        #endregion

        #region UsrLaborCost
        [PXUIField(DisplayName = "In-house Labor Cost")]
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrLaborCost { get; set; }
        public abstract class usrLaborCost : PX.Data.BQL.BqlDecimal.Field<usrLaborCost> { }
        #endregion

        #region UsrHandlingCost
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Handling Cost")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]

        public decimal? UsrHandlingCost { get; set; }
        public abstract class usrHandlingCost : PX.Data.BQL.BqlDecimal.Field<usrHandlingCost> { }
        #endregion

        #region UsrFreightCost
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Freight Cost")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrFreightCost { get; set; }
        public abstract class usrFreightCost : PX.Data.BQL.BqlDecimal.Field<usrFreightCost> { }
        #endregion

        #region UsrDutyCost
        [PXUIField(DisplayName = "Duty Cost")]
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrDutyCost { get; set; }
        public abstract class usrDutyCost : PX.Data.BQL.BqlDecimal.Field<usrDutyCost> { }
        #endregion

        #region UsrDutyCostPct
        [PXUIField(DisplayName = "Duty, %")]
        [PXDBDecimal(2, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrDutyCostPct { get; set; }
        public abstract class usrDutyCostPct : PX.Data.BQL.BqlDecimal.Field<usrDutyCostPct> { }
        #endregion

        #region UsrOtherCost
        [PXUIField(DisplayName = "Other Cost", Visible = false)]
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrOtherCost { get; set; }
        public abstract class usrOtherCost : PX.Data.BQL.BqlDecimal.Field<usrOtherCost> { }
        #endregion

        #region UsrPackagingCost
        [PXUIField(DisplayName = "Packaging Cost")]
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrPackagingCost { get; set; }
        public abstract class usrPackagingCost : PX.Data.BQL.BqlDecimal.Field<usrPackagingCost> { }
        #endregion

        #region UsrPackagingLaborCost
        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Labor Packaging")]
        public decimal? UsrPackagingLaborCost { get; set; }
        public abstract class usrPackagingLaborCost : PX.Data.BQL.BqlDecimal.Field<usrPackagingLaborCost> { }
        #endregion

        #region UsrUnitCost
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Unit Cost", Visibility = PXUIVisibility.Visible, Enabled = false)]
        [PXFormula(typeof(Add<Add<Add<Add<usrPackagingLaborCost, usrOtherMaterialsCost>, usrFabricationCost>, usrPackagingCost>, usrPreciousMetalCost>))]
        public decimal? UsrUnitCost { get; set; }
        public abstract class usrUnitCost : PX.Data.BQL.BqlDecimal.Field<usrUnitCost> { }
        #endregion

        #region UsrEstLandedCost
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Est. Landed Cost", Visibility = PXUIVisibility.Visible, Enabled = false)]
        [PXFormula(typeof(Add<Add<Add<Add<usrDutyCost, usrHandlingCost>, usrFreightCost>, usrLaborCost>, usrUnitCost>))]
        public decimal? UsrEstLandedCost { get; set; }
        public abstract class usrEstLandedCost : PX.Data.BQL.BqlDecimal.Field<usrEstLandedCost> { }
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