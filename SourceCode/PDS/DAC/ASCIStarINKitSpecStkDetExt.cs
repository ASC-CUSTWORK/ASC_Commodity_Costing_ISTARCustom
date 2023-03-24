using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects;
using System.Collections.Generic;
using System;

namespace ASCISTARCustom
{
    public class ASCIStarINKitSpecStkDetExt : PXCacheExtension<PX.Objects.IN.INKitSpecStkDet>
    {
        public static bool IsActive() => true;
        //#region UsrItemClassID
        //[PXInt]
        //[PXParent(typeof(Select<
        //InventoryItem,
        //Where<
        //    InventoryItem.inventoryID, Equal<Current<INKitSpecStkDet.compInventoryID>>>>))]
        //[PXUIField(DisplayName = "Item Cost")]
        //[PXFormula(typeof(Parent<InventoryItem.itemClassID>))]
        //public virtual int? UsrItemClassID { get; set; }
        //public abstract class usrItemClassID : PX.Data.BQL.BqlInt.Field<usrItemClassID> { }
        //#endregion

        #region UsrUnitCost
        [PXDBDecimal(6)]
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
        /*[PXFormula(typeof(Mult<INKitSpecStkDet.baseDfltCompQty, usrUnitCost>))]*/
        [PXFormula(typeof(Mult<INKitSpecStkDet.dfltCompQty, usrUnitCost>), typeof(SumCalc<ASCIStarINKitSpecStkDetExt.usrExtCost>))]
        public virtual decimal? UsrExtCost { get; set; }
        public abstract class usrExtCost : PX.Data.BQL.BqlDecimal.Field<usrExtCost> { }
        #endregion

        #region UsrCostingType
        [PXDBString(1, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Costing Type")]
        [CostingType.List]
        [PXDefault("F")]
        public virtual string UsrCostingType { get; set; }
        public abstract class usrCostingType : PX.Data.BQL.BqlString.Field<usrCostingType> { }
        #endregion

        #region UsrCostRollupType
        [PXString(1, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Rollup Type", Enabled = false)]
        [CostRollupType.List]
        public virtual string UsrCostRollupType { get; set; }
        public abstract class usrCostRollupType : PX.Data.BQL.BqlString.Field<usrCostRollupType> { }
        #endregion   



    }
}