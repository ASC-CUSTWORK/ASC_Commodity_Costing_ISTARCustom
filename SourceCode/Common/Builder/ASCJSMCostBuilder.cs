using ASCJSMCustom.Common.DTO.Interfaces;
using ASCJSMCustom.Common.Helper;
using ASCJSMCustom.PO.CacheExt;
using ASCJSMCustom.IN.DAC;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.PO;
using System;
using static ASCJSMCustom.Common.Descriptor.ASCJSMConstants;

namespace ASCJSMCustom.Common.Builder
{
    public class ASCJSMCostBuilder
    {
        #region Properies
        private PXGraph _graph;

        private string Currency { get; set; } = "USD";
        private bool IsEnabledOverrideVendor { get; set; }
        private InventoryItem PreciousMetalItem { get; set; }
        public ASCJSMINJewelryItem INJewelryItem { get; set; }
        public IASCJSMItemCostSpecDTO ItemCostSpecification { get; set; }
        public POVendorInventory POVendorInventory { get; set; }
        public ASCJSMPOVendorInventoryExt POVendorInventoryExt { get; set; }
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

        public ASCJSMCostBuilder(PXGraph graph)
        {
            _graph = graph;
        }

        #region Chain Method Calls
        public ASCJSMCostBuilder WithInventoryItem(IASCJSMItemCostSpecDTO inventory)
        {
            ItemCostSpecification = inventory;
            return this;
        }
        public ASCJSMCostBuilder WithJewelryAttrData(ASCJSMINJewelryItem jewelryItem = null)
        {
            if (jewelryItem == null)
                INJewelryItem = GetASCIStarINJewelryItem(ItemCostSpecification.InventoryID);
            else
                INJewelryItem = jewelryItem;

            return this;
        }
        public ASCJSMCostBuilder WithPOVendorInventory(POVendorInventory vendorInventory)
        {
            POVendorInventory = vendorInventory;
            POVendorInventoryExt = PXCache<POVendorInventory>.GetExtension<ASCJSMPOVendorInventoryExt>(vendorInventory);
            IsEnabledOverrideVendor = POVendorInventoryExt.UsrIsOverrideVendor == true;
            return this;
        }
        public ASCJSMCostBuilder WithCurrency(string currency)
        {
            Currency = currency;
            return this;
        }
        public ASCJSMCostBuilder WithPricingData(DateTime pricingData)
        {
            PricingDate = pricingData;
            return this;
        }
        public virtual ASCJSMCostBuilder Build()
        {
            if (INJewelryItem == null || INJewelryItem.MetalType == null)
            {
                INJewelryItem = GetASCIStarINJewelryItem(ItemCostSpecification.InventoryID);

                if (INJewelryItem == null || INJewelryItem.MetalType == null) return null;
            }

            if (ASCJSMMetalType.IsGold(INJewelryItem.MetalType))
                PreciousMetalItem = ASCJSMMetalType.GetInventoryItemByInvenctoryCD(_graph, "24K");
            else if (ASCJSMMetalType.IsSilver(INJewelryItem.MetalType))
                PreciousMetalItem = ASCJSMMetalType.GetInventoryItemByInvenctoryCD(_graph, "SSS");

            if (PreciousMetalItem == null) return null;

            PreciousMetalContractCostPerTOZ = IsEnabledOverrideVendor ? POVendorInventoryExt.UsrCommodityVendorPrice : GetVendorPricePerTOZ(POVendorInventory.VendorID, PreciousMetalItem.InventoryID, true);
            PreciousMetalMarketCostPerTOZ = GetVendorPricePerTOZ(POVendorInventoryExt.UsrMarketID, PreciousMetalItem.InventoryID);

            if (ASCJSMMetalType.IsGold(INJewelryItem.MetalType))
            {
                PreciousMetalContractCostPerGram = PreciousMetalContractCostPerTOZ * ASCJSMMetalType.GetMultFactorConvertTOZtoGram("24K");
                PreciousMetalMarketCostPerGram = PreciousMetalMarketCostPerTOZ * ASCJSMMetalType.GetMultFactorConvertTOZtoGram("24K");
                BasisValue = PreciousMetalContractCostPerTOZ;
            }
            else if (ASCJSMMetalType.IsSilver(INJewelryItem.MetalType))
            {
                PreciousMetalContractCostPerGram = PreciousMetalContractCostPerTOZ * ASCJSMMetalType.GetMultFactorConvertTOZtoGram("SSS");
                PreciousMetalMarketCostPerGram = PreciousMetalMarketCostPerTOZ * ASCJSMMetalType.GetMultFactorConvertTOZtoGram("SSS");
                BasisValue = (PreciousMetalContractCostPerTOZ + (PreciousMetalContractCostPerTOZ + ItemCostSpecification.UsrMatrixStep)) / 2;
            }
            AvrPreciousMetalUnitCost = GetPresiousMetalAvrCost();

            return this;
        }
        #endregion


        ///<summary>Calculates the precious metal cost for an item based on its cost specification, effective base price per gram and metal type. 
        ///It uses the ASCIStarMetalType class to determine if the metal type is gold or silver and calculates the precious metal cost accordingly. The metal loss and surcharge values are also factored in.</summary>
        ///<param name="costingType">The data transfer object containing the item's cost specifications.</param>
        ///<returns>The cost of the precious metals in the item.</returns>
        public virtual decimal? CalculatePreciousMetalCost(string costingType = null)
        {
            decimal? preciousMetalCost = decimal.Zero;

            decimal priciousMetalMultFactor = ASCJSMMetalType.GetMultFactorConvertTOZtoGram(INJewelryItem?.MetalType);

            if (ASCJSMMetalType.IsGold(INJewelryItem?.MetalType))
            {
                switch (costingType ?? ItemCostSpecification.UsrCostingType)
                {
                    case CostingType.ContractCost:
                        preciousMetalCost = PreciousMetalContractCostPerTOZ;
                        break;
                    case CostingType.MarketCost:
                        preciousMetalCost = PreciousMetalMarketCostPerTOZ;
                        break;
                    case CostingType.StandardCost:
                        return AvrPreciousMetalUnitCost - ItemCostSpecification.UsrFabricationCost - ItemCostSpecification.UsrOtherMaterialsCost - ItemCostSpecification.UsrPackagingCost;

                    default: break;
                }
                preciousMetalCost = preciousMetalCost * priciousMetalMultFactor * (ItemCostSpecification.UsrActualGRAMGold ?? 0.0m);

            }
            else if (ASCJSMMetalType.IsSilver(INJewelryItem?.MetalType))
            {
                switch (costingType ?? ItemCostSpecification.UsrCostingType)
                {
                    case CostingType.ContractCost:
                        PreciousMetalAvrSilverMarketCostPerTOZ = GetSilverMetalCostPerOZ(PreciousMetalContractCostPerTOZ, PreciousMetalContractCostPerTOZ, ItemCostSpecification.UsrMatrixStep);
                        break;
                    case CostingType.MarketCost:
                        PreciousMetalAvrSilverMarketCostPerTOZ = GetSilverMetalCostPerOZ(PreciousMetalContractCostPerTOZ, PreciousMetalMarketCostPerTOZ, ItemCostSpecification.UsrMatrixStep); ;
                        break;
                    case CostingType.StandardCost:
                        return AvrPreciousMetalUnitCost - ItemCostSpecification.UsrFabricationCost - ItemCostSpecification.UsrOtherMaterialsCost - ItemCostSpecification.UsrPackagingCost;

                    default: break;
                }
                preciousMetalCost = PreciousMetalAvrSilverMarketCostPerTOZ * priciousMetalMultFactor * (ItemCostSpecification.UsrActualGRAMSilver ?? 0.0m);
            }

            decimal? surchargeValue = (100.0m + (ItemCostSpecification.UsrContractSurcharge ?? 0.0m)) / 100.0m;
            decimal? metalLossValue = (100.0m + (ItemCostSpecification.UsrContractLossPct ?? 0.0m)) / 100.0m;
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

        public virtual decimal? CalculateIncrementValue(IASCJSMItemCostSpecDTO itemCostSpecification)
        {
            var metalFactor = ASCJSMMetalType.GetMultFactorConvertTOZtoGram(INJewelryItem?.MetalType);

            decimal? incrementValue = metalFactor * (1.0m + (itemCostSpecification.UsrContractSurcharge ?? 0.0m) / 100.0m);

            return incrementValue;
        }

        public static decimal? CalculateSurchargeValue(decimal? increment, string metalType)
        {
            decimal convFactor = ASCJSMMetalType.GetMultFactorConvertTOZtoGram(metalType);
            decimal? surchargeNewValue = (increment / convFactor - 1.0m) * 100.0m;

            return surchargeNewValue;
        }

        public static decimal? CalculateDutyCost(IASCJSMItemCostSpecDTO costSpecDTO, decimal? newValue)
        {
            return (costSpecDTO.UsrDutyCostPct + newValue) / 100m;
        }

        public static decimal? CalculateDutyCostPct(IASCJSMItemCostSpecDTO costSpecDTO, decimal? newValue)
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
            var inItemCost = INItemCost.PK.Find(_graph, ItemCostSpecification.InventoryID, Currency);

            if (inItemCost == null || inItemCost.QtyOnHand == 0.0m) return 0.0m;

            return inItemCost.TotalCost / inItemCost.QtyOnHand;
        }

        public decimal? GetPurchaseUnitCost(string costingType)
        {
            if (PreciousMetalMarketCostPerTOZ == decimal.Zero) return decimal.Zero;

            ItemCostSpecification.UsrPreciousMetalCost = CalculatePreciousMetalCost(costingType);

            return (ItemCostSpecification.UsrPreciousMetalCost ?? 0m)
                 + (ItemCostSpecification.UsrOtherMaterialsCost ?? 0m)
                 + (ItemCostSpecification.UsrFabricationCost ?? 0m)
                 + (ItemCostSpecification.UsrPackagingCost ?? 0m)
                 + (ItemCostSpecification.UsrPackagingLaborCost ?? 0m);
        }

        #region ServiceQueries
        private ASCJSMINJewelryItem GetASCIStarINJewelryItem(int? inventoryID)
            => PXSelect<ASCJSMINJewelryItem, Where<ASCJSMINJewelryItem.inventoryID, Equal<Required<ASCJSMINJewelryItem.inventoryID>>>>.Select(_graph, inventoryID);

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
