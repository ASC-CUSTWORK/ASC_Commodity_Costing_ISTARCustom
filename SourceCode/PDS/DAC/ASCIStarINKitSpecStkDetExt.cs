using PX.Data;
using PX.Objects.IN;
using System;
using ASCISTARCustom.Cost.Descriptor;

namespace ASCISTARCustom
{
    public sealed class ASCIStarINKitSpecStkDetExt : PXCacheExtension<PX.Objects.IN.INKitSpecStkDet>
    {
        public static bool IsActive() => true;

        #region UsrUnitCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Unit Cost")]
        public decimal? UsrUnitCost { get; set; }
        public abstract class usrUnitCost : PX.Data.BQL.BqlDecimal.Field<usrUnitCost> { }
        #endregion

        #region UsrUnitPct
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Unit Pct")]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrUnitPct { get; set; }
        public abstract class usrUnitPct : PX.Data.BQL.BqlDecimal.Field<usrUnitPct> { }
        #endregion

        #region UsrExtCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Ext Cost")]
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrUnitCost>))]
        public decimal? UsrExtCost { get; set; }
        public abstract class usrExtCost : PX.Data.BQL.BqlDecimal.Field<usrExtCost> { }
        #endregion

        #region UsrCostingType
        [PXDBString(1, IsUnicode = true, InputMask = "")]
        [ASCIStarCostingType.List]
        [PXUIField(DisplayName = "Costing Type")]
        [PXFormula(typeof(Selector<INKitSpecStkDet.compInventoryID, ASCIStarINInventoryItemExt.usrCostingType>))]
        public string UsrCostingType { get; set; }
        public abstract class usrCostingType : PX.Data.BQL.BqlString.Field<usrCostingType> { }
        #endregion

        #region UsrCostRollupType
        [PXString(1, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Rollup Type")]
        [ASCIStarCostRollupType.List]
        public string UsrCostRollupType { get; set; }
        public abstract class usrCostRollupType : PX.Data.BQL.BqlString.Field<usrCostRollupType> { }
        #endregion   
    }
}