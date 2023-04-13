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
using static ASCISTARCustom.Common.Descriptor.ASCIStarConstants;

namespace ASCISTARCustom 
{
    public class ASCIStarINUnitExt : PXCacheExtension<PX.Objects.IN.INUnit>
    {
        #region Static Method
        public static bool IsActive()
        {
            return true;
        }
        #endregion

        #region UsrCommodity 
        [PXDBString(1)]
        [PXUIField(DisplayName = "Commodity Type")]
        [CommodityType.List]
        public virtual string UsrCommodity { get; set; }
        public abstract class usrCommodity : PX.Data.BQL.BqlString.Field<usrCommodity> { }
        #endregion

    }
}