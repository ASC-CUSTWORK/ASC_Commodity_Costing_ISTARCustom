using PX.Data;
using static ASCJSMCustom.Common.Descriptor.ASCJSMConstants;

namespace ASCJSMCustom.IN.CacheExt
{
    public class ASCJSMINUnitExt : PXCacheExtension<PX.Objects.IN.INUnit>
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