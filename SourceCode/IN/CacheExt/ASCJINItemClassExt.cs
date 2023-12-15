using PX.Data;
using PX.Objects.IN;
using System;
using static ASCJewelryLibrary.Common.Descriptor.ASCJConstants;

namespace ASCJewelryLibrary.IN.CacheExt
{
    [Serializable]
    [PXCacheName("IN Item Class Extension")]
    public class ASCJINItemClassExt : PXCacheExtension<INItemClass>
    {
        public static bool IsActive() => true;

        #region UsrASCJCostingType
        [PXDBString(1, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Costing Type")]
        [CostingType.ASCJList]
        [PXDefault(CostingType.StandardCost, PersistingCheck = PXPersistingCheck.Null)]
        public virtual string UsrASCJCostingType { get; set; }
        public abstract class usrASCJCostingType : PX.Data.BQL.BqlString.Field<usrASCJCostingType> { }
        #endregion
    }
}