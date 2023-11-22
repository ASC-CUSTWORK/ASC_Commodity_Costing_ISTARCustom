using PX.Data;
using PX.Objects.AP;


namespace ASCISTARCustom.AP.CacheExtension
{
    public class ASCIStarAPVendorPriceFilterExt: PXCacheExtension<APVendorPriceFilter>
    {
        public static bool IsActive() => true;

        #region UsrOnlyMarkets
        [PXBool()]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Show Only Markets")]
        public virtual bool? UsrOnlyMarkets { get; set; }
        public abstract class usrOnlyMarkets : PX.Data.BQL.BqlBool.Field<usrOnlyMarkets> { }
        #endregion
    }
}
