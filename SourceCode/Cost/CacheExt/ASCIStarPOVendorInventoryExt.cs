using ASCISTARCustom.Common.Descriptor;
using ASCISTARCustom.Common.DTO.Interfaces;
using ASCISTARCustom.Inventory.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.PO;
using System;
using static ASCISTARCustom.Common.Descriptor.ASCIStarConstants;

namespace ASCISTARCustom.Cost.CacheExt
{
    public class ASCIStarPOVendorInventoryExt : PXCacheExtension<POVendorInventory>, IASCIStarItemCostSpecDTO
    {
        public static bool IsActive() => true;

        #region InventoryID
        [Inventory(Filterable = true, DirtyRead = true, Enabled = false)]
        [PXParent(typeof(SelectFrom<InventoryItem>.Where<InventoryItem.inventoryID.IsEqual<inventoryID.FromCurrent>>))]
        [PXDBDefault(typeof(InventoryItem.inventoryID))]
        public virtual int? InventoryID { get; set; }
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        #endregion

        #region UsrMarketID
        [PXDBInt()]
        [PXUIField(DisplayName = "Market")]
        [PXSelector(typeof(Search2<Vendor.bAccountID, InnerJoin<VendorClass,
                On<Vendor.vendorClassID, Equal<VendorClass.vendorClassID>>>, Where<VendorClass.vendorClassID, Equal<MarketClass>>>),
            typeof(Vendor.acctCD), typeof(Vendor.acctName)
            , SubstituteKey = typeof(Vendor.acctCD), DescriptionField = typeof(Vendor.acctName))]
        [PXDefault(typeof(Search<ASCIStarVendorExt.usrMarketID,
            Where<Vendor.bAccountID, Equal<Current<POVendorInventory.vendorID>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        public int? UsrMarketID { get; set; }
        public abstract class usrMarketID : PX.Data.BQL.BqlInt.Field<usrMarketID> { }
        #endregion

        #region UsrCommodityID
        [PXDBInt()]
        [PXUIField(DisplayName = "Metal")]
        [PXSelector(typeof(Search2<InventoryItem.inventoryID,
            InnerJoin<INItemClass, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>,
                Where<INItemClass.itemClassCD, Equal<ASCIStarConstants.CommodityClass>>>),
            typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr)
            , SubstituteKey = typeof(InventoryItem.inventoryCD), DescriptionField = typeof(InventoryItem.descr))]
        public int? UsrCommodityID { get; set; }
        public abstract class usrCommodityID : PX.Data.BQL.BqlInt.Field<usrCommodityID> { }
        #endregion

        #region UsrIsOverrideVendor
        [PXDBBool()]
        [PXUIField(DisplayName = "Override Vendor", Visible = false)]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public bool? UsrIsOverrideVendor { get; set; }
        public abstract class usrIsOverrideVendor : PX.Data.BQL.BqlBool.Field<usrIsOverrideVendor> { }
        #endregion

        #region UsrCommodityVendorPrice
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Custom Price", Visible = false)]
        public decimal? UsrCommodityVendorPrice { get; set; }
        public abstract class usrCommodityVendorPrice : PX.Data.BQL.BqlDecimal.Field<usrCommodityVendorPrice> { }
        #endregion

        #region UsrBasisPrice
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Basis Price", IsReadOnly = true)]
        public decimal? UsrBasisPrice { get; set; }
        public abstract class usrBasisPrice : PX.Data.BQL.BqlDecimal.Field<usrBasisPrice> { }
        #endregion

        #region UsrBasisValue
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Price / TOZ @ Basis", IsReadOnly = true)]
        public decimal? UsrBasisValue { get; set; }
        public abstract class usrBasisValue : PX.Data.BQL.BqlDecimal.Field<usrBasisValue> { }
        #endregion

        #region UsrContractIncrement
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Increment")]
        public decimal? UsrContractIncrement { get; set; }
        public abstract class usrContractIncrement : PX.Data.BQL.BqlDecimal.Field<usrContractIncrement> { }
        #endregion

        #region UsrMatrixStep
        [PXDBDecimal(6, MinValue = 0, MaxValue = 10)]
        [PXUIField(DisplayName = "Matrix Step")]
        [PXDefault(TypeCode.Decimal, "0.500000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrMatrixStep { get; set; }
        public abstract class usrMatrixStep : PX.Data.BQL.BqlDecimal.Field<usrMatrixStep> { }
        #endregion

        #region UsrFloor
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Floor", IsReadOnly = true)]
        public decimal? UsrFloor { get; set; }
        public abstract class usrFloor : PX.Data.BQL.BqlDecimal.Field<usrFloor> { }
        #endregion

        #region UsrCeiling
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Ceiling", IsReadOnly = true)]
        public decimal? UsrCeiling { get; set; }
        public abstract class usrCeiling : PX.Data.BQL.BqlDecimal.Field<usrCeiling> { }
        #endregion

        #region UsrContractLossPct
        [PXDBDecimal(2)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Metal Loss %", Visible = false)]
        public decimal? UsrContractLossPct { get; set; }
        public abstract class usrContractLossPct : PX.Data.BQL.BqlDecimal.Field<usrContractLossPct> { }
        #endregion

        #region UsrContractSurcharge
        [PXDBDecimal(2)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Surcharge, %", Visible = false)]
        public decimal? UsrContractSurcharge { get; set; }
        public abstract class usrContractSurcharge : PX.Data.BQL.BqlDecimal.Field<usrContractSurcharge> { }
        #endregion

        #region UsrPreciousMetalCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Precious Metal Cost")]
        public decimal? UsrPreciousMetalCost { get; set; }
        public abstract class usrPreciousMetalCost : PX.Data.BQL.BqlDecimal.Field<usrPreciousMetalCost> { }
        #endregion

        #region UsrOtherMaterialsCost
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Other Material Cost")]
        public decimal? UsrOtherMaterialsCost { get; set; }
        public abstract class usrOtherMaterialsCost : PX.Data.BQL.BqlDecimal.Field<usrOtherMaterialsCost> { }
        #endregion

        #region UsrFabricationCost
        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fabrication / Value Add")]
        public decimal? UsrFabricationCost { get; set; }
        public abstract class usrFabricationCost : PX.Data.BQL.BqlDecimal.Field<usrFabricationCost> { }
        #endregion

        #region UsrPackagingCost
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Packaging Cost")]
        public decimal? UsrPackagingCost { get; set; }
        public abstract class usrPackagingCost : PX.Data.BQL.BqlDecimal.Field<usrPackagingCost> { }
        #endregion

        #region UsrPackagingLaborCost
        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Labor packaging")]
        public decimal? UsrPackagingLaborCost { get; set; }
        public abstract class usrPackagingLaborCost : PX.Data.BQL.BqlDecimal.Field<usrPackagingLaborCost> { }
        #endregion

        #region UsrLaborCost
        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "In-house Labor Cost", Visible = false)]
        public decimal? UsrLaborCost { get; set; }
        public abstract class usrLaborCost : PX.Data.BQL.BqlDecimal.Field<usrLaborCost> { }
        #endregion

        #region UsrHandlingCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Handling Cost", Visible = false)]
        public decimal? UsrHandlingCost { get; set; }
        public abstract class usrHandlingCost : PX.Data.BQL.BqlDecimal.Field<usrHandlingCost> { }
        #endregion

        #region UsrFreightCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Freight Cost", Visible = false)]
        public decimal? UsrFreightCost { get; set; }
        public abstract class usrFreightCost : PX.Data.BQL.BqlDecimal.Field<usrFreightCost> { }
        #endregion

        #region UsrDutyCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Duty Cost", Visible = false)]
        public decimal? UsrDutyCost { get; set; }
        public abstract class usrDutyCost : PX.Data.BQL.BqlDecimal.Field<usrDutyCost> { }
        #endregion

        #region UsrOtherCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Other Cost", Enabled = false, Visible = false)]
        public decimal? UsrOtherCost { get; set; }
        public abstract class usrOtherCost : PX.Data.BQL.BqlDecimal.Field<usrOtherCost> { }
        #endregion

        #region UnitCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Unit Cost", Enabled = false)]
        [PXFormula(typeof(Add<Add<Add<Add<usrPreciousMetalCost, usrOtherMaterialsCost>, usrFabricationCost>, usrPackagingCost>, usrPackagingLaborCost>))]
        public decimal? UsrUnitCost { get; set; }
        public abstract class usrUnitCost : PX.Data.BQL.BqlDecimal.Field<usrUnitCost> { }
        #endregion

        #region UsrEstLandedCost
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Est. Landed Cost", Visibility = PXUIVisibility.Visible, Enabled = false)]
        [PXFormula(typeof(Add<Add<Add<Add<usrUnitCost, usrHandlingCost>, usrFreightCost>, usrLaborCost>, usrDutyCost>))]
        public decimal? UsrEstLandedCost { get; set; }
        public abstract class usrEstLandedCost : PX.Data.BQL.BqlDecimal.Field<usrEstLandedCost> { }
        #endregion

        #region UsrFabricationWeight
        [PXDBDecimal(2)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fabrication / Weight", Visibility = PXUIVisibility.Visible)]
        public decimal? UsrFabricationWeight { get; set; }
        public abstract class usrFabricationWeight : PX.Data.BQL.BqlDecimal.Field<usrFabricationWeight> { }
        #endregion

        #region UsrFabricationPiece
        [PXDBDecimal(2)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fabrication / Piece", Visibility = PXUIVisibility.Visible)]
        public decimal? UsrFabricationPiece { get; set; }
        public abstract class usrFabricationPiece : PX.Data.BQL.BqlDecimal.Field<usrFabricationPiece> { }
        #endregion

        #region Implementation Unneeded Interface's fields

        [PXDecimal(6)]
        public decimal? UsrActualGRAMGold { get; set; }
        public abstract class usrActualGRAMGold : PX.Data.BQL.BqlDecimal.Field<usrActualGRAMGold> { }

        [PXDecimal(6)]
        public decimal? UsrPricingGRAMSilver { get; set; }
        public abstract class usrActualGRAMSilver : PX.Data.BQL.BqlDecimal.Field<usrActualGRAMSilver> { }

        [PXDecimal(6)]
        public decimal? UsrPricingGRAMGold { get; set; }
        public abstract class usrPricingGRAMGold : PX.Data.BQL.BqlDecimal.Field<usrPricingGRAMGold> { }

        [PXDecimal(6)]
        public decimal? UsrActualGRAMSilver { get; set; }
        public abstract class usrPricingGRAMSilver : PX.Data.BQL.BqlDecimal.Field<usrPricingGRAMSilver> { }

        [PXDecimal(6)]
        public decimal? UsrDutyCostPct { get; set; }
        public abstract class usrDutyCostPct : PX.Data.BQL.BqlDecimal.Field<usrDutyCostPct> { }

        [PXString]
        public string UsrCostingType { get; set; }
        public abstract class usrCostingType : PX.Data.BQL.BqlDecimal.Field<usrCostingType> { }

        [PXString]
        public string UsrCommodityType { get; set; }
        public abstract class usrCommodityType : PX.Data.BQL.BqlDecimal.Field<usrCommodityType> { }

        #endregion
    }
}