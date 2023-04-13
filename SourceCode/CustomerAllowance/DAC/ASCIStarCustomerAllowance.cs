using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.CS;
using static ASCISTARCustom.Common.Descriptor.ASCIStarConstants;

namespace ASCISTARCustom
{


	[Serializable]
    [PXCacheName("Customer Allowance")]
    public partial class ASCIStarCustomerAllowance : IBqlTable
	{		
		#region Keys
		public class PK : PrimaryKeyOf<ASCIStarCustomerAllowance>.By<customerID, orderType, inventoryID, commodity>
		{
			public static ASCIStarCustomerAllowance Find(PXGraph graph, int customerID, string orderType, int inventoryID, string commodity) => FindBy(graph, customerID, orderType, inventoryID, commodity);
		}
		public static class FK
		{
			public class Customer : PX.Objects.AR.Customer.PK.ForeignKeyOf<ASCIStarCustomerAllowance>.By<customerID> { }
			public class InventoryItem : PX.Objects.IN.InventoryItem.PK.ForeignKeyOf<ASCIStarCustomerAllowance>.By<inventoryID> { }
			public class OrderType : SOOrderType.PK.ForeignKeyOf<SOOrder>.By<orderType> { }
		}
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;
		[PXDefault]
        [CustomerActive(
            typeof(Search<BAccountR.bAccountID, Where<True, Equal<True>>>), // TODO: remove fake Where after AC-101187
            Visibility = PXUIVisibility.SelectorVisible,
            DescriptionField = typeof(Customer.acctName),
            Filterable = true
			, IsKey = true, PersistingCheck = PXPersistingCheck.NullOrBlank)]
        //[Customer()]
		[PXForeignReference(typeof(FK.Customer))]
		public virtual Int32? CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
			}
		}
		#endregion
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected String _OrderType;
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
		public virtual String OrderType
		{
			get
			{
				return this._OrderType;
			}
			set
			{
				this._OrderType = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[ARTranInventoryItem(Filterable = true, IsKey = true, PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Allowance ID")]
		[PXSelector(
		typeof(Search2<InventoryItem.inventoryID, LeftJoin<INItemClass, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>, 
			Where<INItemClass.itemClassCD, Equal<AllowanceClass>>>),
			typeof(InventoryItem.inventoryCD),
			typeof(InventoryItem.descr)

			,SubstituteKey = typeof(InventoryItem.inventoryCD)
			,DescriptionField = typeof(InventoryItem.descr))]
		[PXForeignReference(typeof(FK.InventoryItem))]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}


		public class AllowanceClass : PX.Data.BQL.BqlString.Constant<AllowanceClass>
		{
			public static readonly string value = "ALLOWANCES"; public AllowanceClass() : base(value) { } 
		}
		#endregion
		#region Commodity
		[PXDBString(1, IsKey = true)]
		[PXUIField(DisplayName = "Commodity Type")]
		[CommodityType.List]
		[PXDefault(CommodityType.Undefined, PersistingCheck = PXPersistingCheck.NullOrBlank)]
		public virtual string Commodity { get; set; }
		public abstract class commodity : PX.Data.BQL.BqlString.Field<commodity> { }
		#endregion
		#region AllowancePct
		public abstract class allowancePct : PX.Data.BQL.BqlDecimal.Field<allowancePct> { }
		protected Decimal? _AllowancePct;
		[PXDBDecimal(6, MinValue = -100, MaxValue = 100)]
		[PXUIField(DisplayName = "Allowance Percentage")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? AllowancePct
		{
			get
			{
				return this._AllowancePct;
			}
			set
			{
				this._AllowancePct = value;
			}
		}
		#endregion
		#region EffectiveDate

		public abstract class effectiveDate : PX.Data.BQL.BqlDateTime.Field<effectiveDate> { }
		protected DateTime? _EffectiveDate;
		[PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXDBDate(IsKey = true)]
		[PXUIField(DisplayName = "Effective Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? EffectiveDate
		{
			get
			{
				return this._EffectiveDate;
			}
			set
			{
				this._EffectiveDate = value;
			}
		}
		#endregion
		#region Active
		[PXDBBool()]
        [PXUIField(DisplayName = "Active")]
		[PXDefault(true)]
		public virtual bool? Active { get; set; }
        public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
	}
}
