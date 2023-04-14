using ASCISTARCustom.Inventory.Descriptor.Constants;
using PX.Data;
using PX.Objects.IN;
using System;

namespace ASCISTARCustom
{
    public class ASCIStarINKitSpecNonStkDetExt : PXCacheExtension<PX.Objects.IN.INKitSpecNonStkDet>
    {
        public static bool IsActive() => true;

        #region UsrItemClassID
        [PXInt]
        [PXParent(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<INKitSpecNonStkDet.compInventoryID>>>>))]
        [PXFormula(typeof(Parent<InventoryItem.itemClassID>))]
        public virtual int? UsrItemClassID { get; set; }
        public abstract class usrItemClassID : PX.Data.BQL.BqlInt.Field<usrItemClassID> { }
        #endregion

        #region UsrUnitCost
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Unit Cost")]
        [PXDefault(TypeCode.Decimal, "0.00")]
        public virtual decimal? UsrUnitCost { get; set; }
        public abstract class usrUnitCost : PX.Data.BQL.BqlDecimal.Field<usrUnitCost> { }
        #endregion

        #region UsrUnitPct
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Unit Pct")]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual decimal? UsrUnitPct { get; set; }
        public abstract class usrUnitPct : PX.Data.BQL.BqlDecimal.Field<usrUnitPct> { }
        #endregion

        #region UsrExtCost
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Ext Cost")]
        [PXDefault(TypeCode.Decimal, "0.00")]
        /*[PXFormula(typeof(Mult<INKitSpecNonStkDet.baseDfltCompQty, usrUnitCost>))]*/
        [PXFormula(typeof(Mult<INKitSpecNonStkDet.dfltCompQty, usrUnitCost>), typeof(SumCalc<ASCIStarINKitSpecNonStkDetExt.usrExtCost>))]

        public virtual decimal? UsrExtCost { get; set; }
        public abstract class usrExtCost : PX.Data.BQL.BqlDecimal.Field<usrExtCost> { }
        #endregion

        #region UsrCostingType
        [PXDBString(1, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Costing Type")]
        [ASCIStarCostingType.List]
        [PXDefault("F")]
        public virtual string UsrCostingType { get; set; }
        public abstract class usrCostingType : PX.Data.BQL.BqlString.Field<usrCostingType> { }
        #endregion   

        #region UsrCostRollupType
        [PXString(1, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Rollup Type", Enabled = false)]
        [ASCIStarCostRollupType.List]
        public virtual string UsrCostRollupType { get; set; }
        public abstract class usrCostRollupType : PX.Data.BQL.BqlString.Field<usrCostRollupType> { }
        #endregion          
    }

}