using PX.Data;
using PX.Data.BQL.Fluent;
using System;
using PX.Objects.IN;
using ASCISTARCustom.IN.Helpers;
using ASCISTARCustom.CommonHelpers;

namespace ASCISTARCustom.IN.DAC
{
    [Serializable]
    [PXCacheName("Compliance DAC")]
    public class INCompliance : AuditSystemFieldsDAC, IBqlTable
    {
        #region InventoryID
        [PXDBInt(IsKey = true)]
        [PXParent(typeof(SelectFrom<InventoryItem>.Where<InventoryItem.inventoryID.IsEqual<inventoryID.FromCurrent>>))]
        [PXDBDefault(typeof(InventoryItem.inventoryID))]
        [PXUIField(DisplayName = "Inventory ID", Visible = false)]
        public virtual int? InventoryID { get; set; }
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        #endregion

        #region ComplianceID
        [PXDBIdentity(IsKey = true)]
        public virtual int? ComplianceID { get; set; }
        public abstract class complianceID : PX.Data.BQL.BqlInt.Field<complianceID> { }
        #endregion

        #region CustomerAlphaCode
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Customer Alpha Code")]
        [PXDefault(typeof(SearchFor<INJewelryItemData.customerCode>.In<SelectFrom<INJewelryItemData>.Where<INJewelryItemData.inventoryID.IsEqual<inventoryID.FromCurrent>>>)
            , PersistingCheck = PXPersistingCheck.Nothing)]
        [PXStringList()]
        public virtual string CustomerAlphaCode { get; set; }
        public abstract class customerAlphaCode : PX.Data.BQL.BqlString.Field<customerAlphaCode> { }
        #endregion

        #region Division 
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Division")]
        [PXDefault(typeof(SearchFor<INJewelryItemData.invCategory>.In<SelectFrom<INJewelryItemData>.Where<INJewelryItemData.inventoryID.IsEqual<inventoryID.FromCurrent>>>)
            , PersistingCheck = PXPersistingCheck.Nothing)]
        [PXStringList()]
        public virtual string Division { get; set; }
        public abstract class division : PX.Data.BQL.BqlString.Field<division> { }
        #endregion

        #region TestType  
        [PXDBString(20, IsUnicode = true)]
        [PXUIField(DisplayName = "Test Type")]
        [INComplianceType.TestTypeList()]
        public virtual string TestType { get; set; }
        public abstract class testType : PX.Data.BQL.BqlString.Field<testType> { }
        #endregion

        #region ReportNumber
        [PXDBString(20, IsUnicode = true)]
        [PXUIField(DisplayName = "Report Number")]
        public virtual string ReportNumber { get; set; }
        public abstract class reportNumber : PX.Data.BQL.BqlString.Field<reportNumber> { }
        #endregion

        #region Pass 
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Pass")]
        [INCompliancePassStatus.PassStatusList]
        public virtual string Pass { get; set; }
        public abstract class pass : PX.Data.BQL.BqlString.Field<pass> { }
        #endregion

        #region TestingLab  
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Testing Lab")]
        [PXStringList()]
        public virtual string TestingLab { get; set; }
        public abstract class testingLab : PX.Data.BQL.BqlString.Field<testingLab> { }
        #endregion

        #region GroupReportTest
        [PXDBBool()]
        [PXUIField(DisplayName = "Group Report Test")]
        public virtual bool? GroupReportTest { get; set; }
        public abstract class groupReportTest : PX.Data.BQL.BqlBool.Field<groupReportTest> { }
        #endregion

        #region TestingDate
        [PXDBDate()]
        [PXUIField(DisplayName = "Testing Date", Required = true)]
        [PXDefault()]
        public virtual DateTime? TestingDate { get; set; }
        public abstract class testingDate : PX.Data.BQL.BqlDateTime.Field<testingDate> { }
        #endregion

        #region ProtocolTestedTo
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Protocol Tested To")]
        [PXStringList()]
        public virtual string ProtocolTestedTo { get; set; }
        public abstract class protocolTestedTo : PX.Data.BQL.BqlString.Field<protocolTestedTo> { }
        #endregion

        #region ReportDate
        [PXDBDate()]
        [PXUIField(DisplayName = "Report Date", Required = true)]
        [PXDefault()]
        public virtual DateTime? ReportDate { get; set; }
        public abstract class reportDate : PX.Data.BQL.BqlDateTime.Field<reportDate> { }
        #endregion

        #region ReportExpirationDate
        [PXDBDate()]
        [PXUIField(DisplayName = "Report Expiration Date")]
        public virtual DateTime? ReportExpirationDate { get; set; }
        public abstract class reportExpirationDate : PX.Data.BQL.BqlDateTime.Field<reportExpirationDate> { }
        #endregion

        #region WaiverReasonCode
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Waiver Reason Code")]
        [PXStringList()]
        public virtual string WaiverReasonCode { get; set; }
        public abstract class waiverReasonCode : PX.Data.BQL.BqlString.Field<waiverReasonCode> { }
        #endregion

        #region WaiverChangeReason
        [PXDBString(200, IsUnicode = true)]
        [PXUIField(DisplayName = "Waiver Reason & Change Reason")]
        public virtual string WaiverChangeReason { get; set; }
        public abstract class waiverChangeReason : PX.Data.BQL.BqlString.Field<waiverChangeReason> { }
        #endregion
    }
}
