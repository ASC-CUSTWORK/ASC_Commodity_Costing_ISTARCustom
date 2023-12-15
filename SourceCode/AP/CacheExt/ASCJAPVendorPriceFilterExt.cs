using PX.Data;
using PX.Objects.AP;


namespace ASCJewelryLibrary.AP.CacheExt
{
    [PXCacheName("AP Vendor Price Filter DAC Extension")]
    public class ASCJAPVendorPriceFilterExt: PXCacheExtension<APVendorPriceFilter>
    {
        public static bool IsActive() => true;

        #region UsrASCJOnlyMarkets
        [PXBool()]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Show Only Markets")]
        public virtual bool? UsrASCJOnlyMarkets { get; set; }
        public abstract class usrASCJOnlyMarkets : PX.Data.BQL.BqlBool.Field<usrASCJOnlyMarkets> { }
        #endregion
    }
}
