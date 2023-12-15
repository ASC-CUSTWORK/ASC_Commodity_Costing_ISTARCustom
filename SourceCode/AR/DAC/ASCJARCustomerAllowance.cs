using ASCJewelryLibrary.Common.DAC;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.SO;
using System;
using static ASCJewelryLibrary.Common.Descriptor.ASCJConstants;
using static ASCJewelryLibrary.AR.Descriptor.ASCJARConstants;

namespace ASCJewelryLibrary.AR.DAC
{
    [Serializable]
    [PXCacheName("Customer Allowance")]
    public partial class ASCJARCustomerAllowance : AuditSystemFields, IBqlTable
    {
        #region Keys
        public class PK : PrimaryKeyOf<ASCJARCustomerAllowance>.By<customerID, orderType, inventoryID, commodity>
        {
            public static ASCJARCustomerAllowance Find(PXGraph graph, int customerID, string orderType, int inventoryID, string commodity) => FindBy(graph, customerID, orderType, inventoryID, commodity);
        }
        public static class FK
        {
            public class Customer : PX.Objects.AR.Customer.PK.ForeignKeyOf<ASCJARCustomerAllowance>.By<customerID> { }
            public class InventoryItem : PX.Objects.IN.InventoryItem.PK.ForeignKeyOf<ASCJARCustomerAllowance>.By<inventoryID> { }
            public class OrderType : SOOrderType.PK.ForeignKeyOf<SOOrder>.By<orderType> { }
        }
        #endregion

        #region CustomerID
        [CustomerActive(typeof(Search<BAccountR.bAccountID, Where<True, Equal<True>>>), // TODO: remove fake Where after AC-101187
            Visibility = PXUIVisibility.SelectorVisible,
            DescriptionField = typeof(Customer.acctName),
            Filterable = true, IsKey = true)]
        [PXForeignReference(typeof(FK.Customer))]
        [PXDefault]
        public virtual int? CustomerID { get; set; }
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
        #endregion

        #region OrderType
        [PXDBString(2, IsKey = true, IsFixed = true, InputMask = ">aa")]
        [PXDefault(SOOrderTypeConstants.SalesOrder, typeof(SOSetup.defaultOrderType), PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXSelector(typeof(Search5<SOOrderType.orderType,
            InnerJoin<SOOrderTypeOperation, On2<SOOrderTypeOperation.FK.OrderType, And<SOOrderTypeOperation.operation, Equal<SOOrderType.defaultOperation>>>,
            LeftJoin<SOSetupApproval, On<SOOrderType.orderType, Equal<SOSetupApproval.orderType>>>>,
            Aggregate<GroupBy<SOOrderType.orderType>>>))]
        [PXRestrictor(typeof(Where<SOOrderTypeOperation.iNDocType, NotEqual<INTranType.transfer>, Or<FeatureInstalled<FeaturesSet.warehouse>>>), ErrorMessages.ElementDoesntExist, typeof(SOOrderType.orderType))]
        [PXRestrictor(typeof(Where<SOOrderType.requireAllocation, NotEqual<True>, Or<AllocationAllowed>>), ErrorMessages.ElementDoesntExist, typeof(SOOrderType.orderType))]
        [PXRestrictor(typeof(Where<SOOrderType.active, Equal<True>>), null)]
        [PXUIField(DisplayName = "Order Type", Visibility = PXUIVisibility.SelectorVisible)]
        [PX.Data.EP.PXFieldDescription]
        public virtual string OrderType { get; set; }
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
        #endregion

        #region InventoryID
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Allowance ID", Required =true)]
        [PXSelector(typeof(Search2<InventoryItem.inventoryID, LeftJoin<INItemClass, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>, Where<INItemClass.itemClassCD, Equal<AllowanceClass>>>),
            typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr)
                        , SubstituteKey = typeof(InventoryItem.inventoryCD), DescriptionField = typeof(InventoryItem.descr))]
        [PXDefault()]
        public virtual int? InventoryID { get; set; }
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        #endregion

        #region Commodity
        [PXDBString(1, IsKey = true)]
        [PXUIField(DisplayName = "Commodity Type", Required = true)]
        [CommodityType.List]
        [PXDefault(CommodityType.Undefined)]
        public virtual string Commodity { get; set; }
        public abstract class commodity : PX.Data.BQL.BqlString.Field<commodity> { }
        #endregion

        #region EffectiveDate
        [PXDBDate(IsKey = true)]
        [PXUIField(DisplayName = "Effective Date", Required = true, Visibility = PXUIVisibility.Visible)]
        [PXDefault(typeof(AccessInfo.businessDate))]
        public virtual DateTime? EffectiveDate { get; set; }
        public abstract class effectiveDate : PX.Data.BQL.BqlDateTime.Field<effectiveDate> { }
        #endregion

        #region AllowancePct
        [PXDBDecimal(6/*, MinValue = -100, MaxValue = 100*/)]
        [PXUIField(DisplayName = "Allowance Percentage")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? AllowancePct { get; set; }
        public abstract class allowancePct : PX.Data.BQL.BqlDecimal.Field<allowancePct> { }
        #endregion

        #region Active
        [PXDBBool()]
        [PXUIField(DisplayName = "Active")]
        [PXDefault(true)]
        public virtual bool? Active { get; set; }
        public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
        #endregion

        #region NoteID
        [PXNote]
        public virtual Guid? NoteID { get; set; }
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        #endregion
    }
}
