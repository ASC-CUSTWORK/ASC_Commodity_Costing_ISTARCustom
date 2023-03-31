using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.AR;
using PX.Objects;
using System.Collections.Generic;
using System;
using ASCISTARCustom.Cost.Descriptor;

namespace ASCISTARCustom
{
    public class ASCIStarINItemClassExt : PXCacheExtension<PX.Objects.IN.INItemClass>
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
        [PXDefault(ASCIStarCostRollupType.Other, PersistingCheck = PXPersistingCheck.Null)]
        public virtual string UsrCostRollupType { get; set; }
        public abstract class usrCostRollupType : PX.Data.BQL.BqlString.Field<usrCostRollupType> { }
        #endregion    
    }
}