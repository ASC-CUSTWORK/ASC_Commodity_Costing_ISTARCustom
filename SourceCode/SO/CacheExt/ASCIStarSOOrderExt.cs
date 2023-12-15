using PX.Data;
using PX.Objects.SO;

namespace ASCJewelryLibrary.SO.CacheExt
{
    public class ASCJSOOrderExt : PXCacheExtension<SOOrder>
    {
        public static bool IsActive() => true;

        #region UsrLegacyOrder
        [PXDBString(20)]
        [PXUIField(DisplayName = "Legacy Order")]
        public virtual string UsrLegacyOrder { get; set; }
        public abstract class usrLegacyOrder : PX.Data.BQL.BqlString.Field<usrLegacyOrder> { }
        #endregion
    }
}