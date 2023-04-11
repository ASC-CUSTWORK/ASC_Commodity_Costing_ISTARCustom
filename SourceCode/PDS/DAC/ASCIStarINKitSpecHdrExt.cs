using PX.Data;
using PX.Objects.IN;

namespace ASCISTARCustom
{
    public sealed class ASCIStarINKitSpecHdrExt : PXCacheExtension<INKitSpecHdr>
    {
        public static bool IsActive() => true;

        #region UsrVQuoteLineCtr
        [PXDBInt]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        public int? UsrVQuoteLineCtr { get; set; }
        public abstract class usrVQuoteLineCtr : PX.Data.BQL.BqlInt.Field<usrVQuoteLineCtr> { }
        #endregion

        //TODO Check if this fields are exists in data base table or not, if so then remove them all!
        #region Decpracated 
        //        //#region UsrPricingDate
        //        //[PXDBDate]
        //        //[PXUIField(DisplayName = "Date Value")]
        //        //public DateTime? usrPricingDate { get; set; }
        //        //public abstract class UsrPricingDate : IBqlField { }
        //        //#endregion

        //        #region UsrCommodityID
        //        public abstract class usrCommodityID : PX.Data.BQL.BqlInt.Field<usrCommodityID> { }
        //        protected Int32? _CommodityID;
        //        [PXDBInt()]
        //        [PXUIField(DisplayName = "Metal/Type")]
        //        [PXSelector(
        //        typeof(Search2<InventoryItem.inventoryID, LeftJoin<INItemClass, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>,
        //            Where<INItemClass.itemClassCD, Equal<CommodityClass>>>),
        //            typeof(InventoryItem.inventoryCD),
        //            typeof(InventoryItem.descr)

        //            , SubstituteKey = typeof(InventoryItem.inventoryCD)
        //            , DescriptionField = typeof(InventoryItem.descr))]
        //        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        //        public virtual Int32? UsrCommodityID
        //        {
        //            get
        //            {
        //                return this._CommodityID;
        //            }
        //            set
        //            {
        //                this._CommodityID = value;
        //            }
        //        }
        //        #endregion

        //        #region UsrPricingGRAMGold
        //        [PXDBDecimal(6)]
        //        [PXUIField(DisplayName = "Intrinsic Gold (g)", Enabled = true)]
        //        public virtual Decimal? UsrPricingGRAMGold { get; set; }
        //        public abstract class usrPricingGRAMGold : PX.Data.BQL.BqlDecimal.Field<usrPricingGRAMGold> { }
        //        #endregion

        //        #region UsrPricingGRAMSilver
        //        [PXDBDecimal(6)]
        //        [PXUIField(DisplayName = "Fine Silver (g)", Enabled = true)]
        //        public virtual Decimal? UsrPricingGRAMSilver { get; set; }
        //        public abstract class usrPricingGRAMSilver : PX.Data.BQL.BqlDecimal.Field<usrPricingGRAMSilver> { }
        //        #endregion

        //        #region UsrPricingGRAMPlatinum
        //        [PXDBDecimal(6)]
        //        [PXUIField(DisplayName = "Fine Platinum (g)", Enabled = true)]
        //        public virtual Decimal? UsrPricingGRAMPlatinum { get; set; }
        //        public abstract class usrPricingGRAMPlatinum : PX.Data.BQL.BqlDecimal.Field<usrPricingGRAMPlatinum> { }
        //        #endregion

        //        #region UsrActualGRAMGold
        //        [PXDBDecimal(6)]
        //        [PXUIField(DisplayName = "Gold (g)", Enabled = true)]
        //        public virtual Decimal? UsrActualGRAMGold { get; set; }
        //        public abstract class usrActualGRAMGold : PX.Data.BQL.BqlDecimal.Field<usrActualGRAMGold> { }
        //        #endregion

        //        #region UsrActualGRAMSilver
        //        [PXDBDecimal(6)]
        //        [PXUIField(DisplayName = "Silver (g)", Enabled = true)]
        //        public virtual Decimal? UsrActualGRAMSilver { get; set; }
        //        public abstract class usrActualGRAMSilver : PX.Data.BQL.BqlDecimal.Field<usrActualGRAMSilver> { }
        //        #endregion

        //        #region UsrActualGRAMPlatinum
        //        [PXDBDecimal(6)]
        //        [PXUIField(DisplayName = "Platinum (g)", Enabled = true)]
        //        public virtual Decimal? UsrActualGRAMPlatinum { get; set; }
        //        public abstract class usrActualGRAMPlatinum : PX.Data.BQL.BqlDecimal.Field<usrActualGRAMPlatinum> { }
        //        #endregion

        //        #region UsrContractWgt
        //        [PXDBDecimal(6)]
        //        [PXUIField(DisplayName = "Contract Wgt (g)")]
        //        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        //        public virtual decimal? UsrContractWgt { get; set; }
        //        public abstract class usrContractWgt : PX.Data.BQL.BqlDecimal.Field<usrContractWgt> { }
        //        #endregion

        //        #region UsrContractPrice
        //        [PXDBDecimal(6)]
        //        [PXUIField(DisplayName = "Contract Price")]
        //        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        //        public virtual decimal? UsrContractPrice { get; set; }
        //        public abstract class usrContractPrice : PX.Data.BQL.BqlDecimal.Field<usrContractPrice> { }
        //        #endregion

        //        #region UsrContractLossPct
        //        [PXDBDecimal(4)]
        //        [PXUIField(DisplayName = "Metal Loss Pct")]
        //        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        //        public virtual decimal? UsrContractLossPct { get; set; }
        //        public abstract class usrContractLossPct : PX.Data.BQL.BqlDecimal.Field<usrContractLossPct> { }
        //        #endregion

        //        #region UsrContractIncrement
        //        [PXDBDecimal(6)]
        //        [PXUIField(DisplayName = "Increment", Visible = true)]
        //        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        //        public virtual decimal? UsrContractIncrement { get; set; }
        //        public abstract class usrContractIncrement : PX.Data.BQL.BqlDecimal.Field<usrContractIncrement> { }
        //        #endregion

        //        #region UsrContractSurcharge
        //        [PXDBDecimal(6)]
        //        [PXUIField(DisplayName = "Surcharge", Visible = true)]
        //        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        //        public virtual decimal? UsrContractSurcharge { get; set; }
        //        public abstract class usrContractSurcharge : PX.Data.BQL.BqlDecimal.Field<usrContractSurcharge> { }
        //        #endregion

        //        #region UsrContractSurchargeType
        //        [PXDBString(1, IsUnicode = true, InputMask = "")]
        //        [PXUIField(DisplayName = "Surcharge Type")]
        //        [ContractSurchargeType.List]
        //        [PXDefault(ContractSurchargeType.FixedAmt)]
        //        public virtual string UsrContractSurchargeType { get; set; }
        //        public abstract class usrContractSurchargeType : PX.Data.BQL.BqlString.Field<usrContractSurchargeType> { }
        //        #endregion

        //        #region UsrCommodityCost
        //        [PXDBDecimal(6)]
        //        [PXUIField(DisplayName = "Metal Cost")]
        //        public virtual Decimal? UsrCommodityCost { get; set; }
        //        public abstract class usrCommodityCost : PX.Data.BQL.BqlDecimal.Field<usrCommodityCost> { }
        //        #endregion

        //        #region UsrFabricationCost
        //        [PXDBDecimal(6)]
        //        [PXUIField(DisplayName = "Fabrication Cost")]
        //        public virtual Decimal? UsrFabricationCost { get; set; }
        //        public abstract class usrFabricationCost : PX.Data.BQL.BqlDecimal.Field<usrFabricationCost> { }
        //        #endregion

        //        #region UsrLaborCost
        //        [PXDBDecimal(6)]
        //        [PXUIField(DisplayName = "Labor Cost")]
        //        public virtual Decimal? UsrLaborCost { get; set; }
        //        public abstract class usrLaborCost : PX.Data.BQL.BqlDecimal.Field<usrLaborCost> { }
        //        #endregion

        //        #region UsrHandlingCost
        //        [PXDBDecimal(6)]
        //        [PXUIField(DisplayName = "Handling Cost")]
        //        public virtual Decimal? UsrHandlingCost { get; set; }
        //        public abstract class usrHandlingCost : PX.Data.BQL.BqlDecimal.Field<usrHandlingCost> { }
        //        #endregion

        //        #region UsrFreightCost
        //        [PXDBDecimal(6)]
        //        [PXUIField(DisplayName = "Freight Cost")]
        //        public virtual Decimal? UsrFreightCost { get; set; }
        //        public abstract class usrFreightCost : PX.Data.BQL.BqlDecimal.Field<usrFreightCost> { }
        //        #endregion

        //        #region UsrDutyCost
        //        [PXDBDecimal(6)]
        //        [PXUIField(DisplayName = "Duty Cost")]
        //        public virtual Decimal? UsrDutyCost { get; set; }
        //        public abstract class usrDutyCost : PX.Data.BQL.BqlDecimal.Field<usrDutyCost> { }
        //        #endregion

        //        #region UsrOtherCost
        //        [PXDBDecimal(6)]
        //        [PXUIField(DisplayName = "Other Cost")]
        //        public virtual Decimal? UsrOtherCost { get; set; }
        //        public abstract class usrOtherCost : PX.Data.BQL.BqlDecimal.Field<usrOtherCost> { }
        //        #endregion

        //        #region UsrPackagingCost
        //        [PXDBDecimal(6)]
        //        [PXUIField(DisplayName = "Packaging Cost")]
        //        public virtual Decimal? UsrPackagingCost { get; set; }
        //        public abstract class usrPackagingCost : PX.Data.BQL.BqlDecimal.Field<usrPackagingCost> { }
        //        #endregion

        //        #region UsrContractCost
        //        [PXDecimal(6)]
        //        [PXUIField(DisplayName = "Contract Cost", Visibility = PXUIVisibility.Visible, Enabled = false)]
        //        [PXFormula(typeof(Add<Add<Add<Add<Add<
        //                            ASCIStarINKitSpecHdrExt.usrCommodityCost
        //                           , ASCIStarINKitSpecHdrExt.usrFabricationCost>
        //                           , ASCIStarINKitSpecHdrExt.usrLaborCost>
        //                           , ASCIStarINKitSpecHdrExt.usrHandlingCost>
        //                           , ASCIStarINKitSpecHdrExt.usrPackagingCost>
        //                           , ASCIStarINKitSpecHdrExt.usrOtherCost>
        //                           ))]

        //        public virtual Decimal? UsrContractCost { get; set; }
        //        public abstract class usrContractCost : PX.Data.BQL.BqlDecimal.Field<usrContractCost> { }
        //        #endregion

        //        #region UsrUnitCost
        //        [PXDecimal(6)]
        //        [PXUIField(DisplayName = "Unit Cost", Visibility = PXUIVisibility.Visible, Enabled = false)]
        //        [PXFormula(typeof(Add<Add<Add<Add<Add<Add<Add<
        //                            ASCIStarINKitSpecHdrExt.usrCommodityCost
        //                           , ASCIStarINKitSpecHdrExt.usrFabricationCost>
        //                           , ASCIStarINKitSpecHdrExt.usrLaborCost>
        //                           , ASCIStarINKitSpecHdrExt.usrHandlingCost>
        //                           , ASCIStarINKitSpecHdrExt.usrFreightCost>
        //                           , ASCIStarINKitSpecHdrExt.usrDutyCost>
        //                           , ASCIStarINKitSpecHdrExt.usrPackagingCost>
        //                           , ASCIStarINKitSpecHdrExt.usrOtherCost>
        //                           ))]
        //        public virtual Decimal? UsrUnitCost { get; set; }
        //        public abstract class usrUnitCost : PX.Data.BQL.BqlDecimal.Field<usrUnitCost> { }
        //        #endregion

        #endregion Decpracated 


    }
}