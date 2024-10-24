using ASCJSMCustom.AP.CacheExt;
using ASCJSMCustom.AP.DAC;
using ASCJSMCustom.Common.Builder;
using ASCJSMCustom.Common.Descriptor;
using ASCJSMCustom.Common.DTO.Interfaces;
using ASCJSMCustom.Common.Helper;
using ASCJSMCustom.IN.Descriptor.Constants;
using ASCJSMCustom.PO.CacheExt;
using PX.Common;
using PX.CS;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.PO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static ASCJSMCustom.Common.Descriptor.ASCJSMConstants;
using ASCJSMCustom.IN.CacheExt;
using ASCJSMCustom.IN.DAC;

namespace ASCJSMCustom.IN.GraphExt
{
    public class ASCJSMInventoryItemMaintExt : PXGraphExtension<InventoryItemMaint>
    {
        public static bool IsActive() => true;

        #region Selects

        public SelectFrom<ASCJSMINJewelryItem>.Where<ASCJSMINJewelryItem.inventoryID.IsEqual<InventoryItem.inventoryID.FromCurrent>>.View JewelryItemView;

        public SelectFrom<ASCJSMINVendorDuty>.Where<ASCJSMINVendorDuty.inventoryID.IsEqual<InventoryItem.inventoryID.FromCurrent>>.View VendorDutyView;


        #endregion Selects

        #region Cache Attached
        [PXMergeAttributes(Method = MergeMethod.Replace)]
        [PXDBString(30, IsUnicode = true, InputMask = "####.##.####")]
        [PXUIField(DisplayName = "Tariff / HTS Code")]
        [PXSelector(typeof(SearchFor<ASCJSMAPTariffHTSCode.hSTariffCode>))]
        protected virtual void _(Events.CacheAttached<InventoryItem.hSTariffCode> e) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = "Last Modified Date", IsReadOnly = true)]
        protected virtual void _(Events.CacheAttached<INItemXRef.lastModifiedDateTime> e) { }
        #endregion

        #region Actions
        public PXAction<InventoryItem> UpdateMetalCost;
        [PXUIField(DisplayName = "Update Metal Cost", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable updateMetalCost(PXAdapter adapter)
        {
            if (this.Base.Item.Current == null) return adapter.Get();

            UpdateCommodityCostMetal(this.Base.Item.Cache, this.Base.Item.Current, this.Base.Item.Current.GetExtension<ASCJSMINInventoryItemExt>());

            return adapter.Get();
        }
        #endregion Action

        #region Event Handlers

        #region InventoryItem Events

        protected virtual void _(Events.FieldSelecting<InventoryItem, ASCJSMINInventoryItemExt.usrBasisValue> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(row);
            UpdateCommodityCostMetal(e.Cache, row, rowExt);
            e.ReturnValue = rowExt.UsrBasisValue;
        }

        protected virtual void _(Events.RowSelected<InventoryItem> e)
        {
            var row = e.Row;
            if (row == null) return;
            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(row);

            bool isVisible = IsVisibleFields(rowExt, row.ItemClassID);
            SetVisibleJewelFields(e.Cache, row, isVisible);

            if (this.JewelryItemView.Current == null)
                this.JewelryItemView.Current = this.JewelryItemView.Select();

            SetReadOnlyJewelAttrFields(e.Cache, row, this.JewelryItemView.Current?.MetalType);

            bool isRequire = isVisible || rowExt.UsrCommodityType == CommodityType.Gold || rowExt.UsrCommodityType == CommodityType.Silver;
            PXUIFieldAttribute.SetRequired<ASCJSMINJewelryItem.metalType>(this.JewelryItemView.Cache, isRequire);
            PXDefaultAttribute.SetPersistingCheck<ASCJSMINJewelryItem.metalType>(this.JewelryItemView.Cache, this.JewelryItemView.Current,
                isRequire ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
        }

        protected virtual void _(Events.FieldDefaulting<InventoryItem, ASCJSMINInventoryItemExt.usrCostingType> e)
        {
            if (e.Row == null) return;

            INItemClass itemClass = INItemClass.PK.Find(Base, e.Row.ItemClassID);
            ASCJSMINItemClassExt classExt = itemClass?.GetExtension<ASCJSMINItemClassExt>();
            e.NewValue = classExt?.UsrCostingType ?? ASCJSMConstants.CostingType.ContractCost;
        }

        protected virtual void _(Events.FieldVerifying<InventoryItem, ASCJSMINInventoryItemExt.usrMatrixStep> e)
        {
            if (e.Row == null) return;

            if ((decimal?)e.NewValue <= 0.0m)
            {
                e.Cache.RaiseExceptionHandling<ASCJSMINInventoryItemExt.usrMatrixStep>(e.Row, 0.5m,
                    new PXSetPropertyException(ASCJSMINConstants.Errors.ERPTakeMarketPrice, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldVerifying<InventoryItem, ASCJSMINInventoryItemExt.usrContractSurcharge> e)
        {
            if (e.Row == null) return;

            if ((decimal?)e.NewValue < 0.0m)
            {
                e.Cache.RaiseExceptionHandling<ASCJSMINInventoryItemExt.usrContractSurcharge>(e.Row, e.NewValue,
                    new PXSetPropertyException(ASCJSMINConstants.Warnings.SurchargeIsNegative, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldVerifying<InventoryItem, ASCJSMINInventoryItemExt.usrCostingType> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(row);

            var defaultVendor = GetDefaultVendor();
            if (defaultVendor == null) return;

            var vendorItemExt = PXCache<POVendorInventory>.GetExtension<ASCJSMPOVendorInventoryExt>(defaultVendor);
            if (rowExt != null && e.NewValue?.ToString() != ASCJSMConstants.CostingType.ContractCost && true == defaultVendor.IsDefault == vendorItemExt.UsrIsOverrideVendor)
            {
                Base.Item.Cache.RaiseExceptionHandling<ASCJSMINInventoryItemExt.usrCostingType>(row, e.NewValue,
                    new PXSetPropertyException(ASCJSMINConstants.Warnings.CostingTypeIsNotContract, PXErrorLevel.RowWarning));
            }
        }

        protected virtual void _(Events.FieldUpdating<InventoryItem, InventoryItem.descr> e)
        {
            var row = e.Row;
            if (row == null) return;

            if (this.JewelryItemView.Current == null) this.JewelryItemView.Current = this.JewelryItemView.Select();
            if (this.JewelryItemView.Current == null) this.JewelryItemView.Current = this.JewelryItemView.Insert();

            this.JewelryItemView.SetValueExt<ASCJSMINJewelryItem.shortDesc>(this.JewelryItemView.Current, e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, InventoryItem.itemClassID> e)
        {
            var row = e.Row;
            if (e.Row == null) return;
            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(row);

            bool isVisible = IsVisibleFields(rowExt, row.ItemClassID);
            SetVisibleJewelFields(e.Cache, row, isVisible);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJSMINInventoryItemExt.usrCostingType> e)
        {
            var row = e.Row;
            if (e.Row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(row);
            UpdateCommodityCostMetal(e.Cache, row, rowExt);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJSMINInventoryItemExt.usrActualGRAMGold> e)
        {
            var row = e.Row;
            if (row == null || Base.IsCopyPasteContext) return;

            var mult = ASCJSMMetalType.GetGoldTypeValue(this.JewelryItemView.Current?.MetalType);

            decimal? pricingGRAMGold = (decimal?)e.NewValue * mult / 24;
            e.Cache.SetValueExt<ASCJSMINInventoryItemExt.usrPricingGRAMGold>(row, pricingGRAMGold);

            RecalculateInventoryFabricationValue(row);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJSMINInventoryItemExt.usrActualGRAMSilver> e)
        {
            var row = e.Row;
            if (row == null || Base.IsCopyPasteContext) return;

            var value = ASCJSMMetalType.GetSilverTypeValue(this.JewelryItemView.Current?.MetalType);

            e.Cache.SetValueExt<ASCJSMINInventoryItemExt.usrPricingGRAMSilver>(row, (decimal?)e.NewValue * value);

            RecalculateInventoryFabricationValue(row);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJSMINInventoryItemExt.usrPricingGRAMGold> e)
        {
            var row = e.Row;
            if (row == null || Base.IsCopyPasteContext) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(row);

            UpdateCommodityCostMetal(e.Cache, row, rowExt);

            var valueMult = ASCJSMMetalType.GetGoldTypeValue(this.JewelryItemView.Current?.MetalType);

            var actualGRAMGold = (decimal?)e.NewValue / valueMult * 24;
            if (actualGRAMGold != rowExt.UsrActualGRAMGold)
            {
                rowExt.UsrActualGRAMGold = actualGRAMGold;
            }

            RecalculateInventoryFabricationValue(row);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJSMINInventoryItemExt.usrPricingGRAMSilver> e)
        {
            var row = e.Row;
            if (row == null || Base.IsCopyPasteContext) return;

            ASCJSMINInventoryItemExt rowExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(row);

            UpdateCommodityCostMetal(e.Cache, row, rowExt);

            var valueMult = ASCJSMMetalType.GetSilverTypeValue(this.JewelryItemView.Current?.MetalType);

            var actualGramSilver = (decimal?)e.NewValue / valueMult;
            if (actualGramSilver != rowExt.UsrActualGRAMSilver)
            {
                rowExt.UsrActualGRAMSilver = actualGramSilver;
            }

            RecalculateInventoryFabricationValue(row);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJSMINInventoryItemExt.usrPreciousMetalCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCJSMPOVendorInventoryExt.usrPreciousMetalCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJSMINInventoryItemExt.usrUnitCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(row);
            decimal? newValue = (decimal?)e.NewValue;

            rowExt.UsrDutyCost = rowExt.UsrDutyCostPct * newValue / 100.0m;
            SetValueExtPOVendorInventory<ASCJSMPOVendorInventoryExt.usrDutyCost>(rowExt.UsrDutyCost);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJSMINInventoryItemExt.usrContractIncrement> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(row);

            var isGold = ASCJSMMetalType.IsGold(this.JewelryItemView.Current?.MetalType);

            if (isGold == true)
            {
                UpdateSurcharge<ASCJSMINInventoryItemExt.usrContractSurcharge>(e.Cache, row, rowExt, this.JewelryItemView.Current?.MetalType);
                UpdateCommodityCostMetal(e.Cache, row, rowExt);
            }

            //var isSilver = ASCIStarMetalType.IsGold(this.JewelryItemView.Current?.MetalType);
            //if (isSilver)
            //{
            //    UpdateCommodityCostMetal(e.Cache, row, rowExt);
            //}

            SetValueExtPOVendorInventory<ASCJSMPOVendorInventoryExt.usrContractIncrement>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJSMINInventoryItemExt.usrIncrement> e)
        {
            //var row = e.Row;
            //if (row == null) return;

            //var rowExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(row);

            //var isGold = ASCJSMMetalType.IsGold(this.JewelryItemView.Current?.MetalType);

            //if (isGold == true)
            //{
            //    UpdateSurcharge<ASCJSMINInventoryItemExt.usrIncrement>(e.Cache, row, rowExt, this.JewelryItemView.Current?.MetalType);
            //    UpdateCommodityCostMetal(e.Cache, row, rowExt);
            //}

            //SetValueExtPOVendorInventory<ASCJSMPOVendorInventoryExt.usrContractIncrement>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJSMINInventoryItemExt.usrMatrixStep> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(row);
            UpdateCommodityCostMetal(e.Cache, row, rowExt);

            SetValueExtPOVendorInventory<ASCJSMPOVendorInventoryExt.usrMatrixStep>(e.NewValue);
            SetValueExtPOVendorInventory<ASCJSMPOVendorInventoryExt.usrBasisValue>(rowExt.UsrBasisValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJSMINInventoryItemExt.usrContractSurcharge> e)
        {
            var row = e.Row;
            if (row == null) return;


            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(row);

            //Base.Item.Current.GetExtension<ASCJSMINInventoryItemExt>().UsrAddOnValue = Base.Item.Current.GetExtension<ASCJSMINInventoryItemExt>().UsrBasisValue * (1 + Base.Item.Current.GetExtension<ASCJSMINInventoryItemExt>().UsrContractSurcharge / 100) * Base.Item.Current.GetExtension<ASCJSMINInventoryItemExt>().UsrContractSurchargeAmount;

            UpdateCommodityCostMetal(e.Cache, row, rowExt);

            SetValueExtPOVendorInventory<ASCJSMPOVendorInventoryExt.usrContractSurcharge>((decimal?)e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJSMINInventoryItemExt.usrContractLossPct> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(row);
            UpdateCommodityCostMetal(e.Cache, row, rowExt);

            SetValueExtPOVendorInventory<ASCJSMPOVendorInventoryExt.usrContractLossPct>((decimal?)e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJSMINInventoryItemExt.usrOtherMaterialsCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCJSMPOVendorInventoryExt.usrOtherMaterialsCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJSMINInventoryItemExt.usrFabricationCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCJSMPOVendorInventoryExt.usrFabricationCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJSMINInventoryItemExt.usrPackagingCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCJSMPOVendorInventoryExt.usrPackagingCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJSMINInventoryItemExt.usrPackagingLaborCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCJSMPOVendorInventoryExt.usrPackagingLaborCost>(e.NewValue);
        }

        //protected virtual void _(Events.FieldUpdated<InventoryItem, ASCIStarINInventoryItemExt.usrOtherCost> e)
        //{
        //    var row = e.Row;
        //    if (row == null) return;

        //    UpdatUnitCost(e.Cache, row);
        //    SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrOtherCost>(e.NewValue);
        //}

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJSMINInventoryItemExt.usrLaborCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCJSMPOVendorInventoryExt.usrLaborCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJSMINInventoryItemExt.usrFreightCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCJSMPOVendorInventoryExt.usrFreightCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJSMINInventoryItemExt.usrDutyCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(row);

            SetValueExtPOVendorInventory<ASCJSMPOVendorInventoryExt.usrDutyCost>(e.NewValue);

            if (rowExt.UsrUnitCost == null || rowExt.UsrUnitCost == 0.0m)
            {
                rowExt.UsrDutyCostPct = decimal.Zero;
                return;
            }
            decimal? newCostPctValue = (decimal?)e.NewValue / rowExt.UsrUnitCost * 100.0m;
            if (newCostPctValue == rowExt.UsrDutyCostPct) return;
            rowExt.UsrDutyCostPct = newCostPctValue;
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJSMINInventoryItemExt.usrDutyCostPct> e)
        {
            var row = e.Row;
            if (row == null) return;

            ASCJSMINInventoryItemExt rowExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(row);

            decimal? newDutyCostValue = rowExt.UsrUnitCost * (decimal?)e.NewValue / 100.00m;
            if (newDutyCostValue == rowExt.UsrDutyCost) return;
            e.Cache.SetValueExt<ASCJSMINInventoryItemExt.usrDutyCost>(row, newDutyCostValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJSMINInventoryItemExt.usrHandlingCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetValueExtPOVendorInventory<ASCJSMPOVendorInventoryExt.usrHandlingCost>(e.NewValue);
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, ASCJSMINInventoryItemExt.usrCommodityType> e)
        {
            var row = e.Row;
            if (row == null || e.NewValue == null) return;

            e.Cache.RaiseExceptionHandling<ASCJSMINInventoryItemExt.usrCommodityType>(row, e.NewValue,
                new PXSetPropertyException(ASCJSMINConstants.Warnings.MetalTypeEmpty, PXErrorLevel.Warning));

            //this.JewelryItemView.SetValueExt<ASCIStarINJewelryItem.metalType>(this.JewelryItemView.Current, null);
            JewelryItemView.Cache.RaiseExceptionHandling<ASCJSMINJewelryItem.metalType>(JewelryItemView.Current, null,
                new PXSetPropertyException(ASCJSMINConstants.Warnings.SelectMetalType, PXErrorLevel.Warning));
        }

        #endregion InventoryItem Events

        #region JewelryItem Events

        protected virtual void _(Events.FieldUpdated<ASCJSMINJewelryItem, ASCJSMINJewelryItem.metalType> e)
        {
            var row = e.Row;
            if (row == null || this.Base.Item.Current == null) return;

            UpdateMetalGrams(e.NewValue?.ToString());
            UpdateCommodityCostMetal(this.Base.Item.Cache, this.Base.Item.Current, PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(this.Base.Item.Current));
        }

        protected virtual void _(Events.RowPersisting<ASCJSMINJewelryItem> e)
        {
            var row = e.Row;
            if (row == null || row.MetalType != null) return;

            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(this.Base.Item.Current);

            if (inventoryItemExt.UsrCommodityType == CommodityType.Gold || inventoryItemExt.UsrCommodityType == CommodityType.Silver)
            {
                var errorEx = new PXSetPropertyException<ASCJSMINJewelryItem.metalType>(ASCJSMINConstants.Warnings.MetalTypeEmpty, PXErrorLevel.Error);
                e.Cache.RaiseExceptionHandling<ASCJSMINJewelryItem.metalType>(row, row.MetalType, errorEx);
                throw new PXException();
            }
        }

        #endregion JewelryItem Events

        #region POVendorInventory Events
        protected virtual void _(Events.RowSelected<POVendorInventory> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetReadOnlyPOVendorInventoryFields(e.Cache, row);
            SetVisiblePOVendorInventoryFields(e.Cache);

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJSMPOVendorInventoryExt>(row);
            if (rowExt?.UsrIsOverrideVendor == true)
            {
                e.Cache.RaiseExceptionHandling<ASCJSMPOVendorInventoryExt.usrUnitCost>(row, rowExt.UsrUnitCost,
                      new PXSetPropertyException(ASCJSMINConstants.Warnings.UnitCostIsCustom, PXErrorLevel.Warning));
            }
            else
            {
                e.Cache.RaiseExceptionHandling<ASCJSMPOVendorInventoryExt.usrUnitCost>(row, rowExt.UsrUnitCost, null);
            }
        }

        protected virtual void _(Events.FieldSelecting<POVendorInventory, ASCJSMPOVendorInventoryExt.usrFabricationCost> e)
        {
            var row = e.Row;
            if (row == null || row.VendorID == null) return;

            var poVendorInventoryExt = row.GetExtension<ASCJSMPOVendorInventoryExt>();
            var calculatedFabricationValue = CalculateFabricationValue(row);

            if (poVendorInventoryExt.UsrFabricationCost != calculatedFabricationValue)
            {
                e.Cache.RaiseExceptionHandling<ASCJSMPOVendorInventoryExt.usrFabricationCost>(row, poVendorInventoryExt.UsrFabricationCost,
                    new PXSetPropertyException(ASCJSMINConstants.Warnings.FabricationCostMismatch, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldVerifying<POVendorInventory, POVendorInventory.isDefault> e)
        {
            var row = e.Row;
            if (row == null || (bool)e.NewValue != true) return;

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJSMPOVendorInventoryExt>(row);
            if (rowExt.UsrMarketID == null)
            {
                e.Cache.RaiseExceptionHandling<ASCJSMPOVendorInventoryExt.usrMarketID>(row, false, new PXSetPropertyException(ASCJSMINConstants.Errors.MarketEmpty, PXErrorLevel.RowError));
            }

            var inventoryID = ASCJSMMetalType.GetCommodityInventoryByMetalType(this.Base, this.JewelryItemView.Current?.MetalType);

            var apVendorPrice = ASCJSMCostBuilder.GetAPVendorPrice(this.Base, row.VendorID, inventoryID, ASCJSMConstants.TOZ.value, PXTimeZoneInfo.Today);

            if (apVendorPrice == null && PXCache<POVendorInventory>.GetExtension<ASCJSMPOVendorInventoryExt>(row).UsrIsOverrideVendor != true)
            {
                e.Cache.RaiseExceptionHandling<POVendorInventory.isDefault>(row, false,
                    new PXSetPropertyException(ASCJSMMessages.Error.VendorPriceNotFound, PXErrorLevel.RowWarning));
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

        protected virtual void _(Events.FieldVerifying<POVendorInventory, ASCJSMPOVendorInventoryExt.usrIsOverrideVendor> e)
        {
            var row = e.Row;
            if (row == null || row.IsDefault != true) return;

            bool newValue = (bool)e.NewValue;
            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJSMPOVendorInventoryExt>(row);

            if (newValue == false) return;

            if (rowExt.UsrCommodityVendorPrice == decimal.Zero)
            {
                e.Cache.RaiseExceptionHandling<ASCJSMPOVendorInventoryExt.usrCommodityVendorPrice>(row, rowExt.UsrCommodityVendorPrice,
                    new PXSetPropertyException(ASCJSMMessages.Error.POVendorInventoryVendorPriceEmpty, PXErrorLevel.Error));
            }

            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(Base.Item.Current);
            if (inventoryItemExt != null && inventoryItemExt.UsrCostingType != ASCJSMConstants.CostingType.ContractCost)
            {
                Base.Item.Cache.RaiseExceptionHandling<ASCJSMINInventoryItemExt.usrCostingType>(Base.Item.Current, inventoryItemExt.UsrCostingType,
                    new PXSetPropertyException(ASCJSMINConstants.Warnings.CostingTypeIsNotContract, PXErrorLevel.RowWarning));
            }
        }

        protected virtual void _(Events.FieldVerifying<POVendorInventory, ASCJSMPOVendorInventoryExt.usrCommodityVendorPrice> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJSMPOVendorInventoryExt>(row);
            if ((decimal)e.NewValue == decimal.Zero && rowExt.UsrIsOverrideVendor == true)
            {
                e.Cache.RaiseExceptionHandling<ASCJSMPOVendorInventoryExt.usrCommodityVendorPrice>(row, rowExt.UsrBasisPrice,
                    new PXSetPropertyException(ASCJSMMessages.Error.POVendorInventoryVendorPriceEmpty, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldVerifying<POVendorInventory, ASCJSMPOVendorInventoryExt.usrBasisPrice> e)
        {
            var row = e.Row;
            if (row == null) return;

            if ((decimal?)e.NewValue == decimal.Zero)
            {
                e.Cache.RaiseExceptionHandling<ASCJSMPOVendorInventoryExt.usrBasisPrice>(row, e.NewValue,
                    new PXSetPropertyException(ASCJSMINConstants.Warnings.BasisOrMarketPriceEmpty, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldVerifying<POVendorInventory, ASCJSMPOVendorInventoryExt.usrMatrixStep> e)
        {
            if (e.Row == null) return;

            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(this.Base.Item.Current);
            if ((decimal?)e.NewValue <= 0.0m && inventoryItemExt.UsrCommodityType == ASCJSMConstants.CommodityType.Silver)
            {
                e.Cache.RaiseExceptionHandling<ASCJSMPOVendorInventoryExt.usrMatrixStep>(e.Row, 0.5m,
                    new PXSetPropertyException(ASCJSMINConstants.Errors.ERPTakeMarketPrice, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldVerifying<POVendorInventory, ASCJSMPOVendorInventoryExt.usrContractSurcharge> e)
        {
            if (e.Row == null) return;

            var sdfs = e.Row.GetExtension<ASCJSMPOVendorInventoryExt>();
            if ((decimal?)e.NewValue < 0.0m)
            {
                e.Cache.RaiseExceptionHandling<ASCJSMPOVendorInventoryExt.usrContractSurcharge>(e.Row, e.NewValue,
                    new PXSetPropertyException(ASCJSMINConstants.Warnings.SurchargeIsNegative, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCJSMPOVendorInventoryExt.usrIsOverrideVendor> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJSMPOVendorInventoryExt>(row);

            UpdateItemAndPOVendorInventory(e.Cache, row, rowExt);
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, POVendorInventory.isDefault> e)
        {
            var row = e.Row;
            if (row == null || Base.IsCopyPasteContext) return;

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJSMPOVendorInventoryExt>(row);
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
            ASCJSMVendorExt vendorExt = vendor?.GetExtension<ASCJSMVendorExt>();
            e.Cache.SetValue<ASCJSMPOVendorInventoryExt.usrMarketID>(row, vendorExt.UsrMarketID);

            var inventoryID = ASCJSMMetalType.GetCommodityInventoryByMetalType(this.Base, this.JewelryItemView.Current?.MetalType);

            var apVendorPrice = ASCJSMCostBuilder.GetAPVendorPrice(this.Base, vendor.BAccountID, inventoryID, ASCJSMConstants.TOZ.value, PXTimeZoneInfo.Today);

            if (apVendorPrice == null)
            {
                e.Cache.RaiseExceptionHandling<POVendorInventory.vendorID>(row, e.NewValue,
                    new PXSetPropertyException(ASCJSMINConstants.Warnings.BasisOrMarketPriceEmpty, PXErrorLevel.Warning));
                return;
            }

            var apVendorPriceExt = apVendorPrice.GetExtension<ASCJSMAPVendorPriceExt>();
            e.Cache.SetValueExt<ASCJSMPOVendorInventoryExt.usrContractLossPct>(row, apVendorPriceExt.UsrCommodityLossPct ?? 0.0m);
            e.Cache.SetValueExt<ASCJSMPOVendorInventoryExt.usrMatrixStep>(row, apVendorPriceExt.UsrMatrixStep ?? 0.0m);
            e.Cache.SetValueExt<ASCJSMPOVendorInventoryExt.usrBasisValue>(row, apVendorPriceExt.UsrBasisValue ?? 0.0m);
            e.Cache.SetValueExt<ASCJSMPOVendorInventoryExt.usrCommodityVendorPrice>(row, apVendorPrice.SalesPrice ?? 0.0m);
            e.Cache.SetValueExt<ASCJSMPOVendorInventoryExt.usrBasisPrice>(row, apVendorPrice.SalesPrice ?? 0.0m);
            e.Cache.SetValueExt<ASCJSMPOVendorInventoryExt.usrContractSurcharge>(row, apVendorPriceExt.UsrCommoditySurchargePct ?? 0.0m);

            if (row.IsDefault == true && this.Base.Item.Current != null)
            {
                var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(this.Base.Item.Current);

                inventoryItemExt.UsrContractSurcharge = apVendorPriceExt.UsrCommoditySurchargePct;
                inventoryItemExt.UsrContractLossPct = apVendorPriceExt.UsrCommodityLossPct;
                inventoryItemExt.UsrMatrixStep = apVendorPriceExt.UsrMatrixStep;
            }
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCJSMPOVendorInventoryExt.usrFabricationWeight> e)
        {
            var row = e.Row;
            if (row == null) return;

            RecalculatePOVendorFabricationValue(row);
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCJSMPOVendorInventoryExt.usrFabricationPiece> e)
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

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCJSMPOVendorInventoryExt.usrCommodityVendorPrice> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJSMPOVendorInventoryExt>(row);
            UpdateItemAndPOVendorInventory(e.Cache, row, rowExt);
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCJSMINInventoryItemExt.usrContractIncrement> e)
        {
            var row = e.Row;
            if (row == null || row.IsDefault == true) return;

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJSMPOVendorInventoryExt>(row);

            var isGold = ASCJSMMetalType.IsGold(this.JewelryItemView.Current?.MetalType);
            if (isGold == true)
            {
                UpdateSurcharge<ASCJSMPOVendorInventoryExt.usrContractSurcharge>(e.Cache, row, rowExt, this.JewelryItemView.Current?.MetalType);
            }
            else
            {
                var isSilver = ASCJSMMetalType.IsSilver(this.JewelryItemView.Current?.MetalType);
                if (isSilver)
                {
                    UpdateMetalCalcPOVendorItem(e.Cache, row, rowExt);
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCJSMPOVendorInventoryExt.usrMatrixStep> e)
        {
            var row = e.Row;
            if (row == null || row.IsDefault == true) return;

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJSMPOVendorInventoryExt>(row);
            UpdateMetalCalcPOVendorItem(e.Cache, row, rowExt);
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCJSMPOVendorInventoryExt.usrContractSurcharge> e)
        {
            var row = e.Row;
            if (row == null) return;

            if (row.IsDefault != true)
            {
                var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJSMPOVendorInventoryExt>(row);
                UpdateMetalCalcPOVendorItem(e.Cache, row, rowExt);
            }
        }



        #endregion POVendorInventory Events

        #region VendorDuty Events
        protected virtual void _(Events.FieldUpdated<ASCJSMINVendorDuty, ASCJSMINVendorDuty.vendorID> e)
        {
            if (e.Row == null) return;

            e.Cache.RaiseFieldDefaulting<ASCJSMINVendorDuty.countryID>(e.Row, out object countryID);
            e.Cache.SetValueExt<ASCJSMINVendorDuty.countryID>(e.Row, countryID);
        }
        #endregion

        #region CreationDate Event
        protected void _(Events.RowInserting<INItemXRef> row)
        {
            if (row.Row == null) return;

            var inItemXRef = row.Row;

            var inItemXRefExt = inItemXRef.GetExtension<ASCJSMINItemXRefExt>();

            inItemXRefExt.UsrCreationDate = DateTime.Now;
        }

        #endregion

        #endregion Event Handlers

        #region Helper Methods

        protected virtual void SetVisibleJewelFields(PXCache cache, InventoryItem row, bool isVisible)
        {
            PXUIFieldAttribute.SetVisible<ASCJSMINInventoryItemExt.usrUnitCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJSMINInventoryItemExt.usrEstLandedCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJSMINInventoryItemExt.usrFabricationCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJSMINInventoryItemExt.usrPreciousMetalCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJSMINInventoryItemExt.usrFreightCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJSMINInventoryItemExt.usrLaborCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJSMINInventoryItemExt.usrDutyCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJSMINInventoryItemExt.usrDutyCostPct>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJSMINInventoryItemExt.usrHandlingCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJSMINInventoryItemExt.usrPackagingCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJSMINInventoryItemExt.usrOtherMaterialsCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJSMINInventoryItemExt.usrPackagingLaborCost>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJSMINInventoryItemExt.usrContractLossPct>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJSMINInventoryItemExt.usrContractIncrement>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJSMINInventoryItemExt.usrBasisValue>(cache, row, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJSMINInventoryItemExt.usrContractSurcharge>(cache, row, isVisible);

            var rowExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(row);

            bool isVisibleGold = isVisible && rowExt.UsrCommodityType == ASCJSMConstants.CommodityType.Gold;
            PXUIFieldAttribute.SetVisible<ASCJSMINInventoryItemExt.usrActualGRAMGold>(cache, row, isVisibleGold);
            PXUIFieldAttribute.SetVisible<ASCJSMINInventoryItemExt.usrPricingGRAMGold>(cache, row, isVisibleGold);
            PXUIFieldAttribute.SetVisible<ASCJSMINInventoryItemExt.usrContractIncrement>(cache, row, isVisibleGold);
            PXUIFieldAttribute.SetVisible<ASCJSMINInventoryItemExt.usrIncrement>(cache, row, isVisibleGold);

            bool isVisibleSilver = isVisible && rowExt.UsrCommodityType == ASCJSMConstants.CommodityType.Silver;
            PXUIFieldAttribute.SetVisible<ASCJSMINInventoryItemExt.usrActualGRAMSilver>(cache, row, isVisibleSilver);
            PXUIFieldAttribute.SetVisible<ASCJSMINInventoryItemExt.usrPricingGRAMSilver>(cache, row, isVisibleSilver);
            PXUIFieldAttribute.SetVisible<ASCJSMINInventoryItemExt.usrMatrixStep>(cache, row, isVisibleSilver);
            PXUIFieldAttribute.SetVisible<ASCJSMINInventoryItemExt.usrMatrixPriceGram>(cache, row, isVisibleSilver);
            PXUIFieldAttribute.SetVisible<ASCJSMINInventoryItemExt.usrMatrixPriceTOZ>(cache, row, isVisibleSilver);
        }

        protected virtual bool IsVisibleFields(ASCJSMINInventoryItemExt rowExt, int? itemClassID)
        {
            INItemClass itemClass = INItemClass.PK.Find(Base, itemClassID);

            return itemClass?.ItemClassCD.Trim() != "COMMODITY" && rowExt?.UsrCommodityType != ASCJSMConstants.CommodityType.Undefined;
            // acupower: remove from constant to jewel preferences screen and find from rowSelected
        }

        protected virtual void SetReadOnlyJewelAttrFields(PXCache cache, InventoryItem row, string metalType)
        {
            bool isNotGold = !ASCJSMMetalType.IsGold(metalType);
            bool isNotSilver = !ASCJSMMetalType.IsSilver(metalType);

            PXUIFieldAttribute.SetReadOnly<ASCJSMINInventoryItemExt.usrActualGRAMGold>(cache, row, isNotGold);
            PXUIFieldAttribute.SetReadOnly<ASCJSMINInventoryItemExt.usrPricingGRAMGold>(cache, row, isNotGold);
            PXUIFieldAttribute.SetReadOnly<ASCJSMINInventoryItemExt.usrContractIncrement>(cache, row, isNotGold);

            PXUIFieldAttribute.SetReadOnly<ASCJSMINInventoryItemExt.usrActualGRAMSilver>(cache, row, isNotSilver);
            PXUIFieldAttribute.SetReadOnly<ASCJSMINInventoryItemExt.usrPricingGRAMSilver>(cache, row, isNotSilver);
            PXUIFieldAttribute.SetReadOnly<ASCJSMINInventoryItemExt.usrMatrixStep>(cache, row, isNotSilver);

            if (isNotGold && isNotSilver)
            {
                PXUIFieldAttribute.SetReadOnly<ASCJSMINInventoryItemExt.usrPricingGRAMGold>(cache, row, true);
                PXUIFieldAttribute.SetReadOnly<ASCJSMINInventoryItemExt.usrPricingGRAMSilver>(cache, row, true);
                PXUIFieldAttribute.SetReadOnly<ASCJSMINInventoryItemExt.usrActualGRAMGold>(cache, row, true);
                PXUIFieldAttribute.SetReadOnly<ASCJSMINInventoryItemExt.usrActualGRAMSilver>(cache, row, true);
            }
        }

        protected virtual void SetReadOnlyPOVendorInventoryFields(PXCache cache, POVendorInventory row)
        {
            bool isDefaultVendor = row.IsDefault == true;
            PXUIFieldAttribute.SetReadOnly<ASCJSMPOVendorInventoryExt.usrContractIncrement>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJSMPOVendorInventoryExt.usrContractLossPct>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJSMPOVendorInventoryExt.usrContractSurcharge>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJSMPOVendorInventoryExt.usrPreciousMetalCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJSMPOVendorInventoryExt.usrOtherMaterialsCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJSMPOVendorInventoryExt.usrFabricationCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJSMPOVendorInventoryExt.usrPackagingCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJSMPOVendorInventoryExt.usrLaborCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJSMPOVendorInventoryExt.usrPackagingLaborCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJSMPOVendorInventoryExt.usrHandlingCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJSMPOVendorInventoryExt.usrFreightCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJSMPOVendorInventoryExt.usrDutyCost>(cache, row, isDefaultVendor);
            PXUIFieldAttribute.SetReadOnly<ASCJSMPOVendorInventoryExt.usrMatrixStep>(cache, row, isDefaultVendor);

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJSMPOVendorInventoryExt>(row);
            PXUIFieldAttribute.SetReadOnly<ASCJSMPOVendorInventoryExt.usrCommodityVendorPrice>(cache, row, rowExt.UsrIsOverrideVendor != true);
        }

        protected virtual void SetVisiblePOVendorInventoryFields(PXCache cache)
        {
            if (this.Base.Item.Current == null) return;
            var intentoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(this.Base.Item.Current);

            bool isVisible = intentoryItemExt.UsrCommodityType == ASCJSMConstants.CommodityType.Silver;

            PXUIFieldAttribute.SetVisible<ASCJSMPOVendorInventoryExt.usrFloor>(cache, null, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJSMPOVendorInventoryExt.usrCeiling>(cache, null, isVisible);
            PXUIFieldAttribute.SetVisible<ASCJSMPOVendorInventoryExt.usrMatrixStep>(cache, null, isVisible);

            PXUIFieldAttribute.SetVisible<ASCJSMPOVendorInventoryExt.usrCommodityID>(cache, null, false);
        }

        protected virtual void UpdateCommodityCostMetal(PXCache cache, InventoryItem row, ASCJSMINInventoryItemExt rowExt)
        {
            if (rowExt == null) throw new PXException(ASCJSMINConstants.Errors.NullInCacheSaveItemFirst);

            var jewelCostBuilder = CreateCostBuilder(rowExt);
            if (jewelCostBuilder == null) return;

            rowExt.UsrPreciousMetalCost = jewelCostBuilder.CalculatePreciousMetalCost(rowExt.UsrCostingType);
            cache.SetValueExt<ASCJSMINInventoryItemExt.usrPreciousMetalCost>(row, rowExt.UsrPreciousMetalCost);

            cache.SetValueExt<ASCJSMINInventoryItemExt.usrMarketPriceTOZ>(row, jewelCostBuilder.PreciousMetalMarketCostPerTOZ);
            cache.SetValueExt<ASCJSMINInventoryItemExt.usrMarketPriceGram>(row, jewelCostBuilder.PreciousMetalMarketCostPerGram);
            cache.SetValueExt<ASCJSMINInventoryItemExt.usrBasisValue>(row, jewelCostBuilder.BasisValue);
            //cache.SetValueExt<ASCJSMINInventoryItemExt.usrAddOnValue>(row, jewelCostBuilder.BasisValue * (1 + rowExt.UsrContractSurcharge / 100) + rowExt.UsrContractSurchargeAmount);


            rowExt.UsrContractIncrement = jewelCostBuilder.CalculateIncrementValue(rowExt);
            if (rowExt.UsrCommodityType == CommodityType.Gold)
            {
                cache.SetValueExt<ASCJSMINInventoryItemExt.usrIncrement>(row, rowExt.UsrContractIncrement * rowExt.UsrActualGRAMGold);
            }
            if (rowExt.UsrCommodityType == CommodityType.Silver)
            {
                cache.SetValueExt<ASCJSMINInventoryItemExt.usrIncrement>(row, rowExt.UsrContractIncrement * rowExt.UsrActualGRAMSilver);
            }

            SetValueExtPOVendorInventory<ASCJSMPOVendorInventoryExt.usrContractIncrement>(rowExt.UsrContractIncrement);
            SetValueExtPOVendorInventory<ASCJSMPOVendorInventoryExt.usrBasisValue>(rowExt.UsrBasisValue);
            SetValueExtPOVendorInventory<ASCJSMPOVendorInventoryExt.usrBasisPrice>(jewelCostBuilder.PreciousMetalContractCostPerTOZ);

            if (ASCJSMMetalType.IsSilver(jewelCostBuilder.INJewelryItem?.MetalType))
            {
                //    cache.SetValueExt<ASCIStarINInventoryItemExt.usrMatrixStep>(row, jewelCostBuilder.ma);
                cache.SetValueExt<ASCJSMINInventoryItemExt.usrFloor>(row, jewelCostBuilder.Floor);
                cache.SetValueExt<ASCJSMINInventoryItemExt.usrCeiling>(row, jewelCostBuilder.Ceiling);
                cache.SetValueExt<ASCJSMINInventoryItemExt.usrMatrixPriceTOZ>(row, jewelCostBuilder.PreciousMetalAvrSilverMarketCostPerTOZ);
                cache.SetValueExt<ASCJSMINInventoryItemExt.usrMatrixPriceGram>(row, jewelCostBuilder.PreciousMetalAvrSilverMarketCostPerTOZ / ASCJSMConstants.TOZ2GRAM_31_10348.value);
                //cache.SetValueExt<ASCJSMINInventoryItemExt.usrMarketPriceAddOn>(row, jewelCostBuilder.PreciousMetalAvrSilverMarketCostPerTOZ * (1 + rowExt.UsrContractSurcharge / 100) + rowExt.UsrContractSurchargeAmount);
            }

            UpdateCostsCurrentOverridenPOVendorItem(rowExt);

            VerifyLossAndSurcharge(cache, row, rowExt, jewelCostBuilder);
        }

        private decimal? CalculateFabricationValue(POVendorInventory poVendorInventory)
        {
            var poVendorInventoryExt = PXCache<POVendorInventory>.GetExtension<ASCJSMPOVendorInventoryExt>(poVendorInventory);

            var metalWeight = GetMetalWeight();

            var usrFabricationCost = (metalWeight ?? decimal.Zero) * (poVendorInventoryExt.UsrFabricationWeight ?? 0.0m) + (poVendorInventoryExt.UsrFabricationPiece ?? 0.0m);

            return usrFabricationCost;
        }

        private void RecalculateInventoryFabricationValue(InventoryItem inventoryItem)
        {
            POVendorInventory poVendorInventory = GetDefaultVendor();
            if (poVendorInventory == null) return;

            var usrFabricationCost = CalculateFabricationValue(poVendorInventory);

            Base.Item.Cache.SetValueExt<ASCJSMINInventoryItemExt.usrFabricationCost>(inventoryItem, usrFabricationCost);
        }

        private void RecalculatePOVendorFabricationValue(POVendorInventory poVendorInventory)
        {
            var usrFabricationCost = CalculateFabricationValue(poVendorInventory);

            if (poVendorInventory.IsDefault == true)
            {
                var inventory = Base.Item.Current;
                Base.Item.Cache.SetValueExt<ASCJSMINInventoryItemExt.usrFabricationCost>(inventory, usrFabricationCost);
            }
            else
            {
                SetValueExtPOVendorInventory<ASCJSMPOVendorInventoryExt.usrFabricationCost>(usrFabricationCost, poVendorInventory);
            }
        }

        private decimal? GetMetalWeight()
        {
            var inventoryExt = Base.Item.Current.GetExtension<ASCJSMINInventoryItemExt>();

            var metalType = this.JewelryItemView.Current?.MetalType;
            decimal? metalWeight;

            switch (metalType)
            {
                case var type when ASCJSMMetalType.IsGold(type):
                    metalWeight = inventoryExt.UsrActualGRAMGold;
                    break;
                case var type when ASCJSMMetalType.IsSilver(type):
                    metalWeight = inventoryExt.UsrActualGRAMSilver;
                    break;
                default:
                    metalWeight = 0;
                    break;
            }

            return metalWeight;
        }

        private void UpdateCostsCurrentOverridenPOVendorItem(ASCJSMINInventoryItemExt inventoryItemExt)
        {
            if (Base.VendorItems.Current == null)
            {
                Base.VendorItems.Current = GetDefaultVendor();
                if (Base.VendorItems.Current == null) return;
            }

            if (inventoryItemExt.UsrCostingType == ASCJSMConstants.CostingType.MarketCost)
            {
                UpdateMetalCalcPOVendorItem(Base.VendorItems.Cache, Base.VendorItems.Current, Base.VendorItems.Current.GetExtension<ASCJSMPOVendorInventoryExt>());
            }
            else
            {
                if (Base.VendorItems.Current.IsDefault == true)
                {
                    Base.VendorItems.SetValueExt<ASCJSMPOVendorInventoryExt.usrPreciousMetalCost>(Base.VendorItems.Current, inventoryItemExt.UsrPreciousMetalCost);
                    Base.VendorItems.SetValueExt<ASCJSMPOVendorInventoryExt.usrUnitCost>(Base.VendorItems.Current, inventoryItemExt.UsrUnitCost);
                    Base.VendorItems.SetValueExt<ASCJSMPOVendorInventoryExt.usrFloor>(Base.VendorItems.Current, inventoryItemExt.UsrFloor);
                    Base.VendorItems.SetValueExt<ASCJSMPOVendorInventoryExt.usrCeiling>(Base.VendorItems.Current, inventoryItemExt.UsrCeiling);
                }
            }
        }

        private void UpdateSurcharge<TField>(PXCache cache, object row, IASCJSMItemCostSpecDTO rowExt, string metalType) where TField : IBqlField
        {
            if ((rowExt.UsrActualGRAMSilver == null || rowExt.UsrActualGRAMSilver == 0.0m) && (rowExt.UsrActualGRAMGold == null || rowExt.UsrActualGRAMGold == 0.0m)) return;

            var jewelCostBuilder = CreateCostBuilder(rowExt);
            if (jewelCostBuilder == null) return;

            decimal? surchargeValue = ASCJSMCostBuilder.CalculateSurchargeValue(rowExt.UsrContractIncrement, metalType);
            cache.SetValueExt<TField>(row, surchargeValue);
        }

        protected virtual void UpdateItemAndPOVendorInventory(PXCache cache, POVendorInventory row, ASCJSMPOVendorInventoryExt rowExt)
        {
            if (row.IsDefault == true)
            {
                var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(this.Base.Item.Current);
                UpdateCommodityCostMetal(this.Base.Item.Cache, this.Base.Item.Current, inventoryItemExt);
            }
            else
            {
                UpdateMetalCalcPOVendorItem(cache, row, rowExt);
            }
        }

        private void UpdateMetalCalcPOVendorItem(PXCache cache, POVendorInventory row, ASCJSMPOVendorInventoryExt rowExt)
        {
            if (Base.Item.Current == null) return;
            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(Base.Item.Current);
            rowExt.UsrActualGRAMGold = inventoryItemExt.UsrActualGRAMGold;
            rowExt.UsrActualGRAMSilver = inventoryItemExt.UsrActualGRAMSilver;

            var jewelCostBuilder = CreateCostBuilder(rowExt, row);
            if (jewelCostBuilder == null) return;

            rowExt.UsrPreciousMetalCost = jewelCostBuilder.CalculatePreciousMetalCost(ASCJSMConstants.CostingType.ContractCost);
            cache.SetValueExt<ASCJSMPOVendorInventoryExt.usrPreciousMetalCost>(row, rowExt.UsrPreciousMetalCost);

            rowExt.UsrContractIncrement = jewelCostBuilder.CalculateIncrementValue(rowExt);
            cache.SetValue<ASCJSMPOVendorInventoryExt.usrContractIncrement>(row, rowExt.UsrContractIncrement);
            //SetValueExtPOVendorInventory<ASCIStarPOVendorInventoryExt.usrContractIncrement>(rowExt.UsrContractIncrement);

            if (ASCJSMMetalType.IsSilver(jewelCostBuilder.INJewelryItem?.MetalType))
            {
                cache.SetValueExt<ASCJSMPOVendorInventoryExt.usrFloor>(row, jewelCostBuilder.Floor);
                cache.SetValueExt<ASCJSMPOVendorInventoryExt.usrCeiling>(row, jewelCostBuilder.Ceiling);
                cache.SetValueExt<ASCJSMPOVendorInventoryExt.usrBasisValue>(row, jewelCostBuilder.BasisValue);
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

            var intentoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(this.Base.Item.Current);

            bool isGold = ASCJSMMetalType.IsGold(metalType) && intentoryItemExt?.UsrCommodityType == ASCJSMConstants.CommodityType.Gold;
            bool isSilver = ASCJSMMetalType.IsSilver(metalType) && intentoryItemExt?.UsrCommodityType == ASCJSMConstants.CommodityType.Silver;

            if (isGold)
            {
                this.Base.Item.Cache.SetValueExt<ASCJSMINInventoryItemExt.usrActualGRAMSilver>(this.Base.Item.Current, decimal.Zero);
                this.Base.Item.Cache.SetValueExt<ASCJSMINInventoryItemExt.usrActualGRAMGold>(this.Base.Item.Current,
                               PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(this.Base.Item.Current).UsrActualGRAMGold);

            }
            if (isSilver)
            {
                this.Base.Item.Cache.SetValueExt<ASCJSMINInventoryItemExt.usrActualGRAMGold>(this.Base.Item.Current, decimal.Zero);
                this.Base.Item.Cache.SetValueExt<ASCJSMINInventoryItemExt.usrActualGRAMSilver>(this.Base.Item.Current,
                                 PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(this.Base.Item.Current).UsrActualGRAMSilver);
            }

            this.Base.Item.Cache.SetValueExt<ASCJSMINInventoryItemExt.usrPreciousMetalCost>(this.Base.Item.Current, decimal.Zero);
        }

        private ASCJSMCostBuilder CreateCostBuilder(IASCJSMItemCostSpecDTO currentRow, POVendorInventory defaultVendor = null)
        {
            if ((currentRow.UsrActualGRAMSilver == null || currentRow.UsrActualGRAMSilver == 0.0m)
                && (currentRow.UsrActualGRAMGold == null || currentRow.UsrActualGRAMGold == 0.0m)) return null;

            if (defaultVendor == null)
                defaultVendor = GetDefaultVendor();
            if (defaultVendor == null) return null;

            if (this.JewelryItemView.Current == null)
                this.JewelryItemView.Current = JewelryItemView.Select().TopFirst;
            if (this.JewelryItemView.Current == null && Base.IsCopyPasteContext) // it is fix of copy-paste bug, missing Precious Metal value
                this.JewelryItemView.Current = this.JewelryItemView.Cache.Cached.RowCast<ASCJSMINJewelryItem>().FirstOrDefault();
            if (this.JewelryItemView.Current == null) return null;

            return new ASCJSMCostBuilder(this.Base)
                        .WithInventoryItem(currentRow)
                        .WithPOVendorInventory(defaultVendor)
                        .WithJewelryAttrData(this.JewelryItemView.Current)
                        .WithPricingData(PXTimeZoneInfo.Today)
                        .Build();
        }

        private void CopyPOVendorInventoryToItem(POVendorInventory row)
        {
            var poVendorInventoryExt = PXCache<POVendorInventory>.GetExtension<ASCJSMPOVendorInventoryExt>(row);

            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJSMINInventoryItemExt>(this.Base.Item.Current);
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

        private void VerifyLossAndSurcharge(PXCache cache, InventoryItem row, ASCJSMINInventoryItemExt rowExt, ASCJSMCostBuilder costBuilder)
        {
            if (costBuilder.APVendorPriceContract == null) return;

            var vendorExt = PXCache<APVendorPrice>.GetExtension<ASCJSMAPVendorPriceExt>(costBuilder.APVendorPriceContract);

            if (rowExt.UsrContractLossPct != vendorExt.UsrCommodityLossPct)
            {
                cache.RaiseExceptionHandling<ASCJSMINInventoryItemExt.usrContractLossPct>(row, rowExt.UsrContractLossPct,
                    new PXSetPropertyException(ASCJSMINConstants.Warnings.MissingMatchesLossOrSurcharge, PXErrorLevel.Warning));
            }
            else
            {
                cache.RaiseExceptionHandling<ASCJSMINInventoryItemExt.usrContractLossPct>(row, rowExt.UsrContractLossPct, null);
            }
            if (rowExt.UsrContractSurcharge != vendorExt.UsrCommoditySurchargePct)
            {
                cache.RaiseExceptionHandling<ASCJSMINInventoryItemExt.usrContractSurcharge>(row, rowExt.UsrContractSurcharge,
                    new PXSetPropertyException(ASCJSMINConstants.Warnings.MissingMatchesLossOrSurcharge, PXErrorLevel.Warning));
            }
            else
            {
                cache.RaiseExceptionHandling<ASCJSMINInventoryItemExt.usrContractSurcharge>(row, rowExt.UsrContractSurcharge, null);
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