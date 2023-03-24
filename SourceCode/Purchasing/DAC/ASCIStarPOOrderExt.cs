using PX.Common;
using PX.Data.BQL;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CA;
using PX.SM;
using PX.Objects.Common.Bql;
using PX.Objects.Common.Discount.Attributes;
using PX.Objects.Common.Discount;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN.Matrix.Interfaces;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.PO;
using PX.Objects.TX;
using PX.Objects;
using System.Collections.Generic;
using System;

namespace ASCISTARCustom
{
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

        #region UsrProgram
        [PXDBString(255)]
        /*[PXUIField(DisplayName="Program", Required = false)]*/
        [PXUIField(DisplayName = "Program", Required = true)]
        /*[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]*/
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        public virtual string UsrProgram { get; set; }
        public abstract class usrProgram : PX.Data.BQL.BqlString.Field<usrProgram> { }
        #endregion

        //#region UsrVerbiageType
        //[PXDBString(1, IsUnicode = true, InputMask = "")]
        //[PXUIField(DisplayName = "Sub Type", Required = true)]
        //[CommodityType.List]
        //[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        //public virtual string UsrVerbiageType { get; set; }
        //public abstract class usrVerbiageType : PX.Data.BQL.BqlString.Field<usrVerbiageType> { }
        //#endregion

        //#region UsrMarket
        //[PXDBString(2, IsUnicode = true, InputMask = "")]
        //[PXUIField(DisplayName = "Market", Required = true)]
        //[MarketList.List]
        //[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        //public virtual string UsrMarket { get; set; }
        //public abstract class usrMarket : PX.Data.BQL.BqlString.Field<usrMarket> { }
        //#endregion

        #region MarketID
        [PXDBInt()]
        [PXUIField(DisplayName = "Market")]
        [PXSelector(
        typeof(Search2<Vendor.bAccountID, InnerJoin<VendorClass, On<Vendor.vendorClassID, Equal<VendorClass.vendorClassID>>>,
            Where<VendorClass.vendorClassID, Equal<MarketClass>>>),
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
        [PXUIField(DisplayName = "Mailing ID")]
        [PXDefault(typeof(ASCIStarVendorExt.usrSetupID))]
        public virtual Guid? UsrSetupID { get; set; }
        public abstract class usrSetupID : PX.Data.BQL.BqlGuid.Field<usrSetupID> { }
        #endregion
    }
}