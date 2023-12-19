namespace ASCJewelryLibrary.Common.DTO.Interfaces
{
    public interface IASCJItemCostSpecDTO
    {
        int? UsrASCJInventoryID { get; set; }
        decimal? UsrASCJActualGRAMGold { get; set; }
        decimal? UsrASCJPricingGRAMSilver { get; set; }
        decimal? UsrASCJPricingGRAMGold { get; set; }
        decimal? UsrASCJActualGRAMSilver { get; set; }
        decimal? UsrASCJContractLossPct { get; set; }
        decimal? UsrASCJContractSurcharge { get; set; }
        decimal? UsrASCJContractIncrement { get; set; }
        decimal? UsrASCJMatrixStep { get; set; }
        decimal? UsrASCJUnitCost { get; set; }
        decimal? UsrASCJBasisValue { get; set; }
        decimal? UsrASCJEstLandedCost { get; set; }
        decimal? UsrASCJPreciousMetalCost { get; set; }
        decimal? UsrASCJFabricationCost { get; set; }
        decimal? UsrASCJPackagingCost { get; set; }
        decimal? UsrASCJPackagingLaborCost { get; set; }
        decimal? UsrASCJOtherMaterialsCost { get; set; }
        decimal? UsrASCJOtherCost { get; set; }
        decimal? UsrASCJFreightCost { get; set; }
        decimal? UsrASCJHandlingCost { get; set; }
        decimal? UsrASCJLaborCost { get; set; }
        decimal? UsrASCJDutyCost { get; set; }
        decimal? UsrASCJDutyCostPct { get; set; }
        string UsrASCJCostingType { get; set; }
        string UsrASCJCommodityType { get; set; }
    }
}
