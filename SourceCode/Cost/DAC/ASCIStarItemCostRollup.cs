//using System;
//using PX.Data;


//namespace ASCISTARCustom
//{
//    using PX.Objects.IN;
//    using PX.Objects.AR;
//    using PX.Objects.AP;
//    using PX.Data.ReferentialIntegrity.Attributes;

//    [Serializable]
//    [PXCacheName("ASCIStarItemCostRollup")]
//    public class ASCIStarItemCostRollup : IBqlTable
//    {
//        public class PK : PrimaryKeyOf<ASCIStarItemCostRollup>.By<inventoryID, bAccountID>
//        {
//            public static ASCIStarItemCostRollup Find(PXGraph graph, int inventoryID, int bAccountID) => FindBy(graph, inventoryID, bAccountID);
//        }
//        public static class FK
//        {
//            public class InventoryItem : PX.Objects.IN.InventoryItem.PK.ForeignKeyOf<ASCIStarItemCostRollup>.By<inventoryID> { }
//        }
//        #region InventoryID
//        [PXDBInt(IsKey = true)]
//        [PXUIField(DisplayName = "Inventory ID")]
//        [ARTranInventoryItem(Filterable = true)]
//        [PXDefault(typeof(InventoryItem.inventoryID))]
//        public virtual int? InventoryID { get; set; }
//        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
//        #endregion

//        #region BAccountID
//        [PXDBInt(IsKey = true)]
//        [PXUIField(DisplayName = "BAccount ID")]
//        [PXDefault(typeof(PXCache<Vendor>))]
//        public virtual int? BAccountID { get; set; }
//        public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
//        #endregion

//        #region CommodityCost
//        [PXDBDecimal()]
//        [PXUIField(DisplayName = "Commodity Cost")]
//        public virtual Decimal? CommodityCost { get; set; }
//        public abstract class commodityCost : PX.Data.BQL.BqlDecimal.Field<commodityCost> { }
//        #endregion

//        #region FabricationCost
//        [PXDBDecimal()]
//        [PXUIField(DisplayName = "Fabrication Cost")]
//        public virtual Decimal? FabricationCost { get; set; }
//        public abstract class fabricationCost : PX.Data.BQL.BqlDecimal.Field<fabricationCost> { }
//        #endregion

//        #region LaborCost
//        [PXDBDecimal()]
//        [PXUIField(DisplayName = "Labor Cost")]
//        public virtual Decimal? LaborCost { get; set; }
//        public abstract class laborCost : PX.Data.BQL.BqlDecimal.Field<laborCost> { }
//        #endregion

//        #region HandlingCost
//        [PXDBDecimal()]
//        [PXUIField(DisplayName = "Handling Cost")]
//        public virtual Decimal? HandlingCost { get; set; }
//        public abstract class handlingCost : PX.Data.BQL.BqlDecimal.Field<handlingCost> { }
//        #endregion

//        #region FreightCost
//        [PXDBDecimal()]
//        [PXUIField(DisplayName = "Freight Cost")]
//        public virtual Decimal? FreightCost { get; set; }
//        public abstract class freightCost : PX.Data.BQL.BqlDecimal.Field<freightCost> { }
//        #endregion

//        #region DutyCost
//        [PXDBDecimal()]
//        [PXUIField(DisplayName = "Duty Cost")]
//        public virtual Decimal? DutyCost { get; set; }
//        public abstract class dutyCost : PX.Data.BQL.BqlDecimal.Field<dutyCost> { }
//        #endregion

//        #region OtherCost
//        [PXDBDecimal()]
//        [PXUIField(DisplayName = "Other Cost")]
//        public virtual Decimal? OtherCost { get; set; }
//        public abstract class otherCost : PX.Data.BQL.BqlDecimal.Field<otherCost> { }
//        #endregion

//        #region UnitCost
//        [PXDBDecimal()]
//        [PXUIField(DisplayName = "Unit Cost")]
//        public virtual Decimal? UnitCost { get; set; }
//        public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
//        #endregion

//        #region CreatedByID
//        [PXDBCreatedByID()]
//        public virtual Guid? CreatedByID { get; set; }
//        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
//        #endregion

//        #region CreatedByScreenID
//        [PXDBCreatedByScreenID()]
//        public virtual string CreatedByScreenID { get; set; }
//        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
//        #endregion

//        #region CreatedDateTime
//        [PXDBCreatedDateTime()]
//        public virtual DateTime? CreatedDateTime { get; set; }
//        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
//        #endregion

//        #region LastModifiedByID
//        [PXDBLastModifiedByID()]
//        public virtual Guid? LastModifiedByID { get; set; }
//        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
//        #endregion

//        #region LastModifiedByScreenID
//        [PXDBLastModifiedByScreenID()]
//        public virtual string LastModifiedByScreenID { get; set; }
//        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
//        #endregion

//        #region LastModifiedDateTime
//        [PXDBLastModifiedDateTime()]
//        public virtual DateTime? LastModifiedDateTime { get; set; }
//        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
//        #endregion

//        #region Tstamp
//        [PXDBTimestamp()]
//        [PXUIField(DisplayName = "Tstamp")]
//        public virtual byte[] Tstamp { get; set; }
//        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
//        #endregion
//    }
//}