using ASCJewelryLibrary.Common.DAC;
using ASCJewelryLibrary.AP.Descriptor;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CM;
using PX.Objects.CS;
using System;

namespace ASCJewelryLibrary.AP.DAC
{
    [Serializable]
    [PXCacheName(_cacheName)]
    [PXPrimaryGraph(typeof(ASCJAPMetalRatesSetupMaint))]
    public class ASCJAPMetalRatesSetup : AuditSystemFields, IBqlTable
    {
        private const string _cacheName = "AP Metal Rates Setup DAC";

        #region BaseURL
        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Base URL", Required = true, Enabled = false)]
        public virtual string BaseURL { get; set; }
        public abstract class baseURL : PX.Data.BQL.BqlString.Field<baseURL> { }
        #endregion

        #region AccessKey
        [PXDBString(256, IsUnicode = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Access Key")]
        public virtual string AccessKey { get; set; }
        public abstract class accessKey : PX.Data.BQL.BqlString.Field<accessKey> { }
        #endregion

        #region BaseCurrency
        [PXDBString(5, IsUnicode = true)]
        [PXDefault]
        [PXSelector(typeof(Search<CurrencyList.curyID, Where<CurrencyList.isActive, Equal<boolTrue>>>))]
        [PXUIField(DisplayName = "Base Currency")]
        [PXFieldDescription]
        public virtual string BaseCurrency { get; set; }
        public abstract class baseCurrency : PX.Data.BQL.BqlString.Field<baseCurrency> { }
        #endregion

        #region Symbols
        [PXDBString(256, IsUnicode = true)]
        [ASCJSymbols.List(MultiSelect = true)]
        [PXDefault()]
        [PXUIField(DisplayName = "Symbols")]
        public virtual string Symbols { get; set; }
        public abstract class symbols : PX.Data.BQL.BqlString.Field<symbols> { }
        #endregion

        #region NoteID
        [PXNote]
        public virtual Guid? NoteID { get; set; }
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        #endregion
    }
}
