using PX.Data;
using PX.Objects.PO;
using System;

namespace ASCISTARCustom
{
    [Serializable]
    [PXCacheName("ASC POLine Extension")]
    public class ASCIStarPOLineExt : PXCacheExtension<POLine>
    {
        public static bool IsActive() => true;

        #region UsrPurity
        [PXDBString(20)]
        [PXUIField(DisplayName = "Purity", Enabled = false)]
        public string UsrPurity { get; set; }
        public abstract class usrPurity : PX.Data.BQL.BqlString.Field<usrPurity> { }
        #endregion

        #region UsrWeight
        [PXDBDecimal]
        [PXUIField(DisplayName = "Weight/gram", Enabled = false)]
        public decimal? UsrWeight { get; set; }
        public abstract class usrWeight : PX.Data.BQL.BqlDecimal.Field<usrWeight> { }
        #endregion

        #region UsrTotalWeight
        [PXDecimal]
        [PXUIField(DisplayName = "Total Weight (g)", Enabled = false)]
        public virtual decimal? UsrTotWeight { get; set; }
        public abstract class usrTotWeight : PX.Data.BQL.BqlDecimal.Field<usrTotWeight> { }
        #endregion

        #region UsrMarketPrice
        [PXUIField(DisplayName = "Market Price", IsReadOnly = true)]
        [PXDBDecimal(6)]
        public virtual decimal? UsrMarketPrice { get; set; }
        public abstract class usrMarketPrice : PX.Data.BQL.BqlDecimal.Field<usrMarketPrice> { }
        #endregion

        #region UsrContractIncrement
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Increment", IsReadOnly = true)]
        public virtual decimal? UsrContractIncrement { get; set; }
        public abstract class usrContractIncrement : PX.Data.BQL.BqlDecimal.Field<usrContractIncrement> { }
        #endregion

        #region UsrActualGRAMGold
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Gold, Grams", IsReadOnly = true)]
        public virtual decimal? UsrActualGRAMGold { get; set; }
        public abstract class usrActualGRAMGold : PX.Data.BQL.BqlDecimal.Field<usrActualGRAMGold> { }
        #endregion

        #region UsrPricingGRAMGold
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Fine Gold, Grams", IsReadOnly = true)]
        public virtual decimal? UsrPricingGRAMGold { get; set; }
        public abstract class usrPricingGRAMGold : PX.Data.BQL.BqlDecimal.Field<usrPricingGRAMGold> { }
        #endregion

        #region UsrActualGRAMSilver
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Silver, Grams", IsReadOnly = true)]
        public virtual decimal? UsrActualGRAMSilver { get; set; }
        public abstract class usrActualGRAMSilver : PX.Data.BQL.BqlDecimal.Field<usrActualGRAMSilver> { }
        #endregion

        #region UsrPricingGRAMSilver
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Fine Silver, Grams", IsReadOnly = true)]
        public virtual decimal? UsrPricingGRAMSilver { get; set; }
        public abstract class usrPricingGRAMSilver : PX.Data.BQL.BqlDecimal.Field<usrPricingGRAMSilver> { }
        #endregion

        #region UsrRatePerGram
        [PXDBDecimal]
        [PXUIField(DisplayName = "Rate/gram", IsReadOnly = true)]
        public decimal? UsrRatePerGram { get; set; }
        public abstract class usrRatePerGram : PX.Data.BQL.BqlDecimal.Field<usrRatePerGram> { }
        #endregion

        #region UsrMaterialCost
        [PXDBDecimal]
        [PXUIField(DisplayName = "Material Cost", IsReadOnly = true)]
        public decimal? UsrMaterialCost { get; set; }
        public abstract class usrMaterialCost : PX.Data.BQL.BqlDecimal.Field<usrMaterialCost> { }
        #endregion
    }
}