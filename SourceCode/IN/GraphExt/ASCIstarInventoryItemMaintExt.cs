using ASCISTARCustom.AP.CacheExt;
using ASCISTARCustom.AP.DAC;
using ASCISTARCustom.Common.Builder;
using ASCISTARCustom.Common.Descriptor;
using ASCISTARCustom.Common.DTO.Interfaces;
using ASCISTARCustom.Common.Helper;
using ASCISTARCustom.IN.CacheExt;
using ASCISTARCustom.IN.DAC;
using ASCISTARCustom.IN.Descriptor.Constants;
using ASCISTARCustom.PO.CacheExt;
using PX.Common;
using PX.CS;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CR.Standalone;
using PX.Objects.IN;
using PX.Objects.PO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static ASCISTARCustom.Common.Descriptor.ASCIStarConstants;

namespace ASCISTARCustom.IN.GraphExt
{
    public class ASCIStarInventoryItemMaintExt : PXGraphExtension<InventoryItemMaint>
    {
        public static bool IsActive() => true;

        #region Selects

        public POVendorInventorySelect<POVendorInventory,
            InnerJoin<Vendor, On<BqlOperand<Vendor.bAccountID, IBqlInt>.IsEqual<POVendorInventory.vendorID>>,
            LeftJoin<Location, On<BqlChainableConditionBase<TypeArrayOf<IBqlBinary>
                .FilledWith<And<Compare<Location.bAccountID, Equal<POVendorInventory.vendorID>>>>>
                .And<BqlOperand<Location.locationID, IBqlInt>.IsEqual<POVendorInventory.vendorLocationID>>>>>,
            Where<POVendorInventory.inventoryID, Equal<Current<InventoryItem.inventoryID>>,
                And<Where<Vendor.baseCuryID, Equal<Current<AccessInfo.baseCuryID>>, Or<Vendor.baseCuryID, IsNull>>>>, InventoryItem> VendorItems;

        public SelectFrom<ASCIStarINJewelryItem>.Where<ASCIStarINJewelryItem.inventoryID.IsEqual<InventoryItem.inventoryID.FromCurrent>>.View JewelryItemView;

        public SelectFrom<ASCIStarINVendorDuty>.Where<ASCIStarINVendorDuty.inventoryID.IsEqual<InventoryItem.inventoryID.FromCurrent>>.View VendorDutyView;

        [PXFilterable]
        [PXCopyPasteHiddenView(IsHidden = true)]
        public SelectFrom<ASCIStarINCompliance>.Where<ASCIStarINCompliance.inventoryID.IsEqual<InventoryItem.inventoryID.FromCurrent>>.View ComplianceView;

        #endregion Selects

        #region Cache Attached
        [PXMergeAttributes(Method = MergeMethod.Replace)]
        [PXDBString(30, IsUnicode = true, InputMask = "####.##.####")]
        [PXSelector(typeof(SearchFor<ASCIStarAPTariffHTSCode.hSTariffCode>))]
        protected virtual void _(Events.CacheAttached<InventoryItem.hSTariffCode> e) { }
        #endregion

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

        #region Event Handlers

        #region InventoryItem Events

        protected virtual void _(Events.FieldSelecting<InventoryItem, ASCIStarINInventoryItemExt.usrBasisValue> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);
            UpdateCommodityCostMetal(e.Cache, row, rowExt);
            e.ReturnValue = rowExt.UsrBasisValue;
        }

        protected virtual void _(Events.RowSelected<InventoryItem> e)
        {
            var row = e.Row;
            if (row == null) return;

            bool isVisible = IsVisibleFields(row.ItemClassID);
            SetVisibleJewelFields(e.Cache, row, isVisible);

            if (this.JewelryItemView.Current == null)
                this.JewelryItemView.Current = this.JewelryItemView.Select();

            SetReadOnlyJewelAttrFields(e.Cache, row, this.JewelryItemView.Current?.MetalType);

            PXUIFieldAttribute.SetRequired<ASCIStarINJewelryItem.metalType>(this.JewelryItemView.Cache, isVisible);
            PXDefaultAttribute.SetPersistingCheck<ASCIStarINJewelryItem.metalType>(this.JewelryItemView.Cache, this.JewelryItemView.Current,
                isVisible ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
        }

        protected virtual void _(Events.FieldDefaulting<InventoryItem, ASCIStarINInventoryItemExt.usrCostingType> e)
        {
            if (e.Row == null) return;

            INItemClass itemClass = INItemClass.PK.Find(Base, e.Row.ItemClassID);
            ASCIStarINItemClassExt classExt = itemClass?.GetExtension<ASCIStarINItemClassExt>();
            e.NewValue = classExt?.UsrCostingType ?? ASCIStarConstants.CostingType.ContractCost;
        }

        //protected virtual void _(Events.FieldDefaulting<InventoryItem, ASCIStarINInventoryItemExt.usrCostRollupType> e)
        //{
        //    if (e.Row == null) return;

        //    INItemClass itemClass = INItemClass.PK.Find(Base, e.Row.ItemClassID);
        //    ASCIStarINItemClassExt classExt = itemClass?.GetExtension<ASCIStarINItemClassExt>();
        //    e.NewValue = classExt?.UsrCostRollupType ?? ASCIStarConstants.CostRollupType.Blank;
        //}

        protected virtual void _(Events.FieldVerifying<InventoryItem, ASCIStarINInventoryItemExt.usrMatrixStep> e)
        {
            if (e.Row == null) return;

            if ((decimal?)e.NewValue <= 0.0m)
            {
                e.Cache.RaiseExceptionHandling<ASCIStarINInventoryItemExt.usrMatrixStep>(e.Row, 0.5m,
                    new PXSetPropertyException(ASCIStarINConstants.Errors.ERPTakeMarketPrice, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldVerifying<InventoryItem, ASCIStarINInventoryItemExt.usrContractSurcharge> e)
        {
            if (e.Row == null) return;

            if ((decimal?)e.NewValue < 0.0m)
            {
                e.Cache.RaiseExceptionHandling<ASCIStarINInventoryItemExt.usrContractSurcharge>(e.Row, e.NewValue,
                    new PXSetPropertyException(ASCIStarINConstants.Warnings.SurchargeIsNegative, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldVerifying<InventoryItem, ASCIStarINInventoryItemExt.usrCostingType> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);

            var defaultVendor = GetDefaultVendor();
            if (defaultVendor == null) return;

            var vendorItemExt = PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(defaultVendor);
            if (rowExt != null && e.NewValue?.ToString() != ASCIStarConstants.CostingType.ContractCost && true == defaultVendor.IsDefault == vendorItemExt.UsrIsOverrideVendor)
            {
                Base.Item.Cache.RaiseExceptionHandling<ASCIStarINInventoryItemExt.usrCostingType>(row, e.NewValue,
                    new PXSetPropertyException(ASCIStarINConstants.Warnings.CostingTypeIsNotContract, PXErrorLevel.RowWarning));
            }
        }

        protected virtual void _(Events.FieldUpdating<InventoryItem, InventoryItem.descr> e)
        {
            var row = e.Row;
            if (row == null) return;

            this.JewelryItemView.SetValueExt<ASCIStarINJewelryItem.shortDesc>(this.JewelryItemView.Current, e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, InventoryItem.itemClassID> e)
        {
            var row = e.Row;
            if (e.Row == null) return;

            bool isVisible = IsVisibleFields(row.ItemClassID);
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
            if (row == null || Base.IsCopyPasteContext) return;

            var mult = ASCIStarMetalType.GetGoldTypeValue(this.JewelryItemView.Current?.MetalType);

            decimal? pricingGRAMGold = (decimal?)e.NewValue * mult / 24;
            e.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrPricingGRAMGold>(row, pricingGRAMGold);

            RecalculateInventoryFabricationValue(row);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrActualGRAMSilver> e)
        {
            var row = e.Row;
            if (row == null || Base.IsCopyPasteContext) return;

            var value = ASCIStarMetalType.GetSilverTypeValue(this.JewelryItemView.Current?.MetalType);

            e.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrPricingGRAMSilver>(row, (decimal?)e.NewValue * value);

            RecalculateInventoryFabricationValue(row);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrPricingGRAMGold> e)
        {
            var row = e.Row;
            if (row == null || Base.IsCopyPasteContext) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);

            UpdateCommodityCostMetal(e.Cache, row, rowExt);

            var valueMult = ASCIStarMetalType.GetGoldTypeValue(this.JewelryItemView.Current?.MetalType);

            var actualGRAMGold = (decimal?)e.NewValue / valueMult * 24;
            if (actualGRAMGold != rowExt.UsrActualGRAMGold)
            {
                rowExt.UsrActualGRAMGold = actualGRAMGold;
            }

            RecalculateInventoryFabricationValue(row);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrPricingGRAMSilver> e)
        {
            var row = e.Row;
            if (row == null || Base.IsCopyPasteContext) return;

            ASCIStarINInventoryItemExt rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);

            UpdateCommodityCostMetal(e.Cache, row, rowExt);

            var valueMult = ASCIStarMetalType.GetSilverTypeValue(this.JewelryItemView.Current?.MetalType);

            var actualGramSilver = (decimal?)e.NewValue / valueMult;
            if (actualGramSilver != rowExt.UsrActualGRAMSilver)
            {
                rowExt.UsrActualGRAMSilver = actualGramSilver;
            }

            RecalculateInventoryFabricationValue(row);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrPreciousMetalCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrPreciousMetalCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrUnitCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);
            decimal? newValue = (decimal?)e.NewValue;

            rowExt.UsrDutyCost = rowExt.UsrDutyCostPct * newValue / 100.0m;
            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrDutyCost>(rowExt.UsrDutyCost);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrContractIncrement> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);

            var isGold = ASCIStarMetalType.IsGold(this.JewelryItemView.Current?.MetalType);

            if (isGold == true)
            {
                UpdateSurcharge<ASCIStarINInventoryItemExt.usrContractSurcharge>(e.Cache, row, rowExt, this.JewelryItemView.Current?.MetalType);
                UpdateCommodityCostMetal(e.Cache, row, rowExt);
            }

            //var isSilver = ASCIStarMetalType.IsGold(this.JewelryItemView.Current?.MetalType);
            //if (isSilver)
            //{
            //    UpdateCommodityCostMetal(e.Cache, row, rowExt);
            //}

            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrContractIncrement>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrMatrixStep> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);
            UpdateCommodityCostMetal(e.Cache, row, rowExt);

            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrMatrixStep>(e.NewValue);
            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrBasisValue>(rowExt.UsrBasisValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrContractSurcharge> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);
            UpdateCommodityCostMetal(e.Cache, row, rowExt);

            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrContractSurcharge>((decimal?)e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrContractLossPct> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);
            UpdateCommodityCostMetal(e.Cache, row, rowExt);

            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrContractLossPct>((decimal?)e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrOtherMaterialsCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrOtherMaterialsCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrFabricationCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrFabricationCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrPackagingCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrPackagingCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrPackagingLaborCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrPackagingLaborCost>(e.NewValue);
        }

        //protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrOtherCost> e)
        //{
        //    var row = e.Row;
        //    if (row == null) return;

        //    UpdatUnitCost(e.Cache, row);
        //    SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrOtherCost>(e.NewValue);
        //}

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrLaborCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrLaborCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrFreightCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrFreightCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrDutyCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);

            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrDutyCost>(e.NewValue);

            if (rowExt.UsrUnitCost == null || rowExt.UsrUnitCost == 0.0m)
            {
                rowExt.UsrDutyCostPct = decimal.Zero;
                return;
            }
            decimal? newCostPctValue = (decimal?)e.NewValue / rowExt.UsrUnitCost * 100.0m;
            if (newCostPctValue == rowExt.UsrDutyCostPct) return;
            rowExt.UsrDutyCostPct = newCostPctValue;
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrDutyCostPct> e)
        {
            var row = e.Row;
            if (row == null) return;

            ASCIStarINInventoryItemExt rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);

            decimal? newDutyCostValue = rowExt.UsrUnitCost * (decimal?)e.NewValue / 100.00m;
            if (newDutyCostValue == rowExt.UsrDutyCost) return;
            e.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrDutyCost>(row, newDutyCostValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrHandlingCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrHandlingCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrCommodityType> e)
        {
            var row = e.Row;
            if (row == null || e.NewValue == null) return;

            e.Cache.RaiseExceptionHandling<ASCIStarINInventoryItemExt.usrCommodityType>(row, e.NewValue,
                new PXSetPropertyException(ASCIStarINConstants.Warnings.MetalTypeEmpty, PXErrorLevel.Warning));

            this.JewelryItemView.SetValueExt<ASCIStarINJewelryItem.metalType>(this.JewelryItemView.Current, null);
            JewelryItemView.Cache.RaiseExceptionHandling<ASCIStarINJewelryItem.metalType>(JewelryItemView.Current, null,
                new PXSetPropertyException(ASCIStarINConstants.Warnings.SelectMetalType, PXErrorLevel.Warning));

            ASCIStarINInventoryItemExt rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);
        }

        #endregion InventoryItem Events

        #region JewelryItem Events

        protected virtual void _(Events.FieldUpdated<ASCIStarINJewelryItem, ASCIStarINJewelryItem.metalType> e)
        {
            var row = e.Row;
            if (row == null || this.Base.Item.Current == null) return;

            UpdateMetalGrams(e.NewValue?.ToString());
            UpdateCommodityCostMetal(this.Base.Item.Cache, this.Base.Item.Current, PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(this.Base.Item.Current));
        }

        #endregion JewelryItem Events

        #region POVendorInventory Events
        protected virtual void _(Events.RowSelected<POVendorInventory> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetReadOnlyPOVendorInventoryFields(e.Cache, row);
            SetVisiblePOVendorInventoryFields(e.Cache);

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(row);
            if (rowExt?.UsrIsOverrideVendor == true)
            {
                e.Cache.RaiseExceptionHandling<ASCIStarPOVendorInventoryExt.usrUnitCost>(row, rowExt.UsrUnitCost,
                      new PXSetPropertyException(ASCIStarINConstants.Warnings.UnitCostIsCustom, PXErrorLevel.Warning));
            }
            else
            {
                e.Cache.RaiseExceptionHandling<ASCIStarPOVendorInventoryExt.usrUnitCost>(row, rowExt.UsrUnitCost, null);
            }
        }

        protected virtual void _(Events.FieldSelecting<POVendorInventory, ASCIStarPOVendorInventoryExt.usrFabricationCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var poVendorInventoryExt = row.GetExtension<ASCIStarPOVendorInventoryExt>();
            var calculatedFabricationValue = CalculateFabricationValue(row);

            if (poVendorInventoryExt.UsrFabricationCost != calculatedFabricationValue)
            {
                e.Cache.RaiseExceptionHandling<ASCIStarPOVendorInventoryExt.usrFabricationCost>(row, poVendorInventoryExt.UsrFabricationCost,
                    new PXSetPropertyException(ASCIStarINConstants.Warnings.FabricationCostMismatch, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldVerifying<POVendorInventory, POVendorInventory.isDefault> e)
        {
            var row = e.Row;
            if (row == null || (bool)e.NewValue != true) return;

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(row);
            if (rowExt.UsrMarketID == null)
            {
                e.Cache.RaiseExceptionHandling<ASCIStarPOVendorInventoryExt.usrMarketID>(row, false, new PXSetPropertyException(ASCIStarINConstants.Errors.MarketEmpty, PXErrorLevel.RowError));
            }

            var inventoryID = ASCIStarMetalType.GetBaseInventoryID(this.Base, this.JewelryItemView.Current?.MetalType);

            var apVendorPrice = ASCIStarCostBuilder.GetAPVendorPrice(this.Base, row.VendorID, inventoryID, ASCIStarConstants.TOZ.value, PXTimeZoneInfo.Today);

            if (apVendorPrice == null && PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(row).UsrIsOverrideVendor != true)
            {
                e.Cache.RaiseExceptionHandling<POVendorInventory.isDefault>(row, false,
                    new PXSetPropertyException(ASCIStarMessages.Error.VendorPriceNotFound, PXErrorLevel.RowWarning));
            }

            List<POVendorInventory> selectPOVendors = VendorItems.Select()?.FirstTableItems.ToList();
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

        protected virtual void _(Events.FieldVerifying<POVendorInventory, ASCIStarPOVendorInventoryExt.usrIsOverrideVendor> e)
        {
            var row = e.Row;
            if (row == null || row.IsDefault != true) return;

            bool newValue = (bool)e.NewValue;
            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(row);

            if (newValue == false) return;

            if (rowExt.UsrCommodityVendorPrice == decimal.Zero)
            {
                e.Cache.RaiseExceptionHandling<ASCIStarPOVendorInventoryExt.usrCommodityVendorPrice>(row, rowExt.UsrCommodityVendorPrice,
                    new PXSetPropertyException(ASCIStarMessages.Error.POVendorInventoryVendorPriceEmpty, PXErrorLevel.Error));
            }

            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(Base.Item.Current);
            if (inventoryItemExt != null && inventoryItemExt.UsrCostingType != ASCIStarConstants.CostingType.ContractCost)
            {
                Base.Item.Cache.RaiseExceptionHandling<ASCIStarINInventoryItemExt.usrCostingType>(Base.Item.Current, inventoryItemExt.UsrCostingType,
                    new PXSetPropertyException(ASCIStarINConstants.Warnings.CostingTypeIsNotContract, PXErrorLevel.RowWarning));
            }
        }

        protected virtual void _(Events.FieldVerifying<POVendorInventory, ASCIStarPOVendorInventoryExt.usrCommodityVendorPrice> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(row);
            if ((decimal)e.NewValue == decimal.Zero && rowExt.UsrIsOverrideVendor == true)
            {
                e.Cache.RaiseExceptionHandling<ASCIStarPOVendorInventoryExt.usrCommodityVendorPrice>(row, rowExt.UsrBasisPrice,
                    new PXSetPropertyException(ASCIStarMessages.Error.POVendorInventoryVendorPriceEmpty, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldVerifying<POVendorInventory, ASCIStarPOVendorInventoryExt.usrBasisPrice> e)
        {
            var row = e.Row;
            if (row == null) return;

            if ((decimal?)e.NewValue == decimal.Zero)
            {
                e.Cache.RaiseExceptionHandling<ASCIStarPOVendorInventoryExt.usrBasisPrice>(row, e.NewValue,
                    new PXSetPropertyException(ASCIStarINConstants.Warnings.BasisOrMarketPriceEmpty, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldVerifying<POVendorInventory, ASCIStarPOVendorInventoryExt.usrMatrixStep> e)
        {
            if (e.Row == null) return;

            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(this.Base.Item.Current);
            if ((decimal?)e.NewValue <= 0.0m && inventoryItemExt.UsrCommodityType == ASCIStarConstants.CommodityType.Silver)
            {
                e.Cache.RaiseExceptionHandling<ASCIStarPOVendorInventoryExt.usrMatrixStep>(e.Row, 0.5m,
                    new PXSetPropertyException(ASCIStarINConstants.Errors.ERPTakeMarketPrice, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldVerifying<POVendorInventory, ASCIStarPOVendorInventoryExt.usrContractSurcharge> e)
        {
            if (e.Row == null) return;

            var sdfs = e.Row.GetExtension<ASCIStarPOVendorInventoryExt>();
            if ((decimal?)e.NewValue < 0.0m)
            {
                e.Cache.RaiseExceptionHandling<ASCIStarPOVendorInventoryExt.usrContractSurcharge>(e.Row, e.NewValue,
                    new PXSetPropertyException(ASCIStarINConstants.Warnings.SurchargeIsNegative, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCIStarPOVendorInventoryExt.usrIsOverrideVendor> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(row);

            UpdateItemAndPOVendorInventory(e.Cache, row, rowExt);
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, POVendorInventory.isDefault> e)
        {
            var row = e.Row;
            if (row == null || Base.IsCopyPasteContext) return;

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(row);
            if ((bool)e.NewValue == true)
            {
                CopyPOVendorInventoryToItem(row);
            }

            UpdateItemAndPOVendorInventory(e.Cache, row, rowExt);
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, POVendorInventory.vendorID> e)
        {
            var row = e.Row;
            if (row == null || e.NewValue == null || Base.IsCopyPasteContext) return;

            Vendor vendor = Vendor.PK.Find(Base, (int?)e.NewValue);
            ASCIStarVendorExt vendorExt = vendor?.GetExtension<ASCIStarVendorExt>();
            e.Cache.SetValue<ASCIStarPOVendorInventoryExt.usrMarketID>(row, vendorExt.UsrMarketID);

            var inventoryID = ASCIStarMetalType.GetBaseInventoryID(this.Base, this.JewelryItemView.Current?.MetalType);

            var apVendorPrice = ASCIStarCostBuilder.GetAPVendorPrice(this.Base, vendor.BAccountID, inventoryID, ASCIStarConstants.TOZ.value, PXTimeZoneInfo.Today);

            if (apVendorPrice == null)
            {
                e.Cache.RaiseExceptionHandling<POVendorInventory.vendorID>(row, e.NewValue,
                    new PXSetPropertyException(ASCIStarINConstants.Warnings.BasisOrMarketPriceEmpty, PXErrorLevel.Warning));
                return;
            }

            var apVendorPriceExt = apVendorPrice.GetExtension<ASCIStarAPVendorPriceExt>();
            e.Cache.SetValueExt<ASCIStarPOVendorInventoryExt.usrContractLossPct>(row, apVendorPriceExt.UsrCommodityLossPct ?? 0.0m);
            e.Cache.SetValueExt<ASCIStarPOVendorInventoryExt.usrMatrixStep>(row, apVendorPriceExt.UsrMatrixStep ?? 0.0m);
            e.Cache.SetValueExt<ASCIStarPOVendorInventoryExt.usrBasisValue>(row, apVendorPriceExt.UsrBasisValue ?? 0.0m);
            e.Cache.SetValueExt<ASCIStarPOVendorInventoryExt.usrCommodityVendorPrice>(row, apVendorPrice.SalesPrice ?? 0.0m);
            e.Cache.SetValueExt<ASCIStarPOVendorInventoryExt.usrBasisPrice>(row, apVendorPrice.SalesPrice ?? 0.0m);
            e.Cache.SetValueExt<ASCIStarPOVendorInventoryExt.usrContractSurcharge>(row, apVendorPriceExt.UsrCommoditySurchargePct ?? 0.0m);
            //e.Cache.SetValueExt<ASCIStarPOVendorInventoryExt.usrFabricationWeight>(row, apVendorPriceExt.UsrLaborPerUnit ?? 0.0m);

            if (row.IsDefault == true)
            {
                if (this.Base.Item.Current != null)
                {
                    var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(this.Base.Item.Current);

                    inventoryItemExt.UsrContractSurcharge = apVendorPriceExt.UsrCommoditySurchargePct;
                    inventoryItemExt.UsrContractLossPct = apVendorPriceExt.UsrCommodityLossPct;
                    inventoryItemExt.UsrMatrixStep = apVendorPriceExt.UsrMatrixStep;
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCIStarPOVendorInventoryExt.usrFabricationWeight> e)
        {
            var row = e.Row;
            if (row == null) return;

            RecalculatePOVendorFabricationValue(row);
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCIStarPOVendorInventoryExt.usrFabricationPiece> e)
        {
            var row = e.Row;
            if (row == null) return;

            RecalculatePOVendorFabricationValue(row);
        }

        //protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCIStarPOVendorInventoryExt.usrFabricationCost> e)
        //{
        //    var row = e.Row;
        //    if (row == null) return;

        //    var poVendorExt = row.GetExtension<ASCIStarPOVendorInventoryExt>();

        //    var metalWeight = GetMetalWeight();

        //    var usrLaborPerUnit = metalWeight == 0
        //        ? (decimal?)e.NewValue
        //        : (decimal?)e.NewValue / metalWeight;

        //    if (usrLaborPerUnit != poVendorExt.UsrFabricationWeight)
        //    {
        //        poVendorExt.UsrFabricationWeight = usrLaborPerUnit;
        //    }
        //}

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCIStarPOVendorInventoryExt.usrCommodityVendorPrice> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(row);
            UpdateItemAndPOVendorInventory(e.Cache, row, rowExt);
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCIStarINInventoryItemExt.usrContractIncrement> e)
        {
            var row = e.Row;
            if (row == null || row.IsDefault == true) return;

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(row);

            var isGold = ASCIStarMetalType.IsGold(this.JewelryItemView.Current?.MetalType);
            if (isGold == true)
            {
                UpdateSurcharge<ASCIStarPOVendorInventoryExt.usrContractSurcharge>(e.Cache, row, rowExt, this.JewelryItemView.Current?.MetalType);
            }
            else
            {
                var isSilver = ASCIStarMetalType.IsSilver(this.JewelryItemView.Current?.MetalType);
                if (isSilver)
                {
                    UpdateMetalCalcPOVendorItem(e.Cache, row, rowExt);
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCIStarPOVendorInventoryExt.usrMatrixStep> e)
        {
            var row = e.Row;
            if (row == null || row.IsDefault == true) return;

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(row);
            UpdateMetalCalcPOVendorItem(e.Cache, row, rowExt);
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCIStarPOVendorInventoryExt.usrContractSurcharge> e)
        {
            var row = e.Row;
            if (row == null) return;

            if (row.IsDefault != true)
            {
                var rowExt = PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(row);
                UpdateMetalCalcPOVendorItem(e.Cache, row, rowExt);
            }
        }



        #endregion POVendorInventory Events

        #region Compliance Events
        public virtual void _(Events.FieldSelecting<ASCIStarINCompliance, ASCIStarINCompliance.customerAlphaCode> e)
        {
            SetStringList<ASCIStarINCompliance.customerAlphaCode>(e.Cache, ASCIStarINConstants.INJewelryAttributesID.CustomerCode);
        }

        public virtual void _(Events.FieldSelecting<ASCIStarINCompliance, ASCIStarINCompliance.division> e)
        {
            SetStringList<ASCIStarINCompliance.division>(e.Cache, ASCIStarINConstants.INJewelryAttributesID.InventoryCategory);
        }

        public virtual void _(Events.FieldSelecting<ASCIStarINCompliance, ASCIStarINCompliance.testingLab> e)
        {
            SetStringList<ASCIStarINCompliance.testingLab>(e.Cache, ASCIStarINConstants.INJewelryAttributesID.CPTESTTYPE);
        }

        public virtual void _(Events.FieldSelecting<ASCIStarINCompliance, ASCIStarINCompliance.protocolTestedTo> e)
        {
            SetStringList<ASCIStarINCompliance.protocolTestedTo>(e.Cache, ASCIStarINConstants.INJewelryAttributesID.CPPROTOCOL);
        }

        public virtual void _(Events.FieldSelecting<ASCIStarINCompliance, ASCIStarINCompliance.waiverReasonCode> e)
        {
            SetStringList<ASCIStarINCompliance.waiverReasonCode>(e.Cache, ASCIStarINConstants.INJewelryAttributesID.REASONCODE);
        }
        #endregion Compliance Events

        #region VendorDuty Events
        protected virtual void _(Events.FieldUpdated<ASCIStarINVendorDuty, ASCIStarINVendorDuty.vendorID> e)
        {
            if (e.Row == null) return;

            e.Cache.RaiseFieldDefaulting<ASCIStarINVendorDuty.countryID>(e.Row, out object countryID);
            e.Cache.SetValueExt<ASCIStarINVendorDuty.countryID>(e.Row, countryID);
        }
        #endregion
       
        #endregion Event Handlers

        #region Helper Methods

        protected virtual void SetVisibleJewelFields(PXCache cache, InventoryItem row, bool isVisible)
        {
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrUnitCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrEstLandedCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrFabricationCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrPreciousMetalCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrFreightCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrLaborCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrDutyCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrDutyCostPct>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrHandlingCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrPackagingCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrOtherMaterialsCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrPackagingLaborCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrContractLossPct>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrContractIncrement>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrBasisValue>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrContractSurcharge>(cache, row, isVisible);

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(row);

            bool isVisibleGold = isVisible && rowExt.UsrCommodityType == ASCIStarConstants.CommodityType.Gold;
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrActualGRAMGold>(cache, row, isVisibleGold);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrPricingGRAMGold>(cache, row, isVisibleGold);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrContractIncrement>(cache, row, isVisibleGold);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrIncrement>(cache, row, isVisibleGold);

            bool isVisibleSilver = isVisible && rowExt.UsrCommodityType == ASCIStarConstants.CommodityType.Silver;
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrActualGRAMSilver>(cache, row, isVisibleSilver);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrPricingGRAMSilver>(cache, row, isVisibleSilver);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrMatrixStep>(cache, row, isVisibleSilver);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrMatrixPriceGram>(cache, row, isVisibleSilver);
            PXUIFieldAttribute.SetVisible<ASCIStarINInventoryItemExt.usrMatrixPriceTOZ>(cache, row, isVisibleSilver);
        }

        protected virtual bool IsVisibleFields(int? itemClassID)
        {
            INItemClass itemClass = INItemClass.PK.Find(Base, itemClassID);

            return itemClass?.ItemClassCD.Trim() != "COMMODITY" && PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(this.Base.Item.Current)?.UsrCommodityType != ASCIStarConstants.CommodityType.Undefined;
            // acupower: remove from constant to jewel preferences screen and find from rowSelected
        }

        protected virtual void SetReadOnlyJewelAttrFields(PXCache cache, InventoryItem row, string metalType)
        {
            bool isNotGold = !ASCIStarMetalType.IsGold(metalType);
            bool isNotSilver = !ASCIStarMetalType.IsSilver(metalType);

            PXUIFieldAttribute.SetReadOnly<ASCIStarINInventoryItemExt.usrActualGRAMGold>(cache, row, isNotGold);
            PXUIFieldAttribute.SetReadOnly<ASCIStarINInventoryItemExt.usrPricingGRAMGold>(cache, row, isNotGold);
            PXUIFieldAttribute.SetReadOnly<ASCIStarINInventoryItemExt.usrContractIncrement>(cache, row, isNotGold);

            PXUIFieldAttribute.SetReadOnly<ASCIStarINInventoryItemExt.usrActualGRAMSilver>(cache, row, isNotSilver);
            PXUIFieldAttribute.SetReadOnly<ASCIStarINInventoryItemExt.usrPricingGRAMSilver>(cache, row, isNotSilver);
            PXUIFieldAttribute.SetReadOnly<ASCIStarINInventoryItemExt.usrMatrixStep>(cache, row, isNotSilver);

            if (isNotGold && isNotSilver)
            {
                PXUIFieldAttribute.SetReadOnly<ASCIStarINInventoryItemExt.usrPricingGRAMGold>(cache, row, true);
                PXUIFieldAttribute.SetReadOnly<ASCIStarINInventoryItemExt.usrPricingGRAMSilver>(cache, row, true);
                PXUIFieldAttribute.SetReadOnly<ASCIStarINInventoryItemExt.usrActualGRAMGold>(cache, row, true);
                PXUIFieldAttribute.SetReadOnly<ASCIStarINInventoryItemExt.usrActualGRAMSilver>(cache, row, true);
            }
        }

        protected virtual void SetReadOnlyPOVendorInventoryFields(PXCache cache, POVendorInventory row)
        {
            bool isDefaultVendor = row.IsDefault == true;
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrContractIncrement>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrContractLossPct>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrContractSurcharge>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrPreciousMetalCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrOtherMaterialsCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrFabricationCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrPackagingCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrLaborCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrPackagingLaborCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrHandlingCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrFreightCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrDutyCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrMatrixStep>(cache, row, isDefaultVendor);

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(row);
            PXUIFieldAttribute.SetReadOnly<ASCIStarPOVendorInventoryExt.usrCommodityVendorPrice>(cache, row, rowExt.UsrIsOverrideVendor != true);
        }

        protected virtual void SetVisiblePOVendorInventoryFields(PXCache cache)
        {
            if (this.Base.Item.Current == null) return;
            var intentoryItemExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(this.Base.Item.Current);

            bool isVisible = intentoryItemExt.UsrCommodityType == ASCIStarConstants.CommodityType.Silver;

            PXUIFieldAttribute.SetVisible<ASCIStarPOVendorInventoryExt.usrFloor>(cache, null, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarPOVendorInventoryExt.usrCeiling>(cache, null, isVisible);
            PXUIFieldAttribute.SetVisible<ASCIStarPOVendorInventoryExt.usrMatrixStep>(cache, null, isVisible);
        }

        protected virtual void UpdateCommodityCostMetal(PXCache cache, InventoryItem row, ASCIStarINInventoryItemExt rowExt)
        {
            if (rowExt == null) throw new PXException(ASCIStarINConstants.Errors.NullInCacheSaveItemFirst);

            var jewelCostBuilder = CreateCostBuilder(rowExt);
            if (jewelCostBuilder == null) return;

            rowExt.UsrPreciousMetalCost = jewelCostBuilder.CalculatePreciousMetalCost(rowExt.UsrCostingType);
            cache.SetValueExt<ASCIStarINInventoryItemExt.usrPreciousMetalCost>(row, rowExt.UsrPreciousMetalCost);

            cache.SetValueExt<ASCIStarINInventoryItemExt.usrMarketPriceTOZ>(row, jewelCostBuilder.PreciousMetalMarketCostPerTOZ);
            cache.SetValueExt<ASCIStarINInventoryItemExt.usrMarketPriceGram>(row, jewelCostBuilder.PreciousMetalMarketCostPerGram);
            cache.SetValueExt<ASCIStarINInventoryItemExt.usrBasisValue>(row, jewelCostBuilder.BasisValue);


            rowExt.UsrContractIncrement = jewelCostBuilder.CalculateIncrementValue(rowExt);
            if (rowExt.UsrCommodityType == CommodityType.Gold)
            {
                cache.SetValueExt<ASCIStarINInventoryItemExt.usrIncrement>(row, rowExt.UsrContractIncrement * rowExt.UsrActualGRAMGold);
            }
            if (rowExt.UsrCommodityType == CommodityType.Silver)
            {
                cache.SetValueExt<ASCIStarINInventoryItemExt.usrIncrement>(row, rowExt.UsrContractIncrement * rowExt.UsrActualGRAMSilver);
            }

            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrContractIncrement>(rowExt.UsrContractIncrement);
            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrBasisValue>(rowExt.UsrBasisValue);
            SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrBasisPrice>(jewelCostBuilder.PreciousMetalContractCostPerTOZ);

            if (ASCIStarMetalType.IsSilver(jewelCostBuilder.INJewelryItem?.MetalType))
            {
                //    cache.SetValueExt<ASCIStarINInventoryItemExt.usrMatrixStep>(row, jewelCostBuilder.ma);
                cache.SetValueExt<ASCIStarINInventoryItemExt.usrFloor>(row, jewelCostBuilder.Floor);
                cache.SetValueExt<ASCIStarINInventoryItemExt.usrCeiling>(row, jewelCostBuilder.Ceiling);
                cache.SetValueExt<ASCIStarINInventoryItemExt.usrMatrixPriceTOZ>(row, jewelCostBuilder.PreciousMetalAvrSilverMarketCostPerTOZ);
                cache.SetValueExt<ASCIStarINInventoryItemExt.usrMatrixPriceGram>(row, jewelCostBuilder.PreciousMetalAvrSilverMarketCostPerTOZ / ASCIStarConstants.TOZ2GRAM_31_10348.value);
            }

            UpdateCostsCurrentOverridenPOVendorItem(rowExt);

            VerifyLossAndSurcharge(cache, row, rowExt, jewelCostBuilder);
        }

        private decimal? CalculateFabricationValue(POVendorInventory poVendorInventory) {
            var poVendorInventoryExt = PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(poVendorInventory);

            var metalWeight = GetMetalWeight();

            var usrFabricationCost = metalWeight * (poVendorInventoryExt.UsrFabricationWeight ?? 0.0m) + (poVendorInventoryExt.UsrFabricationPiece ?? 0.0m);

            return usrFabricationCost;
        }

        private void RecalculateInventoryFabricationValue(InventoryItem inventoryItem)
        {
            POVendorInventory poVendorInventory = GetDefaultVendor();
            if (poVendorInventory == null) return;

            var usrFabricationCost = CalculateFabricationValue(poVendorInventory);

            Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrFabricationCost>(inventoryItem, usrFabricationCost);
        }

        private void RecalculatePOVendorFabricationValue(POVendorInventory poVendorInventory)
        {
            var usrFabricationCost = CalculateFabricationValue(poVendorInventory);

            if (poVendorInventory.IsDefault == true)
            {
                var inventory = Base.Item.Current;
                Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrFabricationCost>(inventory, usrFabricationCost);
            }
            else
            {
                SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrFabricationCost>(usrFabricationCost, poVendorInventory);
            }
        }

        private decimal? GetMetalWeight()
        {
            var inventoryExt = Base.Item.Current.GetExtension<ASCIStarINInventoryItemExt>();

            var metalType = this.JewelryItemView.Current?.MetalType;
            decimal? metalWeight;

            switch (metalType)
            {
                case var type when ASCIStarMetalType.IsGold(type):
                    metalWeight = inventoryExt.UsrActualGRAMGold;
                    break;
                case var type when ASCIStarMetalType.IsSilver(type):
                    metalWeight = inventoryExt.UsrActualGRAMSilver;
                    break;
                default:
                    metalWeight = 0;
                    break;
            }

            return metalWeight;
        }

        private void UpdateCostsCurrentOverridenPOVendorItem(ASCIStarINInventoryItemExt inventoryItemExt)
        {
            if (VendorItems.Current == null)
            {
                VendorItems.Current = GetDefaultVendor();
                if (VendorItems.Current == null) return;
            }

            if (inventoryItemExt.UsrCostingType == ASCIStarConstants.CostingType.MarketCost)
            {
                UpdateMetalCalcPOVendorItem(VendorItems.Cache, VendorItems.Current, VendorItems.Current.GetExtension<ASCIStarPOVendorInventoryExt>());
            }
            else
            {
                if (VendorItems.Current.IsDefault == true)
                {
                    VendorItems.SetValueExt<ASCIStarPOVendorInventoryExt.usrPreciousMetalCost>(VendorItems.Current, inventoryItemExt.UsrPreciousMetalCost);
                    VendorItems.SetValueExt<ASCIStarPOVendorInventoryExt.usrUnitCost>(VendorItems.Current, inventoryItemExt.UsrUnitCost);
                    VendorItems.SetValueExt<ASCIStarPOVendorInventoryExt.usrFloor>(VendorItems.Current, inventoryItemExt.UsrFloor);
                    VendorItems.SetValueExt<ASCIStarPOVendorInventoryExt.usrCeiling>(VendorItems.Current, inventoryItemExt.UsrCeiling);
                }
            }
        }

        private void UpdateSurcharge<TField>(PXCache cache, object row, IASCIStarItemCostSpecDTO rowExt, string metalType) where TField : IBqlField
        {
            if ((rowExt.UsrActualGRAMSilver == null || rowExt.UsrActualGRAMSilver == 0.0m) && (rowExt.UsrActualGRAMGold == null || rowExt.UsrActualGRAMGold == 0.0m)) return;

            var jewelCostBuilder = CreateCostBuilder(rowExt);
            if (jewelCostBuilder == null) return;

            decimal? surchargeValue = ASCIStarCostBuilder.CalculateSurchargeValue(rowExt.UsrContractIncrement, metalType);
            cache.SetValueExt<TField>(row, surchargeValue);
        }

        protected virtual void UpdateItemAndPOVendorInventory(PXCache cache, POVendorInventory row, ASCIStarPOVendorInventoryExt rowExt)
        {
            if (row.IsDefault == true)
            {
                var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(this.Base.Item.Current);
                UpdateCommodityCostMetal(this.Base.Item.Cache, this.Base.Item.Current, inventoryItemExt);
            }
            else
            {
                UpdateMetalCalcPOVendorItem(cache, row, rowExt);
            }
        }

        private void UpdateMetalCalcPOVendorItem(PXCache cache, POVendorInventory row, ASCIStarPOVendorInventoryExt rowExt)
        {
            if (Base.Item.Current == null) return;
            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(Base.Item.Current);
            rowExt.UsrActualGRAMGold = inventoryItemExt.UsrActualGRAMGold;
            rowExt.UsrActualGRAMSilver = inventoryItemExt.UsrActualGRAMSilver;

            var jewelCostBuilder = CreateCostBuilder(rowExt, row);
            if (jewelCostBuilder == null) return;

            rowExt.UsrPreciousMetalCost = jewelCostBuilder.CalculatePreciousMetalCost(ASCIStarConstants.CostingType.ContractCost);
            cache.SetValueExt<ASCIStarPOVendorInventoryExt.usrPreciousMetalCost>(row, rowExt.UsrPreciousMetalCost);

            rowExt.UsrContractIncrement = jewelCostBuilder.CalculateIncrementValue(rowExt);
            cache.SetValue<ASCIStarPOVendorInventoryExt.usrContractIncrement>(row, rowExt.UsrContractIncrement);
            //SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrContractIncrement>(rowExt.UsrContractIncrement);

            if (ASCIStarMetalType.IsSilver(jewelCostBuilder.INJewelryItem?.MetalType))
            {
                cache.SetValueExt<ASCIStarPOVendorInventoryExt.usrFloor>(row, jewelCostBuilder.Floor);
                cache.SetValueExt<ASCIStarPOVendorInventoryExt.usrCeiling>(row, jewelCostBuilder.Ceiling);
                cache.SetValueExt<ASCIStarPOVendorInventoryExt.usrBasisValue>(row, jewelCostBuilder.BasisValue);
            }
        }

        private void SetValueExtPOVendorInventory<TField>(object newValue, POVendorInventory poVendorInventory = null) where TField : IBqlField
        {
            if (poVendorInventory == null)
            {
                poVendorInventory = GetDefaultVendor();
            }

            if (poVendorInventory == null) return;

            this.VendorItems.Cache.SetValueExt<TField>(poVendorInventory, newValue);
            this.VendorItems.Cache.MarkUpdated(poVendorInventory);
        }

        private void UpdateMetalGrams(string metalType)
        {
            if (metalType == null) return;

            var intentoryItemExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(this.Base.Item.Current);

            bool isGold = ASCIStarMetalType.IsGold(metalType) && intentoryItemExt?.UsrCommodityType == ASCIStarConstants.CommodityType.Gold;
            bool isSilver = ASCIStarMetalType.IsSilver(metalType) && intentoryItemExt?.UsrCommodityType == ASCIStarConstants.CommodityType.Silver;

            if (isGold)
            {
                this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrActualGRAMSilver>(this.Base.Item.Current, decimal.Zero);
                this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrActualGRAMGold>(this.Base.Item.Current,
                               PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(this.Base.Item.Current).UsrActualGRAMGold);

            }
            if (isSilver)
            {
                this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrActualGRAMGold>(this.Base.Item.Current, decimal.Zero);
                this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrActualGRAMSilver>(this.Base.Item.Current,
                                 PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(this.Base.Item.Current).UsrActualGRAMSilver);
            }

            this.Base.Item.Cache.SetValueExt<ASCIStarINInventoryItemExt.usrPreciousMetalCost>(this.Base.Item.Current, decimal.Zero);
        }

        private ASCIStarCostBuilder CreateCostBuilder(IASCIStarItemCostSpecDTO currentRow, POVendorInventory defaultVendor = null)
        {
            if ((currentRow.UsrActualGRAMSilver == null || currentRow.UsrActualGRAMSilver == 0.0m)
                && (currentRow.UsrActualGRAMGold == null || currentRow.UsrActualGRAMGold == 0.0m)) return null;

            if (defaultVendor == null)
                defaultVendor = GetDefaultVendor();
            if (defaultVendor == null) return null;

            if (this.JewelryItemView.Current == null)
                this.JewelryItemView.Current = JewelryItemView.Select().TopFirst;
            if (this.JewelryItemView.Current == null && Base.IsCopyPasteContext) // it is fix of copy-paste bug, missing Precious Metal value
                this.JewelryItemView.Current = this.JewelryItemView.Cache.Cached.RowCast<ASCIStarINJewelryItem>().FirstOrDefault();
            if (this.JewelryItemView.Current == null) return null;

            return new ASCIStarCostBuilder(this.Base)
                        .WithInventoryItem(currentRow)
                        .WithPOVendorInventory(defaultVendor)
                        .WithJewelryAttrData(this.JewelryItemView.Current)
                        .WithPricingData(PXTimeZoneInfo.Today)
                        .Build();
        }

        private void CopyPOVendorInventoryToItem(POVendorInventory row)
        {
            var poVendorInventoryExt = PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(row);

            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(this.Base.Item.Current);
            inventoryItemExt.UsrFabricationCost = poVendorInventoryExt.UsrFabricationCost;
            inventoryItemExt.UsrOtherMaterialsCost = poVendorInventoryExt.UsrOtherMaterialsCost;
            inventoryItemExt.UsrPackagingCost = poVendorInventoryExt.UsrPackagingCost;
            inventoryItemExt.UsrPackagingLaborCost = poVendorInventoryExt.UsrPackagingLaborCost;

            inventoryItemExt.UsrLaborCost = poVendorInventoryExt.UsrLaborCost;
            inventoryItemExt.UsrHandlingCost = poVendorInventoryExt.UsrHandlingCost;

            inventoryItemExt.UsrFreightCost = poVendorInventoryExt.UsrFreightCost;
            inventoryItemExt.UsrDutyCost = poVendorInventoryExt.UsrDutyCost;

            inventoryItemExt.UsrContractSurcharge = poVendorInventoryExt.UsrContractSurcharge;
            inventoryItemExt.UsrContractLossPct = poVendorInventoryExt.UsrContractLossPct;
            inventoryItemExt.UsrBasisValue = poVendorInventoryExt.UsrBasisValue;
            inventoryItemExt.UsrMatrixStep = poVendorInventoryExt.UsrMatrixStep;

            this.Base.Item.UpdateCurrent();
        }

        private void VerifyLossAndSurcharge(PXCache cache, InventoryItem row, ASCIStarINInventoryItemExt rowExt, ASCIStarCostBuilder costBuilder)
        {
            if (costBuilder.APVendorPriceContract == null) return;

            var vendorExt = PXCache<APVendorPrice>.GetExtension<ASCIStarAPVendorPriceExt>(costBuilder.APVendorPriceContract);

            if (rowExt.UsrContractLossPct != vendorExt.UsrCommodityLossPct)
            {
                cache.RaiseExceptionHandling<ASCIStarINInventoryItemExt.usrContractLossPct>(row, rowExt.UsrContractLossPct,
                    new PXSetPropertyException(ASCIStarINConstants.Warnings.MissingMatchesLossOrSurcharge, PXErrorLevel.Warning));
            }
            else
            {
                cache.RaiseExceptionHandling<ASCIStarINInventoryItemExt.usrContractLossPct>(row, rowExt.UsrContractLossPct, null);
            }
            if (rowExt.UsrContractSurcharge != vendorExt.UsrCommoditySurchargePct)
            {
                cache.RaiseExceptionHandling<ASCIStarINInventoryItemExt.usrContractSurcharge>(row, rowExt.UsrContractSurcharge,
                    new PXSetPropertyException(ASCIStarINConstants.Warnings.MissingMatchesLossOrSurcharge, PXErrorLevel.Warning));
            }
            else
            {
                cache.RaiseExceptionHandling<ASCIStarINInventoryItemExt.usrContractSurcharge>(row, rowExt.UsrContractSurcharge, null);
            }
        }

        private void SetStringList<TField>(PXCache cache, string attributeID) where TField : IBqlField
        {
            List<string> values = new List<string>();
            List<string> labels = new List<string>();
            SelectAttributeDetails(attributeID).ForEach(x =>
            {
                values.Add(x.ValueID);
                labels.Add(x.Description);
            });
            PXStringListAttribute.SetList<TField>(cache, null, values.ToArray(), labels.ToArray());
        }

        private POVendorInventory GetDefaultVendor() =>
            this.VendorItems.Select().FirstTableItems.FirstOrDefault(x => x.IsDefault == true);

        private List<CSAttributeDetail> SelectAttributeDetails(string attributeID) =>
             SelectFrom<CSAttributeDetail>.Where<CSAttributeDetail.attributeID.IsEqual<@P.AsString>>.View.Select(this.Base, attributeID)?.FirstTableItems.ToList();


        #endregion Helper Methods
    }
}