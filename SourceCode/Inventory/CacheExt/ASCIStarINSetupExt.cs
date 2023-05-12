using PX.Data;
using PX.Objects.IN;

namespace ASCISTARCustom.Inventory.CacheExt
{
    public class ASCIStarINSetupExt : PXCacheExtension<INSetup>
    {
        public static bool IsActive() => true;

        #region UsrIsPDSTenant
        [PXDBBool()]
        [PXUIField(DisplayName = "Is PDS Tenant")]
        public virtual bool? UsrIsPDSTenant { get; set; }
        public abstract class usrIsPDSTenant : PX.Data.BQL.BqlBool.Field<usrIsPDSTenant> { }
        #endregion
    }
}
