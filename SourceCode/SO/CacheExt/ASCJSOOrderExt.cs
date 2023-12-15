using PX.Data;
using PX.Objects.SO;
using System;

namespace ASCJewelryLibrary.SO.CacheExt
{
    [Serializable]
    [PXCacheName("SOOrder Extension")]
    public class ASCJSOOrderExt : PXCacheExtension<SOOrder>
    {
        public static bool IsActive() => true;

        #region UsrASCJLegacyOrder
        [PXDBString(20)]
        [PXUIField(DisplayName = "Legacy Order")]
        public virtual string UsrASCJLegacyOrder { get; set; }
        public abstract class usrASCJLegacyOrder : PX.Data.BQL.BqlString.Field<usrASCJLegacyOrder> { }
        #endregion
    }
}