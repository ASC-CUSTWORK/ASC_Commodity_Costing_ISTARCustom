using PX.Data;
using PX.Data.BQL;
using PX.Objects.IN;
using System;

namespace ASCJSMCustom.IN.CacheExt
{
    public class ASCJSMINItemXRefExt : PXCacheExtension<INItemXRef>
    {
        public static bool IsActive() => true;

        #region UsrCreationDate
        [PXDBDate()]
        [PXUIField(DisplayName = "Creation Date", IsReadOnly = true)]
        public DateTime? UsrCreationDate { get; set; }
        public abstract class usrCreationDate : BqlDateTime.Field<usrCreationDate> { }
        #endregion
    }
}
