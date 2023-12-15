namespace ASCJewelryLibrary.Common.DTO.Interfaces
{
    public interface IASCJItemCostSpecDTO
    {
        int? InventoryID { get; set; }
        decimal? UsrActualGRAMGold { get; set; }
        decimal? UsrPricingGRAMSilver { get; set; }
        decimal? UsrPricingGRAMGold { get; set; }
        decimal? UsrActualGRAMSilver { get; set; }
        decimal? UsrContractLossPct { get; set; }
        decimal? UsrContractSurcharge { get; set; }
        decimal? UsrContractIncrement { get; set; }
        decimal? UsrMatrixStep { get; set; }
        decimal? UsrUnitCost { get; set; }
        decimal? UsrBasisValue { get; set; }
        decimal? UsrEstLandedCost { get; set; }
        decimal? UsrPreciousMetalCost { get; set; }
        decimal? UsrFabricationCost { get; set; }
        decimal? UsrPackagingCost { get; set; }
        decimal? UsrPackagingLaborCost { get; set; }
        decimal? UsrOtherMaterialsCost { get; set; }
        decimal? UsrOtherCost { get; set; }
        decimal? UsrFreightCost { get; set; }
        decimal? UsrHandlingCost { get; set; }
        decimal? UsrLaborCost { get; set; }
        decimal? UsrDutyCost { get; set; }
        decimal? UsrDutyCostPct { get; set; }
        string UsrCostingType { get; set; }
        string UsrCommodityType { get; set; }
    }
}
