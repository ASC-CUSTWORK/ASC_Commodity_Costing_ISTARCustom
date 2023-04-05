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
using ASCISTARCustom.Cost.Descriptor;

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
            e.NewValue = classExt.UsrCostingType ?? ASCIStarCostingType.StandardCost;
        }

        protected virtual void _(Events.FieldDefaulting<InventoryItem, ASCIStarINInventoryItemExt.usrCostRollupType> e)
        {
            if (e.Row == null) return;

            INItemClass itemClass = INItemClass.PK.Find(Base, e.Row.ItemClassID);
            ASCIStarINItemClassExt classExt = itemClass?.GetExtension<ASCIStarINItemClassExt>();
            e.NewValue = classExt.UsrCostRollupType ?? ASCIStarCostRollupType.Other;
        }

        protected virtual void _(Events.FieldVerifying<InventoryItem, ASCIStarINInventoryItemExt.usrContractSurcharge> e)
        {
            var row = e.Row;
            if (row == null) return;
            if ((decimal?)e.NewValue < 0.0m)
            {
                if (GetMetalType() == true)
                {
                    //PXUIFieldAttribute.SetError<ASCIStarINInventoryItemExt.usrContractIncrement>(e.Cache, row, "Please, increase increment");
                    e.Cache.RaiseExceptionHandling<ASCIStarINInventoryItemExt.usrContractIncrement>(row, 0.0m, new PXSetPropertyException("Please, increase increment"));
                }

                //PXUIFieldAttribute.SetError<ASCIStarINInventoryItemExt.usrContractSurcharge>(e.Cache, row, "Surcharge can not be less 0%!");
                e.Cache.RaiseExceptionHandling<ASCIStarINInventoryItemExt.usrContractSurcharge>(row, e.NewValue, new PXSetPropertyException("Surcharge can not be less 0%!"));
            }
            else
            {
                //e.Cache.RaiseExceptionHandling<ASCIStarINInventoryItemExt.usrContractIncrement>(row, e.NewValue, null);
                //e.Cache.RaiseExceptionHandling<ASCIStarINInventoryItemExt.usrContractSurcharge>(row, null, null);
            }
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, InventoryItem.itemClassID> e)
        {
            var row = e.Row;
            if (e.Row == null) return;

            bool isVisible = IsVisibleFileds(row.ItemClassID);
            SetVisibleJewelFields(e.Cache, row, isVisible);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrCostingType> e)
        {
            var row = e.Row;
            if (e.Row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);
            UpdateCommodityCostMetal(e.Cache, row, rowExt);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrActualGRAMGold> e)
        {
            var row = e.Row;
            if (row == null || this.JewelryItemView.Current == null) return;

            decimal? mult = GetGoldMult();

            ASCIStarINInventoryItemExt rowExt = row.GetExtension<ASCIStarINInventoryItemExt>();
            decimal? pricingGRAMGold = rowExt?.UsrActualGRAMGold * (mult / 24);
            e.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrPricingGRAMGold>(row, pricingGRAMGold);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrActualGRAMSilver> e)
        {
            var row = e.Row;
            if (row == null || this.JewelryItemView.Current == null) return;

            decimal? mult = GetSilverMult();

            var rowExt = row.GetExtension<ASCIStarINInventoryItemExt>();
            e.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrPricingGRAMSilver>(row, rowExt?.UsrActualGRAMSilver * mult);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrPricingGRAMGold> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);

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

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);
            UpdatePurchaseContractCost(e.Cache, row, rowExt);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrContractCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);
            decimal? newValue = (decimal?)e.NewValue;

            rowExt.UsrDutyCost = rowExt.UsrDutyCostPct * newValue / 100.0m;
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrContractIncrement> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);
            bool? metalType = GetMetalType();
            if (metalType == true)
                UpdateSurcharge(e.Cache, row, rowExt);
            else
                UpdateCommodityCostMetal(e.Cache, row, rowExt);

            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrCommodityIncrement>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrContractSurcharge> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);
            UpdateCommodityCostMetal(e.Cache, row, rowExt);

            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrCommoditySurchargePct>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrContractLossPct> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);
            UpdateCommodityCostMetal(e.Cache, row, rowExt);

            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrCommodityLossPct>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrOtherMaterialCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);
            UpdatePurchaseContractCost(e.Cache, row, rowExt);

            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrOtherMaterialCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrFabricationCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);
            UpdatePurchaseContractCost(e.Cache, row, rowExt);

            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrFabricationCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrPackagingCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);
            UpdatePurchaseContractCost(e.Cache, row, rowExt);
            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrPackagingCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrOtherCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);
            UpdatePurchaseContractCost(e.Cache, row, rowExt);

            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrOtherCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrLaborCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);
            UpdatePurchaseContractCost(e.Cache, row, rowExt);

            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrLaborCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrFreightCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);
            UpdateUnitCost(e.Cache, row, rowExt);

            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrFreightCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrDutyCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);
            UpdateUnitCost(e.Cache, row, rowExt);

            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrDutyCost>(e.NewValue);

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

            ASCIStarINInventoryItemExt rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);

            decimal? newDutyCostValue = rowExt.UsrContractCost * (decimal?)e.NewValue / 100.00m;
            if (newDutyCostValue == rowExt.UsrDutyCost) return;
            e.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrDutyCost>(row, newDutyCostValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrHandlingCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);
            UpdateUnitCost(e.Cache, row, rowExt);
            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrHandlingCost>(e.NewValue);
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
                this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrActualGRAMGold>(this.Base.Item.Current, this.Base.Item.Current?.GetExtension<ASCIStarINInventoryItemExt>()?.UsrActualGRAMGold);
            else
                this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrActualGRAMSilver>(this.Base.Item.Current, this.Base.Item.Current?.GetExtension<ASCIStarINInventoryItemExt>()?.UsrActualGRAMSilver);
            // UpdateCommodityCostMetal(this.Base.Item.Cache, this.Base.Item.Current, this.Base.Item.Current?.GetExtension<ASCIStarINInventoryItemExt>());
        }

        #endregion JewelryItem Events

        #region POVendorInventory Events
        protected virtual void _(Events.RowSelected<POVendorInventory> e)
        {
            var row = e.Row;
            if (row == null) return;

            bool isDefaultVendor = row.IsDefault == true && row.GetExtension<ASCIStarPOVendorInventoryExt>().UsrVendorDefault == true;
            SetReadOnlyVendorsFields(this.VendorItems.Cache, row, isDefaultVendor);
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, POVendorInventory.vendorID> e)
        {
            if (e.Row == null || e.NewValue == null) return;

            Vendor vendor = Vendor.PK.Find(Base, (int?)e.NewValue);

            ASCIStarVendorExt vendorExt = vendor?.GetExtension<ASCIStarVendorExt>();
            var inventoryCD = GetMetalType() == true ? "24K" : "SSS";
            var inventoryID = SelectFrom<InventoryItem>.Where<InventoryItem.inventoryCD.IsEqual<P.AsString>>.View.Select(Base, inventoryCD)?.TopFirst.InventoryID;
            e.Cache.SetValueExt<ASCIStarPOVendorInventoryExt.usrMarketID>(e.Row, vendorExt.UsrMarketID);
            var apVendorPrice = new PXSelect<APVendorPrice,
                Where<APVendorPrice.vendorID, Equal<Required<APVendorPrice.vendorID>>,
                    And<APVendorPrice.inventoryID, Equal<Required<APVendorPrice.inventoryID>>,
                    And<APVendorPrice.uOM, Equal<Required<APVendorPrice.uOM>>,
                    And<APVendorPrice.effectiveDate, LessEqual<Required<APVendorPrice.effectiveDate>>,
                    And<APVendorPrice.effectiveDate, LessEqual<Required<APVendorPrice.effectiveDate>>>>>>>,
                OrderBy<Desc<APVendorPrice.effectiveDate>>>(Base).SelectSingle(
                    e.Row.VendorID,
                    inventoryID,
                    "TOZ",
                    DateTime.Today,
                    DateTime.Today);
            if (apVendorPrice == null) return;
            var apVendorPriceExt = apVendorPrice.GetExtension<ASCIStarAPVendorPriceExt>();
            this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrContractSurcharge>(this.Base.Item.Current, apVendorPriceExt.UsrCommoditySurchargePct);
            this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrContractLossPct>(this.Base.Item.Current, apVendorPriceExt.UsrCommodityLossPct);
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, POVendorInventory.isDefault> e)
        {
            var row = e.Row;
            if (row == null) return;

            bool isDefaultVendor = (bool?)e.NewValue == true && row.GetExtension<ASCIStarPOVendorInventoryExt>().UsrVendorDefault == true;

            SetReadOnlyVendorsFields(this.VendorItems.Cache, row, isDefaultVendor);

            if (isDefaultVendor)
            {
                var rowExt = row.GetExtension<ASCIStarPOVendorInventoryExt>();
                if (rowExt.UsrMarketID == null)
                {
                    row.IsDefault = false;
                    throw new PXSetPropertyException<ASCIStarPOVendorInventoryExt.usrMarketID>("Can't be empty, select Market first!");
                }
                List<POVendorInventory> selectPOVendors = this.Base.VendorItems.Select()?.FirstTableItems.ToList();
                foreach (var vendorInventory in selectPOVendors)
                {
                    if (vendorInventory.IsDefault == true && vendorInventory != row)
                    {
                        this.VendorItems.Cache.SetValue<POVendorInventory.isDefault>(vendorInventory, false);
                        this.VendorItems.View.RequestRefresh();
                        break;
                    }
                }
            }

            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(this.Base.Item.Current);
            UpdateCommodityCostMetal(this.Base.Item.Cache, this.Base.Item.Current, inventoryItemExt);

        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCIStarPOVendorInventoryExt.usrVendorDefault> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = row.GetExtension<ASCIStarPOVendorInventoryExt>();
            if (rowExt.UsrVendorDefault != true) return;

            if (row.IsDefault != true) throw new PXSetPropertyException<POVendorInventory.isDefault>("First make Vendor as default, then use Override logic");

            if (rowExt.UsrCommodityID == null) throw new PXSetPropertyException<ASCIStarPOVendorInventoryExt.usrCommodityID>("Metal Item can not be empty.");
            if (rowExt.UsrCommodityPrice == null) throw new PXSetPropertyException<ASCIStarPOVendorInventoryExt.usrCommodityPrice>("Vendor Price can not be empty.");

            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(this.Base.Item.Current);
            UpdateCommodityCostMetal(this.Base.Item.Cache, this.Base.Item.Current, inventoryItemExt);
        }

        //protected virtual void POVendorInventory_UsrCommodityLossPct_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        //{
        //    if (InvokeBaseHandler != null)
        //        InvokeBaseHandler(cache, e);
        //    PXTrace.WriteInformation("POVendorInventory_UsrCommodityLossPct_FieldUpdated");
        //    POVendorInventory row = e.Row as POVendorInventory;
        //    ASCIStarPOVendorInventoryExt porow = row.GetExtension<ASCIStarPOVendorInventoryExt>();
        //    if (porow != null)
        //    {
        //        PXTrace.WriteInformation("porow != null");
        //        InventoryItem item = InventoryItem.PK.Find(cache.Graph, row.InventoryID);
        //        ASCIStarINInventoryItemExt ext = item.GetExtension<ASCIStarINInventoryItemExt>();
        //        //ext.UsrContractSurcharge = porow.UsrCommodityLossPct + porow.UsrCommoditySurchargePct;
        //        cache.SetValueExt<ASCIStarINInventoryItemExt.usrContractSurcharge>(cache.Current, porow.UsrCommodityLossPct + porow.UsrCommoditySurchargePct);



        //    }
        //}

        //protected virtual void POVendorInventory_UsrCommoditySurchargePct_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        //{
        //    if (InvokeBaseHandler != null)
        //        InvokeBaseHandler(cache, e);
        //    PXTrace.WriteInformation("POVendorInventory_UsrCommoditySurchargePct_FieldUpdated");

        //    POVendorInventory row = e.Row as POVendorInventory;
        //    ASCIStarPOVendorInventoryExt porow = row.GetExtension<ASCIStarPOVendorInventoryExt>();
        //    if (porow != null)
        //    {
        //        PXTrace.WriteInformation("porow != null");
        //        InventoryItem item = InventoryItem.PK.Find(cache.Graph, row.InventoryID);
        //        ASCIStarINInventoryItemExt ext = item.GetExtension<ASCIStarINInventoryItemExt>();
        //        cache.SetValueExt<ASCIStarINInventoryItemExt.usrContractSurcharge>(cache.Current, porow.UsrCommodityLossPct + porow.UsrCommoditySurchargePct);

        //    }
        //}

        //protected virtual void POVendorInventory_UsrCommodityPrice_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        //{
        //    PXTrace.WriteInformation($"Entering POVendorInventory_UsrCommodityPrice_FieldDefaulting");
        //    POVendorInventory row = e.Row as POVendorInventory;
        //    if (row == null || row.VendorID == null)
        //        return;

        //    ASCIStarPOVendorInventoryExt rowExt = row.GetExtension<ASCIStarPOVendorInventoryExt>();
        //    if (rowExt == null || rowExt.UsrCommodityID == null)
        //        return;

        //    ASCIStarMarketCostHelper.JewelryCost costHelper = new ASCIStarMarketCostHelper.JewelryCost(Base, Base.Item.Current);
        //    Vendor v = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(Base, row.VendorID);
        //    costHelper.CostBasis.ItemVendor = v;
        //    ASCIStarVendorExt vExt = v.GetExtension<ASCIStarVendorExt>();

        //    if (row != null)
        //    {
        //        e.NewValue = costHelper.vendorPrice.UsrCommodityPrice;
        //    }
        //}

        //protected virtual void POVendorInventory_UsrCommodityIncrement_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        //{
        //    PXTrace.WriteInformation($"Entering POVendorInventory_UsrCommodityIncrement_FieldDefaulting");
        //    POVendorInventory row = e.Row as POVendorInventory;
        //    if (row == null || row.VendorID == null)
        //        return;

        //    ASCIStarPOVendorInventoryExt rowExt = row.GetExtension<ASCIStarPOVendorInventoryExt>();
        //    if (rowExt == null || rowExt.UsrCommodityID == null)
        //        return;

        //    ASCIStarMarketCostHelper.JewelryCost costHelper = new ASCIStarMarketCostHelper.JewelryCost(Base, Base.Item.Current);
        //    Vendor v = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(Base, row.VendorID);
        //    costHelper.CostBasis.ItemVendor = v;
        //    ASCIStarVendorExt vExt = v.GetExtension<ASCIStarVendorExt>();

        //    if (row != null)
        //    {
        //        e.NewValue = costHelper.vendorPrice.UsrCommodityIncrement;
        //    }
        //}

        //protected virtual void POVendorInventory_UsrCommodityLossPct_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        //{
        //    PXTrace.WriteInformation($"Entering POVendorInventory_UsrCommodityIncrement_FieldDefaulting");
        //    POVendorInventory row = e.Row as POVendorInventory;
        //    if (row == null || row.VendorID == null)
        //        return;

        //    ASCIStarPOVendorInventoryExt rowExt = row.GetExtension<ASCIStarPOVendorInventoryExt>();
        //    if (rowExt == null || rowExt.UsrCommodityID == null)
        //        return;

        //    ASCIStarMarketCostHelper.JewelryCost costHelper = new ASCIStarMarketCostHelper.JewelryCost(Base, Base.Item.Current);
        //    Vendor v = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(Base, row.VendorID);
        //    costHelper.CostBasis.ItemVendor = v;
        //    ASCIStarVendorExt vExt = v.GetExtension<ASCIStarVendorExt>();

        //    if (row != null)
        //    {
        //        e.NewValue = costHelper.vendorPrice.UsrCommodityLossPct;
        //    }
        //}

        //protected virtual void POVendorInventory_UsrCommoditySurchargePct_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        //{
        //    PXTrace.WriteInformation($"Entering POVendorInventory_UsrCommodityIncrement_FieldDefaulting");
        //    POVendorInventory row = e.Row as POVendorInventory;
        //    if (row == null || row.VendorID == null)
        //        return;

        //    ASCIStarPOVendorInventoryExt rowExt = row.GetExtension<ASCIStarPOVendorInventoryExt>();
        //    if (rowExt == null || rowExt.UsrCommodityID == null)
        //        return;

        //    ASCIStarMarketCostHelper.JewelryCost costHelper = new ASCIStarMarketCostHelper.JewelryCost(Base, Base.Item.Current);
        //    Vendor v = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(Base, row.VendorID);
        //    costHelper.CostBasis.ItemVendor = v;
        //    ASCIStarVendorExt vExt = v.GetExtension<ASCIStarVendorExt>();

        //    if (row != null)
        //    {
        //        e.NewValue = costHelper.vendorPrice.UsrCommoditySurchargePct;
        //    }
        //}

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

                PXUIFieldAttribute.SetReadOnly<ASCIStarINInventoryItemExt.usrContractLossPct>(cache, row, !isReadOnly);
            }
        }

        private void SetReadOnlyVendorsFields(PXCache cache, POVendorInventory row, bool isDefaultVendor)
        {
            //PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrCommodityPrice>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrCommodityIncrement>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrCommodityLossPct>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrCommoditySurchargePct>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrCommodityCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrOtherMaterialCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrFabricationCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrPackagingCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrLaborCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrHandlingCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrFreightCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrDutyCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrOtherCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrUnitCost>(cache, row, isDefaultVendor);
        }

        /// <summary>
        /// Method check metal type by details values in Attributes tables from JSMetal attribute
        /// </summary>
        /// <returns>Returns true if gold, false - if silver, and returns null if cannot find Metal Type</returns>
        private bool? GetMetalType()
        {
            if (this.JewelryItemView.Current == null)
                this.JewelryItemView.Current = this.JewelryItemView.Select().TopFirst;

            switch (this.JewelryItemView.Current?.MetalType?.ToUpper())
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

        private void SetValueExtPOVendorInventory<Field>(object newValue) where Field : IBqlField
        {
            POVendorInventory vendorInventory = GetDefaultOverrideVendor();
            if (vendorInventory == null) return;

            this.VendorItems.Cache.SetValueExt<Field>(vendorInventory, newValue);
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
            this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrContractLossPct>(this.Base.Item.Current, decimal.Zero);
            this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrContractSurcharge>(this.Base.Item.Current, decimal.Zero);
        }

        private void UpdateCommodityCostMetal(PXCache cache, InventoryItem row, ASCIStarINInventoryItemExt rowExt)
        {
            if (rowExt == null) throw new PXException("Save Item first!");
            if ((rowExt.UsrActualGRAMSilver == null || rowExt.UsrActualGRAMSilver == 0.0m) && (rowExt.UsrActualGRAMGold == null || rowExt.UsrActualGRAMGold == 0.0m)) return;
            POVendorInventory vendorInventory = GetDefaultOverrideVendor();
            decimal? costFineMetalPerGramm = decimal.Zero;

            ASCIStarMarketCostHelper.JewelryCost jewelryCostProvider = GetCostProvider(row);
            var metalType = GetMetalType();

            if (vendorInventory == null)
            {
                costFineMetalPerGramm = jewelryCostProvider.CostRollupTotal[ASCIStarCostRollupType.Commodity];
            }
            else
            {
                if (metalType == true)
                {
                    var vendorInventoryExt = vendorInventory.GetExtension<ASCIStarPOVendorInventoryExt>();
                    jewelryCostProvider.CostBasis.GoldBasis.EffectiveBasisPerOz = vendorInventoryExt.UsrCommodityPrice.HasValue ? vendorInventoryExt.UsrCommodityPrice.Value : decimal.Zero;

                    decimal goldMultFactor = GetGoldMult().HasValue ? GetGoldMult().Value : 0.0m;
                    costFineMetalPerGramm = jewelryCostProvider.CostBasis.GoldBasis.EffectiveBasisPerOz * goldMultFactor / 24 / 31.10348m * rowExt.UsrActualGRAMGold;
                }
                if (metalType == false)
                {
                    var vendorInventoryExt = vendorInventory.GetExtension<ASCIStarPOVendorInventoryExt>();
                    jewelryCostProvider.CostBasis.SilverBasis.EffectiveBasisPerOz = vendorInventoryExt.UsrCommodityPrice.HasValue ? vendorInventoryExt.UsrCommodityPrice.Value : decimal.Zero;

                    decimal silverMultFactor = GetSilverMult().HasValue ? GetSilverMult().Value : 0.0m;
                    costFineMetalPerGramm = jewelryCostProvider.CostBasis.SilverBasis.EffectiveBasisPerOz * silverMultFactor / 31.10348m * rowExt.UsrActualGRAMSilver;
                }
            }

            decimal? lossValue = metalType == true ? 1.0m + rowExt.UsrContractLossPct / 100.0m : 1.0m; // for silver don't calc loss
            decimal? surchargeValue = 1.0m + rowExt.UsrContractSurcharge / 100.0m;

            rowExt.UsrCommodityCost = costFineMetalPerGramm * lossValue * surchargeValue;
            cache.SetValueExt<ASCIStarINInventoryItemExt.usrCommodityCost>(row, rowExt.UsrCommodityCost);

            UpdateIncrement(cache, row, rowExt, jewelryCostProvider);
        }

        private void UpdatePurchaseContractCost(PXCache cache, InventoryItem row, ASCIStarINInventoryItemExt rowExt)
        {
         //   decimal? laborValue = rowExt.UsrCostingType == ASCIStarCostingType.WeightCost ? rowExt.UsrLaborCost * (rowExt.UsrActualGRAMSilver + rowExt.UsrActualGRAMGold) : rowExt.UsrLaborCost;
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
            bool? metaltype = GetMetalType();
            switch (metaltype)
            {
                case true: UpdateGoldIncrement(row, rowExt, costProvider); return;
                case false: UpdateSilverIncrement(row, rowExt, costProvider); return;
                default: return;
            }
        }

        private void UpdateGoldIncrement(InventoryItem row, ASCIStarINInventoryItemExt rowExt, ASCIStarMarketCostHelper.JewelryCost costProvider)
        {
            if (costProvider == null || costProvider.CostBasis == null || costProvider.CostBasis.GoldBasis == null
                  || costProvider.CostBasis.GoldBasis.EffectiveBasisPerOz == decimal.Zero || costProvider.CostBasis.GoldBasis.EffectiveBasisPerOz == 0.0m) return;

            // decimal? temp1 = costProvider.CostBasis.GoldBasis.BasisPerFineOz[this.JewelryItemView.Current.MetalType?.ToUpper()] / 31.10348m / costProvider.CostBasis.GoldBasis.EffectiveBasisPerOz;
            decimal? goldMultFactor = GetGoldMult();
            decimal? temp1 = costProvider.CostBasis.GoldBasis.EffectiveBasisPerOz * goldMultFactor / 24 / 31.10348m / costProvider.CostBasis.GoldBasis.EffectiveBasisPerOz;
            decimal? temp2 = temp1 * (1.0m + rowExt.UsrContractSurcharge / 100.0m);
            decimal? temp3 = 1.0m;//costProvider.CostBasis.GoldBasis.EffectiveBasisPerOz - costProvider.CostBasis.GoldBasis.EffectiveMarketPerOz;


            decimal? newIncrementValue = (temp3 == 0.0m || temp3 == null) ? temp2 : temp2 * temp3;

            //  decimal? temp11 = costProvider.CostBasis.GoldBasis.EffectiveBasisPerOz / 31.10348m - costProvider.CostBasis.GoldBasis.EffectiveMarketPerOz / 31.10348m;
            newIncrementValue = temp2;

            if (newIncrementValue == rowExt.UsrContractIncrement) return;

            rowExt.UsrContractIncrement = newIncrementValue;
        }

        private void UpdateSilverIncrement(InventoryItem row, ASCIStarINInventoryItemExt rowExt, ASCIStarMarketCostHelper.JewelryCost costProvider)
        {



        }

        private void UpdateSurcharge(PXCache cache, InventoryItem row, ASCIStarINInventoryItemExt rowExt)
        {
            if ((rowExt.UsrActualGRAMSilver == null || rowExt.UsrActualGRAMSilver == 0.0m) && (rowExt.UsrActualGRAMGold == null || rowExt.UsrActualGRAMGold == 0.0m)) return;

            if (rowExt.UsrContractIncrement == null || rowExt.UsrContractIncrement == 0.0m)
            {
                cache.RaiseExceptionHandling<ASCIStarINInventoryItemExt.usrContractSurcharge>(row, 0.0m,
                    new PXSetPropertyException<ASCIStarINInventoryItemExt.usrContractSurcharge>("Surcharge will be negative, uncrease Increment!"));
            }

            ASCIStarMarketCostHelper.JewelryCost costProvider = GetCostProvider(row);

            if (costProvider == null || costProvider.CostBasis == null || costProvider.CostBasis.GoldBasis == null /*|| costProvider.CostBasis.GoldBasis.EffectiveBasisPerOz == decimal.Zero*/) return;

            decimal? temp1 = rowExt?.UsrContractIncrement;// / (costProvider.CostBasis.GoldBasis.EffectiveBasisPerOz - costProvider.CostBasis.GoldBasis.EffectiveMarketPerOz);
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

            if (vendorItem.VendorID == null)
            {
                throw new PXSetPropertyException("No default vendor on Vendors tab");
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

        private POVendorInventory GetDefaultOverrideVendor()
        {
            return this.VendorItems.Select().FirstTableItems
                .FirstOrDefault(x => x.IsDefault == true && x.GetExtension<ASCIStarPOVendorInventoryExt>().UsrVendorDefault == true);
        }
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