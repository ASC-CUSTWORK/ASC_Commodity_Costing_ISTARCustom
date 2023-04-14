using ASCISTARCustom.Inventory.Descriptor.Constants;
using PX.Data;
using PX.Objects.IN;

namespace ASCISTARCustom
{
    public class ASCIStarINItemClassExt : PXCacheExtension<INItemClass>
    {
        public static bool IsActive() => true;

        #region UsrCostingType
        [PXDBString(1, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Costing Type")]
        [ASCIStarCostingType.List]
        [PXDefault(ASCIStarCostingType.StandardCost, PersistingCheck = PXPersistingCheck.Null)]
        public virtual string UsrCostingType { get; set; }
        public abstract class usrCostingType : PX.Data.BQL.BqlString.Field<usrCostingType> { }
        #endregion

        #region UsrCostRollupType
        [PXDBString(1, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Rollup Type")]
        [ASCIStarCostRollupType.List]
        [PXDefault(ASCIStarCostRollupType.Blank, PersistingCheck = PXPersistingCheck.Null)]
        public virtual string UsrCostRollupType { get; set; }
        public abstract class usrCostRollupType : PX.Data.BQL.BqlString.Field<usrCostRollupType> { }
        #endregion    
    }
}