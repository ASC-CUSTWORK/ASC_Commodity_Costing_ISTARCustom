using ASCISTARCustom.AP.DAC;
using ASCISTARCustom.Common.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using System;

namespace ASCISTARCustom.IN.DAC
{
    [Serializable]
    [PXCacheName("Vendor Duty DAC")]
    public class ASCIStarINVendorDuty : AuditSystemFields, IBqlTable
    {
        #region InventoryID
        [PXDBInt(IsKey = true)]
        [PXParent(typeof(SelectFrom<InventoryItem>.Where<InventoryItem.inventoryID.IsEqual<inventoryID.FromCurrent>>))]
        [PXDBDefault(typeof(InventoryItem.inventoryID))]
        [PXUIField(DisplayName = "Inventory ID", Visible = false)]
        public virtual int? InventoryID { get; set; }
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        #endregion

        #region VendorID
        [VendorNonEmployeeActive(IsKey = true, DisplayName = "Vendor ID",
            Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Vendor.acctName), Filterable = true)]
        [PXDefault]
        public virtual int? VendorID { get; set; }
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
        #endregion

        #region HSTariffCode
        [PXDBString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Tariff / HTS Code")]
        [PXSelector(typeof(SearchFor<ASCIStarAPTariffHTSCode.hSTariffCode>))]
        [PXDefault(typeof(InventoryItem.hSTariffCode))]
        public virtual string HSTariffCode { get; set; }
        public abstract class hSTariffCode : PX.Data.BQL.BqlString.Field<hSTariffCode> { }
        #endregion

        #region CountryID
        [PXDBString(100)]
        [PXUIField(DisplayName = "Country")]
        [Country]
        [PXDefault(typeof(SearchFor<Address.countryID>.Where<Address.bAccountID.IsEqual<vendorID.FromCurrent>>), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string CountryID { get; set; }
        public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
        #endregion

        #region DutyPct
        [PXUIField(DisplayName = "Duty, %")]
        [PXDBDecimal(2, MinValue = 0, MaxValue = 100)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? DutyPct { get; set; }
        public abstract class dutyPct : PX.Data.BQL.BqlDecimal.Field<dutyPct> { }
        #endregion

        #region EffectiveDate
        [PXDBDate()]
        [PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Effective Date")]
        public virtual DateTime? EffectiveDate { get; set; }
        public abstract class effectiveDate : PX.Data.BQL.BqlDateTime.Field<effectiveDate> { }
        #endregion

        #region NoteID
        [PXNote()]
        public virtual Guid? NoteID { get; set; }
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        #endregion
    }
}


