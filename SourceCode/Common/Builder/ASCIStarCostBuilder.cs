using ASCISTARCustom.Common.Descriptor;
using ASCISTARCustom.Common.DTO;
using ASCISTARCustom.Common.Helper;
using ASCISTARCustom.Common.Services.DataProvider.Interfaces;
using ASCISTARCustom.Inventory.DAC;
using PX.Common;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASCISTARCustom.Common.Builder
{
    public class ASCIStarCostBuilder
    {
        #region Properies

        #region Private
        private ASCIStarItemCostSpecDTO ItemCostSpecification { get; set; }
        private POVendorInventory VendorInventory { get; set; }
        private string UOM { get; set; }
        private string Currency { get; set; } = "USD";
        private DateTime PricingDate { get; set; } = PXTimeZoneInfo.Now;
        #endregion

        #region Public
        public decimal IncrementValue { get; private set; }
        public decimal PreciousMetalCost { get; private set; }
        public decimal UnitCost { get; private set; }
        public decimal LandedCost { get; private set; }
        public decimal Surcharge { get; private set; }
        #endregion

        #region Dependency Injection
        public IASCIStarVendorDataProvider _vendorDataProvider { get; set; }
        #endregion

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
            VendorInventory = vendorInventory;
            return this;
        }
        public ASCIStarCostBuilder WithUOM(string uom)
        {
            UOM = uom;
            return this;
        }
        public ASCIStarCostBuilder WithCurrency(string currency)
        {
            Currency = currency;
            return this;
        }
        public ASCIStarCostBuilder WithPriceingData(DateTime pricingData)
        {
            PricingDate = pricingData > PXTimeZoneInfo.Today? PXTimeZoneInfo.Now : pricingData;
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
            var effectiveBasePricePerOz = EffectiveBasePricePerOz();
            var jewelryItem = GetASCIStarINJewelryItem(ItemCostSpecification.InventoryID);

            PreciousMetalCost = CalculatePreciousMetalCost(ItemCostSpecification, effectiveBasePricePerOz, jewelryItem.MetalType);
            IncrementValue = CalculateGoldIncrementValue(ItemCostSpecification, effectiveBasePricePerOz, jewelryItem.MetalType);
            Surcharge = CalculateSurchargeValue(ItemCostSpecification, effectiveBasePricePerOz, jewelryItem.MetalType);
        }

        private decimal CalculateSurchargeValue(ASCIStarItemCostSpecDTO itemCostSpecification, decimal? effectiveBasePricePerOz, string metalType)
        {
            var surchargeValue = decimal.Zero;
            var result = GetINUnits("24").Where(_ => _.FromUnit == metalType).FirstOrDefault();
            if (result != null)
            {
                var calcResult = (result.UnitRate / 31.10348m / effectiveBasePricePerOz);
                surchargeValue = (decimal)(IncrementValue / calcResult - 1m) * 100m;
            }

            return surchargeValue;
        }

        /// <summary>
        /// Calculates the value of gold increment based on the effective base price per ounce, the metal type and the item cost specification.
        /// </summary>
        /// <param name="itemCostSpecification">The item cost specification</param>
        /// <param name="effectiveBasePricePerOz">The effective base price per ounce</param>
        /// <param name="metalType">The metal type</param>
        /// <returns>The value of gold increment</returns>
        public virtual decimal CalculateGoldIncrementValue(ASCIStarItemCostSpecDTO itemCostSpecification, decimal? effectiveBasePricePerOz, string metalType)
        {
            var incrementValue = decimal.Zero;
            if (ASCIStarMetalType.IsGold(metalType) == true)
            {
                var goldMetalFactor = ASCIStarMetalType.GetGoldTypeValue(metalType);
                incrementValue = ((decimal)effectiveBasePricePerOz * goldMetalFactor / 24 / 31.10348m) * (1 + (decimal)itemCostSpecification.SurchargePct / 100m);
            }

            return incrementValue;
        }

        ///<summary>Calculates the precious metal cost for an item based on its cost specification, effective base price per ounce, and metal type. 
        ///It uses the ASCIStarMetalType class to determine if the metal type is gold or silver and calculates the precious metal cost accordingly. The metal loss and surcharge values are also factored in.</summary>
        ///<param name="costSpecDTO">The data transfer object containing the item's cost specifications.</param>
        ///<param name="effectiveBasePricePerOz">The effective base price per ounce of the metal in the item.</param>
        ///<param name="metalType">The type of metal in the item.</param>
        ///<returns>The cost of the precious metals in the item.</returns>
        public virtual decimal CalculatePreciousMetalCost(ASCIStarItemCostSpecDTO costSpecDTO, decimal? effectiveBasePricePerOz, string metalType)
        {
            var preciousMetalCost = decimal.Zero;
            var metalLossValue    = decimal.Zero;
            var surchargeValue    = decimal.Zero;

            if (ASCIStarMetalType.IsGold(metalType) == true)
            {
                var goldMetalFactor = ASCIStarMetalType.GetGoldTypeValue(metalType);
                preciousMetalCost = (decimal)effectiveBasePricePerOz * goldMetalFactor / 24 / 31.10348m * costSpecDTO.GoldGrams.Value;
                metalLossValue = 1m + costSpecDTO.MetalLossPct.Value / 100m;
            }
            else if (ASCIStarMetalType.IsSilver(metalType) == true)
            {
                var silverMetalFactor = ASCIStarMetalType.GetSilverTypeValue(metalType);
                preciousMetalCost = (decimal)effectiveBasePricePerOz * silverMetalFactor / 31.10348m * costSpecDTO.SilverGrams.Value;
            }

            surchargeValue = 1m + costSpecDTO.SurchargePct ?? -1m / 100m;
            
            return preciousMetalCost * metalLossValue * surchargeValue;
        }

        ///<summary>
        ///Effective base price per ounce based on the vendor sales price and the unit of measure (UOM).
        ///</summary>
        ///<returns>Effective base price per ounce.</returns>
        public virtual decimal? EffectiveBasePricePerOz()
        {
            var vendorSalesPrice = GetVendorMarketPrice();
            return TryConvertSalesPrice(UOM, vendorSalesPrice);
        }

        /// <summary>
        /// Retrieves the vendor market price for the inventory item based on the vendor and pricing information.
        /// Gets the APVendorPrice record for the vendor, inventory item, unit of measure, and pricing date using the _vendorDataProvider.
        /// If the record is null, throws a PXException with a message containing vendor, inventory, and pricing date information.
        /// Returns the sales price from the APVendorPrice record.
        /// </summary>
        /// <returns>The vendor market price as a decimal.</returns>
        /// <exception cref="PXException">Thrown when the APVendorPrice record is not found for the specified vendor, inventory item, unit of measure, and pricing date.</exception>
        public virtual decimal? GetVendorMarketPrice()
        {
            var result = _vendorDataProvider.GetAPVendorPrice(VendorInventory.VendorID, ItemCostSpecification.InventoryID, UOM, PricingDate);
            if (result == null)
            {
                var vendor = _vendorDataProvider.GetVendor(VendorInventory.VendorID, withException: true);
                throw new PXException(PXLocalizer.LocalizeFormat(ASCIStarMessages.Error.VendorDoesNotContainValidPrice, vendor.AcctCD, ItemCostSpecification.InventoryCD, PricingDate.ToString("MM/dd/yyyy")));
            }

            return result.SalesPrice;
        }

        /// <summary>
        /// Method that tries to convert the sales price of an item to its equivalent value in troy ounces.
        /// Gets the conversion factor between the specified UOM and TOZ (troy ounce) units by calling the GetConversionFactor method.
        /// Multiplies the sales price by the conversion factor if the conversion factor multiplier/divisor is set to Multiply, and divides the sales price by the conversion factor otherwise.
        /// </summary>
        /// <param name="UOM">The unit of measure to convert from.</param>
        /// <param name="salesPrice">The sales price to convert.</param>
        /// <returns>The converted sales price as a decimal.</returns>
        private decimal? TryConvertSalesPrice(string UOM, decimal? salesPrice)
        {
            // TODO create constant for "TOZ"
            var conversionFactor = GetConversionFactor(UOM, "TOZ");
            if (conversionFactor.Item2 == MultDiv.Multiply)
            {
                return salesPrice * conversionFactor.Item1;
            }

            return salesPrice / conversionFactor.Item1;
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
            return costSpecDTO.UnitCost + costSpecDTO.HandlingCost + costSpecDTO.FreightCost + costSpecDTO.LandedCost + costSpecDTO.DutyCost;
        }
        #region ServiceQueries
        private ASCIStarINJewelryItem GetASCIStarINJewelryItem(int? inventoryID) =>
            PXSelect<
                ASCIStarINJewelryItem, 
                Where<ASCIStarINJewelryItem.inventoryID, Equal<Required<ASCIStarINJewelryItem.inventoryID>>>>
                .Select(_graph, inventoryID);
        private INUnit GetINUnit(string fromUnit, string toUnit) =>
            PXSelect<
                INUnit, 
                Where<INUnit.fromUnit, Equal<Required<INUnit.fromUnit>>, 
                    And<INUnit.toUnit, Equal<Required<INUnit.toUnit>>>>>
                .Select(_graph, fromUnit, toUnit);
        private IEnumerable<INUnit> GetINUnits(string toUnit) =>
            PXSelect<
                INUnit, 
                Where<INUnit.fromUnit, Equal<Required<INUnit.toUnit>>>>
                .Select(_graph, toUnit).FirstTableItems;

        #endregion
    }
}
