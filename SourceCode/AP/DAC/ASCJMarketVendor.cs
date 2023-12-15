using ASCJewelryLibrary.AP.CacheExt;
using PX.Data;
using PX.Data.EP;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.IN;
using System;
using static ASCJewelryLibrary.Common.Descriptor.ASCJConstants;

namespace ASCJewelryLibrary.AP.DAC.Projections
{
    [Serializable]
    [PXCacheName(_cacheName)]
    [PXProjection(typeof(Select5<
        APVendorPrice,
        InnerJoin<InventoryItem,
            On<InventoryItem.inventoryID, Equal<APVendorPrice.inventoryID>>,
        InnerJoin<INItemClass,
            On<INItemClass.itemClassID, Equal<InventoryItem.itemClassID>>,
        InnerJoin<Vendor,
            On<Vendor.bAccountID, Equal<APVendorPrice.vendorID>>>>>,
        Where<Vendor.vendorClassID, Equal<MarketClass>>,
        Aggregate<
            GroupBy<Vendor.bAccountID,
            GroupBy<APVendorPrice.inventoryID>>>>), Persistent = false)]
    public class ASCJMarketVendor : IBqlTable
    {
        private const string _cacheName = "Market Vendor";

        #region Selected
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected { get; set; }
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
        #endregion

        #region RecordID
        [PXDBInt(IsKey = true, BqlField = typeof(APVendorPrice.recordID))]
        public virtual int? RecordID { get; set; }
        public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
        #endregion

        #region VendorID
        [Vendor(BqlField = typeof(APVendorPrice.vendorID))]
        public virtual int? VendorID { get; set; }
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
        #endregion

        #region InventoryID
        [AnyInventory(BqlField = typeof(APVendorPrice.inventoryID))]
        public virtual int? InventoryID { get; set; }
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        #endregion

        [PXDBString(30, IsUnicode = true, BqlField = typeof(INItemClass.itemClassCD))]
        [PXUIField(DisplayName = "Class ID", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
        [PXFieldDescription]
        public virtual string ItemClassCD { get; set; }
        public abstract class itemClassCD : PX.Data.BQL.BqlString.Field<itemClassCD> { }

        #region CuryID
        [PXDBString(5, BqlField = typeof(APVendorPrice.curyID))]
        [PXSelector(typeof(Currency.curyID), CacheGlobal = true)]
        [PXUIField(DisplayName = "Currency", Required = false)]
        public virtual string CuryID { get; set; }
        public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
        #endregion

        #region UOM
        [PXDBString(6, BqlField = typeof(APVendorPrice.uOM))]
        [PXFormula(typeof(Selector<inventoryID, InventoryItem.purchaseUnit>))]
        [PXUIField(DisplayName = "UOM", Enabled = false)]
        public virtual string UOM { get; set; }
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
        #endregion

        #region Market
        [PXDBString(2, IsUnicode = true, InputMask = "", BqlField = typeof(ASCJAPVendorPriceExt.usrMarket))]
        [PXUIField(DisplayName = "Market", Required = true)]
        [MarketList.List]
        [PXDefault(MarketList.LondonPM)]
        public virtual string Market { get; set; }
        public abstract class market : PX.Data.BQL.BqlString.Field<market> { }
        #endregion

        #region MarketID
        [PXDBInt(BqlField = typeof(ASCJAPVendorPriceExt.usrMarketID))]
        [PXUIField(DisplayName = "Market Vendor")]
        [PXSelector(
        typeof(Search2<Vendor.bAccountID, InnerJoin<VendorClass, On<Vendor.vendorClassID, Equal<VendorClass.vendorClassID>>>, Where<VendorClass.vendorClassID, Equal<MarketClass>>>),
            typeof(Vendor.acctCD), typeof(Vendor.acctName)
            , SubstituteKey = typeof(Vendor.acctCD), DescriptionField = typeof(Vendor.acctName))]
        public virtual int? MarketID { get; set; }
        public abstract class marketID : PX.Data.BQL.BqlDecimal.Field<marketID> { }
        #endregion

        #region Commodity
        [PXDBString(1, BqlField = typeof(ASCJAPVendorPriceExt.usrCommodity))]
        [PXUIField(DisplayName = "Commodity Metal Type")]
        [CommodityType.List]
        [PXDefault(CommodityType.Undefined, PersistingCheck = PXPersistingCheck.Null)]
        public virtual string Commodity { get; set; }
        public abstract class commodity : PX.Data.BQL.BqlString.Field<commodity> { }
        #endregion
    }
}
