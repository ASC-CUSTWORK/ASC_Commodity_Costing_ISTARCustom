namespace ASCISTARCustom.Common.DTO.Interfaces
{
    public interface IASCIStarItemCostSpecDTO
    {
        int? InventoryID { get; set; }
        string InventoryCD { get; set; }
        string RevisionID { get; set; }
        decimal? GoldGrams { get; set; }
        decimal? SilverGrams { get; set; }
        decimal? FineGoldGrams { get; set; }
        decimal? FineSilverGrams { get; set; }
        decimal? MetalLossPct { get; set; }
        decimal? SurchargePct { get; set; }
        decimal? Increment { get; set; }
        decimal? UnitCost { get; set; }
        decimal? PreciousMetalCost { get; set; }
        decimal? FabricationCost { get; set; }
        decimal? LandedCost { get; set; }
        decimal? PackagingCost { get; set; }
        decimal? OtherMaterialCost { get; set; }
        decimal? FreightCost { get; set; }
        decimal? HandlingCost { get; set; }
        decimal? LaborCost { get; set; }
        decimal? DutyCost { get; set; }
        decimal? DutyCostPct { get; set; }
    }
}
