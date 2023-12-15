using ASCJewelryLibrary.Common.DTO.Interfaces;
using PX.Data;
using PX.Objects.IN;

namespace ASCJewelryLibrary.Common.DTO
{
    ///<summary>
    /// This class represents a data transfer object (DTO) for the cost specification of an item.
    /// It contains properties for various costs related to the item
    /// The class also provides two implicit conversion operators for converting from the InventoryItem, ASCJINInventoryItemExt and ASCJItemWeightCostSpec classes to the ASCJItemCostSpecDTO class.
    ///</summary>
    //public class ASCJItemCostSpecDTO //: IASCJItemCostSpecDTO
    //{
    //    public int? InventoryID { get; set; }
    //    public string InventoryCD { get; set; }
    //    public string RevisionID { get; set; }
    //    public decimal? GoldGrams { get; set; }
    //    public decimal? SilverGrams { get; set; }
    //    public decimal? FineGoldGrams { get; set; }
    //    public decimal? FineSilverGrams { get; set; }
    //    public decimal? MetalLossPct { get; set; }
    //    public decimal? SurchargePct { get; set; }
    //    public decimal? Increment { get; set; }
    //    public decimal? MatrixStep { get; set; }
    //    public decimal? Floor { get; set; }
    //    public decimal? Ceiling { get; set; }
    //    public decimal? UnitCost { get; set; }
    //    public decimal? PreciousMetalCost { get; set; }
    //    public decimal? FabricationCost { get; set; }
    //    public decimal? LandedCost { get; set; }
    //    public decimal? PackagingCost { get; set; }
    //    public decimal? PackagingLaborCost { get; set; }
    //    public decimal? MaterialsCost { get; set; }
    //    public decimal? FreightCost { get; set; }
    //    public decimal? HandlingCost { get; set; }
    //    public decimal? LaborCost { get; set; }
    //    public decimal? DutyCost { get; set; }
    //    public decimal? DutyCostPct { get; set; }
    //    public string CostingType { get; set; }


    //    public decimal? UsrActualGRAMGold { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public decimal? UsrPricingGRAMSilver { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public decimal? UsrPricingGRAMGold { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public decimal? UsrActualGRAMSilver { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public decimal? UsrContractLossPct { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public decimal? UsrContractSurcharge { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public decimal? UsrContractIncrement { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public decimal? UsrMatrixStep { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public decimal? UsrUnitCost { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public decimal? UsrEstLandedCost { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public decimal? UsrPreciousMetalCost { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public decimal? UsrFabricationCost { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public decimal? UsrPackagingCost { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public decimal? UsrPackagingLaborCost { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public decimal? UsrOtherMaterialsCost { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public decimal? UsrOtherCost { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public decimal? UsrFreightCost { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public decimal? UsrHandlingCost { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public decimal? UsrLaborCost { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public decimal? UsrDutyCost { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public decimal? UsrDutyCostPct { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public string UsrCostingType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public string UsrCostRollupType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //    public string UsrCommodityType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }



    //    public static implicit operator ASCJItemCostSpecDTO(InventoryItem value)
    //    {
    //        if (value == null) return null;

    //        var valueExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(value);
    //        return new ASCJItemCostSpecDTO
    //        {
    //            InventoryID = value.InventoryID,
    //            InventoryCD = value.InventoryCD,
    //            GoldGrams = valueExt.UsrActualGRAMGold,
    //            SilverGrams = valueExt.UsrActualGRAMSilver,
    //            FineGoldGrams = valueExt.UsrPricingGRAMGold,
    //            FineSilverGrams = valueExt.UsrActualGRAMSilver,
    //            MetalLossPct = valueExt.UsrContractLossPct,
    //            SurchargePct = valueExt.UsrContractSurcharge,
    //            Increment = valueExt.UsrContractIncrement,
    //            MatrixStep = valueExt.UsrMatrixStep,
    //            Floor = valueExt.UsrFloor,
    //            Ceiling = valueExt.UsrCeiling,
    //            UnitCost = valueExt.UsrUnitCost,
    //            PreciousMetalCost = valueExt.UsrPreciousMetalCost,
    //            FabricationCost = valueExt.UsrFabricationCost,
    //            LandedCost = valueExt.UsrEstLandedCost,
    //            PackagingCost = valueExt.UsrPackagingCost,
    //            PackagingLaborCost = valueExt.UsrPackagingLaborCost,
    //            MaterialsCost = valueExt.UsrOtherMaterialsCost,
    //            FreightCost = valueExt.UsrFreightCost,
    //            HandlingCost = valueExt.UsrHandlingCost,
    //            LaborCost = valueExt.UsrLaborCost,
    //            DutyCost = valueExt.UsrDutyCost,
    //            DutyCostPct = valueExt.UsrDutyCostPct,
    //            CostingType = valueExt.UsrCostingType,
    //        };
    //    }

    //    public static implicit operator ASCJItemCostSpecDTO(INKitSpecStkDet value)
    //    {
    //        if (value == null) return null;

    //        var valueExt = PXCache<INKitSpecStkDet>.GetExtension<ASCJINKitSpecStkDetExt>(value);
    //        return new ASCJItemCostSpecDTO
    //        {
    //            InventoryID = value.CompInventoryID,
    //            RevisionID = value.RevisionID,
    //            GoldGrams = valueExt.UsrActualGRAMGold,
    //            SilverGrams = valueExt.UsrActualGRAMSilver,
    //            FineGoldGrams = valueExt.UsrPricingGRAMGold,
    //            FineSilverGrams = valueExt.UsrPricingGRAMSilver,
    //            MetalLossPct = valueExt.UsrContractLossPct,
    //            SurchargePct = valueExt.UsrContractSurcharge,
    //            Increment = valueExt.UsrContractIncrement,
    //            MatrixStep = valueExt.UsrMatrixStep,
    //            UnitCost = valueExt.UsrUnitCost,
    //            FabricationCost = valueExt.UsrFabricationCost,
    //            LandedCost = valueExt.UsrUnitCost,
    //            PackagingCost = valueExt.UsrPackagingCost,
    //            MaterialsCost = valueExt.UsrOtherMaterialsCost,
    //            FreightCost = valueExt.UsrFreightCost,
    //            HandlingCost = valueExt.UsrHandlingCost,
    //            LaborCost = valueExt.UsrLaborCost,
    //            DutyCost = valueExt.UsrDutyCost,
    //            CostingType = valueExt.UsrCostingType,
    //        };
    //    }
    //}
}
