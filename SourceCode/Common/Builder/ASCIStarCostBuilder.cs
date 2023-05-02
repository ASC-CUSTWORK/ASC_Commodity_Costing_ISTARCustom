﻿using ASCISTARCustom.Common.Descriptor;
using ASCISTARCustom.Common.DTO;
using ASCISTARCustom.Common.DTO.Interfaces;
using ASCISTARCustom.Common.Helper;
using ASCISTARCustom.Cost.CacheExt;
using ASCISTARCustom.Inventory.DAC;
using ASCISTARCustom.Inventory.Descriptor.Constants;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.PO;
using System;
using System.Collections.Generic;
using System.Linq;
using static ASCISTARCustom.Common.Descriptor.ASCIStarConstants;

namespace ASCISTARCustom.Common.Builder
{
    public class ASCIStarCostBuilder
    {
        #region Properies
        private string Currency { get; set; } = "USD";
        private bool IsEnabledOverrideVendor { get; set; }
        private InventoryItem PreciousMetalItem { get; set; }

        public ASCIStarINJewelryItem INJewelryItem { get; set; }
        public IASCIStarItemCostSpecDTO ItemCostSpecification { get; set; }
        public POVendorInventory POVendorInventory { get; set; }
        public ASCIStarPOVendorInventoryExt POVendorInventoryExt { get; set; }
        public DateTime PricingDate { get; set; } = PXTimeZoneInfo.Today;
        public decimal? PreciousMetalContractCostPerTOZ { get; private set; }
        public decimal? PreciousMetalMarketCostPerTOZ { get; private set; }
        public decimal? PreciousMetalContractCostPerGram { get; private set; }
        public decimal? PreciousMetalMarketCostPerGram { get; private set; }
        public decimal? PreciousMetalUnitCost { get; private set; }
        public decimal? AvrPreciousMetalUnitCost { get; private set; }
        public decimal? Floor { get; private set; }
        public decimal? Ceiling { get; private set; }
        public decimal? BasisValue { get; private set; } = decimal.Zero;
        #endregion 

        private readonly PXGraph _graph;

        #region ctor
        public ASCIStarCostBuilder(PXGraph graph)
        {
            _graph = graph;
        }
        #endregion

        #region Chain Method Calls
        public ASCIStarCostBuilder WithInventoryItem(IASCIStarItemCostSpecDTO inventory)
        {
            ItemCostSpecification = inventory;
            return this;
        }
        public ASCIStarCostBuilder WithPOVendorInventory(POVendorInventory vendorInventory)
        {
            POVendorInventory = vendorInventory;
            POVendorInventoryExt = PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(vendorInventory);
            IsEnabledOverrideVendor = POVendorInventoryExt.UsrIsOverrideVendor == true;
            return this;
        }
        public ASCIStarCostBuilder WithCurrency(string currency)
        {
            Currency = currency;
            return this;
        }
        public ASCIStarCostBuilder WithPricingData(DateTime pricingData)
        {
            PricingDate = pricingData;
            return this;
        }
        public ASCIStarCostBuilder Build()
        {
            Initialize();
            return this;
        }
        #endregion

        public virtual void Initialize()
        {
            INJewelryItem = GetASCIStarINJewelryItem(ItemCostSpecification.InventoryID);
            if (INJewelryItem != null && INJewelryItem.MetalType != null)
            {
                if (ASCIStarMetalType.IsGold(INJewelryItem.MetalType))
                    PreciousMetalItem = GetInventoryItemByInvenctoryCD("24K");
                else if (ASCIStarMetalType.IsSilver(INJewelryItem.MetalType))
                    PreciousMetalItem = GetInventoryItemByInvenctoryCD("SSS");

                if (PreciousMetalItem == null) return;

                PreciousMetalContractCostPerTOZ = IsEnabledOverrideVendor ? POVendorInventoryExt.UsrCommodityVendorPrice : GetVendorPricePerTOZ(POVendorInventory.VendorID, PreciousMetalItem.InventoryID);
                PreciousMetalMarketCostPerTOZ = GetVendorPricePerTOZ(POVendorInventoryExt.UsrMarketID, PreciousMetalItem.InventoryID);

                if (ASCIStarMetalType.IsGold(INJewelryItem.MetalType))
                {
                    PreciousMetalContractCostPerGram = PreciousMetalContractCostPerTOZ * ASCIStarMetalType.GetMultFactorConvertTOZtoGram("24K");
                    PreciousMetalMarketCostPerGram = PreciousMetalMarketCostPerTOZ * ASCIStarMetalType.GetMultFactorConvertTOZtoGram("24K");
                    BasisValue = PreciousMetalContractCostPerTOZ;
                }
                else if (ASCIStarMetalType.IsSilver(INJewelryItem.MetalType))
                {
                    PreciousMetalContractCostPerGram = PreciousMetalContractCostPerTOZ * ASCIStarMetalType.GetMultFactorConvertTOZtoGram("SSS");
                    PreciousMetalMarketCostPerGram = PreciousMetalMarketCostPerTOZ * ASCIStarMetalType.GetMultFactorConvertTOZtoGram("SSS");
                    BasisValue = (PreciousMetalContractCostPerTOZ + (PreciousMetalContractCostPerTOZ + ItemCostSpecification.UsrMatrixStep)) / 2;
                }
                AvrPreciousMetalUnitCost = GetPresiousMetalAvrCost();
            }
        }

        public virtual decimal? CalculateSurchargeValue(IASCIStarItemCostSpecDTO itemCostSpecification)
        {
            decimal? tempValue = itemCostSpecification.UsrContractIncrement * TOZ2GRAM_31_10348.value;

            decimal? surchargeNewValue = (tempValue - 1.0m) * 100.0m;

            return surchargeNewValue;
        }

        /// <summary>
        /// Calculates the value of gold increment based on the effective base price per ounce, the metal type and the item cost specification.
        /// </summary>
        /// <param name="itemCostSpecification">The item cost specification</param>
        /// <param name="effectiveBasePricePerOz">The effective base price per ounce</param>
        /// <param name="metalType">The metal type</param>
        /// <returns>The value of gold increment</returns>
        public virtual decimal? CalculateGoldIncrementValue(IASCIStarItemCostSpecDTO itemCostSpecification)
        {
            var goldMetalFactor = ASCIStarMetalType.GetMultFactorConvertTOZtoGram(INJewelryItem?.MetalType);

            decimal? incrementValue = goldMetalFactor * (1.0m + (itemCostSpecification.UsrContractSurcharge ?? 0.0m) / 100.0m);

            return incrementValue;
        }

        ///<summary>Calculates the precious metal cost for an item based on its cost specification, effective base price per ounce, and metal type. 
        ///It uses the ASCIStarMetalType class to determine if the metal type is gold or silver and calculates the precious metal cost accordingly. The metal loss and surcharge values are also factored in.</summary>
        ///<param name="costSpecDTO">The data transfer object containing the item's cost specifications.</param>
        ///<param name="effectivePricePerOz">The effective base price per ounce of the metal in the item.</param>
        ///<param name="metalType">The type of metal in the item.</param>
        ///<returns>The cost of the precious metals in the item.</returns>
        public virtual decimal? CalculatePreciousMetalCost(string costingType = null)
        {
            decimal? preciousMetalCost = decimal.Zero;

            decimal priciousMetalMultFactor = ASCIStarMetalType.GetMultFactorConvertTOZtoGram(INJewelryItem?.MetalType);

            if (ASCIStarMetalType.IsGold(INJewelryItem?.MetalType))
            {
                switch (costingType ?? ItemCostSpecification.UsrCostingType)
                {
                    case ASCIStarCostingType.ContractCost:
                        preciousMetalCost = PreciousMetalContractCostPerTOZ;
                        break;
                    case ASCIStarCostingType.MarketCost:
                        preciousMetalCost = PreciousMetalMarketCostPerTOZ;
                        break;
                    case ASCIStarCostingType.StandardCost:
                        return AvrPreciousMetalUnitCost - ItemCostSpecification.UsrFabricationCost - ItemCostSpecification.UsrOtherMaterialsCost - ItemCostSpecification.UsrPackagingCost;

                    default: break;
                }
                preciousMetalCost = preciousMetalCost * priciousMetalMultFactor * ItemCostSpecification.UsrActualGRAMGold ?? 0m;

            }
            else if (ASCIStarMetalType.IsSilver(INJewelryItem?.MetalType))
            {
                switch (costingType ?? ItemCostSpecification.UsrCostingType)
                {
                    case ASCIStarCostingType.ContractCost:
                        //preciousMetalCost = GetSilverMetalCostPerOZ(PreciousMetalContractCostPerTOZ, PreciousMetalMarketCostPerTOZ, ItemCostSpecification.MatrixStep);
                        break;
                    case ASCIStarCostingType.MarketCost:
                        //preciousMetalCost = GetSilverMetalCostPerOZ(PreciousMetalContractCostPerTOZ, PreciousMetalMarketCostPerTOZ, ItemCostSpecification.MatrixStep);
                        break;
                    case ASCIStarCostingType.StandardCost:
                        return AvrPreciousMetalUnitCost - ItemCostSpecification.UsrFabricationCost - ItemCostSpecification.UsrOtherMaterialsCost - ItemCostSpecification.UsrPackagingCost;

                    default: break;
                }
                preciousMetalCost = GetSilverMetalCostPerOZ(PreciousMetalContractCostPerTOZ, PreciousMetalMarketCostPerTOZ, ItemCostSpecification.UsrMatrixStep)
                                        * priciousMetalMultFactor * ItemCostSpecification.UsrActualGRAMSilver;
            }

            decimal? surchargeValue = (100.0m + (ItemCostSpecification.UsrContractSurcharge ?? 0.0m)) / 100.0m;
            decimal? metalLossValue = (100.0m + (ItemCostSpecification.UsrContractLossPct ?? 0.0m)) / 100.0m;
            PreciousMetalUnitCost = preciousMetalCost * metalLossValue * surchargeValue;
            return PreciousMetalUnitCost;
        }

        public virtual decimal? GetVendorPricePerTOZ(int? vendorID, int? inventoryID)
        {
            var result = GetAPVendorPrice(_graph, vendorID, inventoryID, TOZ.value, PricingDate);
            if (result == null)
            {
                return 0.0m;
            }

            return result.SalesPrice;
        }

        //public static decimal? CalculateUnitCost(IASCIStarItemCostSpecDTO itemCostSpecification)
        //{
        //    if (itemCostSpecification == null) return 0;

        //    return (itemCostSpecification?.UsrPreciousMetalCost ?? 0m)
        //         + (itemCostSpecification?.UsrOtherMaterialsCost ?? 0m)
        //         + (itemCostSpecification?.UsrFabricationCost ?? 0m)
        //         + (itemCostSpecification?.UsrPackagingCost ?? 0m)
        //         + (itemCostSpecification?.UsrPackagingLaborCost ?? 0m);
        //}

        //public static decimal? CalculateEstLandedCost(IASCIStarItemCostSpecDTO itemCostSpecification)
        //{
        //    return (itemCostSpecification?.UsrUnitCost ?? 0m)
        //        + (itemCostSpecification?.UsrHandlingCost ?? 0m)
        //        + (itemCostSpecification?.UsrFreightCost ?? 0m)
        //        + (itemCostSpecification?.UsrLaborCost ?? 0m)
        //        + (itemCostSpecification?.UsrDutyCost ?? 0m);
        //}

        //public static decimal? CalculateUnitCost(ASCIStarItemCostSpecDTO costSpecDTO)
        //{
        //    return (costSpecDTO?.PreciousMetalCost ?? 0m)
        //        + (costSpecDTO?.MaterialsCost ?? 0m)
        //        + (costSpecDTO?.FabricationCost ?? 0m)
        //        + (costSpecDTO?.PackagingCost ?? 0m)
        //        + (costSpecDTO?.PackagingLaborCost ?? 0m);
        //}

        //public static decimal? CalculateLandedCost(ASCIStarItemCostSpecDTO costSpecDTO)
        //{
        //    return (costSpecDTO?.UnitCost ?? 0m)
        //        + (costSpecDTO?.HandlingCost ?? 0m)
        //        + (costSpecDTO?.FreightCost ?? 0m)
        //        + (costSpecDTO?.LaborCost ?? 0m)
        //        + (costSpecDTO?.DutyCost ?? 0m);
        //}

        //public static decimal? CalculateUnitCost(INKitSpecHdr kitSpecHdr)
        //{
        //    if (kitSpecHdr == null) return 0;

        //    var kitSpecHdrExt = PXCache<INKitSpecHdr>.GetExtension<ASCIStarINKitSpecHdrExt>(kitSpecHdr);
        //    return (kitSpecHdrExt?.UsrPreciousMetalCost ?? 0m)
        //         + (kitSpecHdrExt?.UsrOtherMaterialsCost ?? 0m)
        //         + (kitSpecHdrExt?.UsrFabricationCost ?? 0m)
        //         + (kitSpecHdrExt?.UsrPackagingCost ?? 0m)
        //         + (kitSpecHdrExt?.UsrPackagingLaborCost ?? 0m);
        //}



        //public static decimal? CalculateLandedCost(INKitSpecHdr kitSpecHdr)
        //{
        //    var kitSpecHdrExt = PXCache<INKitSpecHdr>.GetExtension<ASCIStarINKitSpecHdrExt>(kitSpecHdr);
        //    return (kitSpecHdrExt?.UsrUnitCost ?? 0m)
        //        + (kitSpecHdrExt?.UsrHandlingCost ?? 0m)
        //        + (kitSpecHdrExt?.UsrFreightCost ?? 0m)
        //        + (kitSpecHdrExt?.UsrLaborCost ?? 0m)
        //        + (kitSpecHdrExt?.UsrDutyCost ?? 0m);
        //}

        public static decimal? CalculateDutyCost(IASCIStarItemCostSpecDTO costSpecDTO, decimal? newValue)
        {
            return (costSpecDTO.UsrDutyCostPct + newValue) / 100m;
        }

        public static decimal? CalculateDutyCostPct(IASCIStarItemCostSpecDTO costSpecDTO, decimal? newValue)
        {
            var value = decimal.Zero;

            if (costSpecDTO.UsrUnitCost != 0m)
            {
                value = (decimal)(newValue / costSpecDTO.UsrUnitCost) * 100m;
            }

            return value;
        }

        public decimal? GetSilverMetalCostPerOZ(decimal? basisCost, decimal? marketCost, decimal? matrixStep)
        {
            if (basisCost == null || basisCost == 0.0m || marketCost == null || marketCost == 0.0m) return 0.0m;
            //    throw new PXException(ASCIStarMessages.Error.VendorPriceNotFound);

            //if (marketCost == null || marketCost == 0.0m)
            //    throw new PXException(ASCIStarMessages.Error.MarketPriceNotFound);

            if (matrixStep <= 0.0m || matrixStep == null)
            {
                Floor = marketCost;
                Ceiling = marketCost;
                return marketCost;
            }

            // first aproach

            //decimal? temp = (marketCost / matrixStep) - (basisCost / matrixStep);
            //decimal steps = Math.Truncate(temp ?? 0.0m);

            //Floor = basisCost + (steps * matrixStep);
            //Ceiling = Floor + matrixStep;

            //second approach
            decimal? temp = (marketCost / matrixStep) - (basisCost / matrixStep);
            decimal steps = Math.Truncate(temp ?? 0.0m);

            Floor = (steps + (basisCost / matrixStep)) * matrixStep;
            Ceiling = (1 + steps + (basisCost / matrixStep)) * matrixStep;

            return (Floor + Ceiling) / 2.000000m;
        }

        private decimal? GetPresiousMetalAvrCost()
        {
            var inItemCost = INItemCost.PK.Find(_graph, ItemCostSpecification.InventoryID, Currency);

            if (inItemCost == null || inItemCost.QtyOnHand == 0.0m) return 0.0m;

            return inItemCost.TotalCost / inItemCost.QtyOnHand;
        }

        public decimal? GetPurchaseUnitCost(string costingType)
        {
            ItemCostSpecification.UsrPreciousMetalCost = CalculatePreciousMetalCost(costingType);

            // return CalculateUnitCost(ItemCostSpecification);
            return (ItemCostSpecification.UsrPreciousMetalCost ?? 0m)
                 + (ItemCostSpecification.UsrOtherMaterialsCost ?? 0m)
                 + (ItemCostSpecification.UsrFabricationCost ?? 0m)
                 + (ItemCostSpecification.UsrPackagingCost ?? 0m)
                 + (ItemCostSpecification.UsrPackagingLaborCost ?? 0m);
        }

        #region ServiceQueries
        private ASCIStarINJewelryItem GetASCIStarINJewelryItem(int? inventoryID)
            => PXSelect<ASCIStarINJewelryItem, Where<ASCIStarINJewelryItem.inventoryID, Equal<Required<ASCIStarINJewelryItem.inventoryID>>>>.Select(_graph, inventoryID);

        private InventoryItem GetInventoryItemByInvenctoryCD(string inventoryCD)
            => SelectFrom<InventoryItem>.Where<InventoryItem.inventoryCD.IsEqual<P.AsString>>.View.Select(_graph, inventoryCD)?.TopFirst;

        public static APVendorPrice GetAPVendorPrice(PXGraph graph, int? vendorID, int? inventoryID, string UOM, DateTime effectiveDate)
            => SelectFrom<APVendorPrice>
            .Where<APVendorPrice.vendorID.IsEqual<P.AsInt>
                .And<APVendorPrice.inventoryID.IsEqual<P.AsInt>
                    .And<APVendorPrice.uOM.IsEqual<P.AsString>
                       .And<Brackets<APVendorPrice.effectiveDate.IsLessEqual<P.AsDateTime>.Or<APVendorPrice.effectiveDate.IsNull>>
                         .And<Brackets<APVendorPrice.expirationDate.IsGreaterEqual<P.AsDateTime>.Or<APVendorPrice.expirationDate.IsNull>>>>>>>
            .OrderBy<APVendorPrice.effectiveDate.Desc>
            .View.Select(graph, vendorID, inventoryID, UOM, effectiveDate, effectiveDate)?.TopFirst;
        #endregion
    }
}
