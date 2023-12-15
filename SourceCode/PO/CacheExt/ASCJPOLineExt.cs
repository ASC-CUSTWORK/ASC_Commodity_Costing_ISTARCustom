using PX.Data;
using PX.Objects.PO;
using System;

namespace ASCJewelryLibrary.PO.DAC
{
    [Serializable]
    [PXCacheName("POLine Extension")]
    public class ASCJPOLineExt : PXCacheExtension<POLine>
    {
        public static bool IsActive() => true;

        #region UsrASCJPurity
        [PXDBString(20)]
        [PXUIField(DisplayName = "Purity", Enabled = false)]
        public string UsrASCJPurity { get; set; }
        public abstract class usrASCJPurity : PX.Data.BQL.BqlString.Field<usrASCJPurity> { }
        #endregion

        #region UsrASCJWeight
        [PXDBDecimal]
        [PXUIField(DisplayName = "Weight/gram", Enabled = false)]
        public decimal? UsrASCJWeight { get; set; }
        public abstract class usrASCJWeight : PX.Data.BQL.BqlDecimal.Field<usrASCJWeight> { }
        #endregion

        #region UsrASCJTotalWeight
        [PXDecimal]
        [PXUIField(DisplayName = "Total Weight (g)", Enabled = false)]
        public virtual decimal? UsrASCJTotWeight { get; set; }
        public abstract class usrASCJTotWeight : PX.Data.BQL.BqlDecimal.Field<usrASCJTotWeight> { }
        #endregion

        #region UsrASCJMarketPrice
        [PXUIField(DisplayName = "Market Price", IsReadOnly = true)]
        [PXDBDecimal(6)]
        public virtual decimal? UsrASCJMarketPrice { get; set; }
        public abstract class usrASCJMarketPrice : PX.Data.BQL.BqlDecimal.Field<usrASCJMarketPrice> { }
        #endregion

        #region UsrASCJContractIncrement
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Increment", IsReadOnly = true)]
        public virtual decimal? UsrASCJContractIncrement { get; set; }
        public abstract class usrASCJContractIncrement : PX.Data.BQL.BqlDecimal.Field<usrASCJContractIncrement> { }
        #endregion

        #region UsrASCJActualGRAMGold
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Gold, Grams", IsReadOnly = true)]
        public virtual decimal? UsrASCJActualGRAMGold { get; set; }
        public abstract class usrASCJActualGRAMGold : PX.Data.BQL.BqlDecimal.Field<usrASCJActualGRAMGold> { }
        #endregion

        #region UsrASCJPricingGRAMGold
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Fine Gold, Grams", IsReadOnly = true)]
        public virtual decimal? UsrASCJPricingGRAMGold { get; set; }
        public abstract class usrASCJPricingGRAMGold : PX.Data.BQL.BqlDecimal.Field<usrASCJPricingGRAMGold> { }
        #endregion

        #region UsrASCJActualGRAMSilver
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Silver, Grams", IsReadOnly = true)]
        public virtual decimal? UsrASCJActualGRAMSilver { get; set; }
        public abstract class usrASCJActualGRAMSilver : PX.Data.BQL.BqlDecimal.Field<usrASCJActualGRAMSilver> { }
        #endregion

        #region UsrASCJPricingGRAMSilver
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Fine Silver, Grams", IsReadOnly = true)]
        public virtual decimal? UsrASCJPricingGRAMSilver { get; set; }
        public abstract class usrASCJPricingGRAMSilver : PX.Data.BQL.BqlDecimal.Field<usrASCJPricingGRAMSilver> { }
        #endregion

        #region UsrASCJRatePerGram
        [PXDBDecimal]
        [PXUIField(DisplayName = "Rate/gram", IsReadOnly = true)]
        public decimal? UsrASCJRatePerGram { get; set; }
        public abstract class usrASCJRatePerGram : PX.Data.BQL.BqlDecimal.Field<usrASCJRatePerGram> { }
        #endregion

        #region UsrASCJMaterialCost
        [PXDBDecimal]
        [PXUIField(DisplayName = "Material Cost", IsReadOnly = true)]
        public decimal? UsrASCJMaterialCost { get; set; }
        public abstract class usrASCJMaterialCost : PX.Data.BQL.BqlDecimal.Field<usrASCJMaterialCost> { }
        #endregion

        #region UsrASCJBasisValue
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Basis Value", IsReadOnly = true)]
        public decimal? UsrASCJBasisValue { get; set; }
        public abstract class usrASCJBasisValue : PX.Data.BQL.BqlDecimal.Field<usrASCJBasisValue> { }
        #endregion
    }
}