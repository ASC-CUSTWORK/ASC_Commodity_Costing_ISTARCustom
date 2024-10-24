using PX.Data;
using PX.Objects.IN;
using static ASCJSMCustom.Common.Descriptor.ASCJSMConstants;

namespace ASCJSMCustom.IN.CacheExt
{
    public class ASCJSMINItemClassExt : PXCacheExtension<INItemClass>
    {
        public static bool IsActive() => true;

        #region UsrCostingType
        [PXDBString(1, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Costing Type")]
        [CostingType.List]
        [PXDefault(CostingType.StandardCost, PersistingCheck = PXPersistingCheck.Null)]
        public virtual string UsrCostingType { get; set; }
        public abstract class usrCostingType : PX.Data.BQL.BqlString.Field<usrCostingType> { }
        #endregion
    }
}