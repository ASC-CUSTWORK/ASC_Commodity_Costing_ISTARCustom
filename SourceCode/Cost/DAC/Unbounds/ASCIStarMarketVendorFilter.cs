using PX.Data;
using PX.Objects.AP;
using PX.Objects.IN;
using System;

namespace ASCISTARCustom.Cost.DAC.Unbounds
{
    [Serializable]
    [PXCacheName(_cacheName)]
    public class ASCIStarMarketVendorFilter : IBqlTable
    {
        private const string _cacheName = "Market Vendor Filter";

        #region VendorID
        [PXUIField(DisplayName = "Vendor")]
        [VendorNonEmployeeActive]
        public virtual int? VendorID { get; set; }
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
        #endregion

        #region InventoryID
        [InventoryIncludingTemplates(DisplayName = "Inventory ID")]
        public virtual int? InventoryID { get; set; }
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        #endregion

        #region ItemClassCD
        [PXString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Item Class", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDimensionSelector("INITEMCLASS", typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr), ValidComboRequired = true)]
        public virtual string ItemClassCD { get; set; }
        public abstract class itemClassCD : PX.Data.BQL.BqlString.Field<itemClassCD> { }
        #endregion
    }
}
