using PX.Data;
using PX.Objects.AP;
using System;
using static ASCISTARCustom.Common.Descriptor.ASCIStarConstants;

namespace ASCISTARCustom
{
    public sealed class ASCIStarAPVendorPriceExt : PXCacheExtension<APVendorPrice>
    {
        public static bool IsActive() => true;

        #region UsrMarket
        [PXDBString(2, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Market", Required = true)]
        [MarketList.List]
        [PXDefault(MarketList.LondonPM, PersistingCheck = PXPersistingCheck.Nothing)]
        public string UsrMarket { get; set; }
        public abstract class usrMarket : PX.Data.BQL.BqlString.Field<usrMarket> { }
        #endregion

        #region UsrMarketID
        [PXDBInt()]
        [PXUIField(DisplayName = "Market Vendor")]
        [PXSelector(
        typeof(Search2<Vendor.bAccountID, InnerJoin<VendorClass, On<Vendor.vendorClassID, Equal<VendorClass.vendorClassID>>>, Where<VendorClass.vendorClassID, Equal<MarketClass>>>),
            typeof(Vendor.acctCD), typeof(Vendor.acctName)
            , SubstituteKey = typeof(Vendor.acctCD), DescriptionField = typeof(Vendor.acctName))]
        public int? UsrMarketID { get; set; }
        public abstract class usrMarketID : PX.Data.BQL.BqlDecimal.Field<usrMarketID> { }

        #endregion

        #region UsrCommodityPrice
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Market Price")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrCommodityPrice { get; set; }
        public abstract class usrCommodityPrice : PX.Data.BQL.BqlDecimal.Field<usrCommodityPrice> { }
        #endregion

        #region UsrCommodityLossPct
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Loss Pct")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrCommodityLossPct { get; set; }
        public abstract class usrCommodityLossPct : PX.Data.BQL.BqlDecimal.Field<usrCommodityLossPct> { }
        #endregion

        #region UsrCommoditySurchargePct
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Surcharge Pct")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrCommoditySurchargePct { get; set; }
        public abstract class usrCommoditySurchargePct : PX.Data.BQL.BqlDecimal.Field<usrCommoditySurchargePct> { }
        #endregion 

        //#region UsrIncrement
        //[PXDBDecimal(6)]
        //[PXUIField(DisplayName = "Increment", Visible = true, Enabled = false)]
        //public virtual decimal? UsrIncrement { get; set; }
        //public abstract class usrIncrement : PX.Data.BQL.BqlDecimal.Field<usrIncrement> { }
        //#endregion

        #region UsrCommodityIncrement
        /* CHECK HERE MATT */
        /* CONVERT TO STANDARD FIELD AND DEFAULT TO MARKET INCREMENT LOOKUP PERCENTAGE FOR VENDOR MARKUP */
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Metal Increment", Visible = true)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrCommodityIncrement { get; set; }
        public abstract class usrCommodityIncrement : PX.Data.BQL.BqlDecimal.Field<usrCommodityIncrement> { }
        #endregion

        #region UsrCommodity
        [PXDBString(1)]
        [PXUIField(DisplayName = "Commodity Metal Type")]
        [CommodityType.List]
        [PXDefault(CommodityType.Undefined, PersistingCheck = PXPersistingCheck.Nothing)]
        public string UsrCommodity { get; set; }
        public abstract class usrCommodity : PX.Data.BQL.BqlString.Field<usrCommodity> { }
        #endregion

        #region UsrCommodityPerGram
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Price/Gram", Visible = true, Enabled = false)]
        public decimal? UsrCommodityPerGram { get; set; }
        public abstract class usrCommodityPerGram : PX.Data.BQL.BqlDecimal.Field<usrCommodityPerGram> { }
        #endregion

        #region UsrIncrementPerGram
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Increment/G", Visible = true, Enabled = false)]
        public decimal? UsrIncrementPerGram { get; set; }
        public abstract class usrIncrementPerGram : PX.Data.BQL.BqlDecimal.Field<usrIncrementPerGram> { }
        #endregion

        #region UsrFormAPI
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Form API", Visible = false, Enabled = false)]
        public bool? UsrFormAPI { get; set; }
        public abstract class usrFormAPI : PX.Data.BQL.BqlBool.Field<usrFormAPI> { }
        #endregion
    }
}