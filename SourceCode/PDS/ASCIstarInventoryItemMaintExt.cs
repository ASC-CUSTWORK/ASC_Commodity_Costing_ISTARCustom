using System;
using System.Collections;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.PO;
using PX.Objects.IN;
using ASCISTARCustom.Inventory.DAC;
using ASCISTARCustom.PDS.CacheExt;
using System.Collections.Generic;
using PX.Api.Models;
using PX.Data.BQL;
using PX.Common;
using PX.Objects.CR.Standalone;
using PX.CS;
using System.Linq;
using ASCISTARCustom.Inventory.Descriptor.Constants;

namespace ASCISTARCustom
{
    public class ASCIstarInventoryItemMaintExt : PXGraphExtension<InventoryItemMaint>
    {
        public static bool IsActive() => true;

        #region Selects

        public PXSelect<INKitSpecHdr, Where<INKitSpecHdr.kitInventoryID, Equal<Current<InventoryItem.inventoryID>>>> KitCostRollup;

        //public PXSelect<POVendorInventory,
        //    Where<POVendorInventory.inventoryID, Equal<Current<InventoryItem.inventoryID>>,
        //    And<POVendorInventory.vendorID, Equal<Current<InventoryItemCurySettings.preferredVendorID>>,
        //    And<POVendorInventory.vendorID, Equal<Current<InventoryItemCurySettings.preferredVendorLocationID>>>>>> DefaultVendorItem;

        //public PXSelect<ASCIStarItemCostRollup, Where<ASCIStarItemCostRollup.inventoryID, Equal<Current<InventoryItem.inventoryID>>, And<ASCIStarItemCostRollup.bAccountID, NotEqual<CompanyBAccount.bAccountID>>>> VendorCostRollup;

        [PXCopyPasteHiddenFields(typeof(POVendorInventory.isDefault))]
        public POVendorInventorySelect<POVendorInventory,
            InnerJoin<Vendor, On<BqlOperand<Vendor.bAccountID, IBqlInt>.IsEqual<POVendorInventory.vendorID>>,
            LeftJoin<Location, On<BqlChainableConditionBase<TypeArrayOf<IBqlBinary>
                .FilledWith<And<Compare<Location.bAccountID, Equal<POVendorInventory.vendorID>>>>>
                .And<BqlOperand<Location.locationID, IBqlInt>.IsEqual<POVendorInventory.vendorLocationID>>>>>,
            Where<POVendorInventory.inventoryID, Equal<Current<InventoryItem.inventoryID>>,
                And<Where<Vendor.baseCuryID, Equal<Current<AccessInfo.baseCuryID>>, Or<Vendor.baseCuryID, IsNull>>>>, InventoryItem> VendorItems;

        [PXFilterable]
        [PXCopyPasteHiddenView(IsHidden = true)]
        public SelectFrom<ASCIStarINCompliance>.Where<ASCIStarINCompliance.inventoryID.IsEqual<InventoryItem.inventoryID.FromCurrent>>.View ComplianceView;

        public SelectFrom<ASCIStarINJewelryItem>.Where<ASCIStarINJewelryItem.inventoryID.IsEqual<InventoryItem.inventoryID.FromCurrent>>.View JewelryItemView;

        #endregion Selects

        #region Event Handlers

        #region InventoryItem Events

        //protected void InventoryItem_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e, PXRowUpdated InvokeBaseHandler)
        //{
        //    //if (InvokeBaseHandler != null)
        //    InvokeBaseHandler(cache, e);

        //    var row = (InventoryItem)e.Row;
        //    if (row != null)
        //    {

        //        ASCIStarINInventoryItemExt itemext = row.GetExtension<ASCIStarINInventoryItemExt>();
        //        POVendorInventory vendorItem = DefaultVendorItem.Select();
        //        foreach (POVendorInventory vitem in Base.VendorItems.Select())
        //        {
        //            //PXTrace.WriteInformation($"{vitem.VendorID}:{vitem.IsDefault}");

        //            if (vitem.IsDefault == true)
        //                vendorItem = vitem;
        //        }
        //        ASCIStarPOVendorInventoryExt vendorItemExt = vendorItem.GetExtension<ASCIStarPOVendorInventoryExt>();
        //        if (vendorItem == null || vendorItemExt == null)
        //        {

        //            itemext.UsrCommodityCost = 0.00m;
        //        }
        //        else
        //        {

        //            if (itemext.UsrPricingGRAMGold > 0 || itemext.UsrPricingGRAMSilver > 0)
        //            {

        //                ASCIStarMarketCostHelper.JewelryCost costHelper
        //                    = new ASCIStarMarketCostHelper.JewelryCost(cache.Graph
        //                                        , row
        //                                        , 0.000000m
        //                                        , 0.000000m
        //                                        , vendorItem.VendorID
        //                                        , vendorItemExt.UsrMarketID
        //                                        , DateTime.Today
        //                                        , row.BaseUnit);

        //                itemext.UsrCommodityCost = costHelper.CostRollupTotal[CostRollupType.Commodity];

        //                //costHelper.CostBasis.GoldBasis.BuildFinePrices();
        //                //itemext.UsrCommodityCost = 0;
        //                //if (itemext.UsrPricingGRAMGold > 0 || itemext.UsrPricingGRAMSilver > 0)
        //                //    itemext.UsrCommodityCost += (itemext.UsrPricingGRAMGold / 31.10348m * GetCommodityPrice(vendorItem, vendorItemExt, "24K")) +
        //                //        (itemext.UsrPricingGRAMSilver / 31.10348m * GetCommodityPrice(vendorItem, vendorItemExt, "SSS"))
        //                //        * (1 + itemext.UsrContractLossPct / 100.0000m)
        //                //        * (1 + itemext.UsrContractSurcharge / 100.0000m);
        //                cache.SetValue<ASCIStarINInventoryItemExt.usrCommodityCost>(cache.Current, itemext.UsrCommodityCost);

        //            }
        //            //if (itemext.UsrPricingGRAMGold > 0 && itemext.UsrPricingGRAMSilver == 0
        //            //    || itemext.UsrPricingGRAMGold == 0 && itemext.UsrPricingGRAMSilver > 0)
        //            //{
        //            //    itemext.UsrCommodityCost = (itemext.UsrPricingGRAMSilver ?? 0.00m + itemext.UsrPricingGRAMGold ?? 0.00m) / 31.10348m * vendorItemExt.UsrCommodityPrice;
        //            //    cache.SetValueExt<ASCIStarINInventoryItemExt.usrCommodityCost>(cache.Current, itemext.UsrCommodityCost);

        //            //}
        //        }
        //        //string msg = "";

        //        if (itemext.UsrPricingGRAMGold > 0 || itemext.UsrPricingGRAMSilver > 0)
        //        {
        //            //itemext.UsrCommodityCost = (itemext.UsrPricingGRAMGold / 31.10348m * GetCommodityPrice(vendorItem, vendorItemExt, "24K")) +
        //            //(itemext.UsrPricingGRAMSilver / 31.10348m * GetCommodityPrice(vendorItem, vendorItemExt, "SSS"))
        //            //* (1 + itemext.UsrContractLossPct / 100.0000m)
        //            //* (1 + itemext.UsrContractSurcharge / 100.0000m);
        //            //cache.SetValue<ASCIStarINInventoryItemExt.usrCommodityCost>(cache.Current, itemext.UsrCommodityCost);


        //            itemext.UsrContractCost = itemext.UsrCommodityCost
        //                + itemext.UsrOtherMaterialCost
        //                + itemext.UsrFabricationCost
        //                + itemext.UsrLaborCost
        //                + itemext.UsrPackagingCost
        //                + itemext.UsrOtherCost;
        //            cache.SetValue<ASCIStarINInventoryItemExt.usrContractCost>(cache.Current, itemext.UsrContractCost);

        //            itemext.UsrUnitCost = itemext.UsrContractCost
        //                + itemext.UsrHandlingCost
        //                + itemext.UsrFreightCost
        //                + itemext.UsrDutyCost;
        //            cache.SetValue<ASCIStarINInventoryItemExt.usrUnitCost>(cache.Current, itemext.UsrUnitCost);
        //        }
        //    }
        //}

        //protected void InventoryItem_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e, PXRowUpdated InvokeBaseHandler)
        //{
        //    //if (InvokeBaseHandler != null)
        //    InvokeBaseHandler(cache, e);

        //    var row = (InventoryItem)e.Row;
        //    if (row != null)
        //    {

        //        ASCIStarINInventoryItemExt itemext = row.GetExtension<ASCIStarINInventoryItemExt>();
        //        POVendorInventory vendorItem = DefaultVendorItem.Select();
        //        foreach (POVendorInventory vitem in Base.VendorItems.Select())
        //        {
        //            //PXTrace.WriteInformation($"{vitem.VendorID}:{vitem.IsDefault}");

        //            if (vitem.IsDefault == true)
        //                vendorItem = vitem;
        //        }
        //        ASCIStarPOVendorInventoryExt vendorItemExt = vendorItem.GetExtension<ASCIStarPOVendorInventoryExt>();
        //        if (vendorItem == null || vendorItemExt == null)
        //        {

        //            itemext.UsrCommodityCost = 0.00m;
        //        }
        //        else
        //        {

        //            if (itemext.UsrPricingGRAMGold > 0 || itemext.UsrPricingGRAMSilver > 0)
        //            {

        //                ASCIStarMarketCostHelper.JewelryCost costHelper
        //                    = new ASCIStarMarketCostHelper.JewelryCost(cache.Graph
        //                                        , row
        //                                        , 0.000000m
        //                                        , 0.000000m
        //                                        , vendorItem.VendorID
        //                                        , vendorItemExt.UsrMarketID
        //                                        , DateTime.Today
        //                                        , row.BaseUnit);

        //                itemext.UsrCommodityCost = costHelper.CostRollupTotal[CostRollupType.Commodity];

        //                cache.SetValue<ASCIStarINInventoryItemExt.usrCommodityCost>(cache.Current, itemext.UsrCommodityCost);

        //            }
        //        }
        //    }
        //}

        protected virtual void _(Events.RowSelected<InventoryItem> e)
        {
            var row = e.Row;
            if (row == null) return;

            bool isVisible = IsVisibleFileds(row.ItemClassID);
            SetVisibleJewelFields(e.Cache, row, isVisible);

            bool? baseMetalType = GetMetalType();
            SetReadOnlyJewelFields(e.Cache, row, baseMetalType);

            PXUIFieldAttribute.SetRequired<ASCIStarINJewelryItem.metalType>(this.JewelryItemView.Cache, isVisible);
            PXDefaultAttribute.SetPersistingCheck<ASCIStarINJewelryItem.metalType>(this.JewelryItemView.Cache, this.JewelryItemView.Current, isVisible ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
        }

        protected virtual void _(Events.FieldDefaulting<InventoryItem, ASCIStarINInventoryItemExt.usrCostingType> e)
        {
            if (e.Row == null) return;

            INItemClass itemClass = INItemClass.PK.Find(Base, e.Row.ItemClassID);
            ASCIStarINItemClassExt classExt = itemClass?.GetExtension<ASCIStarINItemClassExt>();
            e.NewValue = classExt.UsrCostingType ?? CostingType.StandardCost;
        }

        protected virtual void _(Events.FieldDefaulting<InventoryItem, ASCIStarINInventoryItemExt.usrCostRollupType> e)
        {
            if (e.Row == null) return;

            INItemClass itemClass = INItemClass.PK.Find(Base, e.Row.ItemClassID);
            ASCIStarINItemClassExt classExt = itemClass?.GetExtension<ASCIStarINItemClassExt>();
            e.NewValue = classExt.UsrCostRollupType ?? CostRollupType.Other;
        }

        protected virtual void _(Events.FieldVerifying<InventoryItem, ASCIStarINInventoryItemExt.usrContractSurcharge> e)
        {
            var row = e.Row;
            if (row == null) return;
           // if ((decimal?)e.NewValue == null || ) throw new PXSetPropertyException<ASCIStarINInventoryItemExt.usrContractSurcharge>("Surcharge can not be less 0%!");

        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, InventoryItem.itemClassID> e)
        {
            var row = e.Row;
            if (e.Row == null) return;

            bool isVisible = IsVisibleFileds(row.ItemClassID);
            SetVisibleJewelFields(e.Cache, row, isVisible);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrActualGRAMGold> e)
        {
            var row = e.Row;
            if (row == null || this.JewelryItemView.Current == null) return;

            decimal? mult = GetGoldMult();

            ASCIStarINInventoryItemExt rowExt = row.GetExtension<ASCIStarINInventoryItemExt>();
            decimal? pricingGRAMGold = rowExt.UsrActualGRAMGold * (mult / 24);
            e.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrPricingGRAMGold>(row, pricingGRAMGold);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrActualGRAMSilver> e)
        {
            PXTrace.WriteInformation(nameof(e));

            var row = e.Row;
            if (row == null || this.JewelryItemView.Current == null) return;

            decimal? mult = GetSilverMult();
            PXTrace.WriteInformation($"{this.JewelryItemView.Current.MetalType}:{mult}");

            ASCIStarINInventoryItemExt itemext = row.GetExtension<ASCIStarINInventoryItemExt>();
            e.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrPricingGRAMSilver>(row, itemext.UsrActualGRAMSilver * mult);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrPricingGRAMGold> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = row.GetExtension<ASCIStarINInventoryItemExt>();

            UpdateCommodityCostMetal(e.Cache, row, rowExt);
            decimal? mult = GetGoldMult();
            decimal? newActualGRAMGold = mult == null || mult == 0.0m ? decimal.Zero : (decimal?)e.NewValue * 24 / mult;
            if (newActualGRAMGold == rowExt.UsrActualGRAMGold) return;
            rowExt.UsrActualGRAMGold = newActualGRAMGold;
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrPricingGRAMSilver> e)
        {
            var row = e.Row;
            if (row == null) return;

            ASCIStarINInventoryItemExt rowExt = row.GetExtension<ASCIStarINInventoryItemExt>();

            UpdateCommodityCostMetal(e.Cache, row, rowExt);

            decimal? mult = GetSilverMult();

            decimal? newActualSilver = mult == null || mult == 0.0m ? decimal.Zero : (decimal?)e.NewValue / mult;
            if (newActualSilver == rowExt.UsrActualGRAMSilver) return;
            rowExt.UsrActualGRAMSilver = newActualSilver;
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrCommodityCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = row.GetExtension<ASCIStarINInventoryItemExt>();
            UpdatePurchaseContractCost(e.Cache, row, rowExt);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrContractCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = row.GetExtension<ASCIStarINInventoryItemExt>();
            decimal? newValue = (decimal?)e.NewValue;

            rowExt.UsrDutyCost = rowExt.UsrDutyCostPct * newValue / 100.0m;
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrContractIncrement> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = row.GetExtension<ASCIStarINInventoryItemExt>();

            UpdateSurcharge(e.Cache, row, rowExt);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrContractSurcharge> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = row.GetExtension<ASCIStarINInventoryItemExt>();
            UpdateCommodityCostMetal(e.Cache, row, rowExt);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrContractLossPct> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = row.GetExtension<ASCIStarINInventoryItemExt>();
            UpdateCommodityCostMetal(e.Cache, row, rowExt);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrOtherMaterialCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = row.GetExtension<ASCIStarINInventoryItemExt>();
            UpdatePurchaseContractCost(e.Cache, row, rowExt);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrFabricationCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = row.GetExtension<ASCIStarINInventoryItemExt>();
            UpdatePurchaseContractCost(e.Cache, row, rowExt);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrPackagingCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = row.GetExtension<ASCIStarINInventoryItemExt>();
            UpdatePurchaseContractCost(e.Cache, row, rowExt);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrOtherCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = row.GetExtension<ASCIStarINInventoryItemExt>();
            UpdatePurchaseContractCost(e.Cache, row, rowExt);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrLaborCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = row.GetExtension<ASCIStarINInventoryItemExt>();
            UpdatePurchaseContractCost(e.Cache, row, rowExt);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrFreightCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = row.GetExtension<ASCIStarINInventoryItemExt>();
            UpdateUnitCost(e.Cache, row, rowExt);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrDutyCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = row.GetExtension<ASCIStarINInventoryItemExt>();
            UpdateUnitCost(e.Cache, row, rowExt);

            if (rowExt.UsrDutyCost == null || rowExt.UsrDutyCost == 0m) return;
            if (rowExt.UsrContractCost == null || rowExt.UsrContractCost == 0.0m)
            {
                rowExt.UsrDutyCostPct = decimal.Zero;
                return;
            }
            decimal? newCostPctValue = (decimal?)e.NewValue / rowExt.UsrContractCost * 100.0m;
            if (newCostPctValue == rowExt.UsrDutyCostPct) return;
            rowExt.UsrDutyCostPct = newCostPctValue;
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrDutyCostPct> e)
        {
            var row = e.Row;
            if (row == null) return;

            ASCIStarINInventoryItemExt rowExt = row.GetExtension<ASCIStarINInventoryItemExt>();

            decimal? newDutyCostValue = rowExt.UsrContractCost * (decimal?)e.NewValue / 100.00m;
            if (newDutyCostValue == rowExt.UsrDutyCost) return;
            e.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrDutyCost>(row, newDutyCostValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrHandlingCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = row.GetExtension<ASCIStarINInventoryItemExt>();
            UpdateUnitCost(e.Cache, row, rowExt);
        }

        #endregion InventoryItem Events

        #region JewelryItem Events
        protected virtual void _(Events.FieldUpdated<ASCIStarINJewelryItem, ASCIStarINJewelryItem.metalType> e)
        {
            var row = e.Row;
            if (row == null) return;

            bool? baseMetalType = GetMetalType();

            SetReadOnlyJewelFields(this.Base.Item.Cache, this.Base.Item.Current, baseMetalType);

            SetMetalGramsToZero(baseMetalType);

            if (baseMetalType == true)
                this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrActualGRAMGold>(this.Base.Item.Current, this.Base.Item.Current?.GetExtension<ASCIStarINInventoryItemExt>().UsrActualGRAMGold);
            else
                this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrActualGRAMSilver>(this.Base.Item.Current, this.Base.Item.Current?.GetExtension<ASCIStarINInventoryItemExt>().UsrActualGRAMSilver);
            // UpdateCommodityCostMetal(this.Base.Item.Cache, this.Base.Item.Current, this.Base.Item.Current?.GetExtension<ASCIStarINInventoryItemExt>());
        }

        #endregion JewelryItem Events

        #region POVendorInventory Events
        protected virtual void _(Events.FieldUpdated<POVendorInventory, POVendorInventory.vendorID> e)
        {
            if (e.Row == null || e.NewValue == null) return;

            Vendor vendor = Vendor.PK.Find(Base, (int?)e.NewValue);

            ASCIStarVendorExt vendorExt = vendor?.GetExtension<ASCIStarVendorExt>();
            e.Cache.SetValueExt<ASCIStarPOVendorInventoryExt.usrMarketID>(e.Row, vendorExt.UsrMarketID);
        }

        protected virtual void POVendorInventory_UsrCommodityLossPct_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        {
            if (InvokeBaseHandler != null)
                InvokeBaseHandler(cache, e);
            PXTrace.WriteInformation("POVendorInventory_UsrCommodityLossPct_FieldUpdated");
            POVendorInventory row = e.Row as POVendorInventory;
            ASCIStarPOVendorInventoryExt porow = row.GetExtension<ASCIStarPOVendorInventoryExt>();
            if (porow != null)
            {
                PXTrace.WriteInformation("porow != null");
                InventoryItem item = InventoryItem.PK.Find(cache.Graph, row.InventoryID);
                ASCIStarINInventoryItemExt ext = item.GetExtension<ASCIStarINInventoryItemExt>();
                //ext.UsrContractSurcharge = porow.UsrCommodityLossPct + porow.UsrCommoditySurchargePct;
                cache.SetValueExt<ASCIStarINInventoryItemExt.usrContractSurcharge>(cache.Current, porow.UsrCommodityLossPct + porow.UsrCommoditySurchargePct);



            }
        }

        protected virtual void POVendorInventory_UsrCommoditySurchargePct_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        {
            if (InvokeBaseHandler != null)
                InvokeBaseHandler(cache, e);
            PXTrace.WriteInformation("POVendorInventory_UsrCommoditySurchargePct_FieldUpdated");

            POVendorInventory row = e.Row as POVendorInventory;
            ASCIStarPOVendorInventoryExt porow = row.GetExtension<ASCIStarPOVendorInventoryExt>();
            if (porow != null)
            {
                PXTrace.WriteInformation("porow != null");
                InventoryItem item = InventoryItem.PK.Find(cache.Graph, row.InventoryID);
                ASCIStarINInventoryItemExt ext = item.GetExtension<ASCIStarINInventoryItemExt>();
                cache.SetValueExt<ASCIStarINInventoryItemExt.usrContractSurcharge>(cache.Current, porow.UsrCommodityLossPct + porow.UsrCommoditySurchargePct);

            }
        }

        protected virtual void POVendorInventory_UsrCommodityPrice_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXTrace.WriteInformation($"Entering POVendorInventory_UsrCommodityPrice_FieldDefaulting");
            POVendorInventory row = e.Row as POVendorInventory;
            if (row == null || row.VendorID == null)
                return;

            ASCIStarPOVendorInventoryExt rowExt = row.GetExtension<ASCIStarPOVendorInventoryExt>();
            if (rowExt == null || rowExt.UsrCommodityID == null)
                return;

            ASCIStarMarketCostHelper.JewelryCost costHelper = new ASCIStarMarketCostHelper.JewelryCost(Base, Base.Item.Current);
            Vendor v = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(Base, row.VendorID);
            costHelper.CostBasis.ItemVendor = v;
            ASCIStarVendorExt vExt = v.GetExtension<ASCIStarVendorExt>();

            if (row != null)
            {
                e.NewValue = costHelper.vendorPrice.UsrCommodityPrice;
            }
        }

        protected virtual void POVendorInventory_UsrCommodityIncrement_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXTrace.WriteInformation($"Entering POVendorInventory_UsrCommodityIncrement_FieldDefaulting");
            POVendorInventory row = e.Row as POVendorInventory;
            if (row == null || row.VendorID == null)
                return;

            ASCIStarPOVendorInventoryExt rowExt = row.GetExtension<ASCIStarPOVendorInventoryExt>();
            if (rowExt == null || rowExt.UsrCommodityID == null)
                return;

            ASCIStarMarketCostHelper.JewelryCost costHelper = new ASCIStarMarketCostHelper.JewelryCost(Base, Base.Item.Current);
            Vendor v = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(Base, row.VendorID);
            costHelper.CostBasis.ItemVendor = v;
            ASCIStarVendorExt vExt = v.GetExtension<ASCIStarVendorExt>();

            if (row != null)
            {
                e.NewValue = costHelper.vendorPrice.UsrCommodityIncrement;
            }
        }

        protected virtual void POVendorInventory_UsrCommodityLossPct_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXTrace.WriteInformation($"Entering POVendorInventory_UsrCommodityIncrement_FieldDefaulting");
            POVendorInventory row = e.Row as POVendorInventory;
            if (row == null || row.VendorID == null)
                return;

            ASCIStarPOVendorInventoryExt rowExt = row.GetExtension<ASCIStarPOVendorInventoryExt>();
            if (rowExt == null || rowExt.UsrCommodityID == null)
                return;

            ASCIStarMarketCostHelper.JewelryCost costHelper = new ASCIStarMarketCostHelper.JewelryCost(Base, Base.Item.Current);
            Vendor v = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(Base, row.VendorID);
            costHelper.CostBasis.ItemVendor = v;
            ASCIStarVendorExt vExt = v.GetExtension<ASCIStarVendorExt>();

            if (row != null)
            {
                e.NewValue = costHelper.vendorPrice.UsrCommodityLossPct;
            }
        }

        protected virtual void POVendorInventory_UsrCommoditySurchargePct_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXTrace.WriteInformation($"Entering POVendorInventory_UsrCommodityIncrement_FieldDefaulting");
            POVendorInventory row = e.Row as POVendorInventory;
            if (row == null || row.VendorID == null)
                return;

            ASCIStarPOVendorInventoryExt rowExt = row.GetExtension<ASCIStarPOVendorInventoryExt>();
            if (rowExt == null || rowExt.UsrCommodityID == null)
                return;

            ASCIStarMarketCostHelper.JewelryCost costHelper = new ASCIStarMarketCostHelper.JewelryCost(Base, Base.Item.Current);
            Vendor v = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(Base, row.VendorID);
            costHelper.CostBasis.ItemVendor = v;
            ASCIStarVendorExt vExt = v.GetExtension<ASCIStarVendorExt>();

            if (row != null)
            {
                e.NewValue = costHelper.vendorPrice.UsrCommoditySurchargePct;
            }
        }

        //   protected virtual void POVendorInventory_UsrVendorDefault_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        protected virtual void _(Events.FieldUpdated<POVendorInventory, POVendorInventory.isDefault> e)
        {
            var row = e.Row;
            if (row == null) return;

            bool isDefaultVendor = (bool?)e.NewValue == true;

            SetReadOnlyDefaultItemFields(this.Base.Item.Cache, this.Base.Item.Current, !isDefaultVendor);

            if (isDefaultVendor)
            {
                List<POVendorInventory> selectPOVendors = this.Base.VendorItems.Select()?.FirstTableItems.ToList();
                selectPOVendors.ForEach(x =>
                {
                    if (x.IsDefault == true && x != row)
                    {
                        this.VendorItems.Cache.SetValue<POVendorInventory.isDefault>(x, false);
                        this.VendorItems.View.RequestRefresh();
                        return;
                    }
                });
            }


            var inventoryItemExt = this.Base.Item.Current.GetExtension<ASCIStarINInventoryItemExt>();
            UpdateCommodityCostMetal(this.Base.Item.Cache, this.Base.Item.Current, inventoryItemExt);

        }

        #endregion POVendorInventory Events

        #region Compliance Events
        public virtual void _(Events.FieldSelecting<ASCIStarINCompliance, ASCIStarINCompliance.customerAlphaCode> e)
        {
            SetupStringList<ASCIStarINCompliance.customerAlphaCode>(e.Cache, INConstants.INAttributesID.CustomerCode);
        }

        public virtual void _(Events.FieldSelecting<ASCIStarINCompliance, ASCIStarINCompliance.division> e)
        {
            SetupStringList<ASCIStarINCompliance.division>(e.Cache, INConstants.INAttributesID.InventoryCategory);
        }

        public virtual void _(Events.FieldSelecting<ASCIStarINCompliance, ASCIStarINCompliance.testingLab> e)
        {
            SetupStringList<ASCIStarINCompliance.testingLab>(e.Cache, INConstants.INAttributesID.CPTESTTYPE);
        }

        public virtual void _(Events.FieldSelecting<ASCIStarINCompliance, ASCIStarINCompliance.protocolTestedTo> e)
        {
            SetupStringList<ASCIStarINCompliance.protocolTestedTo>(e.Cache, INConstants.INAttributesID.CPPROTOCOL);
        }

        public virtual void _(Events.FieldSelecting<ASCIStarINCompliance, ASCIStarINCompliance.waiverReasonCode> e)
        {
            SetupStringList<ASCIStarINCompliance.waiverReasonCode>(e.Cache, INConstants.INAttributesID.REASONCODE);
        }
        #endregion Compliance Events

        #endregion Event Handlers

        #region Helper Methods

        private void SetVisibleJewelFields(PXCache cache, InventoryItem row, bool isVisible)
        {
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrPriceAsID>(cache, row, !isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrPriceToUnit>(cache, row, !isVisible);

            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrContractCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrUnitCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrFabricationCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrCommodityCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrFreightCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrLaborCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrDutyCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrDutyCostPct>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrHandlingCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrPackagingCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrOtherCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrOtherMaterialCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrPricingGRAMGold>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrPricingGRAMSilver>(cache, row, isVisible);
            //PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrPricingGRAMPlatinum>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrActualGRAMGold>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrActualGRAMSilver>(cache, row, isVisible);
            //PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrActualGRAMPlatinum>(cache, row, isVisible);
        }

        private bool IsVisibleFileds(int? itemClassID)
        {
            INItemClass itemClass = INItemClass.PK.Find(Base, itemClassID);

            return itemClass?.ItemClassCD.Trim() != "COMMODITY";  // acupower: remove from constant to jewel preferences screen and find from rowSelected
        }

        private void SetReadOnlyJewelFields(PXCache cache, InventoryItem row, bool? baseMetalType)
        {
            if (baseMetalType == null)
            {
                PXUIFieldAttribute.SetReadOnly<ASCIStarINInventoryItemExt.usrPricingGRAMGold>(cache, row, true);
                PXUIFieldAttribute.SetReadOnly<ASCIStarINInventoryItemExt.usrPricingGRAMSilver>(cache, row, true);
                PXUIFieldAttribute.SetReadOnly<ASCIStarINInventoryItemExt.usrActualGRAMGold>(cache, row, true);
                PXUIFieldAttribute.SetReadOnly<ASCIStarINInventoryItemExt.usrActualGRAMSilver>(cache, row, true);
            }
            else
            {
                bool isReadOnly = baseMetalType == true;

                PXUIFieldAttribute.SetReadOnly<ASCIStarINInventoryItemExt.usrActualGRAMGold>(cache, row, !isReadOnly);
                PXUIFieldAttribute.SetReadOnly<ASCIStarINInventoryItemExt.usrPricingGRAMGold>(cache, row, !isReadOnly);

                PXUIFieldAttribute.SetReadOnly<ASCIStarINInventoryItemExt.usrActualGRAMSilver>(cache, row, isReadOnly);
                PXUIFieldAttribute.SetReadOnly<ASCIStarINInventoryItemExt.usrPricingGRAMSilver>(cache, row, isReadOnly);
            }
        }

        private void SetReadOnlyDefaultItemFields(PXCache cache, InventoryItem row, bool isDefaultVendor)
        {
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrCommodityIncrement>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrCommoditySurchargePct>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrCommodityLossPct>(cache, row, isDefaultVendor);
        }

        /// <summary>
        /// Method check metal type by details values in Attributes tables from JSMetal attribute
        /// </summary>
        /// <returns>Returns true if gold, false - if silver, and returns null if cannot find Metal Type</returns>
        private bool? GetMetalType()
        {
            if (this.JewelryItemView.Current == null)
                this.JewelryItemView.Current = this.JewelryItemView.Select().TopFirst;

            switch (this.JewelryItemView.Current?.MetalType.ToUpper())
            {
                case "06K": return true;
                case "08K": return true;
                case "09K": return true;
                case "10K": return true;
                case "10F": return true;
                case "12K": return true;
                case "12F": return true;
                case "14K": return true;
                case "14F": return true;
                case "16K": return true;
                case "16F": return true;
                case "18K": return true;
                case "18F": return true;
                case "21K": return true;
                case "21F": return true;
                case "22K": return true;
                case "22F": return true;
                case "23K": return true;
                case "23F": return true;
                case "24K": return true;
                case "24F": return true;
                case "FSS": return false;
                case "SSS": return false;
                default: return null;
            }
        }

        private void SetMetalGramsToZero(bool? baseMetalType)
        {
            switch (baseMetalType)
            {
                case true:
                    {
                        this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrActualGRAMSilver>(this.Base.Item.Current, decimal.Zero);
                        break;
                    }
                case false:
                    {
                        this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrActualGRAMGold>(this.Base.Item.Current, decimal.Zero);
                        this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrContractIncrement>(this.Base.Item.Current, 0.5m);
                        break;
                    }
                default:
                    {
                        this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrActualGRAMSilver>(this.Base.Item.Current, decimal.Zero);
                        this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrActualGRAMGold>(this.Base.Item.Current, decimal.Zero);
                        break;
                    }
            }
            this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrUnitCost>(this.Base.Item.Current, decimal.Zero);
            this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrContractCost>(this.Base.Item.Current, decimal.Zero);
            this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrCommodityCost>(this.Base.Item.Current, decimal.Zero);
        }

        private void UpdateCommodityCostMetal(PXCache cache, InventoryItem row, ASCIStarINInventoryItemExt rowExt)
        {
            ASCIStarMarketCostHelper.JewelryCost jewelryCostProvider = GetCostProvider(row);

            decimal costFineGoldPerGramm = jewelryCostProvider.CostRollupTotal[CostRollupType.Commodity];

            decimal? lossValue = 1.0m + rowExt.UsrContractLossPct / 100.0m;
            decimal? surchargeValue = 1.0m + rowExt.UsrContractSurcharge / 100.0m;

            rowExt.UsrCommodityCost = costFineGoldPerGramm * lossValue * surchargeValue;

            cache.SetValueExt<ASCIStarINInventoryItemExt.usrCommodityCost>(row, rowExt.UsrCommodityCost);

            UpdateIncrement(cache, row, rowExt, jewelryCostProvider);
        }

        private void UpdatePurchaseContractCost(PXCache cache, InventoryItem row, ASCIStarINInventoryItemExt rowExt)
        {
            decimal? newCost = rowExt.UsrCommodityCost + rowExt.UsrOtherMaterialCost + rowExt.UsrFabricationCost + rowExt.UsrLaborCost + rowExt.UsrPackagingCost + rowExt.UsrOtherCost;

            cache.SetValueExt<ASCIStarINInventoryItemExt.usrContractCost>(row, newCost);

            UpdateUnitCost(cache, row, rowExt);
        }

        private void UpdateUnitCost(PXCache cache, InventoryItem row, ASCIStarINInventoryItemExt rowExt)
        {
            decimal? newCost = rowExt.UsrUnitCost = rowExt.UsrContractCost + rowExt.UsrHandlingCost + rowExt.UsrFreightCost + rowExt.UsrDutyCost;

            cache.SetValueExt<ASCIStarINInventoryItemExt.usrUnitCost>(row, newCost);
        }

        private void UpdateIncrement(PXCache cache, InventoryItem row, ASCIStarINInventoryItemExt rowExt, ASCIStarMarketCostHelper.JewelryCost costProvider)
        {
            if (costProvider == null || costProvider.CostBasis == null || costProvider.CostBasis.GoldBasis == null
                || costProvider.CostBasis.GoldBasis.EffectiveBasisPerOz == decimal.Zero || costProvider.CostBasis.GoldBasis.EffectiveBasisPerOz == 0.0m) return;

            decimal? temp1 = costProvider.CostBasis.GoldBasis.BasisPerFineOz[this.JewelryItemView.Current.MetalType?.ToUpper()] / 31.10348m / costProvider.CostBasis.GoldBasis.EffectiveBasisPerOz;
            decimal? temp2 = temp1 * (1.0m + rowExt.UsrContractSurcharge / 100.0m);
            decimal? temp3 = costProvider.CostBasis.GoldBasis.EffectiveBasisPerOz - costProvider.CostBasis.GoldBasis.EffectiveMarketPerOz;

            decimal? newIncrementValue = (temp3 == 0.0m || temp3 == null) ? temp2 : temp2 * temp3;

            if (newIncrementValue == rowExt.UsrContractIncrement) return;

            rowExt.UsrContractIncrement = newIncrementValue;
        }

        private void UpdateSurcharge(PXCache cache, InventoryItem row, ASCIStarINInventoryItemExt rowExt)
        {
            if (rowExt.UsrContractIncrement == null || rowExt.UsrContractIncrement == 0.0m) throw new PXSetPropertyException<ASCIStarINInventoryItemExt.usrContractIncrement>("Increment can not be 0 or empty!");

            ASCIStarMarketCostHelper.JewelryCost costProvider = GetCostProvider(row);

            if (costProvider == null || costProvider.CostBasis == null || costProvider.CostBasis.GoldBasis == null || costProvider.CostBasis.GoldBasis.EffectiveBasisPerOz == decimal.Zero) return;

            decimal? temp1 = rowExt.UsrContractIncrement / (costProvider.CostBasis.GoldBasis.EffectiveBasisPerOz - costProvider.CostBasis.GoldBasis.EffectiveMarketPerOz);
            decimal? temp2 = (costProvider.CostBasis.GoldBasis.BasisPerFineOz[this.JewelryItemView.Current.MetalType?.ToUpper()] / 31.10348m / costProvider.CostBasis.GoldBasis.EffectiveBasisPerOz);

            if (temp2 == null || temp2 == 0.0m) return;

            decimal? surchargeNewValue = (temp1 / temp2 - 1.0m) * 100.0m;

            cache.SetValueExt<ASCIStarINInventoryItemExt.usrContractSurcharge>(row, surchargeNewValue);
        }

        private ASCIStarMarketCostHelper.JewelryCost GetCostProvider(InventoryItem row)
        {
            POVendorInventory vendorItem = new POVendorInventory();
            foreach (POVendorInventory vitem in Base.VendorItems.Select())
            {
                // PXTrace.WriteInformation($"{vitem.VendorID}:{vitem.IsDefault}");

                if (vitem.IsDefault == true)
                {
                    vendorItem = vitem;
                    break;
                }
            }

            if (vendorItem == null)
            {
                throw new PXException("no default vendor");
            }

            ASCIStarPOVendorInventoryExt vendorItemExt = vendorItem?.GetExtension<ASCIStarPOVendorInventoryExt>();

            //rowExt.UsrCommodityCost =
            //      (rowExt.UsrPricingGRAMGold / 31.10348m * GetCommodityPrice(vendorItem, vendorItemExt, "24K"))
            //      + (rowExt.UsrPricingGRAMSilver / 31.10348m * GetCommodityPrice(vendorItem, vendorItemExt, "SSS"))
            //      * (1 + rowExt.UsrContractLossPct / 100.0000m)
            //      * (1 + rowExt.UsrContractSurcharge / 100.0000m);
            //cache.SetValue<ASCIStarINInventoryItemExt.usrCommodityCost>(row, rowExt.UsrCommodityCost);

            //if (rowExt.UsrPricingGRAMGold > 0 || rowExt.UsrPricingGRAMSilver > 0)
            //{

            ASCIStarMarketCostHelper.JewelryCost jewelryCostProvider
                = new ASCIStarMarketCostHelper.JewelryCost(Base, row, 0.000000m, 0.000000m
                                                                      , vendorItem?.VendorID, vendorItemExt?.UsrMarketID
                                                                      , DateTime.Today, row.BaseUnit);
            return jewelryCostProvider;
        }

        //protected decimal GetCommodityPrice(POVendorInventory vendorItem, ASCIStarPOVendorInventoryExt vendorItemExt, string Commodity)
        //{
        //    decimal price = 0.00000m;

        //    if (Commodity != "24K" && Commodity != "SSS")
        //    {
        //        PXTrace.WriteWarning($"{Commodity} must be either 24K or SSS, returning $0 price");
        //        return 0;
        //    }

        //    price = (decimal)vendorItemExt.UsrCommodityPrice;
        //    Vendor vendor = new PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>(Base).SelectSingle(vendorItem.VendorID);


        //    if (vendorItemExt.UsrVendorDefault == false || vendorItemExt.UsrVendorDefault == false)
        //    {

        //        InventoryItem commodity = new PXSelect<InventoryItem, Where<InventoryItem.inventoryCD, Equal<Required<InventoryItem.inventoryCD>>>>(Base).SelectSingle(Commodity);

        //        APVendorPrice vendorDefPrice =
        //        new PXSelect<APVendorPrice,
        //        Where<APVendorPrice.vendorID, Equal<Required<APVendorPrice.vendorID>>,
        //            And<APVendorPrice.inventoryID, Equal<Required<APVendorPrice.inventoryID>>,
        //            And<APVendorPrice.uOM, Equal<Required<APVendorPrice.uOM>>,
        //            And<APVendorPrice.effectiveDate, LessEqual<Required<APVendorPrice.effectiveDate>>,
        //            And<APVendorPrice.effectiveDate, LessEqual<Required<APVendorPrice.effectiveDate>>>>>>>,
        //        OrderBy<Desc<APVendorPrice.effectiveDate>>>(Base).SelectSingle(
        //            vendorItem.VendorID,
        //            commodity.InventoryID,
        //            "TOZ",
        //            DateTime.Today,
        //            DateTime.Today);
        //        price = (decimal)vendorDefPrice.SalesPrice;
        //        PXTrace.WriteInformation($"{vendor.AcctCD}:{Commodity}:{vendorDefPrice.EffectiveDate}:{price}");
        //    }
        //    PXTrace.WriteInformation($"{vendor.AcctCD}:{Commodity}:{System.DateTime.MinValue}:{price}");

        //    return price;
        //}

        private decimal? GetGoldMult()
        {
            if (this.JewelryItemView.Current.MetalType == null) throw new PXException("The Metal Type on Jewelry tab is missing!");

            switch (this.JewelryItemView.Current.MetalType.ToUpper())
            {
                case "24K": return 24.000000m;
                case "22K": return 22.000000m;
                case "20K": return 20.000000m;
                case "18K": return 18.000000m;
                case "16K": return 16.000000m;
                case "14K": return 14.000000m;
                case "12K": return 12.000000m;
                case "10K": return 10.000000m;
                case "08K": return 8.000000m;
                case "06K": return 6.000000m;
                default: return decimal.Zero;//1.000000m;
            }
        }

        private decimal? GetSilverMult()
        {
            if (this.JewelryItemView.Current.MetalType == null) throw new PXException("The Metal Type on Jewelry tab is missing!");

            switch (this.JewelryItemView.Current.MetalType.ToUpper())
            {
                case "FSS": return 1.081080m;
                case "SSS": return 1.000000m;
                default: return decimal.Zero;
            }
        }

        //protected void RecalculateCosts(PXCache cache, InventoryItem item, int? vendorId = null, DateTime? PriceAt = null)
        //{
        //    //decimal silverGrams = 0;



        //    if (item == null) return;

        //    ASCIStarINInventoryItemExt itemExt = item.GetExtension<ASCIStarINInventoryItemExt>();


        //    ASCIStarMarketCostHelper.JewelryCost costHelper = new ASCIStarMarketCostHelper.JewelryCost(cache.Graph, item, 0.000000m);

        //    cache.SetValue<ASCIStarINInventoryItemExt.usrDutyCost>(item, (itemExt.UsrCommodityCost
        //        + itemExt.UsrFabricationCost
        //        + itemExt.UsrLaborCost
        //        + itemExt.UsrHandlingCost
        //        + itemExt.UsrFreightCost
        //        + itemExt.UsrOtherCost
        //        + itemExt.UsrPackagingCost) * (itemExt.UsrDutyCostPct / 100.0000m));


        //}

        //protected void RecalculateCommodity(PXCache cache, InventoryItem item, int vendorId)
        //{
        //    POVendorInventory itemVendor = (POVendorInventory)PXSelect<POVendorInventory,
        //        Where<POVendorInventory.inventoryID, Equal<Required<InventoryItem.inventoryID>>,
        //        And<POVendorInventory.vendorID, Equal<Required<POVendorInventory.vendorID>>>>>.Select(cache.Graph, item.InventoryID, vendorId);
        //    ASCIStarPOVendorInventoryExt basis = itemVendor.GetExtension<ASCIStarPOVendorInventoryExt>();


        //}

        private void SetupStringList<Field>(PXCache cache, string attributeID) where Field : IBqlField
        {
            List<string> values = new List<string>();
            List<string> labels = new List<string>();
            SelectAttributeDetails(attributeID).ForEach(x =>
            {
                values.Add(x.ValueID);
                labels.Add(x.Description);
            });
            PXStringListAttribute.SetList<Field>(cache, null, values.ToArray(), labels.ToArray());
        }

        private List<CSAttributeDetail> SelectAttributeDetails(string attributeID)
        {
            return SelectFrom<CSAttributeDetail>.Where<CSAttributeDetail.attributeID.IsEqual<@P.AsString>>.View.Select(this.Base, attributeID)?.FirstTableItems.ToList();
        }
        #endregion Helper Methods


        #region Actions

        public PXAction<InventoryItem> UpdateMetalCost;
        [PXUIField(DisplayName = "Update Metal Cost", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable updateMetalCost(PXAdapter adapter)
        {
            if (this.Base.Item.Current == null) return adapter.Get();

            UpdateCommodityCostMetal(this.Base.Item.Cache, this.Base.Item.Current, this.Base.Item.Current.GetExtension<ASCIStarINInventoryItemExt>());

            return adapter.Get();
        }

        #endregion Action
    }
}