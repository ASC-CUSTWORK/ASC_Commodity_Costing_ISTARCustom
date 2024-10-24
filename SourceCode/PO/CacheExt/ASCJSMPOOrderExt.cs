using PX.Data;
using System;

namespace ASCJSMCustom.PO.DAC
{
    [Serializable]
    [PXCacheName("ASC POOrder Extension")]
    public class ASCJSMPOOrderExt : PXCacheExtension<PX.Objects.PO.POOrder>
    {
        public static bool IsActive() => true;

        #region UsrCancelDate
        [PXDBDate]
        [PXUIField(DisplayName = "Cancel Date")]
        public virtual DateTime? UsrCancelDate { get; set; }
        public abstract class usrCancelDate : PX.Data.BQL.BqlDateTime.Field<usrCancelDate> { }
        #endregion

        #region UsrEstArrivalDate
        [PXDBDate]
        [PXUIField(DisplayName = "Estimated Arrival")]
        public virtual DateTime? UsrEstArrivalDate { get; set; }
        public abstract class usrEstArrivalDate : PX.Data.BQL.BqlDateTime.Field<usrEstArrivalDate> { }
        #endregion

        #region UsrPricingDate
        [PXDBDate]
        [PXUIField(DisplayName = "Pricing Date", Required = true)]
        [PXDefault(typeof(AccessInfo.businessDate))]
        public virtual DateTime? UsrPricingDate { get; set; }
        public abstract class usrPricingDate : PX.Data.BQL.BqlDateTime.Field<usrPricingDate> { }
        #endregion

        #region UsrTrackingNo
        [PXDBString(25)]
        [PXUIField(DisplayName = "Tracking No")]
        public virtual string UsrTrackingNo { get; set; }
        public abstract class usrTrackingNo : PX.Data.BQL.BqlString.Field<usrTrackingNo> { }
        #endregion
    }
}