using ASCJewelryLibrary.AP.CacheExt;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CA;
using PX.Objects.CS;
using System;
using static ASCJewelryLibrary.Common.Descriptor.ASCJConstants;

namespace ASCJewelryLibrary.PO.DAC
{
    [Serializable]
    [PXCacheName("ASC POOrder Extension")]
    public class ASCJPOOrderExt : PXCacheExtension<PX.Objects.PO.POOrder>
    {
        public static bool IsActive() => true;

        #region UsrASCJCancelDate
        [PXDBDate]
        [PXUIField(DisplayName = "Cancel Date")]
        public virtual DateTime? UsrASCJCancelDate { get; set; }
        public abstract class usrASCJCancelDate : PX.Data.BQL.BqlDateTime.Field<usrASCJCancelDate> { }
        #endregion

        #region UsrASCJEstArrivalDate
        [PXDBDate]
        [PXUIField(DisplayName = "Estimated Arrival")]
        public virtual DateTime? UsrASCJEstArrivalDate { get; set; }
        public abstract class usrASCJEstArrivalDate : PX.Data.BQL.BqlDateTime.Field<usrASCJEstArrivalDate> { }
        #endregion

        #region UsrASCJPricingDate
        [PXDBDate]
        [PXUIField(DisplayName = "Pricing Date", Required = true)]
        [PXDefault(typeof(AccessInfo.businessDate))]
        public virtual DateTime? UsrASCJPricingDate { get; set; }
        public abstract class usrASCJPricingDate : PX.Data.BQL.BqlDateTime.Field<usrASCJPricingDate> { }
        #endregion

        #region UsrASCJProgram
        [PXDBString(255)]
        [PXUIField(DisplayName = "Program", Required = true)]
        [PXDefault]
        public virtual string UsrASCJProgram { get; set; }
        public abstract class usrASCJProgram : PX.Data.BQL.BqlString.Field<usrASCJProgram> { }
        #endregion

        #region UsrASCJMarketID
        [PXDBInt()]
        [PXUIField(DisplayName = "Market", Required = true)]
        [PXDefault()]
        [PXSelector(typeof(Search2<Vendor.bAccountID, InnerJoin<VendorClass, On<Vendor.vendorClassID, Equal<VendorClass.vendorClassID>>>, Where<VendorClass.vendorClassID, Equal<MarketClass>>>),
                        typeof(Vendor.acctCD), typeof(Vendor.acctName)
                        , SubstituteKey = typeof(Vendor.acctCD), DescriptionField = typeof(Vendor.acctName))]

        public virtual int? UsrASCJMarketID { get; set; }
        public abstract class usrASCJMarketID : PX.Data.BQL.BqlInt.Field<usrASCJMarketID> { }

        #endregion

        #region UsrASCJSetupID
        [PXDBGuid()]
        [PXSelector(typeof(Search<NotificationSetup.setupID,
            Where<NotificationSetup.sourceCD, Equal<APNotificationSource.vendor>,
                And<NotificationSetup.module, Like<PXModule.po_>>>>),
            DescriptionField = typeof(NotificationSetup.notificationCD),
            SelectorMode = PXSelectorMode.DisplayModeText | PXSelectorMode.NoAutocomplete)]
        [PXUIField(DisplayName = "Mailing ID", Required = true)]
        [PXDefault(typeof(ASCJVendorExt.usrASCJSetupID))]
        public virtual Guid? UsrASCJSetupID { get; set; }
        public abstract class usrASCJSetupID : PX.Data.BQL.BqlGuid.Field<usrASCJSetupID> { }
        #endregion
    }
}