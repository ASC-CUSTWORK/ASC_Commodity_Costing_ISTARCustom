using PX.Data;
using System;
using static ASCJewelryLibrary.Common.Descriptor.ASCJConstants;

namespace ASCJewelryLibrary.IN.CacheExt
{
    [Serializable]
    [PXCacheName("IN Unit Extension")]
    public class ASCJINUnitExt : PXCacheExtension<PX.Objects.IN.INUnit>
    {
        public static bool IsActive() => true;

        #region UsrASCJCommodity 
        [PXDBString(1)]
        [PXUIField(DisplayName = "Commodity Type")]
        [CommodityType.ASCJList]
        public virtual string UsrASCJCommodity { get; set; }
        public abstract class usrASCJCommodity : PX.Data.BQL.BqlString.Field<usrASCJCommodity> { }
        #endregion

    }
}