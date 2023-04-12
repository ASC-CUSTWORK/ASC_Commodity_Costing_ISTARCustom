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
using ASCISTARCustom.Common.Builder;
using ASCISTARCustom.Common.Helper;

namespace ASCISTARCustom
{
    public class ASCIstarInventoryItemMaintExt : PXGraphExtension<InventoryItemMaint>
    {
        public static bool IsActive() => true;

        #region Selects

        public PXSelect<INKitSpecHdr, Where<INKitSpecHdr.kitInventoryID, Equal<Current<InventoryItem.inventoryID>>>> KitCostRollup;

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

        protected virtual void _(Events.RowSelecting<InventoryItem> e, PXRowSelecting baseEvent)
        {
            if (e.Row == null) return;
            baseEvent(e.Cache, e.Args);

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(e.Row);
            using (new PXConnectionScope())
            {
                //    this.Base.Item.Current = e.Row;
                //  UpdateCommodityCostMetal(e.Cache, e.Row, rowExt);
            }
        }

        protected virtual void _(Events.RowSelected<InventoryItem> e)
        {
            var row = e.Row;
            if (row == null) return;

            bool isVisible = IsVisibleFileds(row.ItemClassID);
            SetVisibleJewelFields(e.Cache, row, isVisible);

            if (this.JewelryItemView.Current == null)
                this.JewelryItemView.Current = this.JewelryItemView.Select();

            bool? baseMetalType = ASCIStarMetalType.GetMetalType(this.JewelryItemView.Current?.MetalType);
            SetReadOnlyJewelFields(e.Cache, row, baseMetalType);

            PXUIFieldAttribute.SetRequired<ASCIStarINJewelryItem.metalType>(this.JewelryItemView.Cache, isVisible);
            PXDefaultAttribute.SetPersistingCheck<ASCIStarINJewelryItem.metalType>(this.JewelryItemView.Cache, this.JewelryItemView.Current, isVisible ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
        }

        protected virtual void _(Events.FieldDefaulting<InventoryItem, ASCIStarINInventoryItemExt.usrCostingType> e)
        {
            if (e.Row == null) return;

            INItemClass itemClass = INItemClass.PK.Find(Base, e.Row.ItemClassID);
            ASCIStarINItemClassExt classExt = itemClass?.GetExtension<ASCIStarINItemClassExt>();
            e.NewValue = classExt.UsrCostingType ?? ASCIStarCostingType.MarketCost;
        }

        protected virtual void _(Events.FieldDefaulting<InventoryItem, ASCIStarINInventoryItemExt.usrCostRollupType> e)
        {
            if (e.Row == null) return;

            INItemClass itemClass = INItemClass.PK.Find(Base, e.Row.ItemClassID);
            ASCIStarINItemClassExt classExt = itemClass?.GetExtension<ASCIStarINItemClassExt>();
            e.NewValue = classExt.UsrCostRollupType ?? ASCIStarCostRollupType.Blank;
        }

        protected virtual void _(Events.FieldVerifying<InventoryItem, ASCIStarINInventoryItemExt.usrContractSurcharge> e)
        {
            var row = e.Row;
            if (row == null) return;
            //if ((decimal?)e.NewValue < 0.0m)
            //{
            //    var result = ASCIStarMetalType.GetMetalType(this.JewelryItemView.Current?.MetalType);
            //    if (result == true)
            //    {
            //        e.Cache.RaiseExceptionHandling<ASCIStarINInventoryItemExt.usrContractIncrement>(row, row.GetExtension<ASCIStarINInventoryItemExt>().UsrContractIncrement,
            //            new PXSetPropertyException("Please, increase increment"));
            //    }

            //    e.Cache.RaiseExceptionHandling<ASCIStarINInventoryItemExt.usrContractSurcharge>(row, e.NewValue, new PXSetPropertyException("Surcharge can not be less 0%!"));
            //}
            //else
            //{
            //    e.Cache.RaiseExceptionHandling<ASCIStarINInventoryItemExt.usrContractSurcharge>(null, null, null);
            //}
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

            var mult = ASCIStarMetalType.GetGoldTypeValue(this.JewelryItemView.Current?.MetalType);

            ASCIStarINInventoryItemExt rowExt = row.GetExtension<ASCIStarINInventoryItemExt>();
            decimal? pricingGRAMGold = rowExt?.UsrActualGRAMGold * mult / 24;
            e.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrPricingGRAMGold>(row, pricingGRAMGold);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrActualGRAMSilver> e)
        {
            var row = e.Row;
            if (row == null || this.JewelryItemView.Current == null) return;

            var value = ASCIStarMetalType.GetSilverTypeValue(this.JewelryItemView.Current?.MetalType);

            var rowExt = row.GetExtension<ASCIStarINInventoryItemExt>();
            e.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrPricingGRAMSilver>(row, rowExt?.UsrActualGRAMSilver * value);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrPricingGRAMGold> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);

            UpdateCommodityCostMetal(e.Cache, row, rowExt);

            var valueMult = ASCIStarMetalType.GetGoldTypeValue(this.JewelryItemView.Current?.MetalType);
            var actualGRAMGold = (decimal?)e.NewValue / valueMult * 24;
            if (actualGRAMGold != rowExt.UsrActualGRAMGold)
            {
                rowExt.UsrActualGRAMGold = actualGRAMGold;
            }
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrPricingGRAMSilver> e)
        {
            var row = e.Row;
            if (row == null) return;

            ASCIStarINInventoryItemExt rowExt = row.GetExtension<ASCIStarINInventoryItemExt>();

            UpdateCommodityCostMetal(e.Cache, row, rowExt);

            var valueMult = ASCIStarMetalType.GetSilverTypeValue(this.JewelryItemView.Current?.MetalType);
            decimal? actualGRAMSilver = 0.0m;

            if (actualGRAMSilver != null && valueMult != 0.0m)
            {
                actualGRAMSilver = (decimal?)e.NewValue / valueMult;
            }
            if (actualGRAMSilver != rowExt.UsrActualGRAMSilver)
            {
                rowExt.UsrActualGRAMSilver = actualGRAMSilver;
            }
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrCommodityCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);
            UpdatUnitCost(e.Cache, row);
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
            var result = ASCIStarMetalType.GetMetalType(this.JewelryItemView.Current?.MetalType);
            if (result == true)
            {
                UpdateSurcharge(e.Cache, row, rowExt);
            }
            else
            {
                UpdateCommodityCostMetal(e.Cache, row, rowExt);
            }

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

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrMaterialsCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);
            UpdatUnitCost(e.Cache, row);

            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrOtherMaterialCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrFabricationCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);
            UpdatUnitCost(e.Cache, row);

            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrFabricationCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrPackagingCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);
            UpdatUnitCost(e.Cache, row);
            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrPackagingCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrOtherCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);
            UpdatUnitCost(e.Cache, row);

            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrOtherCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrLaborCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);
            UpdatUnitCost(e.Cache, row);

            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrLaborCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrFreightCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);
            UpdateLandedCost(e.Cache, row);

            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrFreightCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrDutyCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);
            UpdateLandedCost(e.Cache, row);

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
            UpdateLandedCost(e.Cache, row);
            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrHandlingCost>(e.NewValue);
        }

        #endregion InventoryItem Events

        #region JewelryItem Events
        protected virtual void _(Events.FieldUpdated<ASCIStarINJewelryItem, ASCIStarINJewelryItem.metalType> e)
        {
            var row = e.Row;
            if (row == null) return;

            var result = ASCIStarMetalType.GetMetalType(this.JewelryItemView.Current?.MetalType);
         //   SetReadOnlyJewelFields(this.Base.Item.Cache, this.Base.Item.Current, result);
            SetMetalGramsToZero(result);
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
            var inventoryCD = ASCIStarMetalType.GetMetalType(this.JewelryItemView.Current?.MetalType) == true ? "24K" : "SSS";
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
                    PXTimeZoneInfo.Today,
                    PXTimeZoneInfo.Today);
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
            //  PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrOtherCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrMaterialsCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrPricingGRAMGold>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrPricingGRAMSilver>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrActualGRAMGold>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrActualGRAMSilver>(cache, row, isVisible);
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
                PXUIFieldAttribute.SetReadOnly<ASCIStarINInventoryItemExt.usrContractIncrement>(cache, row, !isReadOnly);
                PXUIFieldAttribute.SetReadOnly<ASCIStarINInventoryItemExt.usrContractLossPct>(cache, row, !isReadOnly);

                PXUIFieldAttribute.SetReadOnly<ASCIStarINInventoryItemExt.usrActualGRAMSilver>(cache, row, isReadOnly);
                PXUIFieldAttribute.SetReadOnly<ASCIStarINInventoryItemExt.usrPricingGRAMSilver>(cache, row, isReadOnly);
                PXUIFieldAttribute.SetReadOnly<ASCIStarINInventoryItemExt.usrMatrixStep>(cache, row, isReadOnly);
            }
        }

        private void SetReadOnlyVendorsFields(PXCache cache, POVendorInventory row, bool isDefaultVendor)
        {
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
            //    PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrOtherCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrUnitCost>(cache, row, isDefaultVendor);
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
                        this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrMatrixStep>(this.Base.Item.Current, decimal.Zero);
                        break;
                    }
                case false:
                    {
                        this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrActualGRAMGold>(this.Base.Item.Current, decimal.Zero);
                        this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrMatrixStep>(this.Base.Item.Current, 0.5m);
                        this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrContractIncrement>(this.Base.Item.Current, decimal.Zero);
                        break;
                    }
                default:
                    {
                        this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrActualGRAMSilver>(this.Base.Item.Current, decimal.Zero);
                        this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrActualGRAMGold>(this.Base.Item.Current, decimal.Zero);
                        break;
                    }
            }
            this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrCommodityCost>(this.Base.Item.Current, decimal.Zero);
            this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrContractLossPct>(this.Base.Item.Current, decimal.Zero);
            this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrContractSurcharge>(this.Base.Item.Current, decimal.Zero);
        }

        private void UpdateCommodityCostMetal(PXCache cache, InventoryItem row, ASCIStarINInventoryItemExt rowExt)
        {
            if (rowExt == null) throw new PXException("Save Item first!");
            if ((rowExt.UsrActualGRAMSilver == null || rowExt.UsrActualGRAMSilver == 0.0m) && (rowExt.UsrActualGRAMGold == null || rowExt.UsrActualGRAMGold == 0.0m)) return;

            var jewelryCostBuilder = CreateCostBuilder(row);

            rowExt.UsrCommodityCost = jewelryCostBuilder.CalculatePreciousMetalCost();
            cache.SetValueExt<ASCIStarINInventoryItemExt.usrCommodityCost>(row, rowExt.UsrCommodityCost);

            if (ASCIStarMetalType.IsGold(jewelryCostBuilder.INJewelryItem.MetalType))
            {
                rowExt.UsrContractIncrement = jewelryCostBuilder.CalculateGoldIncrementValue(row);
            }

            UpdatUnitCost(cache, row);
        }

        private void UpdatUnitCost(PXCache cache, InventoryItem row)
        {
            var newLandedCost = ASCIStarCostBuilder.CalculateUnitCost(row);
            cache.SetValueExt<ASCIStarINInventoryItemExt.usrContractCost>(row, newLandedCost);

            UpdateLandedCost(cache, row);
        }

        private void UpdateLandedCost(PXCache cache, InventoryItem row)
        {
            var newUnitCost = ASCIStarCostBuilder.CalculateLandedCost(row);
            cache.SetValueExt<ASCIStarINInventoryItemExt.usrUnitCost>(row, newUnitCost);
        }

        private void UpdateSurcharge(PXCache cache, InventoryItem row, ASCIStarINInventoryItemExt rowExt)
        {
            if ((rowExt.UsrActualGRAMSilver == null || rowExt.UsrActualGRAMSilver == 0.0m) && (rowExt.UsrActualGRAMGold == null || rowExt.UsrActualGRAMGold == 0.0m)) return;

            //if (rowExt.UsrContractIncrement == null || rowExt.UsrContractIncrement == 0.0m)
            //{
            //    cache.RaiseExceptionHandling<ASCIStarINInventoryItemExt.usrContractSurcharge>(row, 0.0m,
            //        new PXSetPropertyException<ASCIStarINInventoryItemExt.usrContractSurcharge>("Surcharge will be negative, uncrease Increment!"));
            //}

            var costBuilder = CreateCostBuilder(row);
            var surchargeValue = costBuilder.CalculateSurchargeValue(row);
            cache.SetValueExt<ASCIStarINInventoryItemExt.usrContractSurcharge>(row, surchargeValue);
        }

        private ASCIStarCostBuilder CreateCostBuilder(InventoryItem currentRow)
        {
            var defaultVendor = Base.VendorItems.Select().RowCast<POVendorInventory>().FirstOrDefault(_ => _.IsDefault == true);
            if (defaultVendor != null)
            {
                return new ASCIStarCostBuilder(this.Base)
                            .WithInventoryItem(currentRow)
                            .WithPOVendorInventory(defaultVendor)
                            .WithINJewelryItem(this.JewelryItemView.Current)
                            .Build();
            }

            throw new PXSetPropertyException("No default vendor on Vendors tab");
        }

        private POVendorInventory GetDefaultOverrideVendor() => this.VendorItems.Select().FirstTableItems
                .FirstOrDefault(x => x.IsDefault == true && x.GetExtension<ASCIStarPOVendorInventoryExt>().UsrVendorDefault == true);

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