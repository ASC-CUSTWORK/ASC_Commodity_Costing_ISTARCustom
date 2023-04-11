using ASCISTARCustom.Common.DAC;
using ASCISTARCustom.Cost.Descriptor;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.IN;
using System;

namespace ASCISTARCustom.Inventory.DAC
{
    [Serializable]
    [PXCacheName(_cacheName)]
    public class ASCIStarItemWeightCostSpec : AuditSystemFields, IBqlTable
    {
        private const string _cacheName = "Item/Weight Cost Specification";

        #region Keys
        public class PK : PrimaryKeyOf<ASCIStarItemWeightCostSpec>.By<inventoryID>
        {
            public static ASCIStarItemWeightCostSpec Find(PXGraph graph, int? inventoryID)
                => FindBy(graph, inventoryID);
        }
        public static class FK
        {
            public class InventoryItemFK : InventoryItem.PK.ForeignKeyOf<ASCIStarItemWeightCostSpec>.By<inventoryID> { }

            public class KitSpecificationFK : INKitSpecHdr.PK.ForeignKeyOf<ASCIStarItemWeightCostSpec>.By<inventoryID, revisionID> { }
        }
        #endregion

        #region InventoryID
        [StockItem(IsKey = true)]
        [PXDBDefault(typeof(InventoryItem.inventoryID))]
        [PXParent(typeof(FK.InventoryItemFK))]
        public virtual int? InventoryID { get; set; }
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        #endregion

        #region RevisionID
        [PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
        [PXUIField(DisplayName = "Revision ID")]
        public virtual string RevisionID { get; set; }
        public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }
        #endregion

        #region SubItemID
        [SubItem(typeof(ASCIStarItemWeightCostSpec.inventoryID))]
        [PXDefault(typeof(Search<
            InventoryItem.defaultSubItemID,
            Where<InventoryItem.inventoryID, Equal<Current<ASCIStarItemWeightCostSpec.inventoryID>>,
                And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<ASCIStarItemWeightCostSpec.inventoryID>))]
        public virtual int? SubItemID { get; set; }
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
        #endregion

        #region LegacyID
        [PXDBString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Legacy ID")]
        public virtual string LegacyID { get; set; }
        public abstract class legacyID : PX.Data.BQL.BqlString.Field<legacyID> { }
        #endregion

        #region LegacyShortRef
        [PXDBString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Legacy Short Ref")]
        public virtual string LegacyShortRef { get; set; }
        public abstract class legacyShortRef : PX.Data.BQL.BqlString.Field<legacyShortRef> { }
        #endregion

        #region GoldGrams
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Gold, Grams")]
        public virtual decimal? GoldGrams { get; set; }
        public abstract class goldGrams : PX.Data.BQL.BqlDecimal.Field<goldGrams> { }
        #endregion

        #region FineGoldGrams
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Fine Gold, Grams")]
        public virtual decimal? FineGoldGrams { get; set; }
        public abstract class fineGoldGrams : PX.Data.BQL.BqlDecimal.Field<fineGoldGrams> { }
        #endregion

        #region SilverGrams
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Silver, Grams")]
        public virtual decimal? SilverGrams { get; set; }
        public abstract class silverGrams : PX.Data.BQL.BqlDecimal.Field<silverGrams> { }
        #endregion

        #region FineSilverGrams
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Fine Silver, Grams")]
        public virtual decimal? FineSilverGrams { get; set; }
        public abstract class fineSilverGrams : PX.Data.BQL.BqlDecimal.Field<fineSilverGrams> { }
        #endregion

        #region MetalLossPct
        [PXDBDecimal(4, MinValue = 0, MaxValue = 100)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Null)]
        [PXUIField(DisplayName = "Metal Loss, %")]
        public virtual decimal? MetalLossPct { get; set; }
        public abstract class metalLossPct : PX.Data.BQL.BqlDecimal.Field<metalLossPct> { }
        #endregion

        #region SurchargePct
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Null)]
        [PXUIField(DisplayName = "Surcharge %", Visible = true)]
        public virtual decimal? SurchargePct { get; set; }
        public abstract class surchargePct : PX.Data.BQL.BqlDecimal.Field<surchargePct> { }
        #endregion

        #region Increment
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Null)]
        [PXUIField(DisplayName = "Increment")]
        public virtual decimal? Increment { get; set; }
        public abstract class increment : PX.Data.BQL.BqlDecimal.Field<increment> { }
        #endregion

        #region RollupType
        [PXDBString(1, IsFixed = true, InputMask = "")]
        [ASCIStarCostRollupType.List]
        [PXDefault(ASCIStarCostRollupType.Other, PersistingCheck = PXPersistingCheck.Null)]
        [PXUIField(DisplayName = "Rollup Type")]
        public virtual string RollupType { get; set; }
        public abstract class rollupType : PX.Data.BQL.BqlString.Field<rollupType> { }
        #endregion

        #region CostingType
        [PXDBString(1, IsUnicode = true, InputMask = "")]
        [ASCIStarCostingType.List]
        [PXDefault(ASCIStarCostingType.ContractCost, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Costing Type")]
        public virtual string CostingType { get; set; }
        public abstract class costingType : PX.Data.BQL.BqlString.Field<costingType> { }
        #endregion

        #region UnitCost
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Unit Cost", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public virtual decimal? UnitCost { get; set; }
        public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
        #endregion

        #region CommodityCost
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Precious Metal Cost")]
        public virtual decimal? PreciousMetalCost { get; set; }
        public abstract class preciousMetalCost : PX.Data.BQL.BqlDecimal.Field<preciousMetalCost> { }
        #endregion

        #region FabricationCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Null)]
        [PXUIField(DisplayName = "Fabrication Cost")]
        public virtual decimal? FabricationCost { get; set; }
        public abstract class fabricationCost : PX.Data.BQL.BqlDecimal.Field<fabricationCost> { }
        #endregion

        #region PackagingCost
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Null)]
        [PXUIField(DisplayName = "Packaging Cost")]
        public virtual decimal? PackagingCost { get; set; }
        public abstract class packagingCost : PX.Data.BQL.BqlDecimal.Field<packagingCost> { }
        #endregion

        #region LaborCost
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Null)]
        [PXUIField(DisplayName = "In-house Labor Cost")]
        public virtual decimal? LaborCost { get; set; }
        public abstract class laborCost : PX.Data.BQL.BqlDecimal.Field<laborCost> { }
        #endregion

        #region OtherMaterialCost
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Null)]
        [PXUIField(DisplayName = "Other Materials Cost")]
        public virtual decimal? OtherMaterialCost { get; set; }
        public abstract class otherMaterialCost : PX.Data.BQL.BqlDecimal.Field<otherMaterialCost> { }
        #endregion

        #region OtherCost
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Null)]
        [PXUIField(DisplayName = "Other Cost", Visible = false)]
        public virtual decimal? OtherCost { get; set; }
        public abstract class otherCost : PX.Data.BQL.BqlDecimal.Field<otherCost> { }
        #endregion

        #region UnitCost
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Landed Cost", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public virtual decimal? LandedCost { get; set; }
        public abstract class landedCost : PX.Data.BQL.BqlDecimal.Field<landedCost> { }
        #endregion

        #region FreightCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Null)]
        [PXUIField(DisplayName = "Freight Cost")]
        public virtual decimal? FreightCost { get; set; }
        public abstract class freightCost : PX.Data.BQL.BqlDecimal.Field<freightCost> { }
        #endregion

        #region HandlingCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Null)]
        [PXUIField(DisplayName = "Handling Cost")]
        public virtual decimal? HandlingCost { get; set; }
        public abstract class handlingCost : PX.Data.BQL.BqlDecimal.Field<handlingCost> { }
        #endregion

        #region DutyCost
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Null)]
        [PXUIField(DisplayName = "Duty Cost")]
        public virtual decimal? DutyCost { get; set; }
        public abstract class dutyCost : PX.Data.BQL.BqlDecimal.Field<dutyCost> { }
        #endregion

        #region DutyCostPct
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Null)]
        [PXUIField(DisplayName = "Duty %")]
        public virtual decimal? DutyCostPct { get; set; }
        public abstract class dutyCostPct : PX.Data.BQL.BqlDecimal.Field<dutyCostPct> { }
        #endregion

        #region NoteID
        [PXNote]
        public virtual Guid? NoteID { get; set; }
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        #endregion
    }
}
