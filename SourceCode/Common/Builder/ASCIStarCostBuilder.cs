using ASCISTARCustom.Common.Descriptor;
using ASCISTARCustom.Common.DTO;
using ASCISTARCustom.Common.Helper;
using ASCISTARCustom.Cost.Descriptor;
using ASCISTARCustom.Inventory.DAC;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.PO;
using System;
using System.Collections.Generic;

namespace ASCISTARCustom.Common.Builder
{
    public class ASCIStarCostBuilder
    {
        #region Properies
        private string UOM { get; set; }
        private string Currency { get; set; } = "USD";
        public DateTime PricingDate { get; set; } = PXTimeZoneInfo.Now;
        private bool IsEnabledOverideVendor { get; set; }
        private InventoryItem PreciousMetalItem { get; set; }
        private ASCIStarINJewelryItem INJewelryItem { get; set; }

        public ASCIStarItemCostSpecDTO ItemCostSpecification { get; set; }
        public POVendorInventory POVendorInventory { get; set; }
        public ASCIStarPOVendorInventoryExt POVendorInventoryExt { get; set; }
        public decimal? PreciousMetalContractCostPerTOZ { get; set; }
        public decimal? PreciousMetalMarketCostPerTOZ { get; set; }
        public decimal? PreciousMetalContractCostPerGram { get; private set; }
        public decimal? PreciousMetalMarketCostPerGram { get; private set; }
        public decimal? PreciousMetalUnitCost { get; private set; }
        #endregion 

        private readonly PXGraph _graph;

        #region ctor
        public ASCIStarCostBuilder(PXGraph graph)
        {
            _graph = graph;
        }
        #endregion

        #region Chain Method Calls
        public ASCIStarCostBuilder WithInventoryItem(ASCIStarItemCostSpecDTO inventory)
        {
            ItemCostSpecification = inventory;
            return this;
        }
        public ASCIStarCostBuilder WithPOVendorInventory(POVendorInventory vendorInventory)
        {
            POVendorInventory = vendorInventory;
            POVendorInventoryExt = PXCache<POVendorInventory>.GetExtension<ASCIStarPOVendorInventoryExt>(vendorInventory);
            IsEnabledOverideVendor = POVendorInventoryExt.UsrVendorDefault == true;
            return this;
        }
        public ASCIStarCostBuilder WithINJewelryItem(ASCIStarINJewelryItem inJewelryItem)
        {
            INJewelryItem = inJewelryItem;
            return this;
        }
        public ASCIStarCostBuilder WithCurrency(string currency)
        {
            Currency = currency;
            return this;
        }
        public ASCIStarCostBuilder WithPricingData(DateTime pricingData)
        {
            PricingDate = pricingData > PXTimeZoneInfo.Today ? PXTimeZoneInfo.Now : pricingData;
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
            if (INJewelryItem == null) INJewelryItem = GetASCIStarINJewelryItem(ItemCostSpecification.InventoryID);

            if (ASCIStarMetalType.IsGold(INJewelryItem.MetalType))
                PreciousMetalItem = GetInventoryItemByInvenctoryCD("24K");
            else if (ASCIStarMetalType.IsSilver(INJewelryItem.MetalType))
                PreciousMetalItem = GetInventoryItemByInvenctoryCD("SSS");

            PreciousMetalContractCostPerTOZ = IsEnabledOverideVendor ? POVendorInventoryExt.UsrCommodityPrice : GetVendorPricePerTOZ(POVendorInventory.VendorID, PreciousMetalItem.InventoryID);
            PreciousMetalContractCostPerGram = PreciousMetalContractCostPerTOZ * ASCIStarMetalType.GetMultFactorConvertTOZtoGram("SSS");

            PreciousMetalMarketCostPerTOZ = GetVendorPricePerTOZ(POVendorInventoryExt.UsrMarketID, PreciousMetalItem.InventoryID);
            PreciousMetalMarketCostPerGram = PreciousMetalMarketCostPerTOZ * ASCIStarMetalType.GetMultFactorConvertTOZtoGram("24");
        }

        public virtual decimal? CalculateSurchargeValue(ASCIStarItemCostSpecDTO itemCostSpecification)
        {
            decimal? temp1 = itemCostSpecification.Increment;
            decimal? temp2 = (1.0m / 31.10348m);

            decimal? surchargeNewValue = (temp1 / temp2 - 1.0m) * 100.0m;

            return surchargeNewValue;
        }

        /// <summary>
        /// Calculates the value of gold increment based on the effective base price per ounce, the metal type and the item cost specification.
        /// </summary>
        /// <param name="itemCostSpecification">The item cost specification</param>
        /// <param name="effectiveBasePricePerOz">The effective base price per ounce</param>
        /// <param name="metalType">The metal type</param>
        /// <returns>The value of gold increment</returns>
        public virtual decimal? CalculateGoldIncrementValue(ASCIStarItemCostSpecDTO itemCostSpecification)
        {
            var goldMetalFactor = ASCIStarMetalType.GetMultFactorConvertTOZtoGram(INJewelryItem.MetalType);

            decimal? incrementValue = goldMetalFactor * (1.0m + itemCostSpecification.SurchargePct / 100.0m);

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
            var metalLossValue = 1.00m;

            var priciousMetalMultFactor = ASCIStarMetalType.GetMultFactorConvertTOZtoGram(INJewelryItem.MetalType);

            if (ASCIStarMetalType.IsGold(INJewelryItem.MetalType))
            {
                switch (costingType ?? ItemCostSpecification.CostingType)
                {
                    case ASCIStarCostingType.ContractCost:
                        preciousMetalCost = PreciousMetalContractCostPerTOZ;
                        break;
                    case ASCIStarCostingType.MarketCost:
                        preciousMetalCost = PreciousMetalMarketCostPerTOZ;
                        break;
                    case ASCIStarCostingType.StandardCost:

                        break;
                    default: break;
                }
                preciousMetalCost = preciousMetalCost * priciousMetalMultFactor * ItemCostSpecification.FineGoldGrams.Value;
                metalLossValue = 1m + ItemCostSpecification.MetalLossPct.Value / 100m;
            }
            else if (ASCIStarMetalType.IsSilver(INJewelryItem.MetalType))
            {//change to Martix Step field
                preciousMetalCost = GetSilverMetalCostPerOZ(PreciousMetalContractCostPerTOZ, PreciousMetalMarketCostPerTOZ, ItemCostSpecification.Increment)
                    * priciousMetalMultFactor
                    * ItemCostSpecification.FineSilverGrams;
            }

            decimal surchargeValue = 1m + ItemCostSpecification.SurchargePct ?? 0.0m / 100m;
            PreciousMetalUnitCost = preciousMetalCost * metalLossValue * surchargeValue;
            return PreciousMetalUnitCost;
        }

        public virtual decimal? GetVendorPricePerTOZ(int? vendorID, int? inventoryID)
        {
            var result = GetAPVendorPrice(vendorID, inventoryID, UOM, PricingDate);
            if (result == null)
            {
                var vendor = Vendor.PK.Find(_graph, vendorID);
                throw new PXException(PXLocalizer.LocalizeFormat(ASCIStarMessages.Error.VendorDoesNotContainValidPrice, vendor?.AcctCD, ItemCostSpecification.InventoryCD, PricingDate.ToString("MM/dd/yyyy")));
            }

            return result.SalesPrice;
        }

        /// <summary>
        /// Retrieves the conversion factor between two inventory units.
        /// </summary>
        /// <param name="fromUnit">The inventory unit to convert from.</param>
        /// <param name="toUnit">The inventory unit to convert to.</param>
        /// <returns>A tuple containing the conversion factor value and the unit multiplier/divisor.</returns>
        /// <exception cref="PXException">Thrown when the conversion factor between the two units is not found.</exception>
        private (decimal?, string) GetConversionFactor(string fromUnit, string toUnit)
        {
            var result = GetINUnit(fromUnit, toUnit);
            if (result == null)
            {
                throw new PXException(PXLocalizer.LocalizeFormat(ASCIStarMessages.Error.UnitConversionNotFound, fromUnit, toUnit));
            }

            return (result.UnitRate, result.UnitMultDiv);
        }

        public static decimal? CalculateUnitCost(ASCIStarItemCostSpecDTO costSpecDTO)
        {
            return costSpecDTO.PreciousMetalCost + costSpecDTO.OtherMaterialCost + costSpecDTO.FabricationCost + costSpecDTO.PackagingCost; ;
        }

        public static decimal? CalculateDutyCost(ASCIStarItemCostSpecDTO costSpecDTO, decimal? newValue)
        {
            return (costSpecDTO.DutyCostPct + newValue) / 100m;
        }

        public static decimal? CalculateDutyCostPct(ASCIStarItemCostSpecDTO costSpecDTO, decimal? newValue)
        {
            var value = decimal.Zero;

            if (costSpecDTO.UnitCost != 0m)
            {
                value = (decimal)(newValue / costSpecDTO.UnitCost) * 100m;
            }

            return value;
        }

        public static decimal? CalculateLandedCost(ASCIStarItemCostSpecDTO costSpecDTO)
        {
            return costSpecDTO.UnitCost + costSpecDTO.HandlingCost + costSpecDTO.FreightCost + costSpecDTO.LaborCost + costSpecDTO.DutyCost;
        }

        public decimal? GetSilverMetalCostPerOZ(decimal? basisCost, decimal? marketCost, decimal? incrementNullable)
        {
            if (incrementNullable == 0.0m || incrementNullable == null)
            {
                return marketCost;
            }
            decimal? increment = (decimal)incrementNullable;
            decimal? costPerOz = marketCost;
            decimal? temp = (marketCost / increment) - (basisCost / increment);

            decimal steps = Math.Truncate(temp ?? 0.0m);

            decimal? floor = basisCost + (steps * increment);
            decimal? ceiling = floor + increment;
            costPerOz = (floor + ceiling) / 2.000000m;

            return costPerOz;
        }

        public decimal? GetPurchaseUnitCost(string costingType)
        {
            ItemCostSpecification.PreciousMetalCost = CalculatePreciousMetalCost(costingType);

            return CalculateUnitCost(ItemCostSpecification);
        }
        #region ServiceQueries
        private ASCIStarINJewelryItem GetASCIStarINJewelryItem(int? inventoryID) =>
            PXSelect<ASCIStarINJewelryItem, Where<ASCIStarINJewelryItem.inventoryID, Equal<Required<ASCIStarINJewelryItem.inventoryID>>>>.Select(_graph, inventoryID);

        private INUnit GetINUnit(string fromUnit, string toUnit)
            => PXSelect<INUnit,
                Where<INUnit.fromUnit, Equal<Required<INUnit.fromUnit>>, And<INUnit.toUnit, Equal<Required<INUnit.toUnit>>>>>.Select(_graph, fromUnit, toUnit);
        
        private InventoryItem GetInventoryItemByInvenctoryCD(string inventiryCD)
            => SelectFrom<InventoryItem>.Where<InventoryItem.inventoryCD.IsEqual<P.AsString>>.View.Select(_graph, inventiryCD)?.TopFirst;

        private APVendorPrice GetAPVendorPrice(int? vendorID, int? inventoryID, string UOM, DateTime effectiveDate)
            => new PXSelect<APVendorPrice,
                Where<APVendorPrice.vendorID, Equal<Required<APVendorPrice.vendorID>>,
                    And<APVendorPrice.inventoryID, Equal<Required<APVendorPrice.inventoryID>>,
                    And<APVendorPrice.uOM, Equal<Required<APVendorPrice.uOM>>,
                    And<APVendorPrice.effectiveDate, LessEqual<Required<APVendorPrice.effectiveDate>>,
                    And<APVendorPrice.effectiveDate, LessEqual<Required<APVendorPrice.effectiveDate>>>>>>>,
                OrderBy<Desc<APVendorPrice.effectiveDate>>>(_graph).SelectSingle(vendorID, inventoryID, "TOZ", effectiveDate, effectiveDate);
        #endregion
    }
}
