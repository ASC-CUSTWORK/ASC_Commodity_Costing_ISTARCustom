using ASCJewelryLibrary.AP.CacheExt;
using ASCJewelryLibrary.Common.Builder;
using ASCJewelryLibrary.Common.Descriptor;
using ASCJewelryLibrary.Common.Helper.Extensions;
using ASCJewelryLibrary.IN.CacheExt;
using ASCJewelryLibrary.IN.DAC;
using ASCJewelryLibrary.PO.CacheExt;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.PO;
using static ASCJewelryLibrary.Common.Descriptor.ASCJConstants;

namespace ASCJewelryLibrary.AP
{
    public class ASCJVendorMaintExt : PXGraphExtension<VendorMaint>
    {
        public static bool IsActive() => true;

        #region Selects
        [PXFilterable]
        [PXCopyPasteHiddenView]
        public PXSelectJoin<APVendorPrice,
                           InnerJoin<InventoryItem, On<APVendorPrice.inventoryID, Equal<InventoryItem.inventoryID>>,
                           InnerJoin<INItemClass, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>>,
                                    Where<APVendorPrice.vendorID, Equal<Current<Vendor.bAccountID>>,
                                        And<INItemClass.itemClassCD, Equal<ASCJConstants.CommodityClass>>>,
                            OrderBy<Desc<APVendorPrice.effectiveDate>>> ASCJVendorPriceBasis;
        #endregion

        #region CacheAttached
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Basis Floor", Visibility = PXUIVisibility.Visible)]
        protected virtual void _(Events.CacheAttached<APVendorPrice.salesPrice> e) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Commodity Type Item")]
        [PXSelector(typeof(SearchFor<InventoryItem.inventoryID>.In<
                            SelectFrom<InventoryItem>.InnerJoin<INItemClass>
                                .On<InventoryItem.itemClassID.IsEqual<INItemClass.itemClassID>>
            .Where<INItemClass.itemClassCD.IsEqual<ASCJConstants.CommodityClass>>>), SubstituteKey = typeof(InventoryItem.inventoryCD), DescriptionField = typeof(InventoryItem.descr))]
        protected virtual void _(Events.CacheAttached<APVendorPrice.inventoryID> e) { }
        #endregion

        #region EventHandlers
        protected virtual void _(Events.FieldDefaulting<APVendorPrice, APVendorPrice.vendorID> e)
        {
            if (e.Row == null || this.Base.BAccount.Current == null) return;

            if (this.Base.BAccount.Current.BAccountID > 0)
                e.NewValue = this.Base.BAccount.Current.BAccountID;
            else
                throw new PXException("Save Vendor first and then add Costing information.");
        }

        protected virtual void _(Events.RowSelected<VendorR> e)
        {
            if (e.Row is VendorR row)
            {
                PXUIFieldAttribute.SetRequired<ASCJVendorExt.usrASCJMarketID>(e.Cache, row.VendorClassID?.NormalizeCD() != "MARKET");
            }
        }

        protected virtual void _(Events.RowPersisting<VendorR> e)
        {
            if (e.Row is VendorR row)
            {
                if (row.VendorClassID?.NormalizeCD() != "MARKET")
                {
                    var rowExt = PXCache<Vendor>.GetExtension<ASCJVendorExt>(row);
                    if (rowExt?.UsrASCJMarketID == null)
                        PXUIFieldAttribute.SetError<ASCJVendorExt.usrASCJMarketID>(e.Cache, row, "'Market' cannot be empty.");
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<APVendorPrice, APVendorPrice.salesPrice> e)
        {
            var row = e.Row;
            if (row == null) return;

            UpdateCostProviderFields(e.Cache, row);
        }

        protected virtual void _(Events.FieldUpdated<APVendorPrice, ASCJAPVendorPriceExt.usrASCJMatrixStep> e)
        {
            var row = e.Row;
            if (row == null) return;

            UpdateCostProviderFields(e.Cache, row);
        }
        #endregion

        #region Methods
        protected virtual void UpdateCostProviderFields(PXCache cache, APVendorPrice row)
        {
            if (row.InventoryID == null) return;

            var rowExt = PXCache<APVendorPrice>.GetExtension<ASCJAPVendorPriceExt>(row);

            var poVendorInventory = new POVendorInventory() { VendorID = row.VendorID };
            PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(poVendorInventory).UsrASCJMarketID = rowExt.UsrASCJMarketID;

            var inventoryItem = InventoryItem.PK.Find(Base, row.InventoryID);
            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(inventoryItem);
            inventoryItemExt.UsrASCJMatrixStep = rowExt.UsrASCJMatrixStep;


            var inJewelryAttribute = new ASCJINJewelryItem();
            var itemCD = inventoryItem.InventoryCD.Trim();
            if (itemCD == "24K") inJewelryAttribute.MetalType = "24K";
            if (itemCD == "SSS") inJewelryAttribute.MetalType = "SSS";

            var jewelryCostProvider = new ASCJCostBuilder(this.Base)
                            .WithInventoryItem(inventoryItemExt)
                            .WithJewelryAttrData(inJewelryAttribute)
                            .WithPOVendorInventory(poVendorInventory)
                            .Build();
            if (jewelryCostProvider == null) return;
            jewelryCostProvider.CalculatePreciousMetalCost(CostingType.ContractCost);

            cache.SetValueExt<ASCJAPVendorPriceExt.usrASCJFloor>(row, jewelryCostProvider.Floor);
            cache.SetValueExt<ASCJAPVendorPriceExt.usrASCJCeiling>(row, jewelryCostProvider.Ceiling);
            cache.SetValueExt<ASCJAPVendorPriceExt.usrASCJBasisValue>(row, jewelryCostProvider.BasisValue);
        }
        #endregion

    }
}