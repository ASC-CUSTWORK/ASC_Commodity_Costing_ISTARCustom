using ASCJSMCustom.Common.DAC;
using ASCJSMCustom.Cost;
using PX.Data;
using PX.Data.BQL;
using System;

namespace ASCJSMCustom.AP.DAC
{
    [Serializable]
    [PXCacheName("ASC AP Tariff HTS Code DAC")]
    [PXPrimaryGraph(typeof(ASCJSMAPTariffHTSCodeEntry))]
    public class ASCJSMAPTariffHTSCode : AuditSystemFields, IBqlTable
    {
        #region HSTariffCode
        [PXDBString(30, IsKey = true, InputMask = "9999.99.9999", IsUnicode = true)]
        [PXUIField(DisplayName = "Tariff / HTS Code", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string HSTariffCode { get; set; }
        public abstract class hSTariffCode : BqlString.Field<hSTariffCode> { }
        #endregion

        #region  HSTariffCodeDescr
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Tariff / HTS Code Description", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string HSTariffCodeDescr { get; set; }
        public abstract class hSTariffCodeDescr : BqlString.Field<hSTariffCodeDescr> { }
        #endregion

        #region NoteID
        [PXNote()]
        public virtual Guid? NoteID { get; set; }
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        #endregion
    }
}
