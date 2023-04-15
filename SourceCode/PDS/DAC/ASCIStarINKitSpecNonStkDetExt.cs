using ASCISTARCustom.Inventory.Descriptor.Constants;
using ASCISTARCustom.PDS.Descriptor;
using ASCISTARCustom.PDS.Interfaces;
using PX.Data;
using PX.Objects.IN;
using System;

namespace ASCISTARCustom
{
    public sealed class ASCIStarINKitSpecNonStkDetExt : PXCacheExtension<PX.Objects.IN.INKitSpecNonStkDet>, IASCIStarCostRollup
    {
        public static bool IsActive() => true;

        #region UsrItemClassID
        [PXInt]
        [PXParent(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<INKitSpecNonStkDet.compInventoryID>>>>))]
        [PXFormula(typeof(Parent<InventoryItem.itemClassID>))]
        public int? UsrItemClassID { get; set; }
        public abstract class usrItemClassID : PX.Data.BQL.BqlInt.Field<usrItemClassID> { }
        #endregion

        #region UsrUnitCost
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Unit Cost")]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
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
        [PXUIField(DisplayName = "Ext Cost", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Mult<INKitSpecNonStkDet.dfltCompQty, usrUnitCost>))]
        [ASCIStarCostAssignment]
        public decimal? UsrExtCost { get; set; }
        public abstract class usrExtCost : PX.Data.BQL.BqlDecimal.Field<usrExtCost> { }
        #endregion

        #region UsrCostingType
        [PXDBString(1, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Costing Type", Enabled = false, Visible = false)]
        [ASCIStarCostingType.List]
        public string UsrCostingType { get; set; }
        public abstract class usrCostingType : PX.Data.BQL.BqlString.Field<usrCostingType> { }
        #endregion   

        #region UsrCostRollupType
        [PXDBString(1, IsUnicode = true, InputMask = "")]
        [ASCIStarCostRollupType.List]
        [PXDefault(ASCIStarCostRollupType.Fabrication, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Rollup Type", Enabled = true)]
        public string UsrCostRollupType { get; set; }
        public abstract class usrCostRollupType : PX.Data.BQL.BqlString.Field<usrCostRollupType> { }
        #endregion          
    }

}