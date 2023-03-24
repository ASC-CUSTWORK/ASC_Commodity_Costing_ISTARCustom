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
using PX.Objects.SO;
using PX.Objects.TX;
using PX.Objects;
using System.Collections.Generic;
using System;

namespace ASCISTARCustom
{
    public class ASCIStarSOOrderExt : PXCacheExtension<PX.Objects.SO.SOOrder>
    {
        public static bool IsActive() => true;

        #region UsrLegacyOrder
        [PXDBString(20)]
        [PXUIField(DisplayName = "Legacy Order")]
        public virtual string UsrLegacyOrder { get; set; }
        public abstract class usrLegacyOrder : PX.Data.BQL.BqlString.Field<usrLegacyOrder> { }
        #endregion

        //#region UsrDiscountPct
        //[PXDecimal]
        //[PXUIField(DisplayName = "Discount % Total")]
        //[PXFormula(typeof(Switch<
        //Case<Where<
        //SOOrder.curyOrderTotal, Equal<Zero>>,
        //Zero,
        //Case<Where<
        //SOOrder.curyOrderTotal, IsNull>,
        //Zero>>,
        //Div<SOOrder.curyDiscTot, SOOrder.curyOrderTotal>>))]
        //public virtual string UsrDiscountPct { get; set; }
        //public abstract class usrDiscountPct : PX.Data.BQL.BqlString.Field<usrDiscountPct> { }
        //#endregion



    }
}