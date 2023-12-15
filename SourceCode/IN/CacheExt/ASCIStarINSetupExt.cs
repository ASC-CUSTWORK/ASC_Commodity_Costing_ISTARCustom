using PX.Data;
using PX.Objects.IN;

namespace ASCJewelryLibrary.IN.CacheExt
{ 
    public class ASCJINSetupExt : PXCacheExtension<INSetup>
    {
        public static bool IsActive() => true;

        #region UsrIsActiveKitVersion
        [PXDBBool()]
        [PXUIField(DisplayName = "Kit Versions Activation")]
        public virtual bool? UsrIsActiveKitVersion { get; set; }
        public abstract class usrIsActiveKitVersion : PX.Data.BQL.BqlBool.Field<usrIsActiveKitVersion> { }
        #endregion
    }
}
