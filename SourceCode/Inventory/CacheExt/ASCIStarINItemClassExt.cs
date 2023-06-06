using PX.Data;
using PX.Objects.IN;
using static ASCISTARCustom.Common.Descriptor.ASCIStarConstants;

namespace ASCISTARCustom
{
    public class ASCIStarINItemClassExt : PXCacheExtension<INItemClass>
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

        #region UsrCostRollupType
        [PXDBString(1, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Rollup Type")]
        [CostRollupType.List]
        [PXDefault(CostRollupType.Blank, PersistingCheck = PXPersistingCheck.Null)]
        public virtual string UsrCostRollupType { get; set; }
        public abstract class usrCostRollupType : PX.Data.BQL.BqlString.Field<usrCostRollupType> { }
        #endregion    
    }
}