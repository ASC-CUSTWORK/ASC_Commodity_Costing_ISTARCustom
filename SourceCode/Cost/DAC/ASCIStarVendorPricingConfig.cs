//using System;
//using PX.Data;
//using PX.Objects.AP;
//using PX.Objects.IN;
//using PX.Data.ReferentialIntegrity.Attributes;

//namespace ASCISTARCustom
//{
//    [Serializable]
//    [PXCacheName("ASCIStarVendorPriceConfig")]
//    public class ASCIStarVendorPriceConfig : IBqlTable
//    {
//        #region Keys
//        public class PK : PrimaryKeyOf<ASCIStarVendorPriceConfig>.By<bAccountID, inventoryID>
//        {
//            public static ASCIStarVendorPriceConfig Find(PXGraph graph, int bAccountID, int Commodity) => FindBy(graph, bAccountID, Commodity);
//        }
//        public static class FK
//        {
//            public class Vendor : PX.Objects.AP.Vendor.PK.ForeignKeyOf<ASCIStarVendorPriceConfig>.By<bAccountID> { }
//            public class InventoryItem : PX.Objects.IN.InventoryItem.PK.ForeignKeyOf<ASCIStarVendorPriceConfig>.By<inventoryID> { }
//        }
//        #endregion

//        #region Fields

//        #region BAccountID
//        [PXDBInt(IsKey = true)]
//        [PXUIField(DisplayName = "BAccount ID")]
//        [PXDBDefault(typeof(Vendor.bAccountID))]
//        [PXParent(typeof(Select<Vendor, Where<Vendor.bAccountID, Equal<Current<Vendor.bAccountID>>>>))]
//        [PXForeignReference(typeof(FK.Vendor))]
//        public virtual Int32? BAccountID { get; set; }
//        public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
//        #endregion

//        #region InventoryID
//        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
//        protected Int32? _InventoryID;
//        [APTranInventoryItem(Filterable = true, IsKey = true)]
//        [PXSelector(
//        typeof(Search2<InventoryItem.inventoryID, LeftJoin<INItemClass, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>,
//            Where<INItemClass.itemClassCD, Equal<CommodityClass>>>),
//            typeof(InventoryItem.inventoryCD),
//            typeof(InventoryItem.descr)

//            , SubstituteKey = typeof(InventoryItem.inventoryCD)
//            , DescriptionField = typeof(InventoryItem.descr))]
//        [PXForeignReference(typeof(FK.InventoryItem))]
//        public virtual Int32? InventoryID
//        {
//            get
//            {
//                return this._InventoryID;
//            }
//            set
//            {
//                this._InventoryID = value;
//            }
//        }

//        #endregion


//        //#region Commodity
//        //[PXDBString(1, IsKey = true, IsUnicode = true, InputMask = "")]
//        //[PXUIField(DisplayName = "Commodity")]
//        //[CommodityType.List]
//        //[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
//        //public virtual string Commodity { get; set; }
//        //public abstract class commodity : PX.Data.BQL.BqlString.Field<commodity> { }
//        //#endregion

//        #region DefaultMarket
//        [PXDBString(2, IsUnicode = true, InputMask = "")]
//        [PXUIField(DisplayName = "Default Market", Required = true)]
//        [MarketList.List]
//        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]

//        public virtual string DefaultMarket { get; set; }
//        public abstract class defaultMarket : PX.Data.BQL.BqlString.Field<defaultMarket> { }
//        #endregion


//        #region Basis
//        [PXDBDecimal()]
//        [PXUIField(DisplayName = "Basis")]
//        public virtual Decimal? Basis { get; set; }
//        public abstract class basis : PX.Data.BQL.BqlDecimal.Field<basis> { }
//        #endregion

//        #region Increment
//        [PXDBDecimal()]
//        [PXUIField(DisplayName = "Increment")]
//        public virtual Decimal? Increment { get; set; }
//        public abstract class increment : PX.Data.BQL.BqlDecimal.Field<increment> { }
//        #endregion

//        #region Surcharge
//        [PXDBDecimal()]
//        [PXUIField(DisplayName = "Surcharge")]
//        public virtual Decimal? Surcharge { get; set; }
//        public abstract class surcharge : PX.Data.BQL.BqlDecimal.Field<surcharge> { }
//        #endregion

//        #region SurchargePct
//        [PXDBDecimal()]
//        [PXUIField(DisplayName = "Surcharge Pct")]
//        public virtual Decimal? SurchargePct { get; set; }
//        public abstract class surchargePct : PX.Data.BQL.BqlDecimal.Field<surchargePct> { }
//        #endregion

//        #region LossPct
//        [PXDBDecimal()]
//        [PXUIField(DisplayName = "Loss Pct")]
//        public virtual Decimal? LossPct { get; set; }
//        public abstract class lossPct : PX.Data.BQL.BqlDecimal.Field<lossPct> { }
//        #endregion

//        #region Tstamp
//        [PXDBTimestamp()]
//        [PXUIField(DisplayName = "Tstamp")]
//        public virtual byte[] Tstamp { get; set; }
//        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
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

//        #region Noteid
//        [PXNote()]
//        public virtual Guid? Noteid { get; set; }
//        public abstract class noteid : PX.Data.BQL.BqlGuid.Field<noteid> { }
//        #endregion

//        #endregion Fields


//    }
//}