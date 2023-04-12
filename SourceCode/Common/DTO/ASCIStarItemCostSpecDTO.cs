using ASCISTARCustom.Common.DTO.Interfaces;
using ASCISTARCustom.Inventory.DAC;
using PX.Data;
using PX.Objects.IN;

namespace ASCISTARCustom.Common.DTO
{
    ///<summary>
    /// This class represents a data transfer object (DTO) for the cost specification of an item.
    /// It contains properties for various costs related to the item
    /// The class also provides two implicit conversion operators for converting from the InventoryItem, ASCIStarINInventoryItemExt and ASCIStarItemWeightCostSpec classes to the ASCIStarItemCostSpecDTO class.
    ///</summary>
    public class ASCIStarItemCostSpecDTO : IASCIStarItemCostSpecDTO
    {
        public int? InventoryID { get; set; }
        public string InventoryCD { get; set; }
        public string RevisionID { get; set; }
        public decimal? GoldGrams { get; set; }
        public decimal? SilverGrams { get; set; }
        public decimal? FineGoldGrams { get; set; }
        public decimal? FineSilverGrams { get; set; }
        public decimal? MetalLossPct { get; set; }
        public decimal? SurchargePct { get; set; }
        public decimal? Increment { get; set; }
        public decimal? MatrixStep { get; set; }
        public decimal? UnitCost { get; set; }
        public decimal? PreciousMetalCost { get; set; }
        public decimal? FabricationCost { get; set; }
        public decimal? LandedCost { get; set; }
        public decimal? PackagingCost { get; set; }
        public decimal? MaterialsCost { get; set; }
        public decimal? FreightCost { get; set; }
        public decimal? HandlingCost { get; set; }
        public decimal? LaborCost { get; set; }
        public decimal? DutyCost { get; set; }
        public decimal? DutyCostPct { get; set; }
        public string CostingType { get; set; }

        public static implicit operator ASCIStarItemCostSpecDTO(InventoryItem value)
        {
            if (value == null) return null;

            var valueExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(value);
            return new ASCIStarItemCostSpecDTO
            {
                InventoryID = value.InventoryID,
                InventoryCD = value.InventoryCD,
                GoldGrams = valueExt.UsrActualGRAMGold,
                SilverGrams = valueExt.UsrActualGRAMSilver,
                FineGoldGrams = valueExt.UsrPricingGRAMGold,
                FineSilverGrams = valueExt.UsrPricingGRAMSilver,
                MetalLossPct = valueExt.UsrContractLossPct,
                SurchargePct = valueExt.UsrContractSurcharge,
                Increment = valueExt.UsrContractIncrement,
                MatrixStep = valueExt.UsrMatrixStep,
                UnitCost = valueExt.UsrContractCost,
                PreciousMetalCost = valueExt.UsrCommodityCost,
                FabricationCost = valueExt.UsrFabricationCost,
                LandedCost = valueExt.UsrUnitCost,
                PackagingCost = valueExt.UsrPackagingCost,
                MaterialsCost = valueExt.UsrMaterialsCost,
                FreightCost = valueExt.UsrFreightCost,
                HandlingCost = valueExt.UsrHandlingCost,
                LaborCost = valueExt.UsrLaborCost,
                DutyCost = valueExt.UsrDutyCost,
                DutyCostPct = valueExt.UsrDutyCostPct,
                CostingType = valueExt.UsrCostingType,
            };
        }
            

        public static implicit operator ASCIStarItemCostSpecDTO(INKitSpecHdr value)
        {
            if (value == null) return null;

            var valueExt = PXCache<INKitSpecHdr>.GetExtension<ASCIStarINKitSpecHdrExt>(value);
            return new ASCIStarItemCostSpecDTO
            {
                InventoryID = value.KitInventoryID,
                RevisionID = value.RevisionID,
                GoldGrams = valueExt.UsrGoldGrams,
                SilverGrams = valueExt.UsrSilverGrams,
                FineGoldGrams = valueExt.UsrFineGoldGrams,
                FineSilverGrams = valueExt.UsrFineSilverGrams,
                MetalLossPct = valueExt.UsrMetalLossPct,
                SurchargePct = valueExt.UsrSurchargePct,
                Increment = valueExt.UsrIncrement,
                MatrixStep = valueExt.UsrMatrixStep,
                UnitCost = valueExt.UsrUnitCost,
                PreciousMetalCost = valueExt.UsrPreciousMetalCost,
                FabricationCost = valueExt.UsrFabricationCost,
                LandedCost = valueExt.UsrUnitCost,
                PackagingCost = valueExt.UsrPackagingCost,
                MaterialsCost = valueExt.UsrMaterialCost,
                FreightCost = valueExt.UsrFreightCost,
                HandlingCost = valueExt.UsrHandlingCost,
                LaborCost = valueExt.UsrLaborCost,
                DutyCost = valueExt.UsrDutyCost,
                DutyCostPct = valueExt.UsrDutyCostPct
            };
        }
            
    }
}
