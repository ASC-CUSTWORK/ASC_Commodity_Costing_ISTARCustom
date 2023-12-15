using ASCJewelryLibrary.IN.CacheExt;
using ASCJewelryLibrary.INKit.Descriptor;
using ASCJewelryLibrary.INKit.Interfaces;
using PX.Data;
using PX.Objects.IN;
using System;
using static ASCJewelryLibrary.Common.Descriptor.ASCJConstants;

namespace ASCJewelryLibrary.INKit.CacheExt
{
    [Serializable]
    [PXCacheName("IN Kit Spec Non Stock Extension")]
    public sealed class ASCJINKitSpecNonStkDetExt : PXCacheExtension<PX.Objects.IN.INKitSpecNonStkDet>, IASCJCostRollup
    {
        public static bool IsActive() => true;

        #region UsrASCJItemClassID
        [PXInt]
        [PXParent(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<INKitSpecNonStkDet.compInventoryID>>>>))]
        [PXFormula(typeof(Parent<InventoryItem.itemClassID>))]
        public int? UsrASCJItemClassID { get; set; }
        public abstract class usrASCJItemClassID : PX.Data.BQL.BqlInt.Field<usrASCJItemClassID> { }
        #endregion

        #region UsrASCJUnitCost
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Unit Cost")]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrASCJUnitCost { get; set; }
        public abstract class usrASCJUnitCost : PX.Data.BQL.BqlDecimal.Field<usrASCJUnitCost> { }
        #endregion

        #region UsrASCJUnitPct
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Unit Pct")]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrASCJUnitPct { get; set; }
        public abstract class usrASCJUnitPct : PX.Data.BQL.BqlDecimal.Field<usrASCJUnitPct> { }
        #endregion

        #region UsrASCJExtCost
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Ext Cost", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Mult<INKitSpecNonStkDet.dfltCompQty, usrASCJUnitCost>))]
        [ASCJCostAssignment]
        public decimal? UsrASCJExtCost { get; set; }
        public abstract class usrASCJExtCost : PX.Data.BQL.BqlDecimal.Field<usrASCJExtCost> { }
        #endregion

        #region UsrASCJCostingType
        [PXDBString(1, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Costing Type", Enabled = true, Visible = true)]
        [CostingType.List]
        [PXFormula(typeof(Selector<INKitSpecNonStkDet.compInventoryID, ASCJINInventoryItemExt.usrASCJCostingType>))]
        public string UsrASCJCostingType { get; set; }
        public abstract class usrASCJCostingType : PX.Data.BQL.BqlString.Field<usrASCJCostingType> { }
        #endregion   

        #region UsrASCJCostRollupType
        [PXDBString(1, IsUnicode = true, InputMask = "")]
        [CostRollupType.List]
        [PXDefault(CostRollupType.Fabrication, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Rollup Type", Enabled = true)]
        //[PXFormula(typeof(Selector<INKitSpecNonStkDet.compInventoryID, ASCJINInventoryItemExt.usrASCJCostRollupType>))]
        public string UsrASCJCostRollupType { get; set; }
        public abstract class usrASCJCostRollupType : PX.Data.BQL.BqlString.Field<usrASCJCostRollupType> { }
        #endregion          
    }

}