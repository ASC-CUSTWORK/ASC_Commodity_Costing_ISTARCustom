using ASCISTARCustom.Common.Descriptor;
using ASCISTARCustom.Common.Helper.Extensions;
using ASCISTARCustom.Cost.CacheExt;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.IN;
using System;
using static PX.SM.EMailAccount;
//using InfoSmartSearch;

namespace ASCISTARCustom.Cost
{
    public class ASCIStarVendorMaintExt : PXGraphExtension<VendorMaint>
    {
        public static bool IsActive() => true;

        #region Selects

        //public PXSelect<ASCIStarVendorPriceConfig, Where<ASCIStarVendorPriceConfig.bAccountID, Equal<Current<Vendor.bAccountID>>>> PriceConfig;

        //[PXFilterable]
        //public PXSelectJoin<APVendorPrice,
        //        InnerJoin<InventoryItem, On<APVendorPrice.inventoryID, Equal<InventoryItem.inventoryID>>,
        //        InnerJoin<INItemClass, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>>,
        //        Where<APVendorPrice.vendorID, Equal<Current<Vendor.bAccountID>>, 
        //            And<INItemClass.itemClassCD, Equal<CommodityClass>>>> VendorPriceBasis;
        public class today : PX.Data.BQL.BqlDateTime.Constant<today>
        {
            public today() : base(DateTime.Today)
            {
            }
        }

        //[PXFilterable]
        //public PXSelectJoin<APVendorPrice,
        //        InnerJoin<InventoryItem, On<APVendorPrice.inventoryID, Equal<InventoryItem.inventoryID>>,
        //        InnerJoin<POVendorInventory, On<InventoryItem.inventoryID, Equal<POVendorInventory.inventoryID>>,
        //        InnerJoin<InventoryItemCurySettings, On<POVendorInventory.inventoryID, Equal<InventoryItemCurySettings.inventoryID>,
        //            And<POVendorInventory.vendorID, Equal<InventoryItemCurySettings.preferredVendorID>>>,
        //        InnerJoin<INItemClass, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>>>>,
        //        Where<APVendorPrice.vendorID, Equal<Required<APVendorPrice.vendorID>>,
        //            And<INItemClass.itemClassCD, Equal<CommodityClass>,
        //            And<APVendorPrice.effectiveDate, LessEqual<today>,
        //            And<APVendorPrice.expirationDate, GreaterEqual<today>>>>>,
        //        OrderBy<Desc<APVendorPrice.effectiveDate>>> VendorPriceBasis;

        [PXFilterable]
        [PXCopyPasteHiddenView]
        public PXSelectJoin<APVendorPrice,
        InnerJoin<InventoryItem, On<APVendorPrice.inventoryID, Equal<InventoryItem.inventoryID>>,
        InnerJoin<INItemClass, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>>,
        Where<APVendorPrice.vendorID, Equal<Current<Vendor.bAccountID>>,
            And<INItemClass.itemClassCD, Equal<ASCIStarConstants.CommodityClass>>>,
        OrderBy<Desc<APVendorPrice.effectiveDate>>> VendorPriceBasis;

        #endregion Select

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Basis Price", Visibility = PXUIVisibility.Visible)]
        protected virtual void _(Events.CacheAttached<APVendorPrice.salesPrice> e) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]

        //    [APCrossItem(BAccountField = typeof(APVendorPrice.vendorID), WarningOnNonUniqueSubstitution = true, AllowTemplateItems = true)]
        //[PXParent(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<APVendorPrice.inventoryID>>
        //    , And<InventoryItem.itemClassID, Equal<Current<INItemClass.itemClassID>>>>>))]

        [PXSelector(typeof(SearchFor<InventoryItem.inventoryID>.In<
                            SelectFrom<InventoryItem>.InnerJoin<INItemClass>
                                .On<InventoryItem.itemClassID.IsEqual<INItemClass.itemClassID>>
            .Where<INItemClass.itemClassCD.IsEqual<ASCIStarConstants.CommodityClass>>>), SubstituteKey = typeof(InventoryItem.inventoryCD), DescriptionField = typeof(InventoryItem.descr))]
        protected virtual void _(Events.CacheAttached<APVendorPrice.inventoryID> e) { }

        #region EventHandlers
        protected virtual void _(Events.RowInserted<APVendorPrice> e)
        {
            if (e.Row == null) return;

            e.Cache.SetValueExt<APVendorPrice.vendorID>(e.Row, this.Base.BAccount.Current.BAccountID);
        }

        protected virtual void _(Events.RowSelected<VendorR> e)
        {
            if (e.Row is VendorR row)
            {
                PXUIFieldAttribute.SetRequired<ASCIStarVendorExt.usrMarketID>(e.Cache, row.VendorClassID?.NormalizeCD() != "MARKET");
            }
        }
        protected virtual void _(Events.RowPersisting<VendorR> e)
        {
            if (e.Row is VendorR row)
            {
                if (row.VendorClassID?.NormalizeCD() != "MARKET")
                {
                    var rowExt = PXCache<Vendor>.GetExtension<ASCIStarVendorExt>(row);
                    PXUIFieldAttribute.SetError<ASCIStarVendorExt.usrMarketID>(e.Cache, row, "'Market' cannot be empty.");
                }
            }
        }
        #endregion

    }
}