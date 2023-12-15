using PX.Data;
using PX.Objects.IN;
using System;

namespace ASCJewelryLibrary.IN.CacheExt
{
    [Serializable]
    [PXCacheName("IN Setup Extension")]
    public class ASCJINSetupExt : PXCacheExtension<INSetup>
    {
        public static bool IsActive() => true;

        #region UsrASCJIsActiveKitVersion
        [PXDBBool()]
        [PXUIField(DisplayName = "Kit Versions Activation")]
        public virtual bool? UsrASCJIsActiveKitVersion { get; set; }
        public abstract class usrASCJIsActiveKitVersion : PX.Data.BQL.BqlBool.Field<usrASCJIsActiveKitVersion> { }
        #endregion
    }
}
