using ASCISTARCustom.AP.CacheExt;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CA;
using PX.Objects.CS;
using System;
using static ASCISTARCustom.Common.Descriptor.ASCIStarConstants;

namespace ASCISTARCustom.PO.DAC
{
    [Serializable]
    [PXCacheName("ASC POOrder Extension")]
    public class ASCIStarPOOrderExt : PXCacheExtension<PX.Objects.PO.POOrder>
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

        #region UsrFinalPO
        [PXDBBool]
        [PXUIField(DisplayName = "Final PO")]
        public virtual bool? UsrFinalPO { get; set; }
        public abstract class usrFinalPO : PX.Data.BQL.BqlBool.Field<usrFinalPO> { }
        #endregion

        #region UsrProgram
        [PXDBString(255)]
        [PXUIField(DisplayName = "Program", Required = true)]
        [PXDefault]
        public virtual string UsrProgram { get; set; }
        public abstract class usrProgram : PX.Data.BQL.BqlString.Field<usrProgram> { }
        #endregion

        #region UsrTrackingNo
        [PXDBString(25)]
        [PXUIField(DisplayName = "Tracking No")]
        public virtual string UsrTrackingNo { get; set; }
        public abstract class usrTrackingNo : PX.Data.BQL.BqlString.Field<usrTrackingNo> { }
        #endregion

        #region UsrMarketID
        [PXDBInt()]
        [PXUIField(DisplayName = "Market", Required = true)]
        [PXDefault()]
        [PXSelector(typeof(Search2<Vendor.bAccountID, InnerJoin<VendorClass, On<Vendor.vendorClassID, Equal<VendorClass.vendorClassID>>>, Where<VendorClass.vendorClassID, Equal<MarketClass>>>),
                        typeof(Vendor.acctCD), typeof(Vendor.acctName)
                        , SubstituteKey = typeof(Vendor.acctCD), DescriptionField = typeof(Vendor.acctName))]

        public virtual int? UsrMarketID { get; set; }
        public abstract class usrMarketID : PX.Data.BQL.BqlInt.Field<usrMarketID> { }

        #endregion

        #region UsrSetupID
        [PXDBGuid()]
        [PXSelector(typeof(Search<NotificationSetup.setupID,
            Where<NotificationSetup.sourceCD, Equal<APNotificationSource.vendor>,
                And<NotificationSetup.module, Like<PXModule.po_>>>>),
            DescriptionField = typeof(NotificationSetup.notificationCD),
            SelectorMode = PXSelectorMode.DisplayModeText | PXSelectorMode.NoAutocomplete)]
        [PXUIField(DisplayName = "Mailing ID", Required = true)]
        [PXDefault(typeof(ASCIStarVendorExt.usrSetupID))]
        public virtual Guid? UsrSetupID { get; set; }
        public abstract class usrSetupID : PX.Data.BQL.BqlGuid.Field<usrSetupID> { }
        #endregion
    }
}