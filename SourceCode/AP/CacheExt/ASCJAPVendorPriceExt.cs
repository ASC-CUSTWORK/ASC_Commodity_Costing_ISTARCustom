using ASCJewelryLibrary.Common.Descriptor;
using ASCJewelryLibrary.IN.CacheExt;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.IN;
using System;
using static ASCJewelryLibrary.Common.Descriptor.ASCJConstants;

namespace ASCJewelryLibrary.AP.CacheExt
{
    [PXCacheName("Vendor Prices DAC Extension")]
    public sealed class ASCJAPVendorPriceExt : PXCacheExtension<APVendorPrice>
    {
        public static bool IsActive() => true;

        #region UsrASCJMarket
        [PXDBString(2, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Market")]
        [MarketList.ASCJList]
        public string UsrASCJMarket { get; set; }
        public abstract class usrASCJMarket : PX.Data.BQL.BqlString.Field<usrASCJMarket> { }
        #endregion

        #region UsrASCJMarketID
        [PXDBInt()]
        [PXUIField(DisplayName = "Market Vendor", Visible = false)]
        [PXSelector(
        typeof(Search2<Vendor.bAccountID, InnerJoin<VendorClass, On<Vendor.vendorClassID, Equal<VendorClass.vendorClassID>>>, Where<VendorClass.vendorClassID, Equal<MarketClass>>>),
            typeof(Vendor.acctCD), typeof(Vendor.acctName)
            , SubstituteKey = typeof(Vendor.acctCD), DescriptionField = typeof(Vendor.acctName))]
        public int? UsrASCJMarketID { get; set; }
        public abstract class usrASCJMarketID : PX.Data.BQL.BqlDecimal.Field<usrASCJMarketID> { }
        #endregion

        #region UsrASCJCommodity
        [PXDBString(1)]
        [PXUIField(DisplayName = "Commodity Metal Type", IsReadOnly = true)]
        [CommodityType.ASCJList]
        [PXDefault(typeof(Search<ASCJINInventoryItemExt.usrASCJCommodityType, Where<InventoryItem.inventoryID, Equal<Current<APVendorPrice.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<APVendorPrice.inventoryID>))]
        public string UsrASCJCommodity { get; set; }
        public abstract class usrASCJCommodity : PX.Data.BQL.BqlString.Field<usrASCJCommodity> { }
        #endregion

        #region UsrASCJCommodityLossPct
        [PXDBDecimal(2)]
        [PXUIField(DisplayName = "Loss, %")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrASCJCommodityLossPct { get; set; }
        public abstract class usrASCJCommodityLossPct : PX.Data.BQL.BqlDecimal.Field<usrASCJCommodityLossPct> { }
        #endregion

        #region UsrASCJCommoditySurchargePct
        [PXDBDecimal(2)]
        [PXUIField(DisplayName = "Surcharge %")]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrASCJCommoditySurchargePct { get; set; }
        public abstract class usrASCJCommoditySurchargePct : PX.Data.BQL.BqlDecimal.Field<usrASCJCommoditySurchargePct> { }
        #endregion

        //Might be deleted in future
        #region UsrASCJLaborPerUnit
        [PXDBDecimal(2)]
        [PXUIField(DisplayName = "Labor/Grams", Enabled = false, Visible = false)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrASCJLaborPerUnit { get; set; }
        public abstract class usrASCJLaborPerUnit : PX.Data.BQL.BqlDecimal.Field<usrASCJLaborPerUnit> { }
        #endregion

        #region UsrASCJCommodityPerGram
        [PXDecimal(4)]
        [PXUIField(DisplayName = "Basis Price/Gram", IsReadOnly = true)]
        [PXFormula(typeof(Switch<Case<Where<Current<APVendorPrice.uOM>, NotEqual<TOZ>>, Null>, Div<APVendorPrice.salesPrice, TOZ2GRAM_31_10348>>))]
        public decimal? UsrASCJCommodityPerGram { get; set; }
        public abstract class usrASCJCommodityPerGram : PX.Data.BQL.BqlDecimal.Field<usrASCJCommodityPerGram> { }
        #endregion

        #region UsrASCJCommodityIncrement
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Metal Increment", IsReadOnly = true)]
        [PXFormula(typeof(Switch<
            Case<Where<APVendorPrice.uOM.IsNotEqual<TOZ>.Or<APVendorPrice.salesPrice.IsNotNull.And<APVendorPrice.salesPrice.IsEqual<PX.Objects.CS.decimal0>>>>, Null>,
                        Mult<Div<usrASCJCommodityPerGram, APVendorPrice.salesPrice>, Add<ASCJConstants.DecimalOne, Div<usrASCJCommoditySurchargePct, ASCJConstants.DecimalOneHundred>>>>))]
        public decimal? UsrASCJCommodityIncrement { get; set; }
        public abstract class usrASCJCommodityIncrement : PX.Data.BQL.BqlDecimal.Field<usrASCJCommodityIncrement> { }
        #endregion

        #region UsrASCJMatrixStep
        [PXDBDecimal(4, MinValue = 0, MaxValue = 10)]
        [PXUIField(DisplayName = "Matrix Step")]
        [PXDefault(TypeCode.Decimal, "0.5000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrASCJMatrixStep { get; set; }
        public abstract class usrASCJMatrixStep : PX.Data.BQL.BqlDecimal.Field<usrASCJMatrixStep> { }
        #endregion

        #region UsrASCJFloor
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Floor", IsReadOnly = true)]
        public decimal? UsrASCJFloor { get; set; }
        public abstract class usrASCJFloor : PX.Data.BQL.BqlDecimal.Field<usrASCJFloor> { }
        #endregion

        #region UsrASCJCeiling
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Ceiling", IsReadOnly = true)]
        public decimal? UsrASCJCeiling { get; set; }
        public abstract class usrASCJCeiling : PX.Data.BQL.BqlDecimal.Field<usrASCJCeiling> { }
        #endregion

        #region UsrASCJBasisValue
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Price / TOZ @ Basis", IsReadOnly = true)]
        public decimal? UsrASCJBasisValue { get; set; }
        public abstract class usrASCJBasisValue : PX.Data.BQL.BqlDecimal.Field<usrASCJBasisValue> { }
        #endregion

        #region UsrASCJFormAPI
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Form API", Visible = false, Enabled = false)]
        public bool? UsrASCJFormAPI { get; set; }
        public abstract class usrASCJFormAPI : PX.Data.BQL.BqlBool.Field<usrASCJFormAPI> { }
        #endregion
    }
}