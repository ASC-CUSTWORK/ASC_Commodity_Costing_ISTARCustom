using ASCJewelryLibrary.Common.DTO.Interfaces;
using ASCJewelryLibrary.Common.Helper;
using ASCJewelryLibrary.PO.CacheExt;
using ASCJewelryLibrary.IN.DAC;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.PO;
using System;
using static ASCJewelryLibrary.Common.Descriptor.ASCJConstants;

namespace ASCJewelryLibrary.Common.Builder
{
    public class ASCJCostBuilder
    {
        #region Properies
        private PXGraph _graph;

        private string Currency { get; set; } = "USD";
        private bool IsEnabledOverrideVendor { get; set; }
        private InventoryItem PreciousMetalItem { get; set; }
        public ASCJINJewelryItem INJewelryItem { get; set; }
        public IASCJItemCostSpecDTO ItemCostSpecification { get; set; }
        public POVendorInventory POVendorInventory { get; set; }
        public ASCJPOVendorInventoryExt POVendorInventoryExt { get; set; }
        public DateTime PricingDate { get; set; } = PXTimeZoneInfo.Today;
        public decimal? PreciousMetalContractCostPerTOZ { get; private set; }
        public decimal? PreciousMetalMarketCostPerTOZ { get; private set; }
        public decimal? PreciousMetalAvrSilverMarketCostPerTOZ { get; set; }
        public decimal? PreciousMetalContractCostPerGram { get; private set; }
        public decimal? PreciousMetalMarketCostPerGram { get; private set; }
        public decimal? PreciousMetalUnitCost { get; private set; }
        public decimal? AvrPreciousMetalUnitCost { get; private set; }
        public decimal? Floor { get; private set; }
        public decimal? Ceiling { get; private set; }
        public decimal? BasisValue { get; private set; } = decimal.Zero;

        public APVendorPrice APVendorPriceContract { get; set; }
        #endregion 

        public ASCJCostBuilder(PXGraph graph)
        {
            _graph = graph;
        }

        #region Chain Method Calls
        public ASCJCostBuilder WithInventoryItem(IASCJItemCostSpecDTO inventory)
        {
            ItemCostSpecification = inventory;
            return this;
        }
        public ASCJCostBuilder WithJewelryAttrData(ASCJINJewelryItem jewelryItem = null)
        {
            if (jewelryItem == null)
                INJewelryItem = GetASCJINJewelryItem(ItemCostSpecification.UsrASCJInventoryID);
            else
                INJewelryItem = jewelryItem;

            return this;
        }
        public ASCJCostBuilder WithPOVendorInventory(POVendorInventory vendorInventory)
        {
            POVendorInventory = vendorInventory;
            POVendorInventoryExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(vendorInventory);
            IsEnabledOverrideVendor = POVendorInventoryExt.UsrASCJIsOverrideVendor == true;
            return this;
        }
        public ASCJCostBuilder WithCurrency(string currency)
        {
            Currency = currency;
            return this;
        }
        public ASCJCostBuilder WithPricingData(DateTime pricingData)
        {
            PricingDate = pricingData;
            return this;
        }
        public virtual ASCJCostBuilder Build()
        {
            if (INJewelryItem == null || INJewelryItem.MetalType == null)
            {
                INJewelryItem = GetASCJINJewelryItem(ItemCostSpecification.UsrASCJInventoryID);

                if (INJewelryItem == null || INJewelryItem.MetalType == null) return null;
            }

            if (ASCJMetalType.IsGold(INJewelryItem.MetalType))
                PreciousMetalItem = ASCJMetalType.GetInventoryItemByInvenctoryCD(_graph, "24K");
            else if (ASCJMetalType.IsSilver(INJewelryItem.MetalType))
                PreciousMetalItem = ASCJMetalType.GetInventoryItemByInvenctoryCD(_graph, "SSS");

            if (PreciousMetalItem == null) return null;

            PreciousMetalContractCostPerTOZ = IsEnabledOverrideVendor ? POVendorInventoryExt.UsrASCJCommodityVendorPrice : GetVendorPricePerTOZ(POVendorInventory.VendorID, PreciousMetalItem.InventoryID, true);
            PreciousMetalMarketCostPerTOZ = GetVendorPricePerTOZ(POVendorInventoryExt.UsrASCJMarketID, PreciousMetalItem.InventoryID);

            if (ASCJMetalType.IsGold(INJewelryItem.MetalType))
            {
                PreciousMetalContractCostPerGram = PreciousMetalContractCostPerTOZ * ASCJMetalType.GetMultFactorConvertTOZtoGram("24K");
                PreciousMetalMarketCostPerGram = PreciousMetalMarketCostPerTOZ * ASCJMetalType.GetMultFactorConvertTOZtoGram("24K");
                BasisValue = PreciousMetalContractCostPerTOZ;
            }
            else if (ASCJMetalType.IsSilver(INJewelryItem.MetalType))
            {
                PreciousMetalContractCostPerGram = PreciousMetalContractCostPerTOZ * ASCJMetalType.GetMultFactorConvertTOZtoGram("SSS");
                PreciousMetalMarketCostPerGram = PreciousMetalMarketCostPerTOZ * ASCJMetalType.GetMultFactorConvertTOZtoGram("SSS");
                BasisValue = (PreciousMetalContractCostPerTOZ + (PreciousMetalContractCostPerTOZ + ItemCostSpecification.UsrASCJMatrixStep)) / 2;
            }
            AvrPreciousMetalUnitCost = GetPresiousMetalAvrCost();

            return this;
        }
        #endregion


        ///<summary>Calculates the precious metal cost for an item based on its cost specification, effective base price per gram and metal type. 
        ///It uses the ASCJMetalType class to determine if the metal type is gold or silver and calculates the precious metal cost accordingly. The metal loss and surcharge values are also factored in.</summary>
        ///<param name="costingType">The data transfer object containing the item's cost specifications.</param>
        ///<returns>The cost of the precious metals in the item.</returns>
        public virtual decimal? CalculatePreciousMetalCost(string costingType = null)
        {
            decimal? preciousMetalCost = decimal.Zero;

            decimal priciousMetalMultFactor = ASCJMetalType.GetMultFactorConvertTOZtoGram(INJewelryItem?.MetalType);

            if (ASCJMetalType.IsGold(INJewelryItem?.MetalType))
            {
                switch (costingType ?? ItemCostSpecification.UsrASCJCostingType)
                {
                    case CostingType.ContractCost:
                        preciousMetalCost = PreciousMetalContractCostPerTOZ;
                        break;
                    case CostingType.MarketCost:
                        preciousMetalCost = PreciousMetalMarketCostPerTOZ;
                        break;
                    case CostingType.StandardCost:
                        return AvrPreciousMetalUnitCost - ItemCostSpecification.UsrASCJFabricationCost - ItemCostSpecification.UsrASCJOtherMaterialsCost - ItemCostSpecification.UsrASCJPackagingCost;

                    default: break;
                }
                preciousMetalCost = preciousMetalCost * priciousMetalMultFactor * (ItemCostSpecification.UsrASCJActualGRAMGold ?? 0.0m);

            }
            else if (ASCJMetalType.IsSilver(INJewelryItem?.MetalType))
            {
                switch (costingType ?? ItemCostSpecification.UsrASCJCostingType)
                {
                    case CostingType.ContractCost:
                        PreciousMetalAvrSilverMarketCostPerTOZ = GetSilverMetalCostPerOZ(PreciousMetalContractCostPerTOZ, PreciousMetalContractCostPerTOZ, ItemCostSpecification.UsrASCJMatrixStep);
                        break;
                    case CostingType.MarketCost:
                        PreciousMetalAvrSilverMarketCostPerTOZ = GetSilverMetalCostPerOZ(PreciousMetalContractCostPerTOZ, PreciousMetalMarketCostPerTOZ, ItemCostSpecification.UsrASCJMatrixStep); ;
                        break;
                    case CostingType.StandardCost:
                        return AvrPreciousMetalUnitCost - ItemCostSpecification.UsrASCJFabricationCost - ItemCostSpecification.UsrASCJOtherMaterialsCost - ItemCostSpecification.UsrASCJPackagingCost;

                    default: break;
                }
                preciousMetalCost = PreciousMetalAvrSilverMarketCostPerTOZ * priciousMetalMultFactor * (ItemCostSpecification.UsrASCJActualGRAMSilver ?? 0.0m);
            }

            decimal? surchargeValue = (100.0m + (ItemCostSpecification.UsrASCJContractSurcharge ?? 0.0m)) / 100.0m;
            decimal? metalLossValue = (100.0m + (ItemCostSpecification.UsrASCJContractLossPct ?? 0.0m)) / 100.0m;
            PreciousMetalUnitCost = preciousMetalCost * metalLossValue * surchargeValue;
            return PreciousMetalUnitCost;
        }

        public virtual decimal? GetVendorPricePerTOZ(int? vendorID, int? inventoryID, bool isContract = false)
        {
            var result = GetAPVendorPrice(_graph, vendorID, inventoryID, TOZ.value, PricingDate);
            if (result == null)
            {
                return 0.0m;
            }

            if (isContract)
            {
                APVendorPriceContract = result;
            }

            return result.SalesPrice;
        }

        public virtual decimal? CalculateIncrementValue(IASCJItemCostSpecDTO itemCostSpecification)
        {
            var metalFactor = ASCJMetalType.GetMultFactorConvertTOZtoGram(INJewelryItem?.MetalType);

            decimal? incrementValue = metalFactor * (1.0m + (itemCostSpecification.UsrASCJContractSurcharge ?? 0.0m) / 100.0m);

            return incrementValue;
        }

        public static decimal? CalculateSurchargeValue(decimal? increment, string metalType)
        {
            decimal convFactor = ASCJMetalType.GetMultFactorConvertTOZtoGram(metalType);
            decimal? surchargeNewValue = (increment / convFactor - 1.0m) * 100.0m;

            return surchargeNewValue;
        }

        public static decimal? CalculateDutyCost(IASCJItemCostSpecDTO costSpecDTO, decimal? newValue)
        {
            return (costSpecDTO.UsrASCJDutyCostPct + newValue) / 100m;
        }

        public static decimal? CalculateDutyCostPct(IASCJItemCostSpecDTO costSpecDTO, decimal? newValue)
        {
            var value = decimal.Zero;

            if (costSpecDTO.UsrASCJUnitCost != 0m)
            {
                value = (decimal)(newValue / costSpecDTO.UsrASCJUnitCost) * 100m;
            }

            return value;
        }

        public decimal? GetSilverMetalCostPerOZ(decimal? basisCost, decimal? marketCost, decimal? matrixStep)
        {
            if (basisCost == null || basisCost == 0.0m || marketCost == null || marketCost == 0.0m) return 0.0m;

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
            PreciousMetalAvrSilverMarketCostPerTOZ = (Floor + Ceiling) / 2.000000m;
            return PreciousMetalAvrSilverMarketCostPerTOZ;
        }

        private decimal? GetPresiousMetalAvrCost()
        {
            var inItemCost = INItemCost.PK.Find(_graph, ItemCostSpecification.UsrASCJInventoryID, Currency);

            if (inItemCost == null || inItemCost.QtyOnHand == 0.0m) return 0.0m;

            return inItemCost.TotalCost / inItemCost.QtyOnHand;
        }

        public decimal? GetPurchaseUnitCost(string costingType)
        {
            if (PreciousMetalMarketCostPerTOZ == decimal.Zero) return decimal.Zero;

            ItemCostSpecification.UsrASCJPreciousMetalCost = CalculatePreciousMetalCost(costingType);

            return (ItemCostSpecification.UsrASCJPreciousMetalCost ?? 0m)
                 + (ItemCostSpecification.UsrASCJOtherMaterialsCost ?? 0m)
                 + (ItemCostSpecification.UsrASCJFabricationCost ?? 0m)
                 + (ItemCostSpecification.UsrASCJPackagingCost ?? 0m)
                 + (ItemCostSpecification.UsrASCJPackagingLaborCost ?? 0m);
        }

        #region ServiceQueries
        private ASCJINJewelryItem GetASCJINJewelryItem(int? inventoryID)
            => PXSelect<ASCJINJewelryItem, Where<ASCJINJewelryItem.inventoryID, Equal<Required<ASCJINJewelryItem.inventoryID>>>>.Select(_graph, inventoryID);

        public static APVendorPrice GetAPVendorPrice(PXGraph graph, int? vendorID, int? inventoryID, string UOM, DateTime effectiveDate)
            => SelectFrom<APVendorPrice>
            .Where<APVendorPrice.vendorID.IsEqual<P.AsInt>
                .And<APVendorPrice.inventoryID.IsEqual<P.AsInt>
                    .And<APVendorPrice.uOM.IsEqual<P.AsString>
                       .And<Brackets<APVendorPrice.effectiveDate.IsLessEqual<P.AsDateTime>
                         .And<Brackets<APVendorPrice.expirationDate.IsGreaterEqual<P.AsDateTime>.Or<APVendorPrice.expirationDate.IsNull>>>>>>>>
            .OrderBy<APVendorPrice.effectiveDate.Desc>
            .View.Select(graph, vendorID, inventoryID, UOM, effectiveDate, effectiveDate)?.TopFirst;
        #endregion
    }
}
