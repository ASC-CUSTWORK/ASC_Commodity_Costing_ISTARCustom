using PX.Data;
using PX.Objects.IN;

namespace ASCJSMCustom.IN.CacheExt
{ 
    public class ASCJSMINSetupExt : PXCacheExtension<INSetup>
    {
        public static bool IsActive() => true;

        #region UsrIsActiveKitVersion
        [PXDBBool()]
        [PXUIField(DisplayName = "Kit Versions Activation")]
        public virtual bool? UsrIsActiveKitVersion { get; set; }
        public abstract class usrIsActiveKitVersion : PX.Data.BQL.BqlBool.Field<usrIsActiveKitVersion> { }
        #endregion

        //#region UsrUseMetalLoss%inPreciousmetalCalculation
        //[PXDBBool()]
        //[PXUIField(DisplayName = "Use Metal Loss% in Precious metal Calculation")]
        //public virtual bool? UsrUseMetalLossinPreciousmetalCalculation { get; set; }
        //public abstract class usrUseMetalLossinPreciousmetalCalculation : PX.Data.BQL.BqlBool.Field<usrUseMetalLossinPreciousmetalCalculation> { }
        //#endregion
    }
}
