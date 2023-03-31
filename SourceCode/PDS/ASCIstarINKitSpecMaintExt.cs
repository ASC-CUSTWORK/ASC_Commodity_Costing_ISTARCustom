using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.TM;
using PX.Objects;
using PX.Objects.IN;
using PX.Objects.AP;
using PX.Objects.PO;
using System.Collections;
using System.Reflection;
using ASCISTARCustom.PDS.CacheExt;
using ASCISTARCustom.Inventory.DAC;
using ASCISTARCustom.Inventory.CacheExt;
using ASCISTARCustom.Cost.Descriptor;

namespace ASCISTARCustom.PDS
{
    public class ASCIstarINKitSpecMaintExt : PXGraphExtension<INKitSpecMaint>
    {
        #region Static Functions
        public static bool IsActive()
        {
            return true;
        }
        #endregion
        public class today : PX.Data.BQL.BqlDateTime.Constant<today> // <-Why?
        {
            public today() : base(DateTime.Today)
            {
            }
        }

        #region View
        public SelectFrom<InventoryItem>.Where<InventoryItem.inventoryID.IsEqual<INKitSpecHdr.kitInventoryID.FromCurrent>>.View InventoryItemHdr;

        //public PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>> InventoryItemHdr;
        //public PXSelect<ASCIStarInventoryItemCommodity, Where<ASCIStarInventoryItemCommodity.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>> ItemCommodity;
        public PXSelect<ASCIStarINKitSpecHdrAttribute,
            Where<ASCIStarINKitSpecHdrAttribute.kitInventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>
                , And<ASCIStarINKitSpecHdrAttribute.revisionID, Equal<Current<INKitSpecHdr.revisionID>>>>> iStarAttributes;
        //public PXSelect<ASCIStarINKitSpecHdrVendorQuote, Where<ASCIStarINKitSpecHdrVendorQuote.kitInventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>, And<ASCIStarINKitSpecHdrVendorQuote.revisionID, Equal<Current<INKitSpecHdr.revisionID>>>>> iStarVendorQuote;
        public PXSelect<APVendorPrice, Where<APVendorPrice.inventoryID, Equal<Current<INKitSpecStkDet.compInventoryID>>,
            And<APVendorPrice.vendorID, Equal<Required<APVendorPrice.vendorID>>>>> iStarCommodityPrice;

        public PXSelect<POVendorInventory, Where<POVendorInventory.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>> VendorItems;
        public PXSelect<POVendorInventory, Where<POVendorInventory.inventoryID, Equal<Required<INKitSpecHdr.kitInventoryID>>, And<POVendorInventory.isDefault, Equal<True>>>> DefaultVendorItem;
        public PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<INKitSpecStkDet.compInventoryID>>>> BaseItem;


        public SelectFrom<ASCIStarINKitSpecJewelryItem>
                 .Where<ASCIStarINKitSpecJewelryItem.kitInventoryID.IsEqual<INKitSpecHdr.kitInventoryID.FromCurrent>
                    .And<ASCIStarINKitSpecJewelryItem.revisionID.IsEqual<INKitSpecHdr.revisionID.FromCurrent>>>
                        .View JewelryItemView;

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
        public PXSelectJoin<APVendorPrice,
            InnerJoin<POVendorInventory, On<POVendorInventory.vendorID, Equal<Current<APVendorPrice.vendorID>>,
                And<POVendorInventory.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>>,
            InnerJoin<InventoryItemCurySettings, On<InventoryItemCurySettings.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
                And<InventoryItemCurySettings.preferredVendorID, Equal<POVendorInventory.vendorID>>>,
            InnerJoin<InventoryItem, On<APVendorPrice.inventoryID, Equal<InventoryItem.inventoryID>>,
            InnerJoin<INItemClass, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>>>>,
        Where<APVendorPrice.vendorID, Equal<InventoryItemCurySettings.preferredVendorID>,
            And<INItemClass.itemClassCD, Equal<CommodityClass>,
            And<APVendorPrice.effectiveDate, LessEqual<today>,
            And<APVendorPrice.expirationDate, GreaterEqual<today>>>>>,
        OrderBy<Desc<APVendorPrice.effectiveDate>>> VendorPriceBasis;

        //public PXSelectJoin<APVendorPrice,
        //        InnerJoin<InventoryItem, On<APVendorPrice.inventoryID, Equal<InventoryItem.inventoryID>>,
        //        InnerJoin<INKitSpecStkDet, On<INKitSpecStkDet.compInventoryID, Equal<APVendorPrice.inventoryID>>,
        //        InnerJoin<POVendorInventory, On<POVendorInventory.inventoryID, Equal<INKitSpecStkDet.kitInventoryID>,
        //            And<APVendorPrice.vendorID, Equal<POVendorInventory.vendorID>>>,
        //        InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INKitSpecStkDet.compInventoryID>>>>>>,
        //        Where<INKitSpecStkDet.kitInventoryID, Equal<Current<INKitSpecStkDet.kitInventoryID>>>> VendorPriceBasis;


        [PXFilterable]
        public SelectFrom<APVendorPrice>.
            InnerJoin<POVendorInventory>.
            On<POVendorInventory.inventoryID.IsEqual<INKitSpecHdr.kitInventoryID.FromCurrent>.
                And<APVendorPrice.vendorID.IsEqual<POVendorInventory.vendorID>>>.
            InnerJoin<InventoryItem>.
                On<InventoryItem.inventoryID.IsEqual<APVendorPrice.inventoryID>>.
            InnerJoin<INItemClass>.On<InventoryItem.itemClassID.IsEqual<INItemClass.itemClassID>.
                And<INItemClass.itemClassCD.IsEqual<CommodityClass>>>.
            Where<APVendorPrice.effectiveDate.IsLessEqual<AccessInfo.businessDate>.
                And<Brackets<APVendorPrice.expirationDate.IsGreaterEqual<AccessInfo.businessDate>.
                    Or<APVendorPrice.expirationDate.IsNull>>>>.
                    OrderBy<APVendorPrice.effectiveDate.Asc> MarketPriceBasis;


        [PXFilterable]
        public PXSelectJoin<POVendorInventory,
                InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>>> VendorContractPrice;


        public PXSelect<INKitSpecStkDet,
                Where<INKitSpecStkDet.kitInventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
                And<INKitSpecStkDet.kitInventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
                And<INKitSpecStkDet.revisionID, Equal<Current<INKitSpecHdr.revisionID>>>>>>
            SpecComponents;

        public PXSelectJoin<INKitSpecStkDet,
                InnerJoin<InventoryItem, On<INKitSpecStkDet.FK.ComponentInventoryItem>,
                InnerJoin<INItemClass, On<InventoryItem.FK.ItemClass>>>,
                Where<INItemClass.itemClassCD, Equal<CommodityClass>,
                And<INKitSpecStkDet.kitInventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
                And<INKitSpecStkDet.revisionID, Equal<Current<INKitSpecHdr.revisionID>>>>>>
            SpecCommodity;


        public PXSelectJoin<INKitSpecNonStkDet,
                InnerJoin<InventoryItem, On<INKitSpecNonStkDet.FK.ComponentInventoryItem>>,
                Where<INKitSpecNonStkDet.kitInventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
                And<INKitSpecNonStkDet.revisionID, Equal<Current<INKitSpecHdr.revisionID>>>>>
            SpecOverhead;

        public PXSelect<INUnit,
                Where<INUnit.fromUnit, Equal<Required<INUnit.fromUnit>>,
                And<INUnit.toUnit, Equal<Required<INUnit.toUnit>>>>>
            CommodityConversion;

        //public PXSelect<ASCIStarItemCostRollup, Where<ASCIStarItemCostRollup.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>, And<ASCIStarItemCostRollup.bAccountID, Equal<CompanyBAccount.bAccountID>>>> CostRollup;
        //public PXSelect<ASCIStarItemCostRollup, Where<ASCIStarItemCostRollup.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>, And<ASCIStarItemCostRollup.bAccountID, NotEqual<CompanyBAccount.bAccountID>>>> VendorCostRollup;

        public PXSelect<
            ASCIStarItemWeightCostSpec,
            Where<ASCIStarItemWeightCostSpec.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
                And<ASCIStarItemWeightCostSpec.revisionID, Equal<Current<INKitSpecHdr.revisionID>>>>>
            ASCIStarCostSpecification;
        #endregion

        #region CacheAttached
        //public class AAVENDOR : PX.Data.BQL.BqlInt.Constant<AAVENDOR>
        //{
        //    public static readonly int value = 6995;
        //    public AAVENDOR() : base(value) { }
        //}


        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXFormula(typeof(Add<Add<Add<Add<Add<Add<Add<Add<
                             ASCIStarINInventoryItemExt.usrCommodityCost
                           , ASCIStarINInventoryItemExt.usrOtherMaterialCost>
                           , ASCIStarINInventoryItemExt.usrFabricationCost>
                           , ASCIStarINInventoryItemExt.usrLaborCost>
                           , ASCIStarINInventoryItemExt.usrHandlingCost>
                           , ASCIStarINInventoryItemExt.usrFreightCost>
                           , ASCIStarINInventoryItemExt.usrDutyCost>
                           , ASCIStarINInventoryItemExt.usrPackagingCost>
                           , ASCIStarINInventoryItemExt.usrOtherCost>
                           ))]
        protected void InventoryItem_UsrUnitCost_CacheAttached(PXCache sender) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXFormula(typeof(Add<Add<Add<Add<Add<Add<
                     ASCIStarINInventoryItemExt.usrCommodityCost
                   , ASCIStarINInventoryItemExt.usrOtherMaterialCost>
                   , ASCIStarINInventoryItemExt.usrFabricationCost>
                   , ASCIStarINInventoryItemExt.usrLaborCost>
                   , ASCIStarINInventoryItemExt.usrHandlingCost>
                   , ASCIStarINInventoryItemExt.usrPackagingCost>
                   , ASCIStarINInventoryItemExt.usrOtherCost>
                   ))]
        protected void InventoryItem_UsrContractCost_CacheAttached(PXCache sender) { }


        //[PXMergeAttributes(Method = MergeMethod.Append)]
        //protected void INKitSpecHdr_UsrActualGRAMSilver_CacheAttached(PXCache sender) { }

        //[PXMergeAttributes(Method = MergeMethod.Append)]
        //protected void INKitSpecHdr_UsrPricingGRAMSilver_CacheAttached(PXCache sender) { }

        //[PXMergeAttributes(Method = MergeMethod.Append)]
        //protected void INKitSpecHdr_UsrActualGRAMGold_CacheAttached(PXCache sender) { }

        //[PXMergeAttributes(Method = MergeMethod.Append)]
        //protected void INKitSpecHdr_UsrPricingGRAMGold_CacheAttached(PXCache sender) { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXFormula(typeof(Switch<Case<Where<Current<APVendorPrice.uOM>, NotEqual<TOZ>>, Null>, Div<APVendorPrice.salesPrice, TOZ2GRAM>>))]
        protected void APVendorPrice_UsrCommodityPerGram_CacheAttached(PXCache sender) { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXFormula(typeof(Switch<Case<Where<Current<APVendorPrice.uOM>, NotEqual<TOZ>>, Null>, Div<APVendorPrice.salesPrice, ASCIStarAPVendorPriceExt.usrCommodityPrice>>))]
        protected void APVendorPrice_UsrIncrement_CacheAttached(PXCache sender) { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXFormula(typeof(Switch<Case<Where<Current<APVendorPrice.uOM>, NotEqual<TOZ>>, Null>, Div<Div<APVendorPrice.salesPrice, ASCIStarAPVendorPriceExt.usrCommodityPrice>, TOZ2GRAM>>))]
        protected void APVendorPrice_UsrIncrementPerGram_CacheAttached(PXCache sender) { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        //   [PXDBDefault(typeof(INKitSpecHdr.kitInventoryID))]
        [PXDefault(typeof(INKitSpecHdr.kitInventoryID))]
        protected void APVendorPrice_InventoryID_CacheAttached(PXCache sender) { }


        [PXRemoveBaseAttribute(typeof(PXDBDefaultAttribute))]
        [PXRemoveBaseAttribute(typeof(PXParentAttribute))]
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDBDefault(typeof(INKitSpecHdr.kitInventoryID))]
        [PXParent(typeof(ASCIStarItemWeightCostSpec.FK.InventoryItemFK))]
        protected void _(Events.CacheAttached<ASCIStarItemWeightCostSpec.inventoryID> cacheAttached) { }

        [PXMergeAttributes(Method = MergeMethod.Replace)]
        [PXDBString(10, IsKey = true, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
        [PXDBDefault(typeof(INKitSpecHdr.revisionID))]
        [PXParent(typeof(ASCIStarItemWeightCostSpec.FK.KitSpecificationFK))]
        protected void _(Events.CacheAttached<ASCIStarItemWeightCostSpec.revisionID> cacheAttached) { }
        #endregion

        #region Event Handlers

        protected virtual void _(Events.RowInserted<INKitSpecHdr> e)
        {
            var row = e.Row;
            if (row == null || this.Base.Hdr.Current == null) return;

            CopyJewelryItemFields(this.Base.Hdr.Current);
        }

        protected virtual void POVendorInventory_UsrVendorDefault_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        {
            MethodBase m = MethodBase.GetCurrentMethod();
            PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

            if (InvokeBaseHandler != null)
                InvokeBaseHandler(cache, e);

            POVendorInventory row = e.Row as POVendorInventory;
            if (row != null)
            {
                ASCIStarPOVendorInventoryExt ext = row.GetExtension<ASCIStarPOVendorInventoryExt>();
                bool UseVendor = (ext.UsrVendorDefault == true);
                if (UseVendor)
                {
                    ext.UsrCommodityPrice = 0.00m; //REPLACE WITH MARKET CALL
                    PXUIFieldAttribute.SetEnabled<ASCIStarPOVendorInventoryExt.usrCommodityPrice>(cache, row, !UseVendor);
                    PXUIFieldAttribute.SetEnabled<ASCIStarPOVendorInventoryExt.usrCommodityIncrement>(cache, row, !UseVendor);
                    PXUIFieldAttribute.SetEnabled<ASCIStarPOVendorInventoryExt.usrCommoditySurchargePct>(cache, row, !UseVendor);
                    PXUIFieldAttribute.SetEnabled<ASCIStarPOVendorInventoryExt.usrCommodityLossPct>(cache, row, !UseVendor);
                    //Replace with Vendor Defaults
                }
                else
                {

                    PXUIFieldAttribute.SetEnabled<ASCIStarPOVendorInventoryExt.usrCommodityPrice>(cache, row, !UseVendor);
                    PXUIFieldAttribute.SetEnabled<ASCIStarPOVendorInventoryExt.usrCommodityIncrement>(cache, row, !UseVendor);
                    PXUIFieldAttribute.SetEnabled<ASCIStarPOVendorInventoryExt.usrCommoditySurchargePct>(cache, row, !UseVendor);
                    PXUIFieldAttribute.SetEnabled<ASCIStarPOVendorInventoryExt.usrCommodityLossPct>(cache, row, !UseVendor);
                }

            }
        }

        //protected virtual void _(Events.RowSelecting<INKitSpecHdr> e)
        //{
        //    ASCIStarINKitSpecHdrExt ext = e.Row.GetExtension<ASCIStarINKitSpecHdrExt>();
        //    if (ext == null)
        //        return;
        //    var row = (INKitSpecHdr)e.Row;
        //    foreach (PXResult<INKitSpecStkDet, InventoryItem> r in
        //        PXSelectJoin<INKitSpecStkDet,
        //        InnerJoin<InventoryItem, On<INKitSpecStkDet.FK.ComponentInventoryItem>>,
        //        Where<INKitSpecStkDet.kitInventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
        //        And<INKitSpecStkDet.revisionID, Equal<Current<INKitSpecHdr.revisionID>>>>>.
        //        Select(cache.Graph, row.KitInventoryID, row.RevisionID))
        //    {

        //    }
        //        if (e.Row != null && (ext.Cost TotalCostStock == null || e.Row.TotalCostNonStock == null))
        //    {
        //        using (new PXConnectionScope())
        //        {
        //            e.Row.CostPXFormulaAttribute.CalcAggregate<INComponentTran.tranCost>(SpecComponents.Cache, e.Row, true);
        //            cache.RaiseFieldUpdated<INKitRegister.totalCostStock>(e.Row, null);

        //            PXFormulaAttribute.CalcAggregate<INOverheadTran.tranCost>(SpecOverhead.Cache, e.Row, true);
        //            cache.RaiseFieldUpdated<INKitRegister.totalCostNonStock>(e.Row, null);
        //        }
        //    }
        //}
        protected void INKitSpecHdr_RowSelected(PXCache cache, PXRowSelectedEventArgs e, PXRowSelected InvokeBaseHandler)
        {
            MethodBase m = MethodBase.GetCurrentMethod();
            PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

            if (InvokeBaseHandler != null)
                InvokeBaseHandler(cache, e);
            var row = (INKitSpecHdr)e.Row;
            if (iStarAttributes.Current == null)
            {
                PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecHdrAttribute.freightCost>(iStarAttributes.Cache, null, false);
                PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecHdrAttribute.freightPercent>(iStarAttributes.Cache, null, false);
            }
        }
        //protected void INKitSpecHdr_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e, PXRowUpdated InvokeBaseHandler)
        //{
        //    if (InvokeBaseHandler != null)
        //        InvokeBaseHandler(cache, e);
        //    var row = (INKitSpecHdr)e.Row;
        //    if (row == null) return;
        //    InventoryItem item = (InventoryItem)cache.Current;
        //    ASCIStarINInventoryItemExt itemExt = cache.GetExtension<ASCIStarINInventoryItemExt>(row);

        //    //ext.UsrUnitCost = ext.UsrCommodityCost + ext.UsrOtherCost;
        //    //InvokeBaseHandler(cache, e);
        //}
        //protected void INKitSpecHdr_RowInserted(PXCache cache, PXRowInsertedEventArgs e, PXRowInserted InvokeBaseHandler)
        //{
        //    MethodBase m = MethodBase.GetCurrentMethod();
        //    PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

        //    if (InvokeBaseHandler != null)
        //        InvokeBaseHandler(cache, e);
        //    var row = (INKitSpecHdr)e.Row;
        //    if (row == null) return;
        //    ASCIStarINKitSpecHdrExt ext = cache.GetExtension<ASCIStarINKitSpecHdrExt>(row);
        //    if (ext.UsrVQuoteLineCtr == null) ext.UsrVQuoteLineCtr = 0;
        //    //ext.UsrUnitCost = ext.UsrCommodityCost + ext.UsrOtherCost;

        //}
        //protected void INKitSpecHdr_RowPersisting(PXCache cache, PXRowPersistingEventArgs e, PXRowPersisting InvokeBaseHandler)
        //{
        //    MethodBase m = MethodBase.GetCurrentMethod();
        //    PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

        //    if (InvokeBaseHandler != null)
        //        InvokeBaseHandler(cache, e);
        //    var row = (INKitSpecHdr)e.Row;
        //    if (row == null) return;
        //    ASCIStarINKitSpecHdrExt ext = cache.GetExtension<ASCIStarINKitSpecHdrExt>(row);
        //    if (ext.UsrVQuoteLineCtr == null) ext.UsrVQuoteLineCtr = 0;
        //}


        protected void INKitSpecStkDet_DfltCompQty_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        {
            MethodBase m = MethodBase.GetCurrentMethod();
            PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

            if (InvokeBaseHandler != null)
                InvokeBaseHandler(cache, e);
            var row = (INKitSpecStkDet)e.Row;
            if (row == null) return;
            ASCIStarINKitSpecStkDetExt stkDetExt = cache.GetExtension<ASCIStarINKitSpecStkDetExt>(row);
            if (stkDetExt == null) return;
            cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrExtCost>(cache.Current, row.DfltCompQty * stkDetExt.UsrUnitCost);
            InventoryItem item = InventoryItem.PK.Find(cache.Graph, row.CompInventoryID);
            INItemClass itemClass = INItemClass.PK.Find(cache.Graph, item.ItemClassID);

            PXTrace.WriteInformation($"item.InventoryCD:{item.InventoryCD.Trim()}");
            PXTrace.WriteInformation($"itemClass.ItemClassCD:{itemClass.ItemClassCD.Trim()}");

            if (itemClass.ItemClassCD.Trim() == CommodityClass.value)
            {
                ASCIStarINInventoryItemExt itemExt = item.GetExtension<ASCIStarINInventoryItemExt>();
                InventoryItem priceAsItem = InventoryItem.PK.Find(cache.Graph, itemExt.UsrPriceAsID);
                PXTrace.WriteInformation($"priceAsItem.InventoryCD:{priceAsItem.InventoryCD.Trim()}");
                ASCIStarINInventoryItemExt priceAsUnit = priceAsItem.GetExtension<ASCIStarINInventoryItemExt>();

                decimal WeightRollup = 0.00m;

                //if (row.UOM == "")
                //{
                decimal rowWeight = 0.00m;
                rowWeight = row.DfltCompQty.Value;
                if (row.UOM == "DWT")
                    rowWeight *= 1.555174m;
                WeightRollup += rowWeight;
                //}

                PXTrace.WriteInformation($"WeightRollup:{WeightRollup} to be converted {priceAsUnit.UsrPriceToUnit.Trim()}/{priceAsItem.InventoryCD.Trim()}");

                INUnit inUnit =
                    new PXSelect<INUnit,
                    Where<INUnit.fromUnit, Equal<Required<INUnit.fromUnit>>,
                        And<INUnit.toUnit, Equal<Required<INUnit.toUnit>>>>>(cache.Graph).SelectSingle(
                        priceAsUnit.UsrPriceToUnit.Trim(),
                        priceAsItem.InventoryCD.Trim());

                if (inUnit == null) return;
                PXTrace.WriteInformation($"inUnit.UnitRate:{inUnit.UnitRate}");

                decimal convert = (decimal)inUnit.UnitRate;

                if (priceAsItem.InventoryCD.Trim() == "SSS")
                {
                    cache.SetValueExt<ASCIStarINInventoryItemExt.usrActualGRAMSilver>(cache.Current, WeightRollup);  //stkDetExt.UsrExtCost = row.DfltCompQty * stkDetExt.UsrUnitCost;
                    cache.SetValueExt<ASCIStarINInventoryItemExt.usrPricingGRAMSilver>(cache.Current, WeightRollup * convert);  //stkDetExt.UsrExtCost = row.DfltCompQty * stkDetExt.UsrUnitCost;
                    cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrActualGRAMSilver>(e.Row, null);
                    cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrPricingGRAMSilver>(e.Row, null);
                }
                if (priceAsItem.InventoryCD.Trim() == "24K")
                {
                    cache.SetValueExt<ASCIStarINInventoryItemExt.usrActualGRAMGold>(cache.Current, WeightRollup);  //stkDetExt.UsrExtCost = row.DfltCompQty * stkDetExt.UsrUnitCost;
                    cache.SetValueExt<ASCIStarINInventoryItemExt.usrPricingGRAMGold>(cache.Current, WeightRollup * convert);  //stkDetExt.UsrExtCost = row.DfltCompQty * stkDetExt.UsrUnitCost;
                    cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrActualGRAMSilver>(e.Row, null);
                    cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrPricingGRAMSilver>(e.Row, null);
                }


            }
        }

        //protected void INKitSpecNonStkDet_DfltCompQty_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        //{
        //    MethodBase m = MethodBase.GetCurrentMethod();
        //    PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

        //    if (InvokeBaseHandler != null)
        //        InvokeBaseHandler(cache, e);
        //    var row = (INKitSpecStkDet)e.Row;
        //    if (row == null) return;
        //    ASCIStarINKitSpecNonStkDetExt stkDetExt = cache.GetExtension<ASCIStarINKitSpecNonStkDetExt>(row);
        //    if (stkDetExt == null) return;
        //    cache.SetValueExt<ASCIStarINKitSpecNonStkDetExt.usrExtCost>(cache.Current, row.DfltCompQty * stkDetExt.UsrUnitCost);
        //    //stkDetExt.UsrExtCost = row.DfltCompQty * stkDetExt.UsrUnitCost;

        //}

        //protected void ASCIStarINKitSpecHdrExt_FreightType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e, PXFieldDefaulting InvokeBaseHandler)
        //{
        //    if (InvokeBaseHandler != null)
        //        InvokeBaseHandler(cache, e);
        //    var row = (ASCIStarINKitSpecHdrAttribute)e.Row;
        //    if (row == null) return;

        //    InventoryItem baseItem = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>>.Select(cache.Graph);
        //    ASCIStarINInventoryItemExt extItem = baseItem.GetExtension<ASCIStarINInventoryItemExt>();

        //    e.NewValue = "1";
        //}


        //protected virtual void _(Events.FieldDefaulting<INKitSpecHdr, INKitSpecHdr.revisionID> e)
        protected void INKitSpecHdr_RevisionID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e, PXFieldDefaulting InvokeBaseHandler)

        {
            MethodBase m = MethodBase.GetCurrentMethod();
            PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);


            e.NewValue = ASCIStarCostingType.StandardCost;
            INKitSpecHdr row = e.Row as INKitSpecHdr;
            if (row == null)
                return;

            SetVisibleRevisionID();
            //if (PXAccess.GetCompanyName() != "PDS")
            //{
            //    PXUIFieldAttribute.SetVisible<INKitSpecHdr.revisionID>(cache, row, false);
            //    e.NewValue = "01";
            //}

        }
        //        protected virtual void _(Events.FieldSelecting<INKitSpecHdr, INKitSpecHdr.revisionID> e)
        protected void INKitSpecHdr_RevisionID_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e, PXFieldSelecting InvokeBaseHandler)
        {
            MethodBase m = MethodBase.GetCurrentMethod();
            PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

            INKitSpecHdr row = e.Row as INKitSpecHdr;
            if (row == null)
                return;

            SetVisibleRevisionID();

            //if (PXAccess.GetCompanyName() != "PDS")
            //{
            //    PXUIFieldAttribute.SetVisible<INKitSpecHdr.revisionID>(cache, row, false);
            //}
        }

        //        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCIStarINKitSpecStkDetExt.usrUnitCost> e)
        protected void INKitSpecHdr_UsrUnitCost_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e, PXFieldDefaulting InvokeBaseHandler)
        {
            MethodBase m = MethodBase.GetCurrentMethod();
            PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

            e.NewValue = 0.00m;
            INKitSpecStkDet row = e.Row as INKitSpecStkDet;
            if (row == null)
                return;



        }
        //INKitSpecStkDet stkDet = (INKitSpecStkDet)row;
        //ASCIStarINKitSpecStkDetExt stkDetExt = stkDet.GetExtension<ASCIStarINKitSpecStkDetExt>();
        //e.NewValue = 
        ////PXSelectBase<InventoryItem> vendorCostSelect =
        ////    new PXSelectReadonly2<InventoryItem,

        //PXSelectBase<InventoryItem> vendorCostSelect = 
        //    new PXSelectJoin<InventoryItem, 
        //                            LeftJoin<INItemClass, On<INItemClass.itemClassID, Equal<InventoryItem.itemClassID>>>,
        //    Where <InventoryItem.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>>.Select(cache.Graph);

        //InventoryItem item = PXSelectJoin<InventoryItem,
        //    InnerJoin
        //    <INItemClass, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>,
        //    Where<SOLineSplit.pOType, Equal<Current<POOrder.orderType>>,
        //    And<SOLineSplit.pONbr, Equal<Current<POOrder.orderNbr>>,
        //    And<SOOrder.customerID, NotEqual<Required<SOOrder.customerID>>>>>>.Select(this, e.NewValue);

        //if (stkDetExt.UsrCostingType == CostingType.MarketCost && baseItem.ItemClassID == CommodityClass.value) //Item Is Priced Against the market 
        //{ 


        //}
        //POVendorInventory pOVendorInventory = (POVendorInventory)DefaultVendorItem.Select();
        //decimal? vendorUnitCost = null;
        //vendorUnitCost = APVendorPriceMaint.CalculateUnitCost(cache, pOVendorInventory.VendorID, pOVendorInventory.VendorLocationID , pOVendorInventory.InventoryID, null, row.UOM, row.DfltCompQty , System.DateTime.Today, 0.00m, true);


        //Call Pricing Function

        //Call Header/Rollup Refresh

        /*if (e.Row != null && row.CompInventoryID != null && e.Row.UOM != null)
        {
            foreach (PXResult<INKitSpecStkDet, InventoryItem> res in SpecComponents.Select())
            {

                INComponentTran tran = new INComponentTran();
                tran.DocType = spec. cache.Current.DocType;
                tran.TranType = tran.DocType == INDocType.Disassembly ? INTranType.Disassembly : INTranType.Assembly;
                tran.InvtMult = GetInvtMult(tran);
                tran.InventoryID = spec.CompInventoryID;
                tran = PXCache<INComponentTran>.CreateCopy(Components.Insert(tran));

                tran.SubItemID = spec.CompSubItemID;
                if (tran.DocType == INDocType.Disassembly)
                {
                    tran.Qty = spec.DfltCompQty * numberOfKits * spec.DisassemblyCoeff;
                }
                else
                {
                    tran.Qty = spec.DfltCompQty * numberOfKits;
                }
                tran.UOM = spec.UOM;
                tran.SiteID = Document.Current.SiteID;

                if (Document.Current.DocType == INDocType.Disassembly)
                {
                    tran.LocationID = Document.Current.LocationID;
                }

                tran = Components.Update(tran);
            }


            (var itemSite, var item) = res;
            if (item != null && item.InventoryID != null)
                e.NewValue = PO.POItemCostManager.ConvertUOM(this, item, item.BaseUnit, itemSite.TranUnitCost.GetValueOrDefault(), e.Row.UOM);
        }
    }*/

        //protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCIStarINKitSpecStkDetExt.usrCostRollupType> e)
        //{

        //    e.NewValue = CostRollupType.Other;
        //    INKitSpecStkDet row = e.Row as INKitSpecStkDet;
        //    if (row == null)
        //        return;
        //    InventoryItem item = InventoryItem.PK.Find(cache.Graph, row.KitInventoryID);
        //    ASCIStarINKitSpecStkDetExt ext = item.GetExtension<ASCIStarINKitSpecStkDetExt>();
        //    e.NewValue = ext.UsrCostRollupType;

        //}

        //protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCIStarINKitSpecStkDetExt.usrCostingType> e)
        protected void INKitSpecStkDet_UsrCostingType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e, PXFieldDefaulting InvokeBaseHandler)
        {
            MethodBase m = MethodBase.GetCurrentMethod();
            PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

            e.NewValue = ASCIStarCostingType.StandardCost;
            INKitSpecStkDet row = e.Row as INKitSpecStkDet;
            if (row == null) return;
            InventoryItem baseItem = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<INKitSpecStkDet.compInventoryID>>>>.Select(cache.Graph);
            if (baseItem == null) return;
            ASCIStarINInventoryItemExt extItem = baseItem.GetExtension<ASCIStarINInventoryItemExt>();
            if (extItem == null || extItem.UsrCostingType == null) return;

            e.NewValue = extItem.UsrCostingType;

        }
        protected void INKitSpecStkDet_UsrCostingType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        {
            var row = (INKitSpecStkDet)e.Row;
            if (row == null) return;

            ASCIStarINKitSpecStkDetExt ext = cache.GetExtension<ASCIStarINKitSpecStkDetExt>(row);
            PXTrace.WriteInformation($"ext.UsrCostingType:{ext.UsrCostingType}");
            if (/*ext.UsrCostingType == CostingType.PercentageCost*/ false)
            {
                PXTrace.WriteInformation($"Disabling Cost");
                ext.UsrUnitCost = 0.00m;
                PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecStkDetExt.usrUnitCost>(cache, e.Row, false);
                ext.UsrUnitPct = 0.00m;
                PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecStkDetExt.usrUnitPct>(cache, e.Row, true);
            }
            else
            {
                PXTrace.WriteInformation($"Enabling Percentage");
                PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecStkDetExt.usrUnitCost>(cache, e.Row, true);
                PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecStkDetExt.usrUnitPct>(cache, e.Row, false);
            }
        }

        //        protected virtual void _(Events.FieldUpdated<INKitSpecStkDet, ASCIStarINKitSpecStkDetExt.usrUnitCost> e, PXFieldUpdated InvokeBaseHandler)
        protected void INKitSpecStkDet_UsrUnitCost_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        {
            MethodBase m = MethodBase.GetCurrentMethod();
            PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

            var row = (INKitSpecStkDet)e.Row;
            if (row == null) return;
            ASCIStarINKitSpecStkDetExt ext = cache.GetExtension<ASCIStarINKitSpecStkDetExt>(row);

            ext.UsrExtCost = ext.UsrUnitCost * row.DfltCompQty;
        }

        //protected virtual void _(Events.FieldUpdated<INKitSpecStkDet, ASCIStarINKitSpecStkDetExt.usrExtCost> e, PXFieldUpdated InvokeBaseHandler)
        protected void INKitSpecStkDet_UsrExtCost_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        {
            MethodBase m = MethodBase.GetCurrentMethod();
            PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

            var row = (INKitSpecStkDet)e.Row;
            if (row == null) return;

            InventoryItem baseItem = InventoryItem.PK.Find(cache.Graph, row.CompInventoryID);
            if (baseItem == null) return;
            ASCIStarINInventoryItemExt ext = baseItem.GetExtension<ASCIStarINInventoryItemExt>();
            if (ext == null) return;
            decimal Rollup = 0.0m;

            if (e.Row != null)
            {
                PXTrace.WriteInformation($"{ext.UsrCostRollupType}");
                foreach (PXResult<INKitSpecStkDet> rec in
                    SpecComponents.Select(cache))
                {
                    INKitSpecStkDet StkDet = (INKitSpecStkDet)rec;

                    InventoryItem compItem = InventoryItem.PK.Find(cache.Graph, StkDet.CompInventoryID);
                    ASCIStarINInventoryItemExt compItemExt = compItem.GetExtension<ASCIStarINInventoryItemExt>();

                    if (compItemExt.UsrCostRollupType == ext.UsrCostRollupType)
                    {
                        ASCIStarINKitSpecStkDetExt StkDetExt = StkDet.GetExtension<ASCIStarINKitSpecStkDetExt>();
                        Rollup += StkDetExt.UsrExtCost.Value;
                    }
                    PXTrace.WriteInformation($"Rollup:{Rollup}");
                }

                InventoryItem item = InventoryItemHdr.SelectSingle();
                ext = item.GetExtension<ASCIStarINInventoryItemExt>();
                /*Commodity, Fabrication, Labor, Handling, Shipping, Duty, Other*/
                switch (ext.UsrCostRollupType)
                {
                    case ASCIStarCostRollupType.Commodity:
                        ext.UsrCommodityCost = Rollup;
                        break;
                    case ASCIStarCostRollupType.Fabrication:
                        ext.UsrFabricationCost = Rollup;
                        break;
                    case ASCIStarCostRollupType.Materials:
                        ext.UsrOtherMaterialCost = Rollup;
                        break;
                    case ASCIStarCostRollupType.Packaging:
                        ext.UsrPackagingCost = Rollup;
                        break;
                    case ASCIStarCostRollupType.Labor:
                        ext.UsrLaborCost = Rollup;
                        break;
                    case ASCIStarCostRollupType.Handling:
                        ext.UsrHandlingCost = Rollup;
                        break;
                    case ASCIStarCostRollupType.Duty:
                        ext.UsrDutyCost = Rollup;
                        break;
                    case ASCIStarCostRollupType.Other:
                        ext.UsrOtherCost = Rollup;
                        break;
                    default: /* CostingType.StandardCost */
                        break;
                }
            }
        }
        protected void INKitSpecNonStkDet_UsrExtCost_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        {
            MethodBase m = MethodBase.GetCurrentMethod();
            PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

            var row = (INKitSpecNonStkDet)e.Row;
            if (row == null) return;

            InventoryItem baseItem = InventoryItem.PK.Find(cache.Graph, row.CompInventoryID);
            if (baseItem == null) return;
            ASCIStarINInventoryItemExt ext = baseItem.GetExtension<ASCIStarINInventoryItemExt>();
            if (ext == null) return;

            decimal Rollup = 0.0m;

            if (e.Row != null)
            {

                PXTrace.WriteInformation($"{ext.UsrCostRollupType}");
                foreach (PXResult<INKitSpecNonStkDet> rec in
                    SpecOverhead.Select(cache))
                {
                    INKitSpecNonStkDet StkDet = (INKitSpecNonStkDet)rec;

                    InventoryItem compItem = InventoryItem.PK.Find(cache.Graph, StkDet.CompInventoryID);
                    ASCIStarINInventoryItemExt compItemExt = compItem.GetExtension<ASCIStarINInventoryItemExt>();

                    if (compItemExt.UsrCostRollupType == ext.UsrCostRollupType)
                    {
                        ASCIStarINKitSpecNonStkDetExt StkDetExt = StkDet.GetExtension<ASCIStarINKitSpecNonStkDetExt>();
                        Rollup += StkDetExt.UsrExtCost.Value;
                    }
                    PXTrace.WriteInformation($"Rollup:{Rollup}");
                }

                /*Commodity, Fabrication, Labor, Handling, Shipping, Duty, Other*/
                switch (ext.UsrCostRollupType)
                {
                    case ASCIStarCostRollupType.Commodity:
                        ext.UsrCommodityCost = Rollup;
                        cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrCommodityCost>(e.Row, null);
                        break;
                    case ASCIStarCostRollupType.Fabrication:
                        ext.UsrFabricationCost = Rollup;
                        cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrFabricationCost>(e.Row, null);
                        break;
                    case ASCIStarCostRollupType.Materials:
                        ext.UsrOtherMaterialCost = Rollup;
                        cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrOtherMaterialCost>(e.Row, null);
                        break;
                    case ASCIStarCostRollupType.Packaging:
                        ext.UsrPackagingCost = Rollup;
                        cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrPackagingCost>(e.Row, null);
                        break;
                    case ASCIStarCostRollupType.Labor:
                        ext.UsrLaborCost = Rollup;
                        cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrLaborCost>(e.Row, null);
                        break;
                    case ASCIStarCostRollupType.Handling:
                        ext.UsrHandlingCost = Rollup;
                        cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrHandlingCost>(e.Row, null);
                        break;
                    case ASCIStarCostRollupType.Duty:
                        ext.UsrDutyCost = Rollup;
                        cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrDutyCost>(e.Row, null);
                        break;
                    case ASCIStarCostRollupType.Other:
                        ext.UsrOtherCost = Rollup;
                        cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrOtherCost>(e.Row, null);
                        break;
                    default: /* CostingType.StandardCost */
                        break;
                }

            }
        }
        protected void INKitSpecNonStkDet_UsrCostingType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        {
            MethodBase m = MethodBase.GetCurrentMethod();
            PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

            if (InvokeBaseHandler != null)
                InvokeBaseHandler(cache, e);
            var row = (INKitSpecNonStkDet)e.Row;
            if (row == null) return;

            ASCIStarINKitSpecNonStkDetExt ext = row.GetExtension<ASCIStarINKitSpecNonStkDetExt>();
            PXTrace.WriteInformation($"e.NewValue:{ext.UsrCostingType}");
            if (ASCIStarCostingType.WeightCost == (string)ext.UsrCostingType)
            {
                row.DfltCompQty = 1.00m;
                decimal qty = 0.00m;
                row.UOM = "GRAM";
                foreach (INKitSpecStkDet r in
                    SpecCommodity.Select(cache))
                {
                    InventoryItem item = InventoryItem.PK.Find(cache.Graph, r.CompInventoryID);
                    PXTrace.WriteInformation($"item:{item.InventoryCD}");
                    if (r.UOM == "DWT")
                        qty += ((r.DfltCompQty ?? 0.00m) * 1.555170m);
                    else
                        qty += (r.DfltCompQty ?? 0.00m);

                }
                row.DfltCompQty = qty;
            }
            else
                row.UOM = "EA";


        }
        protected void INKitSpecNonStkDet_UsrUnitCost_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        {
            MethodBase m = MethodBase.GetCurrentMethod();
            PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);


            if (InvokeBaseHandler != null)
                InvokeBaseHandler(cache, e);
            var row = (INKitSpecNonStkDet)e.Row;
            if (row == null) return;
            ASCIStarINKitSpecNonStkDetExt ext = cache.GetExtension<ASCIStarINKitSpecNonStkDetExt>(row);

            ext.UsrExtCost = ext.UsrUnitCost * row.DfltCompQty;
            //if (row.UOM == "GRAM")
            //{
            //    ext.UsrExtCost = ext.UsrExtCost / 1.555m;
            //}

            decimal Rollup = 0.0m;

            if (e.Row != null)
            {

                foreach (PXResult<INKitSpecNonStkDet, InventoryItem> r in
                    SpecOverhead.Select(cache))
                {

                    ASCIStarINKitSpecNonStkDetExt stkDetExt = cache.GetExtension<ASCIStarINKitSpecNonStkDetExt>(r);
                    if (stkDetExt.UsrCostRollupType == ext.UsrCostRollupType)
                        Rollup = Rollup + stkDetExt.UsrExtCost.Value;
                    PXTrace.WriteInformation($"Rollup:{Rollup}");
                }
                PXTrace.WriteInformation($"{ext.UsrCostRollupType}");

                /*Commodity, Fabrication, Labor, Handling, Shipping, Duty, Other*/
                switch (ext.UsrCostRollupType ?? ASCIStarCostingType.StandardCost)
                {
                    case ASCIStarCostRollupType.Commodity:
                        cache.SetValueExt<ASCIStarINInventoryItemExt.usrCommodityCost>(cache.Current, Rollup);
                        //cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrCommodityCost>(e.Row, null);
                        break;
                    case ASCIStarCostRollupType.Materials:
                        cache.SetValueExt<ASCIStarINInventoryItemExt.usrOtherMaterialCost>(cache.Current, Rollup);
                        //cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrOtherMaterialCost>(e.Row, null);
                        break;
                    case ASCIStarCostRollupType.Packaging:
                        cache.SetValueExt<ASCIStarINInventoryItemExt.usrPackagingCost>(cache.Current, Rollup);
                        //cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrPackagingCost>(e.Row, null);
                        break;
                    case ASCIStarCostRollupType.Fabrication:
                        cache.SetValueExt<ASCIStarINInventoryItemExt.usrFabricationCost>(cache.Current, Rollup);
                        //cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrFabricationCost>(e.Row, null);

                        break;
                    case ASCIStarCostRollupType.Labor:
                        cache.SetValueExt<ASCIStarINInventoryItemExt.usrLaborCost>(cache.Current, Rollup);
                        //cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrLaborCost>(e.Row, null);

                        break;
                    case ASCIStarCostRollupType.Handling:
                        cache.SetValueExt<ASCIStarINInventoryItemExt.usrHandlingCost>(cache.Current, Rollup);
                        //cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrHandlingCost>(e.Row, null);

                        break;
                    case ASCIStarCostRollupType.Duty:
                        cache.SetValueExt<ASCIStarINInventoryItemExt.usrDutyCost>(cache.Current, Rollup);
                        //cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrDutyCost>(e.Row, null);
                        break;
                    case ASCIStarCostRollupType.Other:
                        cache.SetValueExt<ASCIStarINInventoryItemExt.usrOtherCost>(cache.Current, Rollup);
                        //cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrOtherCost>(e.Row, null);
                        break;
                    default: /* CostingType.StandardCost */
                        break;
                }

            }
        }
        protected void INKitSpecStkDet_UsrUnitPct_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        {
            MethodBase m = MethodBase.GetCurrentMethod();
            PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

            if (InvokeBaseHandler != null)
                InvokeBaseHandler(cache, e);
            var row = (INKitSpecStkDet)e.Row;
            if (row == null) return;
            ASCIStarINKitSpecStkDetExt ext = cache.GetExtension<ASCIStarINKitSpecStkDetExt>(row);
            if (ext.UsrUnitPct != 0.00m)
            {

            }

        }

        protected void INKitSpecStkDet_InventoryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        {
            MethodBase m = MethodBase.GetCurrentMethod();
            PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

            if (InvokeBaseHandler != null)
                InvokeBaseHandler(cache, e);
            var row = (INKitSpecStkDet)e.Row;
            if (row == null) return;
            InventoryItem item = InventoryItem.PK.Find(cache.Graph, row.CompInventoryID);
            InventoryItem kit = InventoryItem.PK.Find(cache.Graph, row.KitInventoryID);
            PXResultset<INKitSpecHdr> iNKitSpecHdr = PXSelect<INKitSpecHdr, Where<INKitSpecHdr.kitInventoryID, Equal<Current<INKitSpecStkDet.kitInventoryID>>>>.SelectWindowed(cache.Graph, 0, 2);

            ASCIStarINInventoryItemExt ext = item.GetExtension<ASCIStarINInventoryItemExt>();
            PXTrace.WriteInformation($"Kit:{kit.InventoryCD}");
            PXTrace.WriteInformation($"Component Item:{item.InventoryCD}");
            ASCIStarMarketCostHelper.JewelryCost itemCost =
                new ASCIStarMarketCostHelper.JewelryCost(cache.Graph, item, 0.00m);

            ext.UsrUnitCost = 0.000000m;
            ext.UsrCostingType = itemCost.costingType;
            ext.UsrCostRollupType = itemCost.costRollupType;
            //ext.UsrExtCost = ext.UsrUnitCost * row.DfltCompQty;

            INItemClass itemClass = INItemClass.PK.Find(cache.Graph, item.ItemClassID);
            if (itemClass.ItemClassCD == CommodityClass.value)
            {
                ext = item.GetExtension<ASCIStarINInventoryItemExt>();
                if (ext == null)
                    return;
                ext.UsrContractWgt = row.DfltCompQty;

            }


            //.UsrMaterialCost = row.OrderQty * ext.UsrWeight * ext.UsrRatePerGram;

        }
        //protected void ASCIStarINKitSpecHdrAttribute_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e, PXRowUpdated InvokeBaseHandler)
        //{
        //    if (InvokeBaseHandler != null)
        //        InvokeBaseHandler(cache, e);
        //    var row = (ASCIStarINKitSpecHdrAttribute)e.Row;
        //    if (row == null) return;
        //    string ival = row.FreightType;
        //    switch (ival)
        //    {
        //        case "1":
        //            row.TotalFreightCost = row.FreightCost;
        //            break;
        //        case "2":
        //            break;
        //        default:
        //            break;
        //    }
        //    InventoryItem item = InventoryItem.PK.Find(Base, Base.Hdr.Current.KitInventoryID);
        //    if (item != null)
        //    {
        //        InfoInventoryItemAttributeExtNV extNV = item.GetExtension<InfoInventoryItemAttributeExtNV>();
        //        row.ProductWt = extNV.ProductWeight;
        //        row.FinishedWt = extNV.UsrFinishedItemWt;
        //    }
        //}

        ////protected void ASCIStarINKitSpecHdrAttribute_RowSelected(PXCache cache, PXRowSelectedEventArgs e, PXRowSelected InvokeBaseHandler)
        ////{
        ////    if (InvokeBaseHandler != null)
        ////        InvokeBaseHandler(cache, e);
        ////    var row = (ASCIStarINKitSpecHdrAttribute)e.Row;
        ////    string ival = row.FreightType;
        ////    PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecHdrAttribute.freightCost>(cache, row, false);
        ////    PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecHdrAttribute.freightPercent>(cache, row, false);
        ////    switch (ival)
        ////    {
        ////        case "1": PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecHdrAttribute.freightCost>(cache, row, true); break;
        ////        case "2": PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecHdrAttribute.freightPercent>(cache, row, true); break;
        ////        default:
        ////            PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecHdrAttribute.freightCost>(cache, row, false);
        ////            PXUIFieldAttribute.SetVisible<ASCIStarINKitSpecHdrAttribute.freightPercent>(cache, row, false); break;
        ////    }
        ////}
        ////protected void ASCIStarINKitSpecHdrAttribute_RowInserting(PXCache cache, PXRowInsertingEventArgs e, PXRowInserting InvokeBaseHandler)
        ////{
        ////    if (InvokeBaseHandler != null)
        ////        InvokeBaseHandler(cache, e);
        ////    var row = (ASCIStarINKitSpecHdrAttribute)e.Row;
        ////    if (row == null) return;
        ////    if (string.IsNullOrEmpty(row.FreightType))
        ////        row.FreightType = "1";
        ////}
        ////protected void ASCIStarINKitSpecHdrAttribute_RowInserted(PXCache cache, PXRowInsertedEventArgs e, PXRowInserted InvokeBaseHandler)
        ////{
        ////    if (InvokeBaseHandler != null)
        ////        InvokeBaseHandler(cache, e);
        ////    var row = (ASCIStarINKitSpecHdrAttribute)e.Row;
        ////    if (row == null) return;
        ////    if (string.IsNullOrEmpty(row.FreightType))
        ////        row.FreightType = "1";
        ////    InventoryItem item = InventoryItem.PK.Find(Base, Base.Hdr.Current.KitInventoryID);
        ////    if (item != null)
        ////    {
        ////        InfoInventoryItemAttributeExtNV extNV = item.GetExtension<InfoInventoryItemAttributeExtNV>();
        ////        row.ProductWt = extNV.ProductWeight;
        ////        row.FinishedWt = extNV.UsrFinishedItemWt;
        ////    }
        ////}
        ////protected void ASCIStarINKitSpecHdrAttribute_FreightType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e, PXFieldDefaulting InvokeBaseHandler)
        ////{
        ////    if (InvokeBaseHandler != null)
        ////        InvokeBaseHandler(cache, e);
        ////    var row = (ASCIStarINKitSpecHdrAttribute)e.Row;
        ////    if (row == null) return;
        ////    e.NewValue = "1";
        ////}


        protected void INKitSpecStkDet_RowInserted(PXCache cache, PXRowInsertedEventArgs e, PXRowInserted InvokeBaseHandler)
        {
            MethodBase m = MethodBase.GetCurrentMethod();
            PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

            if (InvokeBaseHandler != null)
                InvokeBaseHandler(cache, e);
            var row = (INKitSpecStkDet)e.Row;
            if (row == null) return;
            PXTrace.WriteInformation("Row Exists");

            ASCIStarINKitSpecStkDetExt ext = cache.GetExtension<ASCIStarINKitSpecStkDetExt>(row);
            PXTrace.WriteInformation("ASCIStarINKitSpecStkDetExt Exists");
            InventoryItem item = InventoryItem.PK.Find(cache.Graph, row.CompInventoryID);
            INKitSpecHdr kitItem = INKitSpecHdr.PK.Find(cache.Graph, row.KitInventoryID, row.RevisionID);

            POVendorInventory vendorItem = PXSelect<POVendorInventory, Where<POVendorInventory.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>, And<POVendorInventory.isDefault, Equal<True>>>>.Select(cache.Graph);
            int? vendorID = null;
            if (vendorItem != null && vendorItem.VendorID != null)
            {
                PXTrace.WriteInformation($"Found VendorID: {vendorItem.VendorID}");
                vendorID = vendorItem.VendorID;
            }
            if (item != null)
            {
                ASCIStarMarketCostHelper.JewelryCost costHelper = new ASCIStarMarketCostHelper.JewelryCost(cache.Graph, item, 0.000000m);
                //APVendorPrice price = costHelper.GetCommodityPrice(cache, row.CompInventoryID, vendorID, null);
                //if (price.InventoryID == null)
                //    PXTrace.WriteInformation("No Price Returned");

            }

            //switch (ext.UsrCostingType ?? CostingType.StandardCost)
            //{
            //    case CostingType.MarketCost:
            //        PXTrace.WriteInformation("Costing is Market");
            //        break;
            //    case CostingType.ContractCost:
            //        PXTrace.WriteInformation("Costing is Fixed");
            //        break;
            //    case CostingType.WeightCost:
            //        PXTrace.WriteInformation("Costing is Weighted");
            //        break;
            //    case CostingType.PercentageCost:
            //        PXTrace.WriteInformation("Costing is Percentage");
            //        break;

            //    default: /* CostingType.StandardCost */
            //        break;
            //}

        }

        //protected void INKitSpecStkDet_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e, PXRowUpdated InvokeBaseHandler)
        //{
        //    MethodBase m = MethodBase.GetCurrentMethod();
        //    PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

        //    if (InvokeBaseHandler != null)
        //        InvokeBaseHandler(cache, e);
        //    var row = (INKitSpecStkDet)e.Row;
        //    ASCIStarINKitSpecStkDetExt ext = cache.GetExtension<ASCIStarINKitSpecStkDetExt>(row);
        //    if (row == null || ext == null) return;

        //    InventoryItem item = InventoryItem.PK.Find(Base, row.CompInventoryID);
        //    INItemClass itemClass = INItemClass.PK.Find(Base, item.ItemClassID);
        //    if (itemClass.ItemClassCD.ToUpper() == "COMMODITY")
        //    {

        //        //(Base.Hdr.Current.GetExtension<ASCIStarINKitSpecHdrExt>() as ASCIStarINKitSpecHdrExt).UsrCommodityID;
        //    }
        //    InvokeBaseHandler(cache, e);
        //}

        //protected virtual void INKitSpecStkDet_UsrCostRollupType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e, PXFieldDefaulting InvokeBaseHandler)
        //{
        //    if (InvokeBaseHandler != null)
        //        InvokeBaseHandler(cache, e);

        //    INKitSpecStkDet row = e.Row as INKitSpecStkDet;
        //    if (row != null)
        //    {
        //        InventoryItem item = InventoryItem.PK.Find(cache.Graph, row.CompInventoryID);
        //        ASCIStarINInventoryItemExt itemExt = item.GetExtension<ASCIStarINInventoryItemExt>();
        //        if (item == null || itemExt == null)
        //        {
        //            e.NewValue = CostRollupType.Other;
        //            return;
        //        }

        //        e.NewValue = itemExt.UsrCostRollupType ?? CostRollupType.Other;
        //        PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecStkDetExt.UsrCostRollupType>(cache, row, true);
        //    }
        //}

        //protected virtual void INKitSpecNonStkDet_UsrCostRollupType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e, PXFieldDefaulting InvokeBaseHandler)
        //{
        //    if (InvokeBaseHandler != null)
        //        InvokeBaseHandler(cache, e);

        //    INKitSpecNonStkDet row = e.Row as INKitSpecNonStkDet;
        //    if (row != null)
        //    {
        //        InventoryItem item = InventoryItem.PK.Find(cache.Graph, row.CompInventoryID);
        //        ASCIStarINInventoryItemExt itemExt = item.GetExtension<ASCIStarINInventoryItemExt>();
        //        if (item == null || itemExt == null)
        //        {
        //            e.NewValue = CostRollupType.Other;
        //            return;
        //        }

        //        e.NewValue = itemExt.UsrCostRollupType ?? CostRollupType.Other;
        //        PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecNonStkDetExt.UsrCostRollupType>(cache, row, true);
        //    }
        //}

        //public void INKitSpecStkDet_UsrCostRollupType_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
        //{
        //    INKitSpecStkDet row = (INKitSpecStkDet)e.Row;
        //    if (row == null)
        //        return;
        //    InventoryItem item = BaseItem.Select(cache, (int)row.CompInventoryID);
        //    ASCIStarINInventoryItemExt itemExt = item.GetExtension<ASCIStarINInventoryItemExt>();
        //    if (itemExt == null)
        //        return;

        //    e.ReturnValue = itemExt.UsrCostRollupType;
        //}
        //public void INKitSpecNonStkDet_UsrCostRollupType_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
        //{
        //    INKitSpecNonStkDet row = (INKitSpecNonStkDet)e.Row;
        //    if (row == null)
        //        return;
        //    InventoryItem item = BaseItem.Select(cache, (int)row.CompInventoryID);
        //    ASCIStarINInventoryItemExt itemExt = item.GetExtension<ASCIStarINInventoryItemExt>();
        //    if (itemExt == null)
        //        return;

        //    e.ReturnValue = itemExt.UsrCostRollupType;
        //}

        //protected virtual void INKitSpecStkDet_UsrCostingType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e, PXFieldDefaulting InvokeBaseHandler)
        //{
        //    if (InvokeBaseHandler != null)
        //        InvokeBaseHandler(cache, e);

        //    INKitSpecStkDet row = e.Row as INKitSpecStkDet;
        //    if (row != null)
        //    {
        //        InventoryItem item = InventoryItem.PK.Find(cache.Graph, row.CompInventoryID);
        //        if (item == null)
        //        {
        //            e.NewValue = CostingType.StandardCost;
        //            return;
        //        }
        //        ASCIStarINInventoryItemExt itemExt = item.GetExtension<ASCIStarINInventoryItemExt>();
        //        e.NewValue = itemExt.UsrCostingType ?? CostingType.StandardCost;
        //        PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecStkDetExt.usrCostingType>(cache, row, true);
        //    }
        //}
        //protected virtual void INKitSpecStkDet_UsrCostingType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        //{
        //    if (InvokeBaseHandler != null)
        //        InvokeBaseHandler(cache, e);

        //    INKitSpecStkDet row = e.Row as INKitSpecStkDet;

        //    if (row != null)
        //    {
        //        ASCIStarINKitSpecStkDetExt ext = cache.GetExtension<ASCIStarINKitSpecStkDetExt>(row);
        //        if (ext != null)
        //        {
        //            bool IsPct = ext.UsrCostingType == CostingType.PercentageCost;
        //            if (IsPct)
        //                ext.UsrUnitCost = 0.00m;
        //            else
        //                ext.UsrUnitPct = 0.00m;
        //            PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecStkDetExt.usrUnitCost>(cache, row, !IsPct);
        //            PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecStkDetExt.usrUnitPct>(cache, row, IsPct);


        //        }
        //    }
        //}

        protected virtual void INKitSpecNonStkDet_UsrCostingType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e, PXFieldDefaulting InvokeBaseHandler)
        {
            MethodBase m = MethodBase.GetCurrentMethod();
            PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

            if (InvokeBaseHandler != null)
                InvokeBaseHandler(cache, e);

            INKitSpecNonStkDet row = (INKitSpecNonStkDet)e.Row;
            if (row == null)
                return;
            InventoryItem item = InventoryItem.PK.Find(cache.Graph, row.CompInventoryID);
            if (item == null)
            {
                e.NewValue = ASCIStarCostingType.StandardCost;
                return;
            }
            ASCIStarINInventoryItemExt itemExt = item.GetExtension<ASCIStarINInventoryItemExt>();
            e.NewValue = itemExt.UsrCostingType ?? ASCIStarCostingType.StandardCost;
            PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecNonStkDetExt.usrCostingType>(cache, row, true);

        }
        //protected virtual void INKitSpecNonStkDet_UsrCostingType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        //{
        //    if (InvokeBaseHandler != null)
        //        InvokeBaseHandler(cache, e);

        //    INKitSpecNonStkDet row = e.Row as INKitSpecNonStkDet;

        //    if (row != null)
        //    {
        //        ASCIStarINKitSpecNonStkDetExt ext = cache.GetExtension<ASCIStarINKitSpecNonStkDetExt>(row);
        //        if (ext != null)
        //        {
        //            bool IsPct = ext.UsrCostingType == CostingType.PercentageCost;
        //            if (IsPct)
        //                ext.UsrUnitCost = 0.00m;
        //            else
        //                ext.UsrUnitPct = 0.00m;
        //            PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecNonStkDetExt.usrUnitCost>(cache, row, !IsPct);
        //            PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecNonStkDetExt.usrUnitPct>(cache, row, IsPct);


        //        }
        //    }
        //}
        ////protected virtual void INKitSpecNonStkDet_UsrCostRollupType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e, PXFieldDefaulting InvokeBaseHandler)
        ////{
        ////    if (InvokeBaseHandler != null)
        ////        InvokeBaseHandler(cache, e);

        ////    INKitSpecNonStkDet row = e.Row as INKitSpecNonStkDet;
        ////    if (row != null)
        ////    {
        ////        InventoryItem item = InventoryItem.PK.Find(cache.Graph, row.CompInventoryID);
        ////        if (item == null)
        ////        {
        ////            e.NewValue = CostRollupType.Other;
        ////            return;
        ////        }
        ////        ASCIStarINInventoryItemExt itemExt = item.GetExtension<ASCIStarINInventoryItemExt>();
        ////        e.NewValue = itemExt.UsrCostingType ?? CostingType.StandardCost;
        ////        PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecNonStkDetExt.usrCostingType>(cache, row, true);
        ////    }
        ////}
        //protected virtual void INKitSpecNonStkDet_CompInventoryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        //{
        //    if (InvokeBaseHandler != null)
        //        InvokeBaseHandler(cache, e);

        //    INKitSpecNonStkDet row = e.Row as INKitSpecNonStkDet;
        //    if (row != null)
        //    {
        //        InventoryItem item = InventoryItem.PK.Find(Base, row.CompInventoryID);
        //        if (item != null)
        //        {
        //            ASCIStarINInventoryItemExt ext = item.GetExtension<ASCIStarINInventoryItemExt>();
        //            ext.UsrCostingType = ext.UsrCostingType ?? CostingType.StandardCost;
        //        }
        //    }
        //}


        #endregion Event Handlers

        #region Action
        public PXAction<INKitSpecHdr> createitem;
        [PXUIField(DisplayName = "Create Production Item", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable createItem(PXAdapter adapter)
        {
            return adapter.Get();
        }
        #endregion


        #region Helpers Methods

        private void CopyJewelryItemFields(INKitSpecHdr kitSpecHdr)
        {
            var jewelItem = SelectFrom<ASCIStarINJewelryItem>.Where<ASCIStarINJewelryItem.inventoryID.IsEqual<P.AsInt>>.View.Select(this.Base, kitSpecHdr.KitInventoryID)?.TopFirst;

            if (jewelItem == null) return;

            ASCIStarINKitSpecJewelryItem jewelryKitItem = new ASCIStarINKitSpecJewelryItem()
            {
                KitInventoryID = kitSpecHdr.KitInventoryID,
                RevisionID = kitSpecHdr.RevisionID,
                ShortDesc = jewelItem.ShortDesc,
                LongDesc = jewelItem.LongDesc,
                StyleStatus = jewelItem.StyleStatus,
                CustomerCode = jewelItem.CustomerCode,
                InvCategory = jewelItem?.InvCategory,
                ItemType = jewelItem?.ItemType,
                ItemSubType = jewelItem?.ItemSubType,
                Collection = jewelItem.Collection,
                MetalType = jewelItem?.MetalType,
                MetalNote = jewelItem.MetalNote,
                MetalColor = jewelItem?.MetalColor,
                Plating = jewelItem.Plating,
                Finishes = jewelItem.Finishes,
                VendorMaker = jewelItem.VendorMaker,
                OrgCountry = jewelItem?.OrgCountry,
                StoneType = jewelItem.StoneType,
                WebNotesComment = jewelItem.WebNotesComment,
                StoneComment = jewelItem.StoneComment,
                StoneColor = jewelItem?.StoneColor,
                StoneShape = jewelItem.StoneShape,
                StoneCreation = jewelItem.StoneCreation,
                GemstoneTreatment = jewelItem.GemstoneTreatment,
                SettingType = jewelItem.SettingType,
                Findings = jewelItem.Findings,
                FindingsSubType = jewelItem.FindingsSubType,
                ChainType = jewelItem.ChainType,
                RingLength = jewelItem.RingLength,
                RingSize = jewelItem.RingSize,
                OD = jewelItem.OD,
            };

            this.JewelryItemView.Insert(jewelryKitItem);
        }


        private void SetVisibleRevisionID()
        {
            var inSetup = SelectFrom<INSetup>.View.Select(this.Base)?.TopFirst;
            var inSetupExt = inSetup?.GetExtension<ASCIStarINSetupExt>();
            PXUIFieldAttribute.SetVisible<INKitSpecHdr.revisionID>(this.Base.Hdr.Cache, this.Base.Hdr.Current, inSetupExt?.UsrIsPDSTenant == true);
        }
        #endregion Helpers Methods
    }
}