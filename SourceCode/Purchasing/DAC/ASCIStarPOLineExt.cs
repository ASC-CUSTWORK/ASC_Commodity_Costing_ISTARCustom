using PX.Common;
using PX.Data.BQL;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.Common.Bql;
using PX.Objects.Common.Discount.Attributes;
using PX.Objects.Common.Discount;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN.Matrix.Interfaces;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.PO;
using PX.Objects.TX;
using PX.Objects;
using System.Collections.Generic;
using System;

namespace ASCISTARCustom
{
    public class ASCIStarPOLineExt : PXCacheExtension<PX.Objects.PO.POLine>
    {
        public static bool IsActive() => true;
        
        #region UsrPurity
        [PXDBString(20)]
        [PXUIField(DisplayName = "Purity",Enabled =false)]
        public virtual string UsrPurity { get; set; }
        public abstract class usrPurity : PX.Data.BQL.BqlString.Field<usrPurity> { }
        #endregion

        #region UsrWeight
        [PXDBDecimal]
        [PXUIField(DisplayName = "Weight/gram", Enabled = false)]
        public virtual Decimal? UsrWeight { get; set; }
        public abstract class usrWeight : PX.Data.BQL.BqlDecimal.Field<usrWeight> { }
        #endregion

        #region UsrTotalWeight
        [PXDecimal]
        [PXUIField(DisplayName = "Total Weight (g)", Enabled = false)]
        public virtual Decimal? UsrTotWeight { get; set; }
        public abstract class usrTotWeight : PX.Data.BQL.BqlDecimal.Field<usrTotWeight> { }
        #endregion

        #region UsrActualGoldWgt
        [PXDecimal]
        [PXUIField(DisplayName = "Actual Gold (g)", Enabled = false)]
        public virtual Decimal? UsrActualGoldWgt { get; set; }
        public abstract class usrActualGoldWgt : PX.Data.BQL.BqlDecimal.Field<usrActualGoldWgt> { }
        #endregion

        #region UsrIntrinsicGold
        [PXDecimal]
        [PXUIField(DisplayName = "Intrinsic Gold (g)", Enabled = false)]
        public virtual Decimal? UsrIntrinsicGoldWgt { get; set; }
        public abstract class usrIntrinsicGoldWgt : PX.Data.BQL.BqlDecimal.Field<usrIntrinsicGoldWgt> { }
        #endregion

        #region UsrIntrinsicGoldWgt
        [PXDecimal]
        [PXUIField(DisplayName = "Actual Silver (g)", Enabled = false)]
        public virtual Decimal? UsrActualSilverWgt { get; set; }
        public abstract class usrActualSilverWgt : PX.Data.BQL.BqlDecimal.Field<usrActualSilverWgt> { }
        #endregion

        #region UsrIntrinsicSilver
        [PXDecimal]
        [PXUIField(DisplayName = "Intrinsic Silver (g)", Enabled = false)]
        public virtual Decimal? UsrIntrinsicSilverWgt { get; set; }
        public abstract class usrIntrinsicSilverWgt : PX.Data.BQL.BqlDecimal.Field<usrIntrinsicSilverWgt> { }
        #endregion

        #region UsrRatePerGram
        [PXDBDecimal]
        [PXUIField(DisplayName = "Rate/gram", Enabled = false)]
        public virtual Decimal? UsrRatePerGram { get; set; }
        public abstract class usrRatePerGram : PX.Data.BQL.BqlDecimal.Field<usrRatePerGram> { }
        #endregion

        #region UsrMaterialCost
        [PXDBDecimal]
        [PXUIField(DisplayName = "Material Cost", Enabled = false)]
        public virtual Decimal? UsrMaterialCost { get; set; }
        public abstract class usrMaterialCost : PX.Data.BQL.BqlDecimal.Field<usrMaterialCost> { }
        #endregion
    }
}