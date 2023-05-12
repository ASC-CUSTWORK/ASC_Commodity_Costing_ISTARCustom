using ASCISTARCustom.Common.Builder;
using ASCISTARCustom.Common.Descriptor;
using ASCISTARCustom.Common.Helper.Extensions;
using ASCISTARCustom.Cost.CacheExt;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.PO;

namespace ASCISTARCustom.Cost
{
    public class ASCIStarVendorMaintExt : PXGraphExtension<VendorMaint>
    {
        public static bool IsActive() => true;

        #region Selects
        [PXFilterable]
        [PXCopyPasteHiddenView]
        public PXSelectJoin<APVendorPrice,
                           InnerJoin<InventoryItem, On<APVendorPrice.inventoryID, Equal<InventoryItem.inventoryID>>,
                           InnerJoin<INItemClass, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>>,
                                    Where<APVendorPrice.vendorID, Equal<Current<Vendor.bAccountID>>,
                                        And<INItemClass.itemClassCD, Equal<ASCIStarConstants.CommodityClass>>>,
                            OrderBy<Desc<APVendorPrice.effectiveDate>>> VendorPriceBasis;
        #endregion 

        #region CacheAttached
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Basis Price", Visibility = PXUIVisibility.Visible)]
        protected virtual void _(Events.CacheAttached<APVendorPrice.salesPrice> e) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName ="Commodity Type Item")]
        [PXSelector(typeof(SearchFor<InventoryItem.inventoryID>.In<
                            SelectFrom<InventoryItem>.InnerJoin<INItemClass>
                                .On<InventoryItem.itemClassID.IsEqual<INItemClass.itemClassID>>
            .Where<INItemClass.itemClassCD.IsEqual<ASCIStarConstants.CommodityClass>>>), SubstituteKey = typeof(InventoryItem.inventoryCD), DescriptionField = typeof(InventoryItem.descr))]
        protected virtual void _(Events.CacheAttached<APVendorPrice.inventoryID> e) { }
        #endregion

        #region EventHandlers
        protected virtual void _(Events.FieldDefaulting<APVendorPrice, APVendorPrice.vendorID> e)
        {
            if (e.Row == null || this.Base.BAccount.Current == null) return;

            if (this.Base.BAccount.Current.BAccountID > 0)
                e.NewValue = this.Base.BAccount.Current.BAccountID;
            else
                throw new PXException("Save Vendor first!");
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
                    if (rowExt?.UsrMarketID == null)
                        PXUIFieldAttribute.SetError<ASCIStarVendorExt.usrMarketID>(e.Cache, row, "'Market' cannot be empty.");
                }
            }
        }
        #endregion

        protected virtual void UpdateFlorCellingFields(APVendorPrice row)
        {
            var rowExt = PXCache<APVendorPrice>.GetExtension<ASCIStarAPVendorPriceExt>(row);

            var poVendorInventory = SelectFrom<POVendorInventory>
                .Where<POVendorInventory.vendorID.IsEqual<P.AsInt>
                    .And<POVendorInventory.inventoryID.IsEqual<P.AsInt>
                        .And<POVendorInventory.isDefault.IsEqual<True>>>>
                .View.Select(this.Base, this.Base.BAccount.Current?.BAccountID, row.InventoryID)?.TopFirst;

            //if (poVendorInventory == null)
            //{
            //    poVendorInventory = new POVendorInventory() { VendorID = this.Base.BAccount.Current.BAccountID };
            //    PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(poVendorInventory).UsrMarketID = row;
            //}

            //var jewelryCostProvider = new ASCIStarCostBuilder(this.Base)
            //                .WithInventoryItem(inventoryItemExt)
            //                .WithPOVendorInventory(poVendorInventory)
            //                .WithPricingData(poOrderExt.UsrPricingDate ?? PXTimeZoneInfo.Today)
            //                .Build();

        }

       // private asc
    }
}