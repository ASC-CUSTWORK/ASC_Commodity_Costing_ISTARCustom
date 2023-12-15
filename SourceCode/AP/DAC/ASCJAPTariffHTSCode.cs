using ASCJewelryLibrary.Common.DAC;
using ASCJewelryLibrary.AP;
using PX.Data;
using PX.Data.BQL;
using System;

namespace ASCJewelryLibrary.AP.DAC
{
    [Serializable]
    [PXCacheName("ASC AP TariffHTS Code DAC")]
    [PXPrimaryGraph(typeof(ASCJAPTariffHTSCodeEntry))]
    public class ASCJAPTariffHTSCode : AuditSystemFields, IBqlTable
    {
        #region HSTariffCode
        [PXDBString(30, IsKey = true, InputMask = "9999.99.9999", IsUnicode = true)]
        [PXUIField(DisplayName = "Tariff Code", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string HSTariffCode { get; set; }
        public abstract class hSTariffCode : BqlString.Field<hSTariffCode> { }
        #endregion

        #region  HSTariffCodeDescr
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Tariff Code Description", Visibility = PXUIVisibility.SelectorVisible)]
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
