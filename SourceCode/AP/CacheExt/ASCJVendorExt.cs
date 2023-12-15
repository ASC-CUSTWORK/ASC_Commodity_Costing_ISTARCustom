using PX.Data;
using PX.Objects.AP;
using PX.Objects.CA;
using PX.Objects.CS;
using System;
using static ASCJewelryLibrary.Common.Descriptor.ASCJConstants;

namespace ASCJewelryLibrary.AP.CacheExt
{
    [PXCacheName("Vendor DAC Extension")]
    public class ASCJVendorExt : PXCacheExtension<Vendor>
    {
        public static bool IsActive() => true;

        #region UsrASCJMarketID
        [PXDBInt()]
        [PXUIField(DisplayName = "Market")]
        [PXSelector(
        typeof(Search2<Vendor.bAccountID, InnerJoin<VendorClass, On<Vendor.vendorClassID, Equal<VendorClass.vendorClassID>>>,
            Where<VendorClass.vendorClassID, Equal<MarketClass>>>),
            typeof(Vendor.acctCD), typeof(Vendor.acctName)
                        , SubstituteKey = typeof(Vendor.acctCD), DescriptionField = typeof(Vendor.acctName))]
        public virtual int? UsrASCJMarketID { get; set; }
        public abstract class usrASCJMarketID : PX.Data.BQL.BqlInt.Field<usrASCJMarketID> { }
        #endregion

        #region UsrASCJSetupID
        [PXDBGuid()]
        [PXUIField(DisplayName = "Mailing ID")]
        [PXSelector(typeof(Search<NotificationSetup.setupID,
            Where<NotificationSetup.sourceCD, Equal<APNotificationSource.vendor>,
                And<NotificationSetup.module, Like<PXModule.po_>>>>),
            DescriptionField = typeof(NotificationSetup.notificationCD),
            SelectorMode = PXSelectorMode.DisplayModeText | PXSelectorMode.NoAutocomplete)]
        public virtual Guid? UsrASCJSetupID { get; set; }
        public abstract class usrASCJSetupID : PX.Data.BQL.BqlGuid.Field<usrASCJSetupID> { }
        #endregion
    }
}