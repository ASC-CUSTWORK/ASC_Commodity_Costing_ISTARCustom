using ASCJewelryLibrary.AP.CacheExt;
using ASCJewelryLibrary.AP.DAC;
using ASCJewelryLibrary.Common.Builder;
using ASCJewelryLibrary.Common.Descriptor;
using ASCJewelryLibrary.Common.DTO.Interfaces;
using ASCJewelryLibrary.Common.Helper;
using ASCJewelryLibrary.IN.CacheExt;
using ASCJewelryLibrary.IN.DAC;
using ASCJewelryLibrary.IN.Descriptor.Constants;
using ASCJewelryLibrary.PO.CacheExt;
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
using static ASCJewelryLibrary.Common.Descriptor.ASCJConstants;

namespace ASCJewelryLibrary.IN.GraphExt
{
    public class ASCJInventoryItemMaintExt : PXGraphExtension<InventoryItemMaint>
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

        public SelectFrom<ASCJINJewelryItem>.Where<ASCJINJewelryItem.inventoryID.IsEqual<InventoryItem.inventoryID.FromCurrent>>.View JewelryItemView;

        public SelectFrom<ASCJINVendorDuty>.Where<ASCJINVendorDuty.inventoryID.IsEqual<InventoryItem.inventoryID.FromCurrent>>.View VendorDutyView;

        [PXFilterable]
        [PXCopyPasteHiddenView(IsHidden = true)]
        public SelectFrom<ASCJINCompliance>.Where<ASCJINCompliance.inventoryID.IsEqual<InventoryItem.inventoryID.FromCurrent>>.View ComplianceView;

        #endregion Selects

        #region Cache Attached
        [PXMergeAttributes(Method = MergeMethod.Replace)]
        [PXDBString(30, IsUnicode = true, InputMask = "####.##.####")]
        [PXSelector(typeof(SearchFor<ASCJAPTariffHTSCode.hSTariffCode>))]
        protected virtual void _(Events.CacheAttached<InventoryItem.hSTariffCode> e) { }
        #endregion

        #region Actions

        public PXAction<InventoryItem> UpdateMetalCost;
        [PXUIField(DisplayName = "Update Metal Cost", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable updateMetalCost(PXAdapter adapter)
        {
            if (this.Base.Item.Current == null) return adapter.Get();

            UpdateCommodityCostMetal(this.Base.Item.Cache, this.Base.Item.Current, this.Base.Item.Current.GetExtension<ASCJINInventoryItemExt>());

            return adapter.Get();
        }

        #endregion Action

        #region Event Handlers

        #region InventoryItem Events

        protected virtual void _(Events.FieldSelecting<InventoryItem, ASCJINInventoryItemExt.usrBasisValue> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);
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

            PXUIFieldAttribute.SetRequired<ASCJINJewelryItem.metalType>(this.JewelryItemView.Cache, isVisible);
            PXDefaultAttribute.SetPersistingCheck<ASCJINJewelryItem.metalType>(this.JewelryItemView.Cache, this.JewelryItemView.Current,
                isVisible ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
        }

        protected virtual void _(Events.FieldDefaulting<InventoryItem, ASCJINInventoryItemExt.usrCostingType> e)
        {
            if (e.Row == null) return;

            INItemClass itemClass = INItemClass.PK.Find(Base, e.Row.ItemClassID);
            ASCJINItemClassExt classExt = itemClass?.GetExtension<ASCJINItemClassExt>();
            e.NewValue = classExt?.UsrCostingType ?? ASCJConstants.CostingType.ContractCost;
        }

        //protected virtual void _(Events.FieldDefaulting<InventoryItem, ASCJINInventoryItemExt.usrCostRollupType> e)
        //{
        //    if (e.Row == null) return;

        //    INItemClass itemClass = INItemClass.PK.Find(Base, e.Row.ItemClassID);
        //    ASCJINItemClassExt classExt = itemClass?.GetExtension<ASCJINItemClassExt>();
        //    e.NewValue = classExt?.UsrCostRollupType ?? ASCJConstants.CostRollupType.Blank;
        //}

        protected virtual void _(Events.FieldVerifying<InventoryItem, ASCJINInventoryItemExt.usrMatrixStep> e)
        {
            if (e.Row == null) return;

            if ((decimal?)e.NewValue <= 0.0m)
            {
                e.Cache.RaiseExceptionHandling<ASCJINInventoryItemExt.usrMatrixStep>(e.Row, 0.5m,
                    new PXSetPropertyException(ASCJINConstants.Errors.ERPTakeMarketPrice, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldVerifying<InventoryItem, ASCJINInventoryItemExt.usrContractSurcharge> e)
        {
            if (e.Row == null) return;

            if ((decimal?)e.NewValue < 0.0m)
            {
                e.Cache.RaiseExceptionHandling<ASCJINInventoryItemExt.usrContractSurcharge>(e.Row, e.NewValue,
                    new PXSetPropertyException(ASCJINConstants.Warnings.SurchargeIsNegative, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldVerifying<InventoryItem, ASCJINInventoryItemExt.usrCostingType> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);

            var defaultVendor = GetDefaultVendor();
            if (defaultVendor == null) return;

            var vendorItemExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(defaultVendor);
            if (rowExt != null && e.NewValue?.ToString() != ASCJConstants.CostingType.ContractCost && true == defaultVendor.IsDefault == vendorItemExt.UsrIsOverrideVendor)
            {
                Base.Item.Cache.RaiseExceptionHandling<ASCJINInventoryItemExt.usrCostingType>(row, e.NewValue,
                    new PXSetPropertyException(ASCJINConstants.Warnings.CostingTypeIsNotContract, PXErrorLevel.RowWarning));
            }
        }

        protected virtual void _(Events.FieldUpdating<InventoryItem, InventoryItem.descr> e)
        {
            var row = e.Row;
            if (row == null) return;

            this.JewelryItemView.SetValueExt<ASCJINJewelryItem.shortDesc>(this.JewelryItemView.Current, e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, InventoryItem.itemClassID> e)
        {
            var row = e.Row;
            if (e.Row == null) return;

            bool isVisible = IsVisibleFields(row.ItemClassID);
            SetVisibleJewelFields(e.Cache, row, isVisible);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrCostingType> e)
        {
            var row = e.Row;
            if (e.Row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);
            UpdateCommodityCostMetal(e.Cache, row, rowExt);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrActualGRAMGold> e)
        {
            var row = e.Row;
            if (row == null || Base.IsCopyPasteContext) return;

            var mult = ASCJMetalType.GetGoldTypeValue(this.JewelryItemView.Current?.MetalType);

            decimal? pricingGRAMGold = (decimal?)e.NewValue * mult / 24;
            e.Cache.SetValueExt<ASCJINInventoryItemExt.usrPricingGRAMGold>(row, pricingGRAMGold);

            RecalculateInventoryFabricationValue(row);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrActualGRAMSilver> e)
        {
            var row = e.Row;
            if (row == null || Base.IsCopyPasteContext) return;

            var value = ASCJMetalType.GetSilverTypeValue(this.JewelryItemView.Current?.MetalType);

            e.Cache.SetValueExt<ASCJINInventoryItemExt.usrPricingGRAMSilver>(row, (decimal?)e.NewValue * value);

            RecalculateInventoryFabricationValue(row);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrPricingGRAMGold> e)
        {
            var row = e.Row;
            if (row == null || Base.IsCopyPasteContext) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);

            UpdateCommodityCostMetal(e.Cache, row, rowExt);

            var valueMult = ASCJMetalType.GetGoldTypeValue(this.JewelryItemView.Current?.MetalType);

            var actualGRAMGold = (decimal?)e.NewValue / valueMult * 24;
            if (actualGRAMGold != rowExt.UsrActualGRAMGold)
            {
                rowExt.UsrActualGRAMGold = actualGRAMGold;
            }

            RecalculateInventoryFabricationValue(row);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrPricingGRAMSilver> e)
        {
            var row = e.Row;
            if (row == null || Base.IsCopyPasteContext) return;

            ASCJINInventoryItemExt rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);

            UpdateCommodityCostMetal(e.Cache, row, rowExt);

            var valueMult = ASCJMetalType.GetSilverTypeValue(this.JewelryItemView.Current?.MetalType);

            var actualGramSilver = (decimal?)e.NewValue / valueMult;
            if (actualGramSilver != rowExt.UsrActualGRAMSilver)
            {
                rowExt.UsrActualGRAMSilver = actualGramSilver;
            }

            RecalculateInventoryFabricationValue(row);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrPreciousMetalCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrPreciousMetalCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrUnitCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);
            decimal? newValue = (decimal?)e.NewValue;

            rowExt.UsrDutyCost = rowExt.UsrDutyCostPct * newValue / 100.0m;
            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrDutyCost>(rowExt.UsrDutyCost);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrContractIncrement> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);

            var isGold = ASCJMetalType.IsGold(this.JewelryItemView.Current?.MetalType);

            if (isGold == true)
            {
                UpdateSurcharge<ASCJINInventoryItemExt.usrContractSurcharge>(e.Cache, row, rowExt, this.JewelryItemView.Current?.MetalType);
                UpdateCommodityCostMetal(e.Cache, row, rowExt);
            }

            //var isSilver = ASCJMetalType.IsGold(this.JewelryItemView.Current?.MetalType);
            //if (isSilver)
            //{
            //    UpdateCommodityCostMetal(e.Cache, row, rowExt);
            //}

            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrContractIncrement>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrMatrixStep> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);
            UpdateCommodityCostMetal(e.Cache, row, rowExt);

            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrMatrixStep>(e.NewValue);
            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrBasisValue>(rowExt.UsrBasisValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrContractSurcharge> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);
            UpdateCommodityCostMetal(e.Cache, row, rowExt);

            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrContractSurcharge>((decimal?)e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrContractLossPct> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);
            UpdateCommodityCostMetal(e.Cache, row, rowExt);

            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrContractLossPct>((decimal?)e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrOtherMaterialsCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrOtherMaterialsCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrFabricationCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrFabricationCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrPackagingCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrPackagingCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrPackagingLaborCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrPackagingLaborCost>(e.NewValue);
        }

        //protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrOtherCost> e)
        //{
        //    var row = e.Row;
        //    if (row == null) return;

        //    UpdatUnitCost(e.Cache, row);
        //    SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrOtherCost>(e.NewValue);
        //}

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrLaborCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrLaborCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrFreightCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrFreightCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrDutyCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);

            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrDutyCost>(e.NewValue);

            if (rowExt.UsrUnitCost == null || rowExt.UsrUnitCost == 0.0m)
            {
                rowExt.UsrDutyCostPct = decimal.Zero;
                return;
            }
            decimal? newCostPctValue = (decimal?)e.NewValue / rowExt.UsrUnitCost * 100.0m;
            if (newCostPctValue == rowExt.UsrDutyCostPct) return;
            rowExt.UsrDutyCostPct = newCostPctValue;
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrDutyCostPct> e)
        {
            var row = e.Row;
            if (row == null) return;

            ASCJINInventoryItemExt rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);

            decimal? newDutyCostValue = rowExt.UsrUnitCost * (decimal?)e.NewValue / 100.00m;
            if (newDutyCostValue == rowExt.UsrDutyCost) return;
            e.Cache.SetValueExt<ASCJINInventoryItemExt.usrDutyCost>(row, newDutyCostValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrHandlingCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrHandlingCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrCommodityType> e)
        {
            var row = e.Row;
            if (row == null || e.NewValue == null) return;

            e.Cache.RaiseExceptionHandling<ASCJINInventoryItemExt.usrCommodityType>(row, e.NewValue,
                new PXSetPropertyException(ASCJINConstants.Warnings.MetalTypeEmpty, PXErrorLevel.Warning));

            this.JewelryItemView.SetValueExt<ASCJINJewelryItem.metalType>(this.JewelryItemView.Current, null);
            JewelryItemView.Cache.RaiseExceptionHandling<ASCJINJewelryItem.metalType>(JewelryItemView.Current, null,
                new PXSetPropertyException(ASCJINConstants.Warnings.SelectMetalType, PXErrorLevel.Warning));

            ASCJINInventoryItemExt rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);
        }

        #endregion InventoryItem Events

        #region JewelryItem Events

        protected virtual void _(Events.FieldUpdated<ASCJINJewelryItem, ASCJINJewelryItem.metalType> e)
        {
            var row = e.Row;
            if (row == null || this.Base.Item.Current == null) return;

            UpdateMetalGrams(e.NewValue?.ToString());
            UpdateCommodityCostMetal(this.Base.Item.Cache, this.Base.Item.Current, PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(this.Base.Item.Current));
        }

        #endregion JewelryItem Events

        #region POVendorInventory Events
        protected virtual void _(Events.RowSelected<POVendorInventory> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetReadOnlyPOVendorInventoryFields(e.Cache, row);
            SetVisiblePOVendorInventoryFields(e.Cache);

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(row);
            if (rowExt?.UsrIsOverrideVendor == true)
            {
                e.Cache.RaiseExceptionHandling<ASCJPOVendorInventoryExt.usrUnitCost>(row, rowExt.UsrUnitCost,
                      new PXSetPropertyException(ASCJINConstants.Warnings.UnitCostIsCustom, PXErrorLevel.Warning));
            }
            else
            {
                e.Cache.RaiseExceptionHandling<ASCJPOVendorInventoryExt.usrUnitCost>(row, rowExt.UsrUnitCost, null);
            }
        }

        protected virtual void _(Events.FieldSelecting<POVendorInventory, ASCJPOVendorInventoryExt.usrFabricationCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var poVendorInventoryExt = row.GetExtension<ASCJPOVendorInventoryExt>();
            var calculatedFabricationValue = CalculateFabricationValue(row);

            if (poVendorInventoryExt.UsrFabricationCost != calculatedFabricationValue)
            {
                e.Cache.RaiseExceptionHandling<ASCJPOVendorInventoryExt.usrFabricationCost>(row, poVendorInventoryExt.UsrFabricationCost,
                    new PXSetPropertyException(ASCJINConstants.Warnings.FabricationCostMismatch, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldVerifying<POVendorInventory, POVendorInventory.isDefault> e)
        {
            var row = e.Row;
            if (row == null || (bool)e.NewValue != true) return;

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(row);
            if (rowExt.UsrMarketID == null)
            {
                e.Cache.RaiseExceptionHandling<ASCJPOVendorInventoryExt.usrMarketID>(row, false, new PXSetPropertyException(ASCJINConstants.Errors.MarketEmpty, PXErrorLevel.RowError));
            }

            var inventoryID = ASCJMetalType.GetBaseInventoryID(this.Base, this.JewelryItemView.Current?.MetalType);

            var apVendorPrice = ASCJCostBuilder.GetAPVendorPrice(this.Base, row.VendorID, inventoryID, ASCJConstants.TOZ.value, PXTimeZoneInfo.Today);

            if (apVendorPrice == null && PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(row).UsrIsOverrideVendor != true)
            {
                e.Cache.RaiseExceptionHandling<POVendorInventory.isDefault>(row, false,
                    new PXSetPropertyException(ASCJMessages.Error.VendorPriceNotFound, PXErrorLevel.RowWarning));
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

        protected virtual void _(Events.FieldVerifying<POVendorInventory, ASCJPOVendorInventoryExt.usrIsOverrideVendor> e)
        {
            var row = e.Row;
            if (row == null || row.IsDefault != true) return;

            bool newValue = (bool)e.NewValue;
            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(row);

            if (newValue == false) return;

            if (rowExt.UsrCommodityVendorPrice == decimal.Zero)
            {
                e.Cache.RaiseExceptionHandling<ASCJPOVendorInventoryExt.usrCommodityVendorPrice>(row, rowExt.UsrCommodityVendorPrice,
                    new PXSetPropertyException(ASCJMessages.Error.POVendorInventoryVendorPriceEmpty, PXErrorLevel.Error));
            }

            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(Base.Item.Current);
            if (inventoryItemExt != null && inventoryItemExt.UsrCostingType != ASCJConstants.CostingType.ContractCost)
            {
                Base.Item.Cache.RaiseExceptionHandling<ASCJINInventoryItemExt.usrCostingType>(Base.Item.Current, inventoryItemExt.UsrCostingType,
                    new PXSetPropertyException(ASCJINConstants.Warnings.CostingTypeIsNotContract, PXErrorLevel.RowWarning));
            }
        }

        protected virtual void _(Events.FieldVerifying<POVendorInventory, ASCJPOVendorInventoryExt.usrCommodityVendorPrice> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(row);
            if ((decimal)e.NewValue == decimal.Zero && rowExt.UsrIsOverrideVendor == true)
            {
                e.Cache.RaiseExceptionHandling<ASCJPOVendorInventoryExt.usrCommodityVendorPrice>(row, rowExt.UsrBasisPrice,
                    new PXSetPropertyException(ASCJMessages.Error.POVendorInventoryVendorPriceEmpty, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldVerifying<POVendorInventory, ASCJPOVendorInventoryExt.usrBasisPrice> e)
        {
            var row = e.Row;
            if (row == null) return;

            if ((decimal?)e.NewValue == decimal.Zero)
            {
                e.Cache.RaiseExceptionHandling<ASCJPOVendorInventoryExt.usrBasisPrice>(row, e.NewValue,
                    new PXSetPropertyException(ASCJINConstants.Warnings.BasisOrMarketPriceEmpty, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldVerifying<POVendorInventory, ASCJPOVendorInventoryExt.usrMatrixStep> e)
        {
            if (e.Row == null) return;

            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(this.Base.Item.Current);
            if ((decimal?)e.NewValue <= 0.0m && inventoryItemExt.UsrCommodityType == ASCJConstants.CommodityType.Silver)
            {
                e.Cache.RaiseExceptionHandling<ASCJPOVendorInventoryExt.usrMatrixStep>(e.Row, 0.5m,
                    new PXSetPropertyException(ASCJINConstants.Errors.ERPTakeMarketPrice, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldVerifying<POVendorInventory, ASCJPOVendorInventoryExt.usrContractSurcharge> e)
        {
            if (e.Row == null) return;

            var sdfs = e.Row.GetExtension<ASCJPOVendorInventoryExt>();
            if ((decimal?)e.NewValue < 0.0m)
            {
                e.Cache.RaiseExceptionHandling<ASCJPOVendorInventoryExt.usrContractSurcharge>(e.Row, e.NewValue,
                    new PXSetPropertyException(ASCJINConstants.Warnings.SurchargeIsNegative, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCJPOVendorInventoryExt.usrIsOverrideVendor> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(row);

            UpdateItemAndPOVendorInventory(e.Cache, row, rowExt);
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, POVendorInventory.isDefault> e)
        {
            var row = e.Row;
            if (row == null || Base.IsCopyPasteContext) return;

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(row);
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
            ASCJVendorExt vendorExt = vendor?.GetExtension<ASCJVendorExt>();
            e.Cache.SetValue<ASCJPOVendorInventoryExt.usrMarketID>(row, vendorExt.UsrMarketID);

            var inventoryID = ASCJMetalType.GetBaseInventoryID(this.Base, this.JewelryItemView.Current?.MetalType);

            var apVendorPrice = ASCJCostBuilder.GetAPVendorPrice(this.Base, vendor.BAccountID, inventoryID, ASCJConstants.TOZ.value, PXTimeZoneInfo.Today);

            if (apVendorPrice == null)
            {
                e.Cache.RaiseExceptionHandling<POVendorInventory.vendorID>(row, e.NewValue,
                    new PXSetPropertyException(ASCJINConstants.Warnings.BasisOrMarketPriceEmpty, PXErrorLevel.Warning));
                return;
            }

            var apVendorPriceExt = apVendorPrice.GetExtension<ASCJAPVendorPriceExt>();
            e.Cache.SetValueExt<ASCJPOVendorInventoryExt.usrContractLossPct>(row, apVendorPriceExt.UsrCommodityLossPct ?? 0.0m);
            e.Cache.SetValueExt<ASCJPOVendorInventoryExt.usrMatrixStep>(row, apVendorPriceExt.UsrMatrixStep ?? 0.0m);
            e.Cache.SetValueExt<ASCJPOVendorInventoryExt.usrBasisValue>(row, apVendorPriceExt.UsrBasisValue ?? 0.0m);
            e.Cache.SetValueExt<ASCJPOVendorInventoryExt.usrCommodityVendorPrice>(row, apVendorPrice.SalesPrice ?? 0.0m);
            e.Cache.SetValueExt<ASCJPOVendorInventoryExt.usrBasisPrice>(row, apVendorPrice.SalesPrice ?? 0.0m);
            e.Cache.SetValueExt<ASCJPOVendorInventoryExt.usrContractSurcharge>(row, apVendorPriceExt.UsrCommoditySurchargePct ?? 0.0m);
            //e.Cache.SetValueExt<ASCJPOVendorInventoryExt.usrFabricationWeight>(row, apVendorPriceExt.UsrLaborPerUnit ?? 0.0m);

            if (row.IsDefault == true)
            {
                if (this.Base.Item.Current != null)
                {
                    var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(this.Base.Item.Current);

                    inventoryItemExt.UsrContractSurcharge = apVendorPriceExt.UsrCommoditySurchargePct;
                    inventoryItemExt.UsrContractLossPct = apVendorPriceExt.UsrCommodityLossPct;
                    inventoryItemExt.UsrMatrixStep = apVendorPriceExt.UsrMatrixStep;
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCJPOVendorInventoryExt.usrFabricationWeight> e)
        {
            var row = e.Row;
            if (row == null) return;

            RecalculatePOVendorFabricationValue(row);
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCJPOVendorInventoryExt.usrFabricationPiece> e)
        {
            var row = e.Row;
            if (row == null) return;

            RecalculatePOVendorFabricationValue(row);
        }

        //protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCJPOVendorInventoryExt.usrFabricationCost> e)
        //{
        //    var row = e.Row;
        //    if (row == null) return;

        //    var poVendorExt = row.GetExtension<ASCJPOVendorInventoryExt>();

        //    var metalWeight = GetMetalWeight();

        //    var usrLaborPerUnit = metalWeight == 0
        //        ? (decimal?)e.NewValue
        //        : (decimal?)e.NewValue / metalWeight;

        //    if (usrLaborPerUnit != poVendorExt.UsrFabricationWeight)
        //    {
        //        poVendorExt.UsrFabricationWeight = usrLaborPerUnit;
        //    }
        //}

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCJPOVendorInventoryExt.usrCommodityVendorPrice> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(row);
            UpdateItemAndPOVendorInventory(e.Cache, row, rowExt);
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCJINInventoryItemExt.usrContractIncrement> e)
        {
            var row = e.Row;
            if (row == null || row.IsDefault == true) return;

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(row);

            var isGold = ASCJMetalType.IsGold(this.JewelryItemView.Current?.MetalType);
            if (isGold == true)
            {
                UpdateSurcharge<ASCJPOVendorInventoryExt.usrContractSurcharge>(e.Cache, row, rowExt, this.JewelryItemView.Current?.MetalType);
            }
            else
            {
                var isSilver = ASCJMetalType.IsSilver(this.JewelryItemView.Current?.MetalType);
                if (isSilver)
                {
                    UpdateMetalCalcPOVendorItem(e.Cache, row, rowExt);
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCJPOVendorInventoryExt.usrMatrixStep> e)
        {
            var row = e.Row;
            if (row == null || row.IsDefault == true) return;

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(row);
            UpdateMetalCalcPOVendorItem(e.Cache, row, rowExt);
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCJPOVendorInventoryExt.usrContractSurcharge> e)
        {
            var row = e.Row;
            if (row == null) return;

            if (row.IsDefault != true)
            {
                var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(row);
                UpdateMetalCalcPOVendorItem(e.Cache, row, rowExt);
            }
        }



        #endregion POVendorInventory Events

        #region Compliance Events
        public virtual void _(Events.FieldSelecting<ASCJINCompliance, ASCJINCompliance.customerAlphaCode> e)
        {
            SetStringList<ASCJINCompliance.customerAlphaCode>(e.Cache, ASCJINConstants.INJewelryAttributesID.CustomerCode);
        }

        public virtual void _(Events.FieldSelecting<ASCJINCompliance, ASCJINCompliance.division> e)
        {
            SetStringList<ASCJINCompliance.division>(e.Cache, ASCJINConstants.INJewelryAttributesID.InventoryCategory);
        }

        public virtual void _(Events.FieldSelecting<ASCJINCompliance, ASCJINCompliance.testingLab> e)
        {
            SetStringList<ASCJINCompliance.testingLab>(e.Cache, ASCJINConstants.INJewelryAttributesID.CPTESTTYPE);
        }

        public virtual void _(Events.FieldSelecting<ASCJINCompliance, ASCJINCompliance.protocolTestedTo> e)
        {
            SetStringList<ASCJINCompliance.protocolTestedTo>(e.Cache, ASCJINConstants.INJewelryAttributesID.CPPROTOCOL);
        }

        public virtual void _(Events.FieldSelecting<ASCJINCompliance, ASCJINCompliance.waiverReasonCode> e)
        {
            SetStringList<ASCJINCompliance.waiverReasonCode>(e.Cache, ASCJINConstants.INJewelryAttributesID.REASONCODE);
        }
        #endregion Compliance Events

        #region VendorDuty Events
        protected virtual void _(Events.FieldUpdated<ASCJINVendorDuty, ASCJINVendorDuty.vendorID> e)
        {
            if (e.Row == null) return;

            e.Cache.RaiseFieldDefaulting<ASCJINVendorDuty.countryID>(e.Row, out object countryID);
            e.Cache.SetValueExt<ASCJINVendorDuty.countryID>(e.Row, countryID);
        }
        #endregion
       
        #endregion Event Handlers

        #region Helper Methods

        protected virtual void SetVisibleJewelFields(PXCache cache, InventoryItem row, bool isVisible)
        {
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrUnitCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrEstLandedCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrFabricationCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrPreciousMetalCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrFreightCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrLaborCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrDutyCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrDutyCostPct>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrHandlingCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrPackagingCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrOtherMaterialsCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrPackagingLaborCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrContractLossPct>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrContractIncrement>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrBasisValue>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrContractSurcharge>(cache, row, isVisible);

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);

            bool isVisibleGold = isVisible && rowExt.UsrCommodityType == ASCJConstants.CommodityType.Gold;
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrActualGRAMGold>(cache, row, isVisibleGold);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrPricingGRAMGold>(cache, row, isVisibleGold);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrContractIncrement>(cache, row, isVisibleGold);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrIncrement>(cache, row, isVisibleGold);

            bool isVisibleSilver = isVisible && rowExt.UsrCommodityType == ASCJConstants.CommodityType.Silver;
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrActualGRAMSilver>(cache, row, isVisibleSilver);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrPricingGRAMSilver>(cache, row, isVisibleSilver);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrMatrixStep>(cache, row, isVisibleSilver);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrMatrixPriceGram>(cache, row, isVisibleSilver);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrMatrixPriceTOZ>(cache, row, isVisibleSilver);
        }

        protected virtual bool IsVisibleFields(int? itemClassID)
        {
            INItemClass itemClass = INItemClass.PK.Find(Base, itemClassID);

            return itemClass?.ItemClassCD.Trim() != "COMMODITY" && PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(this.Base.Item.Current)?.UsrCommodityType != ASCJConstants.CommodityType.Undefined;
            // acupower: remove from constant to jewel preferences screen and find from rowSelected
        }

        protected virtual void SetReadOnlyJewelAttrFields(PXCache cache, InventoryItem row, string metalType)
        {
            bool isNotGold = !ASCJMetalType.IsGold(metalType);
            bool isNotSilver = !ASCJMetalType.IsSilver(metalType);

            PXUIFieldAttribute.SetReadOnly<ASCJINInventoryItemExt.usrActualGRAMGold>(cache, row, isNotGold);
            PXUIFieldAttribute.SetReadOnly<ASCJINInventoryItemExt.usrPricingGRAMGold>(cache, row, isNotGold);
            PXUIFieldAttribute.SetReadOnly<ASCJINInventoryItemExt.usrContractIncrement>(cache, row, isNotGold);

            PXUIFieldAttribute.SetReadOnly<ASCJINInventoryItemExt.usrActualGRAMSilver>(cache, row, isNotSilver);
            PXUIFieldAttribute.SetReadOnly<ASCJINInventoryItemExt.usrPricingGRAMSilver>(cache, row, isNotSilver);
            PXUIFieldAttribute.SetReadOnly<ASCJINInventoryItemExt.usrMatrixStep>(cache, row, isNotSilver);

            if (isNotGold && isNotSilver)
            {
                PXUIFieldAttribute.SetReadOnly<ASCJINInventoryItemExt.usrPricingGRAMGold>(cache, row, true);
                PXUIFieldAttribute.SetReadOnly<ASCJINInventoryItemExt.usrPricingGRAMSilver>(cache, row, true);
                PXUIFieldAttribute.SetReadOnly<ASCJINInventoryItemExt.usrActualGRAMGold>(cache, row, true);
                PXUIFieldAttribute.SetReadOnly<ASCJINInventoryItemExt.usrActualGRAMSilver>(cache, row, true);
            }
        }

        protected virtual void SetReadOnlyPOVendorInventoryFields(PXCache cache, POVendorInventory row)
        {
            bool isDefaultVendor = row.IsDefault == true;
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrContractIncrement>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrContractLossPct>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrContractSurcharge>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrPreciousMetalCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrOtherMaterialsCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrFabricationCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrPackagingCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrLaborCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrPackagingLaborCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrHandlingCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrFreightCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrDutyCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrMatrixStep>(cache, row, isDefaultVendor);

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(row);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrCommodityVendorPrice>(cache, row, rowExt.UsrIsOverrideVendor != true);
        }

        protected virtual void SetVisiblePOVendorInventoryFields(PXCache cache)
        {
            if (this.Base.Item.Current == null) return;
            var intentoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(this.Base.Item.Current);

            bool isVisible = intentoryItemExt.UsrCommodityType == ASCJConstants.CommodityType.Silver;

            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrFloor>(cache, null, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrCeiling>(cache, null, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrMatrixStep>(cache, null, isVisible);
        }

        protected virtual void UpdateCommodityCostMetal(PXCache cache, InventoryItem row, ASCJINInventoryItemExt rowExt)
        {
            if (rowExt == null) throw new PXException(ASCJINConstants.Errors.NullInCacheSaveItemFirst);

            var jewelCostBuilder = CreateCostBuilder(rowExt);
            if (jewelCostBuilder == null) return;

            rowExt.UsrPreciousMetalCost = jewelCostBuilder.CalculatePreciousMetalCost(rowExt.UsrCostingType);
            cache.SetValueExt<ASCJINInventoryItemExt.usrPreciousMetalCost>(row, rowExt.UsrPreciousMetalCost);

            cache.SetValueExt<ASCJINInventoryItemExt.usrMarketPriceTOZ>(row, jewelCostBuilder.PreciousMetalMarketCostPerTOZ);
            cache.SetValueExt<ASCJINInventoryItemExt.usrMarketPriceGram>(row, jewelCostBuilder.PreciousMetalMarketCostPerGram);
            cache.SetValueExt<ASCJINInventoryItemExt.usrBasisValue>(row, jewelCostBuilder.BasisValue);


            rowExt.UsrContractIncrement = jewelCostBuilder.CalculateIncrementValue(rowExt);
            if (rowExt.UsrCommodityType == CommodityType.Gold)
            {
                cache.SetValueExt<ASCJINInventoryItemExt.usrIncrement>(row, rowExt.UsrContractIncrement * rowExt.UsrActualGRAMGold);
            }
            if (rowExt.UsrCommodityType == CommodityType.Silver)
            {
                cache.SetValueExt<ASCJINInventoryItemExt.usrIncrement>(row, rowExt.UsrContractIncrement * rowExt.UsrActualGRAMSilver);
            }

            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrContractIncrement>(rowExt.UsrContractIncrement);
            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrBasisValue>(rowExt.UsrBasisValue);
            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrBasisPrice>(jewelCostBuilder.PreciousMetalContractCostPerTOZ);

            if (ASCJMetalType.IsSilver(jewelCostBuilder.INJewelryItem?.MetalType))
            {
                //    cache.SetValueExt<ASCJINInventoryItemExt.usrMatrixStep>(row, jewelCostBuilder.ma);
                cache.SetValueExt<ASCJINInventoryItemExt.usrFloor>(row, jewelCostBuilder.Floor);
                cache.SetValueExt<ASCJINInventoryItemExt.usrCeiling>(row, jewelCostBuilder.Ceiling);
                cache.SetValueExt<ASCJINInventoryItemExt.usrMatrixPriceTOZ>(row, jewelCostBuilder.PreciousMetalAvrSilverMarketCostPerTOZ);
                cache.SetValueExt<ASCJINInventoryItemExt.usrMatrixPriceGram>(row, jewelCostBuilder.PreciousMetalAvrSilverMarketCostPerTOZ / ASCJConstants.TOZ2GRAM_31_10348.value);
            }

            UpdateCostsCurrentOverridenPOVendorItem(rowExt);

            VerifyLossAndSurcharge(cache, row, rowExt, jewelCostBuilder);
        }

        private decimal? CalculateFabricationValue(POVendorInventory poVendorInventory) {
            var poVendorInventoryExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(poVendorInventory);

            var metalWeight = GetMetalWeight();

            var usrFabricationCost = metalWeight * (poVendorInventoryExt.UsrFabricationWeight ?? 0.0m) + (poVendorInventoryExt.UsrFabricationPiece ?? 0.0m);

            return usrFabricationCost;
        }

        private void RecalculateInventoryFabricationValue(InventoryItem inventoryItem)
        {
            POVendorInventory poVendorInventory = GetDefaultVendor();
            if (poVendorInventory == null) return;

            var usrFabricationCost = CalculateFabricationValue(poVendorInventory);

            Base.Item.Cache.SetValueExt<ASCJINInventoryItemExt.usrFabricationCost>(inventoryItem, usrFabricationCost);
        }

        private void RecalculatePOVendorFabricationValue(POVendorInventory poVendorInventory)
        {
            var usrFabricationCost = CalculateFabricationValue(poVendorInventory);

            if (poVendorInventory.IsDefault == true)
            {
                var inventory = Base.Item.Current;
                Base.Item.Cache.SetValueExt<ASCJINInventoryItemExt.usrFabricationCost>(inventory, usrFabricationCost);
            }
            else
            {
                SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrFabricationCost>(usrFabricationCost, poVendorInventory);
            }
        }

        private decimal? GetMetalWeight()
        {
            var inventoryExt = Base.Item.Current.GetExtension<ASCJINInventoryItemExt>();

            var metalType = this.JewelryItemView.Current?.MetalType;
            decimal? metalWeight;

            switch (metalType)
            {
                case var type when ASCJMetalType.IsGold(type):
                    metalWeight = inventoryExt.UsrActualGRAMGold;
                    break;
                case var type when ASCJMetalType.IsSilver(type):
                    metalWeight = inventoryExt.UsrActualGRAMSilver;
                    break;
                default:
                    metalWeight = 0;
                    break;
            }

            return metalWeight;
        }

        private void UpdateCostsCurrentOverridenPOVendorItem(ASCJINInventoryItemExt inventoryItemExt)
        {
            if (VendorItems.Current == null)
            {
                VendorItems.Current = GetDefaultVendor();
                if (VendorItems.Current == null) return;
            }

            if (inventoryItemExt.UsrCostingType == ASCJConstants.CostingType.MarketCost)
            {
                UpdateMetalCalcPOVendorItem(VendorItems.Cache, VendorItems.Current, VendorItems.Current.GetExtension<ASCJPOVendorInventoryExt>());
            }
            else
            {
                if (VendorItems.Current.IsDefault == true)
                {
                    VendorItems.SetValueExt<ASCJPOVendorInventoryExt.usrPreciousMetalCost>(VendorItems.Current, inventoryItemExt.UsrPreciousMetalCost);
                    VendorItems.SetValueExt<ASCJPOVendorInventoryExt.usrUnitCost>(VendorItems.Current, inventoryItemExt.UsrUnitCost);
                    VendorItems.SetValueExt<ASCJPOVendorInventoryExt.usrFloor>(VendorItems.Current, inventoryItemExt.UsrFloor);
                    VendorItems.SetValueExt<ASCJPOVendorInventoryExt.usrCeiling>(VendorItems.Current, inventoryItemExt.UsrCeiling);
                }
            }
        }

        private void UpdateSurcharge<TField>(PXCache cache, object row, IASCJItemCostSpecDTO rowExt, string metalType) where TField : IBqlField
        {
            if ((rowExt.UsrActualGRAMSilver == null || rowExt.UsrActualGRAMSilver == 0.0m) && (rowExt.UsrActualGRAMGold == null || rowExt.UsrActualGRAMGold == 0.0m)) return;

            var jewelCostBuilder = CreateCostBuilder(rowExt);
            if (jewelCostBuilder == null) return;

            decimal? surchargeValue = ASCJCostBuilder.CalculateSurchargeValue(rowExt.UsrContractIncrement, metalType);
            cache.SetValueExt<TField>(row, surchargeValue);
        }

        protected virtual void UpdateItemAndPOVendorInventory(PXCache cache, POVendorInventory row, ASCJPOVendorInventoryExt rowExt)
        {
            if (row.IsDefault == true)
            {
                var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(this.Base.Item.Current);
                UpdateCommodityCostMetal(this.Base.Item.Cache, this.Base.Item.Current, inventoryItemExt);
            }
            else
            {
                UpdateMetalCalcPOVendorItem(cache, row, rowExt);
            }
        }

        private void UpdateMetalCalcPOVendorItem(PXCache cache, POVendorInventory row, ASCJPOVendorInventoryExt rowExt)
        {
            if (Base.Item.Current == null) return;
            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(Base.Item.Current);
            rowExt.UsrActualGRAMGold = inventoryItemExt.UsrActualGRAMGold;
            rowExt.UsrActualGRAMSilver = inventoryItemExt.UsrActualGRAMSilver;

            var jewelCostBuilder = CreateCostBuilder(rowExt, row);
            if (jewelCostBuilder == null) return;

            rowExt.UsrPreciousMetalCost = jewelCostBuilder.CalculatePreciousMetalCost(ASCJConstants.CostingType.ContractCost);
            cache.SetValueExt<ASCJPOVendorInventoryExt.usrPreciousMetalCost>(row, rowExt.UsrPreciousMetalCost);

            rowExt.UsrContractIncrement = jewelCostBuilder.CalculateIncrementValue(rowExt);
            cache.SetValue<ASCJPOVendorInventoryExt.usrContractIncrement>(row, rowExt.UsrContractIncrement);
            //SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrContractIncrement>(rowExt.UsrContractIncrement);

            if (ASCJMetalType.IsSilver(jewelCostBuilder.INJewelryItem?.MetalType))
            {
                cache.SetValueExt<ASCJPOVendorInventoryExt.usrFloor>(row, jewelCostBuilder.Floor);
                cache.SetValueExt<ASCJPOVendorInventoryExt.usrCeiling>(row, jewelCostBuilder.Ceiling);
                cache.SetValueExt<ASCJPOVendorInventoryExt.usrBasisValue>(row, jewelCostBuilder.BasisValue);
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

            var intentoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(this.Base.Item.Current);

            bool isGold = ASCJMetalType.IsGold(metalType) && intentoryItemExt?.UsrCommodityType == ASCJConstants.CommodityType.Gold;
            bool isSilver = ASCJMetalType.IsSilver(metalType) && intentoryItemExt?.UsrCommodityType == ASCJConstants.CommodityType.Silver;

            if (isGold)
            {
                this.Base.Item.Cache.SetValueExt<ASCJINInventoryItemExt.usrActualGRAMSilver>(this.Base.Item.Current, decimal.Zero);
                this.Base.Item.Cache.SetValueExt<ASCJINInventoryItemExt.usrActualGRAMGold>(this.Base.Item.Current,
                               PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(this.Base.Item.Current).UsrActualGRAMGold);

            }
            if (isSilver)
            {
                this.Base.Item.Cache.SetValueExt<ASCJINInventoryItemExt.usrActualGRAMGold>(this.Base.Item.Current, decimal.Zero);
                this.Base.Item.Cache.SetValueExt<ASCJINInventoryItemExt.usrActualGRAMSilver>(this.Base.Item.Current,
                                 PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(this.Base.Item.Current).UsrActualGRAMSilver);
            }

            this.Base.Item.Cache.SetValueExt<ASCJINInventoryItemExt.usrPreciousMetalCost>(this.Base.Item.Current, decimal.Zero);
        }

        private ASCJCostBuilder CreateCostBuilder(IASCJItemCostSpecDTO currentRow, POVendorInventory defaultVendor = null)
        {
            if ((currentRow.UsrActualGRAMSilver == null || currentRow.UsrActualGRAMSilver == 0.0m)
                && (currentRow.UsrActualGRAMGold == null || currentRow.UsrActualGRAMGold == 0.0m)) return null;

            if (defaultVendor == null)
                defaultVendor = GetDefaultVendor();
            if (defaultVendor == null) return null;

            if (this.JewelryItemView.Current == null)
                this.JewelryItemView.Current = JewelryItemView.Select().TopFirst;
            if (this.JewelryItemView.Current == null && Base.IsCopyPasteContext) // it is fix of copy-paste bug, missing Precious Metal value
                this.JewelryItemView.Current = this.JewelryItemView.Cache.Cached.RowCast<ASCJINJewelryItem>().FirstOrDefault();
            if (this.JewelryItemView.Current == null) return null;

            return new ASCJCostBuilder(this.Base)
                        .WithInventoryItem(currentRow)
                        .WithPOVendorInventory(defaultVendor)
                        .WithJewelryAttrData(this.JewelryItemView.Current)
                        .WithPricingData(PXTimeZoneInfo.Today)
                        .Build();
        }

        private void CopyPOVendorInventoryToItem(POVendorInventory row)
        {
            var poVendorInventoryExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(row);

            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(this.Base.Item.Current);
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

        private void VerifyLossAndSurcharge(PXCache cache, InventoryItem row, ASCJINInventoryItemExt rowExt, ASCJCostBuilder costBuilder)
        {
            if (costBuilder.APVendorPriceContract == null) return;

            var vendorExt = PXCache<APVendorPrice>.GetExtension<ASCJAPVendorPriceExt>(costBuilder.APVendorPriceContract);

            if (rowExt.UsrContractLossPct != vendorExt.UsrCommodityLossPct)
            {
                cache.RaiseExceptionHandling<ASCJINInventoryItemExt.usrContractLossPct>(row, rowExt.UsrContractLossPct,
                    new PXSetPropertyException(ASCJINConstants.Warnings.MissingMatchesLossOrSurcharge, PXErrorLevel.Warning));
            }
            else
            {
                cache.RaiseExceptionHandling<ASCJINInventoryItemExt.usrContractLossPct>(row, rowExt.UsrContractLossPct, null);
            }
            if (rowExt.UsrContractSurcharge != vendorExt.UsrCommoditySurchargePct)
            {
                cache.RaiseExceptionHandling<ASCJINInventoryItemExt.usrContractSurcharge>(row, rowExt.UsrContractSurcharge,
                    new PXSetPropertyException(ASCJINConstants.Warnings.MissingMatchesLossOrSurcharge, PXErrorLevel.Warning));
            }
            else
            {
                cache.RaiseExceptionHandling<ASCJINInventoryItemExt.usrContractSurcharge>(row, rowExt.UsrContractSurcharge, null);
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