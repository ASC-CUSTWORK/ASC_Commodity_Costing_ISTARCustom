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

        public SelectFrom<ASCJINJewelryItem>.Where<ASCJINJewelryItem.inventoryID.IsEqual<InventoryItem.inventoryID.FromCurrent>>.View ASCJJewelryItemView;

        public SelectFrom<ASCJINVendorDuty>.Where<ASCJINVendorDuty.inventoryID.IsEqual<InventoryItem.inventoryID.FromCurrent>>.View ASCJVendorDutyView;

        [PXFilterable]
        [PXCopyPasteHiddenView(IsHidden = true)]
        public SelectFrom<ASCJINCompliance>.Where<ASCJINCompliance.inventoryID.IsEqual<InventoryItem.inventoryID.FromCurrent>>.View ASCJComplianceView;

        #endregion Selects

        #region Cache Attached
        [PXMergeAttributes(Method = MergeMethod.Replace)]
        [PXDBString(30, IsUnicode = true, InputMask = "####.##.####")]
        [PXSelector(typeof(SearchFor<ASCJAPTariffHTSCode.hSTariffCode>))]
        protected virtual void _(Events.CacheAttached<InventoryItem.hSTariffCode> e) { }
        #endregion

        #region Actions

        public PXAction<InventoryItem> ASCJUpdateMetalCost;
        [PXUIField(DisplayName = "Update Metal Cost", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable aSCJUpdateMetalCost(PXAdapter adapter)
        {
            if (this.Base.Item.Current == null) return adapter.Get();

            UpdateCommodityCostMetal(this.Base.Item.Cache, this.Base.Item.Current, this.Base.Item.Current.GetExtension<ASCJINInventoryItemExt>());

            return adapter.Get();
        }

        #endregion Action

        #region Event Handlers

        #region InventoryItem Events

        protected virtual void _(Events.FieldSelecting<InventoryItem, ASCJINInventoryItemExt.usrASCJBasisValue> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);
            UpdateCommodityCostMetal(e.Cache, row, rowExt);
            e.ReturnValue = rowExt.UsrASCJBasisValue;
        }

        protected virtual void _(Events.RowSelected<InventoryItem> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);
            bool isVisible = IsVisibleFields(rowExt, row.ItemClassID);
            SetVisibleJewelFields(e.Cache, row, isVisible);

            if (this.ASCJJewelryItemView.Current == null)
                this.ASCJJewelryItemView.Current = this.ASCJJewelryItemView.Select();

            SetReadOnlyJewelAttrFields(e.Cache, row, this.ASCJJewelryItemView.Current?.MetalType);

            bool isRequire = isVisible || rowExt.UsrASCJCommodityType == CommodityType.Gold || rowExt.UsrASCJCommodityType == CommodityType.Silver;
            PXUIFieldAttribute.SetRequired<ASCJINJewelryItem.metalType>(this.ASCJJewelryItemView.Cache, isRequire);
            PXDefaultAttribute.SetPersistingCheck<ASCJINJewelryItem.metalType>(this.ASCJJewelryItemView.Cache, this.ASCJJewelryItemView.Current,
                isRequire ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
        }

        protected virtual void _(Events.FieldDefaulting<InventoryItem, ASCJINInventoryItemExt.usrASCJCostingType> e)
        {
            if (e.Row == null) return;

            INItemClass itemClass = INItemClass.PK.Find(Base, e.Row.ItemClassID);
            ASCJINItemClassExt classExt = itemClass?.GetExtension<ASCJINItemClassExt>();
            e.NewValue = classExt?.UsrASCJCostingType ?? ASCJConstants.CostingType.ContractCost;
        }

        //protected virtual void _(Events.FieldDefaulting<InventoryItem, ASCJINInventoryItemExt.usrASCJCostRollupType> e)
        //{
        //    if (e.Row == null) return;

        //    INItemClass itemClass = INItemClass.PK.Find(Base, e.Row.ItemClassID);
        //    ASCJINItemClassExt classExt = itemClass?.GetExtension<ASCJINItemClassExt>();
        //    e.NewValue = classExt?.UsrASCJCostRollupType ?? ASCJConstants.CostRollupType.Blank;
        //}

        protected virtual void _(Events.FieldVerifying<InventoryItem, ASCJINInventoryItemExt.usrASCJMatrixStep> e)
        {
            if (e.Row == null) return;

            if ((decimal?)e.NewValue <= 0.0m)
            {
                e.Cache.RaiseExceptionHandling<ASCJINInventoryItemExt.usrASCJMatrixStep>(e.Row, 0.5m,
                    new PXSetPropertyException(ASCJINConstants.ASCJErrors.ERPTakeMarketPrice, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldVerifying<InventoryItem, ASCJINInventoryItemExt.usrASCJContractSurcharge> e)
        {
            if (e.Row == null) return;

            if ((decimal?)e.NewValue < 0.0m)
            {
                e.Cache.RaiseExceptionHandling<ASCJINInventoryItemExt.usrASCJContractSurcharge>(e.Row, e.NewValue,
                    new PXSetPropertyException(ASCJINConstants.ASCJWarnings.SurchargeIsNegative, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldVerifying<InventoryItem, ASCJINInventoryItemExt.usrASCJCostingType> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);

            var defaultVendor = GetDefaultVendor();
            if (defaultVendor == null) return;

            var vendorItemExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(defaultVendor);
            if (rowExt != null && e.NewValue?.ToString() != ASCJConstants.CostingType.ContractCost && true == defaultVendor.IsDefault == vendorItemExt.UsrASCJIsOverrideVendor)
            {
                Base.Item.Cache.RaiseExceptionHandling<ASCJINInventoryItemExt.usrASCJCostingType>(row, e.NewValue,
                    new PXSetPropertyException(ASCJINConstants.ASCJWarnings.CostingTypeIsNotContract, PXErrorLevel.RowWarning));
            }
        }

        protected virtual void _(Events.FieldUpdating<InventoryItem, InventoryItem.descr> e)
        {
            var row = e.Row;
            if (row == null) return;

            this.ASCJJewelryItemView.SetValueExt<ASCJINJewelryItem.shortDesc>(this.ASCJJewelryItemView.Current, e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, InventoryItem.itemClassID> e)
        {
            var row = e.Row;
            if (e.Row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);

            bool isVisible = IsVisibleFields(rowExt, row.ItemClassID);
            SetVisibleJewelFields(e.Cache, row, isVisible);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrASCJCostingType> e)
        {
            var row = e.Row;
            if (e.Row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);
            UpdateCommodityCostMetal(e.Cache, row, rowExt);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrASCJActualGRAMGold> e)
        {
            var row = e.Row;
            if (row == null || Base.IsCopyPasteContext) return;

            var mult = ASCJMetalType.GetGoldTypeValue(this.ASCJJewelryItemView.Current?.MetalType);

            decimal? pricingGRAMGold = (decimal?)e.NewValue * mult / 24;
            e.Cache.SetValueExt<ASCJINInventoryItemExt.usrASCJPricingGRAMGold>(row, pricingGRAMGold);

            RecalculateInventoryFabricationValue(row);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrASCJActualGRAMSilver> e)
        {
            var row = e.Row;
            if (row == null || Base.IsCopyPasteContext) return;

            var value = ASCJMetalType.GetSilverTypeValue(this.ASCJJewelryItemView.Current?.MetalType);

            e.Cache.SetValueExt<ASCJINInventoryItemExt.usrASCJPricingGRAMSilver>(row, (decimal?)e.NewValue * value);

            RecalculateInventoryFabricationValue(row);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrASCJPricingGRAMGold> e)
        {
            var row = e.Row;
            if (row == null || Base.IsCopyPasteContext) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);

            UpdateCommodityCostMetal(e.Cache, row, rowExt);

            var valueMult = ASCJMetalType.GetGoldTypeValue(this.ASCJJewelryItemView.Current?.MetalType);

            var actualGRAMGold = (decimal?)e.NewValue / valueMult * 24;
            if (actualGRAMGold != rowExt.UsrASCJActualGRAMGold)
            {
                rowExt.UsrASCJActualGRAMGold = actualGRAMGold;
            }

            RecalculateInventoryFabricationValue(row);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrASCJPricingGRAMSilver> e)
        {
            var row = e.Row;
            if (row == null || Base.IsCopyPasteContext) return;

            ASCJINInventoryItemExt rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);

            UpdateCommodityCostMetal(e.Cache, row, rowExt);

            var valueMult = ASCJMetalType.GetSilverTypeValue(this.ASCJJewelryItemView.Current?.MetalType);

            var actualGramSilver = (decimal?)e.NewValue / valueMult;
            if (actualGramSilver != rowExt.UsrASCJActualGRAMSilver)
            {
                rowExt.UsrASCJActualGRAMSilver = actualGramSilver;
            }

            RecalculateInventoryFabricationValue(row);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrASCJPreciousMetalCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrASCJPreciousMetalCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrASCJUnitCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);
            decimal? newValue = (decimal?)e.NewValue;

            rowExt.UsrASCJDutyCost = rowExt.UsrASCJDutyCostPct * newValue / 100.0m;
            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrASCJDutyCost>(rowExt.UsrASCJDutyCost);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrASCJContractIncrement> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);

            var isGold = ASCJMetalType.IsGold(this.ASCJJewelryItemView.Current?.MetalType);

            if (isGold == true)
            {
                UpdateSurcharge<ASCJINInventoryItemExt.usrASCJContractSurcharge>(e.Cache, row, rowExt, this.ASCJJewelryItemView.Current?.MetalType);
                UpdateCommodityCostMetal(e.Cache, row, rowExt);
            }

            //var isSilver = ASCJMetalType.IsGold(this.JewelryItemView.Current?.MetalType);
            //if (isSilver)
            //{
            //    UpdateCommodityCostMetal(e.Cache, row, rowExt);
            //}

            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrASCJContractIncrement>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrASCJMatrixStep> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);
            UpdateCommodityCostMetal(e.Cache, row, rowExt);

            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrASCJMatrixStep>(e.NewValue);
            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrASCJBasisValue>(rowExt.UsrASCJBasisValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrASCJContractSurcharge> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);
            UpdateCommodityCostMetal(e.Cache, row, rowExt);

            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrASCJContractSurcharge>((decimal?)e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrASCJContractLossPct> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);
            UpdateCommodityCostMetal(e.Cache, row, rowExt);

            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrASCJContractLossPct>((decimal?)e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrASCJOtherMaterialsCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrASCJOtherMaterialsCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrASCJFabricationCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrASCJFabricationCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrASCJPackagingCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrASCJPackagingCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrASCJPackagingLaborCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrASCJPackagingLaborCost>(e.NewValue);
        }

        //protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrASCJOtherCost> e)
        //{
        //    var row = e.Row;
        //    if (row == null) return;

        //    UpdatUnitCost(e.Cache, row);
        //    SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrASCJOtherCost>(e.NewValue);
        //}

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrASCJLaborCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrASCJLaborCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrASCJFreightCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrASCJFreightCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrASCJDutyCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);

            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrASCJDutyCost>(e.NewValue);

            if (rowExt.UsrASCJUnitCost == null || rowExt.UsrASCJUnitCost == 0.0m)
            {
                rowExt.UsrASCJDutyCostPct = decimal.Zero;
                return;
            }
            decimal? newCostPctValue = (decimal?)e.NewValue / rowExt.UsrASCJUnitCost * 100.0m;
            if (newCostPctValue == rowExt.UsrASCJDutyCostPct) return;
            rowExt.UsrASCJDutyCostPct = newCostPctValue;
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrASCJDutyCostPct> e)
        {
            var row = e.Row;
            if (row == null) return;

            ASCJINInventoryItemExt rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);

            decimal? newDutyCostValue = rowExt.UsrASCJUnitCost * (decimal?)e.NewValue / 100.00m;
            if (newDutyCostValue == rowExt.UsrASCJDutyCost) return;
            e.Cache.SetValueExt<ASCJINInventoryItemExt.usrASCJDutyCost>(row, newDutyCostValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrASCJHandlingCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrASCJHandlingCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJINInventoryItemExt.usrASCJCommodityType> e)
        {
            var row = e.Row;
            if (row == null || e.NewValue == null) return;

            e.Cache.RaiseExceptionHandling<ASCJINInventoryItemExt.usrASCJCommodityType>(row, e.NewValue,
                new PXSetPropertyException(ASCJINConstants.ASCJWarnings.MetalTypeEmpty, PXErrorLevel.Warning));

            this.ASCJJewelryItemView.SetValueExt<ASCJINJewelryItem.metalType>(this.ASCJJewelryItemView.Current, null);
            ASCJJewelryItemView.Cache.RaiseExceptionHandling<ASCJINJewelryItem.metalType>(ASCJJewelryItemView.Current, null,
                new PXSetPropertyException(ASCJINConstants.ASCJWarnings.SelectMetalType, PXErrorLevel.Warning));

            ASCJINInventoryItemExt rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);
        }

        protected virtual void _(Events.RowPersisting<ASCJINJewelryItem> e)
        {
            var row = e.Row;
            if (row == null || row.MetalType != null) return;

            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(this.Base.Item.Current);

            if (inventoryItemExt.UsrASCJCommodityType == CommodityType.Gold || inventoryItemExt.UsrASCJCommodityType == CommodityType.Silver)
            {
                var errorEx = new PXSetPropertyException<ASCJINJewelryItem.metalType>(ASCJINConstants.ASCJWarnings.MetalTypeEmpty, PXErrorLevel.Error);
                e.Cache.RaiseExceptionHandling<ASCJINJewelryItem.metalType>(row, row.MetalType, errorEx);
                throw new PXException();
            }
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
            if (rowExt?.UsrASCJIsOverrideVendor == true)
            {
                e.Cache.RaiseExceptionHandling<ASCJPOVendorInventoryExt.usrASCJUnitCost>(row, rowExt.UsrASCJUnitCost,
                      new PXSetPropertyException(ASCJINConstants.ASCJWarnings.UnitCostIsCustom, PXErrorLevel.Warning));
            }
            else
            {
                e.Cache.RaiseExceptionHandling<ASCJPOVendorInventoryExt.usrASCJUnitCost>(row, rowExt.UsrASCJUnitCost, null);
            }
        }

        protected virtual void _(Events.FieldSelecting<POVendorInventory, ASCJPOVendorInventoryExt.usrASCJFabricationCost> e)
        {
            var row = e.Row;
            if (row == null || row.VendorID == null) return;

            var poVendorInventoryExt = row.GetExtension<ASCJPOVendorInventoryExt>();
            var calculatedFabricationValue = CalculateFabricationValue(row);

            if (poVendorInventoryExt.UsrASCJFabricationCost != calculatedFabricationValue)
            {
                e.Cache.RaiseExceptionHandling<ASCJPOVendorInventoryExt.usrASCJFabricationCost>(row, poVendorInventoryExt.UsrASCJFabricationCost,
                    new PXSetPropertyException(ASCJINConstants.ASCJWarnings.FabricationCostMismatch, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldVerifying<POVendorInventory, POVendorInventory.isDefault> e)
        {
            var row = e.Row;
            if (row == null || (bool)e.NewValue != true) return;

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(row);
            if (rowExt.UsrASCJMarketID == null)
            {
                e.Cache.RaiseExceptionHandling<ASCJPOVendorInventoryExt.usrASCJMarketID>(row, false, new PXSetPropertyException(ASCJINConstants.ASCJErrors.MarketEmpty, PXErrorLevel.RowError));
            }

            var inventoryID = ASCJMetalType.GetBaseInventoryID(this.Base, this.ASCJJewelryItemView.Current?.MetalType);

            var apVendorPrice = ASCJCostBuilder.GetAPVendorPrice(this.Base, row.VendorID, inventoryID, ASCJConstants.TOZ.value, PXTimeZoneInfo.Today);

            if (apVendorPrice == null && rowExt?.UsrASCJIsOverrideVendor != true)
            {
                e.Cache.RaiseExceptionHandling<POVendorInventory.isDefault>(row, false,
                    new PXSetPropertyException(ASCJMessages.ASCJError.VendorPriceNotFound, PXErrorLevel.RowWarning));
            }

            List<POVendorInventory> selectPOVendors = Base.VendorItems.Select()?.FirstTableItems.ToList();
            foreach (var vendorInventory in selectPOVendors)
            {
                if (vendorInventory.IsDefault == true && vendorInventory != row)
                {
                    this.Base.VendorItems.Cache.SetValue<POVendorInventory.isDefault>(vendorInventory, false);
                    this.Base.VendorItems.View.RequestRefresh();
                    break;
                }
            }
        }

        protected virtual void _(Events.FieldVerifying<POVendorInventory, ASCJPOVendorInventoryExt.usrASCJIsOverrideVendor> e)
        {
            var row = e.Row;
            if (row == null || row.IsDefault != true) return;

            bool newValue = (bool)e.NewValue;
            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(row);

            if (newValue == false) return;

            if (rowExt.UsrASCJCommodityVendorPrice == decimal.Zero)
            {
                e.Cache.RaiseExceptionHandling<ASCJPOVendorInventoryExt.usrASCJCommodityVendorPrice>(row, rowExt.UsrASCJCommodityVendorPrice,
                    new PXSetPropertyException(ASCJMessages.ASCJError.POVendorInventoryVendorPriceEmpty, PXErrorLevel.Error));
            }

            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(Base.Item.Current);
            if (inventoryItemExt != null && inventoryItemExt.UsrASCJCostingType != ASCJConstants.CostingType.ContractCost)
            {
                Base.Item.Cache.RaiseExceptionHandling<ASCJINInventoryItemExt.usrASCJCostingType>(Base.Item.Current, inventoryItemExt.UsrASCJCostingType,
                    new PXSetPropertyException(ASCJINConstants.ASCJWarnings.CostingTypeIsNotContract, PXErrorLevel.RowWarning));
            }
        }

        protected virtual void _(Events.FieldVerifying<POVendorInventory, ASCJPOVendorInventoryExt.usrASCJCommodityVendorPrice> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(row);
            if ((decimal)e.NewValue == decimal.Zero && rowExt.UsrASCJIsOverrideVendor == true)
            {
                e.Cache.RaiseExceptionHandling<ASCJPOVendorInventoryExt.usrASCJCommodityVendorPrice>(row, rowExt.UsrASCJBasisPrice,
                    new PXSetPropertyException(ASCJMessages.ASCJError.POVendorInventoryVendorPriceEmpty, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldVerifying<POVendorInventory, ASCJPOVendorInventoryExt.usrASCJBasisPrice> e)
        {
            var row = e.Row;
            if (row == null) return;

            if ((decimal?)e.NewValue == decimal.Zero)
            {
                e.Cache.RaiseExceptionHandling<ASCJPOVendorInventoryExt.usrASCJBasisPrice>(row, e.NewValue,
                    new PXSetPropertyException(ASCJINConstants.ASCJWarnings.BasisOrMarketPriceEmpty, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldVerifying<POVendorInventory, ASCJPOVendorInventoryExt.usrASCJMatrixStep> e)
        {
            if (e.Row == null) return;

            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(this.Base.Item.Current);
            if ((decimal?)e.NewValue <= 0.0m && inventoryItemExt.UsrASCJCommodityType == ASCJConstants.CommodityType.Silver)
            {
                e.Cache.RaiseExceptionHandling<ASCJPOVendorInventoryExt.usrASCJMatrixStep>(e.Row, 0.5m,
                    new PXSetPropertyException(ASCJINConstants.ASCJErrors.ERPTakeMarketPrice, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldVerifying<POVendorInventory, ASCJPOVendorInventoryExt.usrASCJContractSurcharge> e)
        {
            if (e.Row == null) return;

            var sdfs = e.Row.GetExtension<ASCJPOVendorInventoryExt>();
            if ((decimal?)e.NewValue < 0.0m)
            {
                e.Cache.RaiseExceptionHandling<ASCJPOVendorInventoryExt.usrASCJContractSurcharge>(e.Row, e.NewValue,
                    new PXSetPropertyException(ASCJINConstants.ASCJWarnings.SurchargeIsNegative, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCJPOVendorInventoryExt.usrASCJIsOverrideVendor> e)
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
            e.Cache.SetValue<ASCJPOVendorInventoryExt.usrASCJMarketID>(row, vendorExt.UsrASCJMarketID);

            var inventoryID = ASCJMetalType.GetBaseInventoryID(this.Base, this.ASCJJewelryItemView.Current?.MetalType);

            var apVendorPrice = ASCJCostBuilder.GetAPVendorPrice(this.Base, vendor.BAccountID, inventoryID, ASCJConstants.TOZ.value, PXTimeZoneInfo.Today);

            if (apVendorPrice == null)
            {
                e.Cache.RaiseExceptionHandling<POVendorInventory.vendorID>(row, e.NewValue,
                    new PXSetPropertyException(ASCJINConstants.ASCJWarnings.BasisOrMarketPriceEmpty, PXErrorLevel.Warning));
                return;
            }

            var apVendorPriceExt = apVendorPrice.GetExtension<ASCJAPVendorPriceExt>();
            e.Cache.SetValueExt<ASCJPOVendorInventoryExt.usrASCJContractLossPct>(row, apVendorPriceExt.UsrASCJCommodityLossPct ?? 0.0m);
            e.Cache.SetValueExt<ASCJPOVendorInventoryExt.usrASCJMatrixStep>(row, apVendorPriceExt.UsrASCJMatrixStep ?? 0.0m);
            e.Cache.SetValueExt<ASCJPOVendorInventoryExt.usrASCJBasisValue>(row, apVendorPriceExt.UsrASCJBasisValue ?? 0.0m);
            e.Cache.SetValueExt<ASCJPOVendorInventoryExt.usrASCJCommodityVendorPrice>(row, apVendorPrice.SalesPrice ?? 0.0m);
            e.Cache.SetValueExt<ASCJPOVendorInventoryExt.usrASCJBasisPrice>(row, apVendorPrice.SalesPrice ?? 0.0m);
            e.Cache.SetValueExt<ASCJPOVendorInventoryExt.usrASCJContractSurcharge>(row, apVendorPriceExt.UsrASCJCommoditySurchargePct ?? 0.0m);
            //e.Cache.SetValueExt<ASCJPOVendorInventoryExt.usrASCJFabricationWeight>(row, apVendorPriceExt.UsrASCJLaborPerUnit ?? 0.0m);

            if (row.IsDefault == true && this.Base.Item.Current != null)
            {
                var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(this.Base.Item.Current);

                inventoryItemExt.UsrASCJContractSurcharge = apVendorPriceExt.UsrASCJCommoditySurchargePct;
                inventoryItemExt.UsrASCJContractLossPct = apVendorPriceExt.UsrASCJCommodityLossPct;
                inventoryItemExt.UsrASCJMatrixStep = apVendorPriceExt.UsrASCJMatrixStep;
            }
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCJPOVendorInventoryExt.usrASCJFabricationWeight> e)
        {
            var row = e.Row;
            if (row == null) return;

            RecalculatePOVendorFabricationValue(row);
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCJPOVendorInventoryExt.usrASCJFabricationPiece> e)
        {
            var row = e.Row;
            if (row == null) return;

            RecalculatePOVendorFabricationValue(row);
        }

        //protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCJPOVendorInventoryExt.usrASCJFabricationCost> e)
        //{
        //    var row = e.Row;
        //    if (row == null) return;

        //    var poVendorExt = row.GetExtension<ASCJPOVendorInventoryExt>();

        //    var metalWeight = GetMetalWeight();

        //    var usrASCJLaborPerUnit = metalWeight == 0
        //        ? (decimal?)e.NewValue
        //        : (decimal?)e.NewValue / metalWeight;

        //    if (usrASCJLaborPerUnit != poVendorExt.UsrASCJFabricationWeight)
        //    {
        //        poVendorExt.UsrASCJFabricationWeight = usrASCJLaborPerUnit;
        //    }
        //}

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCJPOVendorInventoryExt.usrASCJCommodityVendorPrice> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(row);
            UpdateItemAndPOVendorInventory(e.Cache, row, rowExt);
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCJINInventoryItemExt.usrASCJContractIncrement> e)
        {
            var row = e.Row;
            if (row == null || row.IsDefault == true) return;

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(row);

            var isGold = ASCJMetalType.IsGold(this.ASCJJewelryItemView.Current?.MetalType);
            if (isGold == true)
            {
                UpdateSurcharge<ASCJPOVendorInventoryExt.usrASCJContractSurcharge>(e.Cache, row, rowExt, this.ASCJJewelryItemView.Current?.MetalType);
            }
            else
            {
                var isSilver = ASCJMetalType.IsSilver(this.ASCJJewelryItemView.Current?.MetalType);
                if (isSilver)
                {
                    UpdateMetalCalcPOVendorItem(e.Cache, row, rowExt);
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCJPOVendorInventoryExt.usrASCJMatrixStep> e)
        {
            var row = e.Row;
            if (row == null || row.IsDefault == true) return;

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(row);
            UpdateMetalCalcPOVendorItem(e.Cache, row, rowExt);
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCJPOVendorInventoryExt.usrASCJContractSurcharge> e)
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
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrASCJUnitCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrASCJEstLandedCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrASCJFabricationCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrASCJPreciousMetalCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrASCJFreightCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrASCJLaborCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrASCJDutyCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrASCJDutyCostPct>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrASCJHandlingCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrASCJPackagingCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrASCJOtherMaterialsCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrASCJPackagingLaborCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrASCJContractLossPct>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrASCJContractIncrement>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrASCJBasisValue>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrASCJContractSurcharge>(cache, row, isVisible);

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(row);

            bool isVisibleGold = isVisible && rowExt.UsrASCJCommodityType == ASCJConstants.CommodityType.Gold;
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrASCJActualGRAMGold>(cache, row, isVisibleGold);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrASCJPricingGRAMGold>(cache, row, isVisibleGold);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrASCJContractIncrement>(cache, row, isVisibleGold);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrASCJIncrement>(cache, row, isVisibleGold);

            bool isVisibleSilver = isVisible && rowExt.UsrASCJCommodityType == ASCJConstants.CommodityType.Silver;
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrASCJActualGRAMSilver>(cache, row, isVisibleSilver);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrASCJPricingGRAMSilver>(cache, row, isVisibleSilver);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrASCJMatrixStep>(cache, row, isVisibleSilver);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrASCJMatrixPriceGram>(cache, row, isVisibleSilver);
            PXUIFieldAttribute.SetVisible<ASCJINInventoryItemExt.usrASCJMatrixPriceTOZ>(cache, row, isVisibleSilver);
        }

        protected virtual bool IsVisibleFields(ASCJINInventoryItemExt rowExt, int? itemClassID)
        {
            INItemClass itemClass = INItemClass.PK.Find(Base, itemClassID);

            return itemClass?.ItemClassCD.Trim() != "COMMODITY" && rowExt?.UsrASCJCommodityType != ASCJConstants.CommodityType.Undefined;
            // acupower: remove from constant to jewel preferences screen and find from rowSelected
        }

        protected virtual void SetReadOnlyJewelAttrFields(PXCache cache, InventoryItem row, string metalType)
        {
            bool isNotGold = !ASCJMetalType.IsGold(metalType);
            bool isNotSilver = !ASCJMetalType.IsSilver(metalType);

            PXUIFieldAttribute.SetReadOnly<ASCJINInventoryItemExt.usrASCJActualGRAMGold>(cache, row, isNotGold);
            PXUIFieldAttribute.SetReadOnly<ASCJINInventoryItemExt.usrASCJPricingGRAMGold>(cache, row, isNotGold);
            PXUIFieldAttribute.SetReadOnly<ASCJINInventoryItemExt.usrASCJContractIncrement>(cache, row, isNotGold);

            PXUIFieldAttribute.SetReadOnly<ASCJINInventoryItemExt.usrASCJActualGRAMSilver>(cache, row, isNotSilver);
            PXUIFieldAttribute.SetReadOnly<ASCJINInventoryItemExt.usrASCJPricingGRAMSilver>(cache, row, isNotSilver);
            PXUIFieldAttribute.SetReadOnly<ASCJINInventoryItemExt.usrASCJMatrixStep>(cache, row, isNotSilver);

            if (isNotGold && isNotSilver)
            {
                PXUIFieldAttribute.SetReadOnly<ASCJINInventoryItemExt.usrASCJPricingGRAMGold>(cache, row, true);
                PXUIFieldAttribute.SetReadOnly<ASCJINInventoryItemExt.usrASCJPricingGRAMSilver>(cache, row, true);
                PXUIFieldAttribute.SetReadOnly<ASCJINInventoryItemExt.usrASCJActualGRAMGold>(cache, row, true);
                PXUIFieldAttribute.SetReadOnly<ASCJINInventoryItemExt.usrASCJActualGRAMSilver>(cache, row, true);
            }
        }

        protected virtual void SetReadOnlyPOVendorInventoryFields(PXCache cache, POVendorInventory row)
        {
            bool isDefaultVendor = row.IsDefault == true;
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrASCJContractIncrement>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrASCJContractLossPct>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrASCJContractSurcharge>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrASCJPreciousMetalCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrASCJOtherMaterialsCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrASCJFabricationCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrASCJPackagingCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrASCJLaborCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrASCJPackagingLaborCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrASCJHandlingCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrASCJFreightCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrASCJDutyCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrASCJMatrixStep>(cache, row, isDefaultVendor);

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(row);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrASCJCommodityVendorPrice>(cache, row, rowExt.UsrASCJIsOverrideVendor != true);
        }

        protected virtual void SetVisiblePOVendorInventoryFields(PXCache cache)
        {
            if (this.Base.Item.Current == null) return;
            var intentoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(this.Base.Item.Current);

            bool isVisible = intentoryItemExt.UsrASCJCommodityType == ASCJConstants.CommodityType.Silver;

            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrASCJFloor>(cache, null, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrASCJCeiling>(cache, null, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrASCJMatrixStep>(cache, null, isVisible);

            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrASCJCommodityID>(cache, null, false);
        }

        protected virtual void UpdateCommodityCostMetal(PXCache cache, InventoryItem row, ASCJINInventoryItemExt rowExt)
        {
            if (rowExt == null) throw new PXException(ASCJINConstants.ASCJErrors.NullInCacheSaveItemFirst);

            var jewelCostBuilder = CreateCostBuilder(rowExt);
            if (jewelCostBuilder == null) return;

            rowExt.UsrASCJPreciousMetalCost = jewelCostBuilder.CalculatePreciousMetalCost(rowExt.UsrASCJCostingType);
            cache.SetValueExt<ASCJINInventoryItemExt.usrASCJPreciousMetalCost>(row, rowExt.UsrASCJPreciousMetalCost);

            cache.SetValueExt<ASCJINInventoryItemExt.usrASCJMarketPriceTOZ>(row, jewelCostBuilder.PreciousMetalMarketCostPerTOZ);
            cache.SetValueExt<ASCJINInventoryItemExt.usrASCJMarketPriceGram>(row, jewelCostBuilder.PreciousMetalMarketCostPerGram);
            cache.SetValueExt<ASCJINInventoryItemExt.usrASCJBasisValue>(row, jewelCostBuilder.BasisValue);


            rowExt.UsrASCJContractIncrement = jewelCostBuilder.CalculateIncrementValue(rowExt);
            if (rowExt.UsrASCJCommodityType == CommodityType.Gold)
            {
                cache.SetValueExt<ASCJINInventoryItemExt.usrASCJIncrement>(row, rowExt.UsrASCJContractIncrement * rowExt.UsrASCJActualGRAMGold);
            }
            if (rowExt.UsrASCJCommodityType == CommodityType.Silver)
            {
                cache.SetValueExt<ASCJINInventoryItemExt.usrASCJIncrement>(row, rowExt.UsrASCJContractIncrement * rowExt.UsrASCJActualGRAMSilver);
            }

            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrASCJContractIncrement>(rowExt.UsrASCJContractIncrement);
            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrASCJBasisValue>(rowExt.UsrASCJBasisValue);
            SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrASCJBasisPrice>(jewelCostBuilder.PreciousMetalContractCostPerTOZ);

            if (ASCJMetalType.IsSilver(jewelCostBuilder.INJewelryItem?.MetalType))
            {
                //    cache.SetValueExt<ASCJINInventoryItemExt.usrASCJMatrixStep>(row, jewelCostBuilder.ma);
                cache.SetValueExt<ASCJINInventoryItemExt.usrASCJFloor>(row, jewelCostBuilder.Floor);
                cache.SetValueExt<ASCJINInventoryItemExt.usrASCJCeiling>(row, jewelCostBuilder.Ceiling);
                cache.SetValueExt<ASCJINInventoryItemExt.usrASCJMatrixPriceTOZ>(row, jewelCostBuilder.PreciousMetalAvrSilverMarketCostPerTOZ);
                cache.SetValueExt<ASCJINInventoryItemExt.usrASCJMatrixPriceGram>(row, jewelCostBuilder.PreciousMetalAvrSilverMarketCostPerTOZ / ASCJConstants.TOZ2GRAM_31_10348.value);
            }

            UpdateCostsCurrentOverridenPOVendorItem(rowExt);

            VerifyLossAndSurcharge(cache, row, rowExt, jewelCostBuilder);
        }

        private decimal? CalculateFabricationValue(POVendorInventory poVendorInventory)
        {
            var poVendorInventoryExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(poVendorInventory);

            var metalWeight = GetMetalWeight();

            var usrASCJFabricationCost = (metalWeight ?? decimal.Zero) * (poVendorInventoryExt.UsrASCJFabricationWeight ?? 0.0m) + (poVendorInventoryExt.UsrASCJFabricationPiece ?? 0.0m);

            return usrASCJFabricationCost;
        }

        private void RecalculateInventoryFabricationValue(InventoryItem inventoryItem)
        {
            POVendorInventory poVendorInventory = GetDefaultVendor();
            if (poVendorInventory == null) return;

            var usrASCJFabricationCost = CalculateFabricationValue(poVendorInventory);

            Base.Item.Cache.SetValueExt<ASCJINInventoryItemExt.usrASCJFabricationCost>(inventoryItem, usrASCJFabricationCost);
        }

        private void RecalculatePOVendorFabricationValue(POVendorInventory poVendorInventory)
        {
            var usrASCJFabricationCost = CalculateFabricationValue(poVendorInventory);

            if (poVendorInventory.IsDefault == true)
            {
                var inventory = Base.Item.Current;
                Base.Item.Cache.SetValueExt<ASCJINInventoryItemExt.usrASCJFabricationCost>(inventory, usrASCJFabricationCost);
            }
            else
            {
                SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrASCJFabricationCost>(usrASCJFabricationCost, poVendorInventory);
            }
        }

        private decimal? GetMetalWeight()
        {
            var inventoryExt = Base.Item.Current.GetExtension<ASCJINInventoryItemExt>();

            var metalType = this.ASCJJewelryItemView.Current?.MetalType;
            decimal? metalWeight;

            switch (metalType)
            {
                case var type when ASCJMetalType.IsGold(type):
                    metalWeight = inventoryExt.UsrASCJActualGRAMGold;
                    break;
                case var type when ASCJMetalType.IsSilver(type):
                    metalWeight = inventoryExt.UsrASCJActualGRAMSilver;
                    break;
                default:
                    metalWeight = 0;
                    break;
            }

            return metalWeight;
        }

        private void UpdateCostsCurrentOverridenPOVendorItem(ASCJINInventoryItemExt inventoryItemExt)
        {
            if (Base.VendorItems.Current == null)
            {
                Base.VendorItems.Current = GetDefaultVendor();
                if (Base.VendorItems.Current == null) return;
            }

            if (inventoryItemExt.UsrASCJCostingType == ASCJConstants.CostingType.MarketCost)
            {
                UpdateMetalCalcPOVendorItem(Base.VendorItems.Cache, Base.VendorItems.Current, Base.VendorItems.Current.GetExtension<ASCJPOVendorInventoryExt>());
            }
            else
            {
                if (Base.VendorItems.Current.IsDefault == true)
                {
                    Base.VendorItems.SetValueExt<ASCJPOVendorInventoryExt.usrASCJPreciousMetalCost>(Base.VendorItems.Current, inventoryItemExt.UsrASCJPreciousMetalCost);
                    Base.VendorItems.SetValueExt<ASCJPOVendorInventoryExt.usrASCJUnitCost>(Base.VendorItems.Current, inventoryItemExt.UsrASCJUnitCost);
                    Base.VendorItems.SetValueExt<ASCJPOVendorInventoryExt.usrASCJFloor>(Base.VendorItems.Current, inventoryItemExt.UsrASCJFloor);
                    Base.VendorItems.SetValueExt<ASCJPOVendorInventoryExt.usrASCJCeiling>(Base.VendorItems.Current, inventoryItemExt.UsrASCJCeiling);
                }
            }
        }

        private void UpdateSurcharge<TField>(PXCache cache, object row, IASCJItemCostSpecDTO rowExt, string metalType) where TField : IBqlField
        {
            if ((rowExt.UsrASCJActualGRAMSilver == null || rowExt.UsrASCJActualGRAMSilver == 0.0m) && (rowExt.UsrASCJActualGRAMGold == null || rowExt.UsrASCJActualGRAMGold == 0.0m)) return;

            var jewelCostBuilder = CreateCostBuilder(rowExt);
            if (jewelCostBuilder == null) return;

            decimal? surchargeValue = ASCJCostBuilder.CalculateSurchargeValue(rowExt.UsrASCJContractIncrement, metalType);
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
            rowExt.UsrASCJActualGRAMGold = inventoryItemExt.UsrASCJActualGRAMGold;
            rowExt.UsrASCJActualGRAMSilver = inventoryItemExt.UsrASCJActualGRAMSilver;

            var jewelCostBuilder = CreateCostBuilder(rowExt, row);
            if (jewelCostBuilder == null) return;

            rowExt.UsrASCJPreciousMetalCost = jewelCostBuilder.CalculatePreciousMetalCost(ASCJConstants.CostingType.ContractCost);
            cache.SetValueExt<ASCJPOVendorInventoryExt.usrASCJPreciousMetalCost>(row, rowExt.UsrASCJPreciousMetalCost);

            rowExt.UsrASCJContractIncrement = jewelCostBuilder.CalculateIncrementValue(rowExt);
            cache.SetValue<ASCJPOVendorInventoryExt.usrASCJContractIncrement>(row, rowExt.UsrASCJContractIncrement);
            //SetValueExtPOVendorInventory<ASCJPOVendorInventoryExt.usrASCJContractIncrement>(rowExt.UsrASCJContractIncrement);

            if (ASCJMetalType.IsSilver(jewelCostBuilder.INJewelryItem?.MetalType))
            {
                cache.SetValueExt<ASCJPOVendorInventoryExt.usrASCJFloor>(row, jewelCostBuilder.Floor);
                cache.SetValueExt<ASCJPOVendorInventoryExt.usrASCJCeiling>(row, jewelCostBuilder.Ceiling);
                cache.SetValueExt<ASCJPOVendorInventoryExt.usrASCJBasisValue>(row, jewelCostBuilder.BasisValue);
            }
        }

        private void SetValueExtPOVendorInventory<TField>(object newValue, POVendorInventory poVendorInventory = null) where TField : IBqlField
        {
            if (poVendorInventory == null)
            {
                poVendorInventory = GetDefaultVendor();
            }

            if (poVendorInventory == null) return;

            this.Base.VendorItems.Cache.SetValueExt<TField>(poVendorInventory, newValue);
            this.Base.VendorItems.Cache.MarkUpdated(poVendorInventory);
        }

        private void UpdateMetalGrams(string metalType)
        {
            if (metalType == null) return;

            var intentoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(this.Base.Item.Current);

            bool isGold = ASCJMetalType.IsGold(metalType) && intentoryItemExt?.UsrASCJCommodityType == ASCJConstants.CommodityType.Gold;
            bool isSilver = ASCJMetalType.IsSilver(metalType) && intentoryItemExt?.UsrASCJCommodityType == ASCJConstants.CommodityType.Silver;

            if (isGold)
            {
                this.Base.Item.Cache.SetValueExt<ASCJINInventoryItemExt.usrASCJActualGRAMSilver>(this.Base.Item.Current, decimal.Zero);
                this.Base.Item.Cache.SetValueExt<ASCJINInventoryItemExt.usrASCJActualGRAMGold>(this.Base.Item.Current,
                               PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(this.Base.Item.Current).UsrASCJActualGRAMGold);

            }
            if (isSilver)
            {
                this.Base.Item.Cache.SetValueExt<ASCJINInventoryItemExt.usrASCJActualGRAMGold>(this.Base.Item.Current, decimal.Zero);
                this.Base.Item.Cache.SetValueExt<ASCJINInventoryItemExt.usrASCJActualGRAMSilver>(this.Base.Item.Current,
                                 PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(this.Base.Item.Current).UsrASCJActualGRAMSilver);
            }

            this.Base.Item.Cache.SetValueExt<ASCJINInventoryItemExt.usrASCJPreciousMetalCost>(this.Base.Item.Current, decimal.Zero);
        }

        private ASCJCostBuilder CreateCostBuilder(IASCJItemCostSpecDTO currentRow, POVendorInventory defaultVendor = null)
        {
            if ((currentRow.UsrASCJActualGRAMSilver == null || currentRow.UsrASCJActualGRAMSilver == 0.0m)
                && (currentRow.UsrASCJActualGRAMGold == null || currentRow.UsrASCJActualGRAMGold == 0.0m)) return null;

            if (defaultVendor == null)
                defaultVendor = GetDefaultVendor();
            if (defaultVendor == null) return null;

            if (this.ASCJJewelryItemView.Current == null)
                this.ASCJJewelryItemView.Current = ASCJJewelryItemView.Select().TopFirst;
            if (this.ASCJJewelryItemView.Current == null && Base.IsCopyPasteContext) // it is fix of copy-paste bug, missing Precious Metal value
                this.ASCJJewelryItemView.Current = this.ASCJJewelryItemView.Cache.Cached.RowCast<ASCJINJewelryItem>().FirstOrDefault();
            if (this.ASCJJewelryItemView.Current == null) return null;

            return new ASCJCostBuilder(this.Base)
                        .WithInventoryItem(currentRow)
                        .WithPOVendorInventory(defaultVendor)
                        .WithJewelryAttrData(this.ASCJJewelryItemView.Current)
                        .WithPricingData(PXTimeZoneInfo.Today)
                        .Build();
        }

        private void CopyPOVendorInventoryToItem(POVendorInventory row)
        {
            var poVendorInventoryExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(row);

            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(this.Base.Item.Current);
            inventoryItemExt.UsrASCJFabricationCost = poVendorInventoryExt.UsrASCJFabricationCost;
            inventoryItemExt.UsrASCJOtherMaterialsCost = poVendorInventoryExt.UsrASCJOtherMaterialsCost;
            inventoryItemExt.UsrASCJPackagingCost = poVendorInventoryExt.UsrASCJPackagingCost;
            inventoryItemExt.UsrASCJPackagingLaborCost = poVendorInventoryExt.UsrASCJPackagingLaborCost;

            inventoryItemExt.UsrASCJLaborCost = poVendorInventoryExt.UsrASCJLaborCost;
            inventoryItemExt.UsrASCJHandlingCost = poVendorInventoryExt.UsrASCJHandlingCost;

            inventoryItemExt.UsrASCJFreightCost = poVendorInventoryExt.UsrASCJFreightCost;
            inventoryItemExt.UsrASCJDutyCost = poVendorInventoryExt.UsrASCJDutyCost;

            inventoryItemExt.UsrASCJContractSurcharge = poVendorInventoryExt.UsrASCJContractSurcharge;
            inventoryItemExt.UsrASCJContractLossPct = poVendorInventoryExt.UsrASCJContractLossPct;
            inventoryItemExt.UsrASCJBasisValue = poVendorInventoryExt.UsrASCJBasisValue;
            inventoryItemExt.UsrASCJMatrixStep = poVendorInventoryExt.UsrASCJMatrixStep;

            this.Base.Item.UpdateCurrent();
        }

        private void VerifyLossAndSurcharge(PXCache cache, InventoryItem row, ASCJINInventoryItemExt rowExt, ASCJCostBuilder costBuilder)
        {
            if (costBuilder.APVendorPriceContract == null) return;

            var vendorExt = PXCache<APVendorPrice>.GetExtension<ASCJAPVendorPriceExt>(costBuilder.APVendorPriceContract);

            if (rowExt.UsrASCJContractLossPct != vendorExt.UsrASCJCommodityLossPct)
            {
                cache.RaiseExceptionHandling<ASCJINInventoryItemExt.usrASCJContractLossPct>(row, rowExt.UsrASCJContractLossPct,
                    new PXSetPropertyException(ASCJINConstants.ASCJWarnings.MissingMatchesLossOrSurcharge, PXErrorLevel.Warning));
            }
            else
            {
                cache.RaiseExceptionHandling<ASCJINInventoryItemExt.usrASCJContractLossPct>(row, rowExt.UsrASCJContractLossPct, null);
            }
            if (rowExt.UsrASCJContractSurcharge != vendorExt.UsrASCJCommoditySurchargePct)
            {
                cache.RaiseExceptionHandling<ASCJINInventoryItemExt.usrASCJContractSurcharge>(row, rowExt.UsrASCJContractSurcharge,
                    new PXSetPropertyException(ASCJINConstants.ASCJWarnings.MissingMatchesLossOrSurcharge, PXErrorLevel.Warning));
            }
            else
            {
                cache.RaiseExceptionHandling<ASCJINInventoryItemExt.usrASCJContractSurcharge>(row, rowExt.UsrASCJContractSurcharge, null);
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
            this.Base.VendorItems.Select().FirstTableItems.FirstOrDefault(x => x.IsDefault == true);

        private List<CSAttributeDetail> SelectAttributeDetails(string attributeID) =>
             SelectFrom<CSAttributeDetail>.Where<CSAttributeDetail.attributeID.IsEqual<@P.AsString>>.View.Select(this.Base, attributeID)?.FirstTableItems.ToList();


        #endregion Helper Methods
    }
}