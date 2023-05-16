using ASCISTARCustom.Common.Descriptor;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.IN;
using System;
using static ASCISTARCustom.Common.Descriptor.ASCIStarConstants;

namespace ASCISTARCustom.Cost.CacheExt
{
    public sealed class ASCIStarAPVendorPriceExt : PXCacheExtension<APVendorPrice>
    {
        public static bool IsActive() => true;

        #region UsrMarket
        [PXDBString(2, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Market")]
        [MarketList.List]
        public string UsrMarket { get; set; }
        public abstract class usrMarket : PX.Data.BQL.BqlString.Field<usrMarket> { }
        #endregion

        #region UsrMarketID
        [PXDBInt()]
        [PXUIField(DisplayName = "Market Vendor", Visible = false)]
        [PXSelector(
        typeof(Search2<Vendor.bAccountID, InnerJoin<VendorClass, On<Vendor.vendorClassID, Equal<VendorClass.vendorClassID>>>, Where<VendorClass.vendorClassID, Equal<MarketClass>>>),
            typeof(Vendor.acctCD), typeof(Vendor.acctName)
            , SubstituteKey = typeof(Vendor.acctCD), DescriptionField = typeof(Vendor.acctName))]
        public int? UsrMarketID { get; set; }
        public abstract class usrMarketID : PX.Data.BQL.BqlDecimal.Field<usrMarketID> { }
        #endregion

        #region UsrCommodity
        [PXDBString(1)]
        [PXUIField(DisplayName = "Commodity Metal Type", Enabled = false)]
        [CommodityType.List]
        [PXDefault(typeof(Search<ASCIStarINInventoryItemExt.usrCommodityType, Where<InventoryItem.inventoryID, Equal<Current<APVendorPrice.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<APVendorPrice.inventoryID>))]
        public string UsrCommodity { get; set; }
        public abstract class usrCommodity : PX.Data.BQL.BqlString.Field<usrCommodity> { }
        #endregion

        #region UsrCommodityLossPct
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Loss")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrCommodityLossPct { get; set; }
        public abstract class usrCommodityLossPct : PX.Data.BQL.BqlDecimal.Field<usrCommodityLossPct> { }
        #endregion

        #region UsrCommoditySurchargePct
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Surcharge")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrCommoditySurchargePct { get; set; }
        public abstract class usrCommoditySurchargePct : PX.Data.BQL.BqlDecimal.Field<usrCommoditySurchargePct> { }
        #endregion

        #region UsrCommodityPerGram
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Basis Price/Gram", IsReadOnly = true)]
        [PXFormula(typeof(Switch<Case<Where<Current<APVendorPrice.uOM>, NotEqual<TOZ>>, Null>, Div<APVendorPrice.salesPrice, TOZ2GRAM_31_10348>>))]
        public decimal? UsrCommodityPerGram { get; set; }
        public abstract class usrCommodityPerGram : PX.Data.BQL.BqlDecimal.Field<usrCommodityPerGram> { }
        #endregion

        #region UsrCommodityIncrement
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Metal Increment", IsReadOnly = true)]
        [PXFormula(typeof(Switch<
            Case<Where<APVendorPrice.uOM.IsNotEqual<TOZ>.Or<APVendorPrice.salesPrice.IsNotNull.And<APVendorPrice.salesPrice.IsEqual<PX.Objects.CS.decimal0>>>>,
                        Null>,
            Case<Where<ASCIStarAPVendorPriceExt.usrCommodity.IsEqual<CommodityType.silver>>,
                        Div<usrCommodityPerGram, APVendorPrice.salesPrice>,
            Case<Where<ASCIStarAPVendorPriceExt.usrCommodity.IsEqual<CommodityType.gold>>,
                        Mult<Div<usrCommodityPerGram, APVendorPrice.salesPrice>, Add<ASCIStarConstants.DecimalOne, Div<usrCommoditySurchargePct, ASCIStarConstants.DecimalOneHundred>>>>>>))]
        public decimal? UsrCommodityIncrement { get; set; }
        public abstract class usrCommodityIncrement : PX.Data.BQL.BqlDecimal.Field<usrCommodityIncrement> { }
        #endregion

        #region UsrMatrixStep
        [PXDBDecimal(6, MinValue = 0, MaxValue = 10)]
        [PXUIField(DisplayName = "Matrix Step")]
        [PXDefault(TypeCode.Decimal, "0.500000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrMatrixStep { get; set; }
        public abstract class usrMatrixStep : PX.Data.BQL.BqlDecimal.Field<usrMatrixStep> { }
        #endregion

        #region UsrFloor
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Floor", IsReadOnly = true)]
        public decimal? UsrFloor { get; set; }
        public abstract class usrFloor : PX.Data.BQL.BqlDecimal.Field<usrFloor> { }
        #endregion

        #region UsrCeiling
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Ceiling", IsReadOnly = true)]
        public decimal? UsrCeiling { get; set; }
        public abstract class usrCeiling : PX.Data.BQL.BqlDecimal.Field<usrCeiling> { }
        #endregion

        #region UsrBasisValue
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Price / TOZ @ Basis", IsReadOnly = true)]
        [PXFormula(typeof(Switch<
            Case<Where<Current<usrCommodity>, Equal<CommodityType.gold>>, APVendorPrice.salesPrice,
            Case<Where<Current<usrCommodity>, Equal<CommodityType.silver>>, Div<Add<APVendorPrice.salesPrice, Add<APVendorPrice.salesPrice, usrMatrixStep>>, ASCIStarConstants.DecimalTwo>>>>))]
        public decimal? UsrBasisValue { get; set; }
        public abstract class usrBasisValue : PX.Data.BQL.BqlDecimal.Field<usrBasisValue> { }
        #endregion

        //#region UsrIncrementPerGram
        //[PXDecimal(6)]
        //[PXUIField(DisplayName = "Increment/G", Visible = false, Enabled = false)]
        //public decimal? UsrIncrementPerGram { get; set; }
        //public abstract class usrIncrementPerGram : PX.Data.BQL.BqlDecimal.Field<usrIncrementPerGram> { }
        //#endregion

        #region UsrFormAPI
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Form API", Visible = false, Enabled = false)]
        public bool? UsrFormAPI { get; set; }
        public abstract class usrFormAPI : PX.Data.BQL.BqlBool.Field<usrFormAPI> { }
        #endregion
    }
}