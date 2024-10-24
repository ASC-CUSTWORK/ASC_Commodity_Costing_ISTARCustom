using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.IN;
using System;
using static ASCJSMCustom.Common.Descriptor.ASCJSMConstants;

namespace ASCJSMCustom.Cost.DAC.Unbounds
{
    [Serializable]
    [PXCacheName(_cacheName)]
    public class ASCJSMMarketVendorFilter : IBqlTable
    {
        private const string _cacheName = "Market Vendor Filter";

        #region VendorID
        [PXUIField(DisplayName = "Vendor")]
        [VendorNonEmployeeActive()]
        [PXRestrictor(typeof(Where<Vendor.vendorClassID.IsEqual<MarketClass>>), "", ShowWarning = false)]
        public virtual int? VendorID { get; set; }
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
        #endregion

        #region InventoryID
        [PXInt]
        [PXSelector(typeof(SearchFor<InventoryItem.inventoryID>.In<SelectFrom<InventoryItem>
            .InnerJoin<INItemClass>.On<InventoryItem.itemClassID.IsEqual<INItemClass.itemClassID>>
            .Where<INItemClass.itemClassCD.IsEqual<CommodityClass>>>)
            , SubstituteKey = typeof(InventoryItem.inventoryCD), DescriptionField = typeof(InventoryItem.descr))]
        [PXUIField(DisplayName = "Commodity Inventory ID")]
        public virtual int? InventoryID { get; set; }
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        #endregion
    }
}
