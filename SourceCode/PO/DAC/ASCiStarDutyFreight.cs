using ASCISTARCustom.Common.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CR;
using PX.Objects.CS;
using System;

namespace ASCISTARCustom.PO.DAC
{
    [Serializable]
    [PXCacheName(_cacheName)]
    public class ASCiStarDutyFreight : AuditSystemFields, IBqlTable
    {
        private const string _cacheName = "ASCiStarDutyFreight";

        #region RecordID
        [PXDBIdentity(IsKey = true)]
        public int? RecordID {  get; set; }
        public abstract class recordID : BqlInt.Field<recordID> { }

        #endregion

        #region HSTariffCode
        [PXDBString(30, InputMask = "9999.99.9999", IsUnicode = true)]
        [PXUIField(DisplayName = "Tariff / HTS Code", Visibility = PXUIVisibility.SelectorVisible)]
        public string HSTariffCode { get; set; }
        public abstract class hSTariffCode : BqlString.Field<hSTariffCode> { }
        #endregion

        #region CountryCode
        [PXDBString(100)]
        [PXUIField(DisplayName = "Country Code")]
        //[PXSelector(typeof(Search<Country.countryID>))]
        [Country]
        public string CountryCode { get; set; }
        public abstract class countryCode : BqlString.Field<countryCode> { }
        #endregion

        #region CountryName
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Country Name", Enabled = false)]
        [PXUnboundDefault(typeof(Search<Country.description,
                    Where<Country.countryID, Equal<Current<ASCiStarDutyFreight.countryCode>>>>))]
        public string CountryName { get; set; }
        public abstract class countryName : PX.Data.BQL.BqlString.Field<countryName> { }
        #endregion

        #region DutyPercent
        [PXDBDecimal(4, MinValue = -100, MaxValue = 100)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Duty, %")]
        public decimal? DutyPercent { get; set; }
        public abstract class dutyPercent : PX.Data.BQL.BqlDecimal.Field<dutyPercent> { }
        #endregion

        #region EffectiveDate
        [PXDBDate()]
        [PXUIField(DisplayName = "Effective Date")]
        public DateTime? EffectiveDate { get; set; }
        public abstract class effectiveDate : BqlDateTime.Field<effectiveDate> { }
        #endregion

        #region NoteID
        [PXNote()]
        public virtual Guid? NoteID { get; set; }
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        #endregion
    }
}
