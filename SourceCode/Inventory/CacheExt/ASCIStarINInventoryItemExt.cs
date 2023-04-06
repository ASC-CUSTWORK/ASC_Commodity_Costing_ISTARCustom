using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.AR;
using PX.Objects;
using System.Collections.Generic;
using System;
using ASCISTARCustom.Cost.Descriptor;

namespace ASCISTARCustom
{
    public class ASCIStarINInventoryItemExt : PXCacheExtension<InventoryItem>
    {
        public static bool IsActive() => true;

        #region UsrLegacyShortRef
        [PXDBString(30)]
        [PXUIField(DisplayName = "Legacy Short Ref")]
        public virtual string UsrLegacyShortRef { get; set; }
        public abstract class usrLegacyShortRef : PX.Data.BQL.BqlString.Field<usrLegacyShortRef> { }
        #endregion

        #region UsrLegacyID
        [PXDBString(30)]
        [PXUIField(DisplayName = "Legacy ID")]
        public virtual string UsrLegacyID { get; set; }
        public abstract class usrLegacyID : PX.Data.BQL.BqlString.Field<usrLegacyID> { }
        #endregion

        #region UsrCommodity
        [PXDBString(1)]
        [PXUIField(DisplayName = "Metal")]
        [CommodityType.List]
        [PXDefault(CommodityType.Undefined, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string UsrCommodity { get; set; }
        public abstract class usrCommodity : PX.Data.BQL.BqlString.Field<usrCommodity> { }
        #endregion

        #region UsrPriceAsID
        [PXDBInt()]
        [PXUIField(DisplayName = "Price as Item")]
        [PXSelector(typeof(Search2<InventoryItem.inventoryID, LeftJoin<INItemClass, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>, Where<INItemClass.itemClassCD, Equal<CommodityClass>>>),
            typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr)
                        , SubstituteKey = typeof(InventoryItem.inventoryCD), DescriptionField = typeof(InventoryItem.descr))]
        public virtual int? UsrPriceAsID { get; set; }
        public abstract class usrPriceAsID : PX.Data.BQL.BqlInt.Field<usrPriceAsID> { }
        #endregion

        #region ToUnit
        [INUnit(DisplayName = "Price To", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault("EACH", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXRestrictor(typeof(Where<ASCIStarINUnitExt.usrCommodity, IsNotNull>), "Market Cost requires that a conversion is selected", typeof(INUnit.fromUnit))]
        public virtual string UsrPriceToUnit { get; set; }
        public abstract class usrPriceToUnit : PX.Data.BQL.BqlString.Field<usrPriceToUnit> { }

        #endregion

        #region UsrPricingGRAMGold
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Fine Gold, Grams")]
        public virtual Decimal? UsrPricingGRAMGold { get; set; }
        public abstract class usrPricingGRAMGold : PX.Data.BQL.BqlDecimal.Field<usrPricingGRAMGold> { }
        #endregion

        #region UsrPricingGRAMSilver
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Fine Silver, Grams")]
        public virtual decimal? UsrPricingGRAMSilver { get; set; }
        public abstract class usrPricingGRAMSilver : PX.Data.BQL.BqlDecimal.Field<usrPricingGRAMSilver> { }
        #endregion

        //#region UsrPricingGRAMPlatinum
        //[PXDBDecimal(6)]
        //[PXUIField(DisplayName = "Fine Platinum Grams", Enabled = false, Visible = false)]
        //public virtual Decimal? UsrPricingGRAMPlatinum { get; set; }
        //public abstract class usrPricingGRAMPlatinum : PX.Data.BQL.BqlDecimal.Field<usrPricingGRAMPlatinum> { }
        //#endregion

        #region UsrActualGRAMGold
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Gold, Grams")]
        public virtual Decimal? UsrActualGRAMGold { get; set; }
        public abstract class usrActualGRAMGold : PX.Data.BQL.BqlDecimal.Field<usrActualGRAMGold> { }
        #endregion

        #region UsrActualGRAMSilver
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Silver, Grams")]
        public virtual Decimal? UsrActualGRAMSilver { get; set; }
        public abstract class usrActualGRAMSilver : PX.Data.BQL.BqlDecimal.Field<usrActualGRAMSilver> { }
        #endregion

        //#region UsrActualGRAMPlatinum
        //[PXDBDecimal(6)]
        //[PXUIField(DisplayName = "Platinum GRAMS", Enabled = false, Visible = false)]
        //public virtual Decimal? UsrActualGRAMPlatinum { get; set; }
        //public abstract class usrActualGRAMPlatinum : PX.Data.BQL.BqlDecimal.Field<usrActualGRAMPlatinum> { }
        //#endregion

        #region UsrContractWgt
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Contract Wgt (g)")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual decimal? UsrContractWgt { get; set; }
        public abstract class usrContractWgt : PX.Data.BQL.BqlDecimal.Field<usrContractWgt> { }
        #endregion

        //#region UsrCommodityID
        //public abstract class usrCommodityID : PX.Data.BQL.BqlInt.Field<usrCommodityID> { }
        //protected Int32? _CommodityID;
        //[ARTranInventoryItem(Filterable = true, Enabled = false)]
        //[PXSelector(
        //typeof(Search2<InventoryItem.inventoryID, LeftJoin<INItemClass, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>,
        //    Where<INItemClass.itemClassCD, Equal<CommodityClass>>>),
        //    typeof(InventoryItem.inventoryCD),
        //    typeof(InventoryItem.descr)

        //    , SubstituteKey = typeof(InventoryItem.inventoryCD)
        //    , DescriptionField = typeof(InventoryItem.descr))]
        //public virtual Int32? UsrCommodityID
        //{
        //    get
        //    {
        //        return this._CommodityID;
        //    }
        //    set
        //    {
        //        this._CommodityID = value;
        //    }
        //}
        //#endregion

        //#region UsrCommodityWgt
        //[PXDBDecimal]
        //[PXUIField(DisplayName = "Metal Weight", Enabled = false)]
        //public virtual Decimal? UsrCommodityWgt { get; set; }
        //public abstract class usrCommodityWgt : PX.Data.BQL.BqlDecimal.Field<usrCommodityWgt> { }
        //#endregion

        //#region UsrCommodityCost
        //[PXDBDecimal]
        //[PXUIField(DisplayName = "Metal Cost", Enabled = false)]
        //public virtual Decimal? UsrCommodityCost { get; set; }
        //public abstract class usrCommodityCost : PX.Data.BQL.BqlDecimal.Field<usrCommodityCost> { }
        //#endregion

        //#region UsrPricingDate
        //[PXDBDate]
        //[PXUIField(DisplayName = "Date Value")]
        //public DateTime? usrPricingDate { get; set; }
        //public abstract class UsrPricingDate : IBqlField { }
        //#endregion

        //#region UsrOtherCost
        //[PXDBDecimal]
        //[PXUIField(DisplayName = "Other Costs", Enabled = false)]
        //public virtual Decimal? UsrOtherCost { get; set; }
        //public abstract class usrOtherCost : PX.Data.BQL.BqlDecimal.Field<usrOtherCost> { }
        //#endregion

        //#region UsrUnitCost
        //[PXDecimal]
        //[PXUIField(DisplayName = "Unit Cost", Visibility = PXUIVisibility.Visible, Enabled = false)]
        //[PXFormula(typeof(Add<ASCIStarINInventoryItemExt.usrOtherCost, ASCIStarINInventoryItemExt.usrCommodityCost>))]
        //public virtual Decimal? UsrUnitCost { get; set; }
        //public abstract class usrUnitCost : PX.Data.BQL.BqlDecimal.Field<usrUnitCost> { }
        //#endregion

        #region UsrCostingType
        [PXDBString(1, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Costing Type")]
        [ASCIStarCostingType.List]
        [PXDefault(ASCIStarCostingType.ContractCost, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string UsrCostingType { get; set; }
        public abstract class usrCostingType : PX.Data.BQL.BqlString.Field<usrCostingType> { }
        #endregion

        #region UsrCostRollupType
        [PXDBString(1, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Rollup Type")]
        [ASCIStarCostRollupType.List]
        [PXDefault(ASCIStarCostRollupType.Other, PersistingCheck = PXPersistingCheck.Null)]
        //[PXDefault(typeof(), PersistingCheck = PXPersistingCheck.Null)]
        public virtual string UsrCostRollupType { get; set; }
        public abstract class usrCostRollupType : PX.Data.BQL.BqlString.Field<usrCostRollupType> { }
        #endregion

        #region UsrContractPrice
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Contract Price")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Null)]
        public virtual decimal? UsrContractPrice { get; set; }
        public abstract class usrContractPrice : PX.Data.BQL.BqlDecimal.Field<usrContractPrice> { }
        #endregion

        #region UsrContractLossPct
        [PXDBDecimal(4, MinValue = 0, MaxValue = 100)]
        [PXUIField(DisplayName = "Metal Loss, %")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Null)]
        public virtual decimal? UsrContractLossPct { get; set; }
        public abstract class usrContractLossPct : PX.Data.BQL.BqlDecimal.Field<usrContractLossPct> { }
        #endregion

        #region UsrContractIncrement
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Increment")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Null)]
        public virtual decimal? UsrContractIncrement { get; set; }
        public abstract class usrContractIncrement : PX.Data.BQL.BqlDecimal.Field<usrContractIncrement> { }
        #endregion

        #region UsrContractSurcharge
        [PXDBDecimal(6/*, MinValue = 0, MaxValue = 1000*/)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Null)]
        [PXUIField(DisplayName = "Surcharge %", Visible = true)]
        public virtual decimal? UsrContractSurcharge { get; set; }
        public abstract class usrContractSurcharge : PX.Data.BQL.BqlDecimal.Field<usrContractSurcharge> { }
        #endregion

        //#region UsrContractSurchargeType
        //[PXDBString(1, IsUnicode = true, InputMask = "")]
        //[PXUIField(DisplayName = "Surcharge Type")]
        //[ContractSurchargeType.List]
        //[PXDefault(ContractSurchargeType.PercentageAmt, PersistingCheck = PXPersistingCheck.Null)]
        //public virtual string UsrContractSurchargeType { get; set; }
        //public abstract class usrContractSurchargeType : PX.Data.BQL.BqlString.Field<usrContractSurchargeType> { }
        //#endregion

        #region UsrCommodityCost
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Precious Metal Cost")]
        public virtual Decimal? UsrCommodityCost { get; set; }
        public abstract class usrCommodityCost : PX.Data.BQL.BqlDecimal.Field<usrCommodityCost> { }
        #endregion

        #region UsrOtherMaterialCost
        [PXUIField(DisplayName = "Other Materials Cost")]
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Null)]
        public virtual Decimal? UsrOtherMaterialCost { get; set; }
        public abstract class usrOtherMaterialCost : PX.Data.BQL.BqlDecimal.Field<usrOtherMaterialCost> { }
        #endregion

        #region UsrFabricationCost
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Fabrication Cost")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Null)]
        public virtual Decimal? UsrFabricationCost { get; set; }
        public abstract class usrFabricationCost : PX.Data.BQL.BqlDecimal.Field<usrFabricationCost> { }
        #endregion

        #region UsrLaborCost
        [PXUIField(DisplayName = "In-house Labor Cost")]
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Null)]

        public virtual Decimal? UsrLaborCost { get; set; }
        public abstract class usrLaborCost : PX.Data.BQL.BqlDecimal.Field<usrLaborCost> { }
        #endregion

        #region UsrHandlingCost
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Handling Cost")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Null)]

        public virtual Decimal? UsrHandlingCost { get; set; }
        public abstract class usrHandlingCost : PX.Data.BQL.BqlDecimal.Field<usrHandlingCost> { }
        #endregion

        #region UsrFreightCost
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Freight Cost")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Null)]
        public virtual Decimal? UsrFreightCost { get; set; }
        public abstract class usrFreightCost : PX.Data.BQL.BqlDecimal.Field<usrFreightCost> { }
        #endregion

        #region UsrDutyCost
        [PXUIField(DisplayName = "Duty Cost")]
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.NullOrBlank)]
        public virtual Decimal? UsrDutyCost { get; set; }
        public abstract class usrDutyCost : PX.Data.BQL.BqlDecimal.Field<usrDutyCost> { }
        #endregion

        #region UsrDutyCostPct

        [PXUIField(DisplayName = "Duty %")]
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.NullOrBlank)]
        public virtual Decimal? UsrDutyCostPct { get; set; }
        public abstract class usrDutyCostPct : PX.Data.BQL.BqlDecimal.Field<usrDutyCostPct> { }
        #endregion

        #region UsrOtherCost
        [PXUIField(DisplayName = "Other Cost", Visible = false)]
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Null)]
        public virtual Decimal? UsrOtherCost { get; set; }
        public abstract class usrOtherCost : PX.Data.BQL.BqlDecimal.Field<usrOtherCost> { }
        #endregion

        #region UsrPackagingCost

        [PXUIField(DisplayName = "Packaging Cost")]
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Null)]
        public virtual Decimal? UsrPackagingCost { get; set; }
        public abstract class usrPackagingCost : PX.Data.BQL.BqlDecimal.Field<usrPackagingCost> { }
        #endregion

        #region UsrContractCost
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Unit Cost", Visibility = PXUIVisibility.Visible, Enabled = false)]
        //[PXFormula(typeof(Add<Add<Add<Add<
        //            ASCIStarINInventoryItemExt.usrCommodityCost
        //           , ASCIStarINInventoryItemExt.usrFabricationCost>
        //           , ASCIStarINInventoryItemExt.usrHandlingCost>
        //           , ASCIStarINInventoryItemExt.usrPackagingCost>
        //           , ASCIStarINInventoryItemExt.usrOtherCost>
        //           ))]

        public virtual Decimal? UsrContractCost { get; set; }
        public abstract class usrContractCost : PX.Data.BQL.BqlDecimal.Field<usrContractCost> { }
        #endregion

        #region UsrUnitCost
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Landed Cost", Visibility = PXUIVisibility.Visible, Enabled = false)]
        //[PXFormula(typeof(Mult<Add<Add<Add<Add<Add<Add<
        //                    ASCIStarINInventoryItemExt.usrCommodityCost
        //                   , ASCIStarINInventoryItemExt.usrFabricationCost>
        //                   , ASCIStarINInventoryItemExt.usrLaborCost>
        //                   , ASCIStarINInventoryItemExt.usrHandlingCost>
        //                   , ASCIStarINInventoryItemExt.usrFreightCost>
        //                   , ASCIStarINInventoryItemExt.usrOtherCost>
        //                   , ASCIStarINInventoryItemExt.usrPackagingCost>
        //                   , ASCIStarINInventoryItemExt.usrDutyCost>
        //                   ))]
        public virtual Decimal? UsrUnitCost { get; set; }
        public abstract class usrUnitCost : PX.Data.BQL.BqlDecimal.Field<usrUnitCost> { }
        #endregion

    }
}