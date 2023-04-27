//using System;
//using PX.Data;
//using PX.Objects.AP;
//using PX.Objects.PO;

//namespace ASCISTARCustom
//{
//    //using InfoSmartSearch;
//    using PX.CS;
//    using PX.Objects.IN;

//    [Serializable]
//    [PXCacheName("InfoINKitSpecHdriStarVendorQuote")]
//    public class ASCIStarINKitSpecHdrVendorQuote : IBqlTable
//    {
//        #region KitInventoryID
//        [PXDBInt(IsKey = true)]
//        [PXUIField(DisplayName = "Kit Inventory ID")]
//        [PXDBDefault(typeof(INKitSpecHdr.kitInventoryID))]
//        public virtual int? KitInventoryID { get; set; }
//        public abstract class kitInventoryID : PX.Data.BQL.BqlInt.Field<kitInventoryID> { }
//        #endregion

//        #region RevisionID
//        [PXDBString(10, IsKey = true, IsUnicode = true, InputMask = "")]
//        [PXUIField(DisplayName = "Revision ID")]
//        [PXDBDefault(typeof(INKitSpecHdr.revisionID))]
//        [PXParent(typeof(Select<INKitSpecHdr,
//          Where<INKitSpecHdr.kitInventoryID, Equal<Current<ASCIStarINKitSpecHdrVendorQuote.kitInventoryID>>, And<INKitSpecHdr.revisionID, Equal<Current<ASCIStarINKitSpecHdrVendorQuote.revisionID>>>>>))]

//        public virtual string RevisionID { get; set; }
//        public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }
//        #endregion

//        #region LineNbr
//        [PXDBInt(IsKey = true)]
//        [PXUIField(DisplayName = "Line Nbr",Enabled =false)]
//        [PXLineNbr(typeof(ASCIStarINKitSpecHdrExt.usrVQuoteLineCtr))]
//        public virtual int? LineNbr { get; set; }
//        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
//        #endregion

//        #region VendorID
//        //[PXDBInt()]
//        //[PXUIField(DisplayName = "Vendor ID")]
//        [POVendor(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true, DisplayName = "Vendor ID")]
//        [PXDefault]
//        public virtual int? VendorID { get; set; }
//        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
//        #endregion

//        #region FirstCost
//        [PXDBDecimal()]
//        [PXUIField(DisplayName = "First Cost")]
//        public virtual Decimal? FirstCost { get; set; }
//        public abstract class firstCost : PX.Data.BQL.BqlDecimal.Field<firstCost> { }
//        #endregion

//        #region VendorPrice
//        [PXDBDecimal()]
//        [PXUIField(DisplayName = "Vendor Price")]
//        public virtual Decimal? VendorPrice { get; set; }
//        public abstract class vendorPrice : PX.Data.BQL.BqlDecimal.Field<vendorPrice> { }
//        #endregion

//        #region Loss
//        [PXDBDecimal()]
//        [PXUIField(DisplayName = "Loss (%)")]
//        public virtual Decimal? Loss { get; set; }
//        public abstract class loss : PX.Data.BQL.BqlDecimal.Field<loss> { }
//        #endregion

//        #region SilverContent
//        [PXDBDecimal()]
//        [PXUIField(DisplayName = "Silver Content")]
//        public virtual Decimal? SilverContent { get; set; }
//        public abstract class silverContent : PX.Data.BQL.BqlDecimal.Field<silverContent> { }
//        #endregion

//        #region Country
//        [PXDBString(15, IsUnicode = true, InputMask = "")]
//        [PXUIField(DisplayName = "Country")]
//        [PXSelector(typeof(Search<CSAttributeDetail.valueID, Where<CSAttributeDetail.attributeID, Equal<AttributeNames.countryOfOrigion>>, OrderBy<Asc<CSAttributeDetail.sortOrder>>>),
//                         typeof(CSAttributeDetail.valueID),
//                         typeof(CSAttributeDetail.description),
//                         DescriptionField = typeof(CSAttributeDetail.description))]
//        public virtual string Country { get; set; }
//        public abstract class country : PX.Data.BQL.BqlString.Field<country> { }
//        #endregion

//        #region SilverWt
//        [PXDBDecimal()]
//        [PXUIField(DisplayName = "Silver Wt")]
//        public virtual Decimal? SilverWt { get; set; }
//        public abstract class silverWt : PX.Data.BQL.BqlDecimal.Field<silverWt> { }
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

//        #region QuoteCreated
//        [PXDBDate()]
//        [PXUIField(DisplayName = "Quote Created")]
//        public virtual DateTime? QuoteCreated { get; set; }
//        public abstract class quoteCreated : PX.Data.BQL.BqlDateTime.Field<quoteCreated> { }
//        #endregion

//        #region LeadTime
//        [PXDBInt()]
//        [PXUIField(DisplayName = "Lead Time")]
//        public virtual int? LeadTime { get; set; }
//        public abstract class leadTime : PX.Data.BQL.BqlInt.Field<leadTime> { }
//        #endregion

//        #region QuoteLastDate
//        [PXDBDate()]
//        [PXUIField(DisplayName = "Quote Last Date")]
//        public virtual DateTime? QuoteLastDate { get; set; }
//        public abstract class quoteLastDate : PX.Data.BQL.BqlDateTime.Field<quoteLastDate> { }
//        #endregion

//        #region Status
//        [PXDBString(1, IsFixed = true, InputMask = "")]
//        [PXUIField(DisplayName = "Status")]
//        [PXDefault("X",PersistingCheck = PXPersistingCheck.Nothing)]
//        [PXStringList(new[] { "A", "R", "H", "N", "X" },
//                       new[] { "Accepted", "Regected", "On Hold", "New", " " })]
//        public virtual string Status { get; set; }
//        public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
//        #endregion
//    }
//}