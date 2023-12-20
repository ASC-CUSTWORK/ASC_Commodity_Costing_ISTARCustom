using ASCJewelryLibrary.AP.CacheExt;
using ASCJewelryLibrary.Common.Descriptor;
using ASCJewelryLibrary.Common.DTO.Interfaces;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.PO;
using System;
using static ASCJewelryLibrary.Common.Descriptor.ASCJConstants;

namespace ASCJewelryLibrary.PO.CacheExt
{
    [Serializable]
    [PXCacheName("ASC PO Vendor Inventory Extension")]
    public class ASCJPOVendorInventoryExt : PXCacheExtension<POVendorInventory>, IASCJItemCostSpecDTO
    {
        public static bool IsActive() => true;

        #region InventoryID
        [PXInt]
        [PXFormula(typeof(POVendorInventory.inventoryID))]
        public virtual int? UsrASCJInventoryID { get; set; }
        public abstract class usrASCJInventoryID : PX.Data.BQL.BqlInt.Field<usrASCJInventoryID> { }
        #endregion

        #region UsrASCJMarketID
        [PXDBInt()]
        [PXUIField(DisplayName = "Market")]
        [PXSelector(typeof(Search2<Vendor.bAccountID, InnerJoin<VendorClass,
                On<Vendor.vendorClassID, Equal<VendorClass.vendorClassID>>>, Where<VendorClass.vendorClassID, Equal<MarketClass>>>),
            typeof(Vendor.acctCD), typeof(Vendor.acctName)
            , SubstituteKey = typeof(Vendor.acctCD), DescriptionField = typeof(Vendor.acctName))]
        [PXDefault(typeof(Search<ASCJVendorExt.usrASCJMarketID,
            Where<Vendor.bAccountID, Equal<Current<POVendorInventory.vendorID>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        public int? UsrASCJMarketID { get; set; }
        public abstract class usrASCJMarketID : PX.Data.BQL.BqlInt.Field<usrASCJMarketID> { }
        #endregion

        #region UsrASCJCommodityID
        [PXDBInt()]
        [PXUIField(DisplayName = "Metal")]
        [PXSelector(typeof(Search2<InventoryItem.inventoryID,
            InnerJoin<INItemClass, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>,
                Where<INItemClass.itemClassCD, Equal<ASCJConstants.CommodityClass>>>),
            typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr)
            , SubstituteKey = typeof(InventoryItem.inventoryCD), DescriptionField = typeof(InventoryItem.descr))]
        public int? UsrASCJCommodityID { get; set; }
        public abstract class usrASCJCommodityID : PX.Data.BQL.BqlInt.Field<usrASCJCommodityID> { }
        #endregion

        #region UsrASCJIsOverrideVendor
        [PXDBBool()]
        [PXUIField(DisplayName = "Override Vendor", Visible = false)]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public bool? UsrASCJIsOverrideVendor { get; set; }
        public abstract class usrASCJIsOverrideVendor : PX.Data.BQL.BqlBool.Field<usrASCJIsOverrideVendor> { }
        #endregion

        #region UsrASCJCommodityVendorPrice
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Custom Price", Visible = false)]
        public decimal? UsrASCJCommodityVendorPrice { get; set; }
        public abstract class usrASCJCommodityVendorPrice : PX.Data.BQL.BqlDecimal.Field<usrASCJCommodityVendorPrice> { }
        #endregion

        #region UsrASCJBasisPrice
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Basis Price", IsReadOnly = true)]
        public decimal? UsrASCJBasisPrice { get; set; }
        public abstract class usrASCJBasisPrice : PX.Data.BQL.BqlDecimal.Field<usrASCJBasisPrice> { }
        #endregion

        #region UsrASCJBasisValue
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Price / TOZ @ Basis", IsReadOnly = true)]
        public decimal? UsrASCJBasisValue { get; set; }
        public abstract class usrASCJBasisValue : PX.Data.BQL.BqlDecimal.Field<usrASCJBasisValue> { }
        #endregion

        #region UsrASCJContractIncrement
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Increment")]
        public decimal? UsrASCJContractIncrement { get; set; }
        public abstract class usrASCJContractIncrement : PX.Data.BQL.BqlDecimal.Field<usrASCJContractIncrement> { }
        #endregion

        #region UsrASCJMatrixStep
        [PXDBDecimal(4, MinValue = 0, MaxValue = 10)]
        [PXUIField(DisplayName = "Matrix Step")]
        [PXDefault(TypeCode.Decimal, "0.5000", PersistingCheck = PXPersistingCheck.Nothing)]
        public decimal? UsrASCJMatrixStep { get; set; }
        public abstract class usrASCJMatrixStep : PX.Data.BQL.BqlDecimal.Field<usrASCJMatrixStep> { }
        #endregion

        #region UsrASCJFloor
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Floor", IsReadOnly = true)]
        public decimal? UsrASCJFloor { get; set; }
        public abstract class usrASCJFloor : PX.Data.BQL.BqlDecimal.Field<usrASCJFloor> { }
        #endregion

        #region UsrASCJCeiling
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Ceiling", IsReadOnly = true)]
        public decimal? UsrASCJCeiling { get; set; }
        public abstract class usrASCJCeiling : PX.Data.BQL.BqlDecimal.Field<usrASCJCeiling> { }
        #endregion

        #region UsrASCJContractLossPct
        [PXDBDecimal(2)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Metal Loss %", Visible = false)]
        public decimal? UsrASCJContractLossPct { get; set; }
        public abstract class usrASCJContractLossPct : PX.Data.BQL.BqlDecimal.Field<usrASCJContractLossPct> { }
        #endregion

        #region UsrASCJContractSurcharge
        [PXDBDecimal(2)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Surcharge %", Visible = false)]
        public decimal? UsrASCJContractSurcharge { get; set; }
        public abstract class usrASCJContractSurcharge : PX.Data.BQL.BqlDecimal.Field<usrASCJContractSurcharge> { }
        #endregion

        #region UsrASCJPreciousMetalCost
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Precious Metal Cost")]
        public decimal? UsrASCJPreciousMetalCost { get; set; }
        public abstract class usrASCJPreciousMetalCost : PX.Data.BQL.BqlDecimal.Field<usrASCJPreciousMetalCost> { }
        #endregion

        #region UsrASCJOtherMaterialsCost
        [PXDBDecimal(4, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Other Material Cost")]
        public decimal? UsrASCJOtherMaterialsCost { get; set; }
        public abstract class usrASCJOtherMaterialsCost : PX.Data.BQL.BqlDecimal.Field<usrASCJOtherMaterialsCost> { }
        #endregion

        #region UsrASCJFabricationCost
        [PXDBDecimal(4, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fabrication / Value Add")]
        public decimal? UsrASCJFabricationCost { get; set; }
        public abstract class usrASCJFabricationCost : PX.Data.BQL.BqlDecimal.Field<usrASCJFabricationCost> { }
        #endregion

        #region UsrASCJPackagingCost
        [PXDBDecimal(4, MinValue = 0, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Packaging Cost")]
        public decimal? UsrASCJPackagingCost { get; set; }
        public abstract class usrASCJPackagingCost : PX.Data.BQL.BqlDecimal.Field<usrASCJPackagingCost> { }
        #endregion

        #region UsrASCJPackagingLaborCost
        [PXDBDecimal(4, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.0000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Labor packaging")]
        public decimal? UsrASCJPackagingLaborCost { get; set; }
        public abstract class usrASCJPackagingLaborCost : PX.Data.BQL.BqlDecimal.Field<usrASCJPackagingLaborCost> { }
        #endregion

        #region UsrASCJLaborCost
        [PXDBDecimal(4, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "In-house Labor Cost", Visible = false)]
        public decimal? UsrASCJLaborCost { get; set; }
        public abstract class usrASCJLaborCost : PX.Data.BQL.BqlDecimal.Field<usrASCJLaborCost> { }
        #endregion

        #region UsrASCJHandlingCost
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Handling Cost", Visible = false)]
        public decimal? UsrASCJHandlingCost { get; set; }
        public abstract class usrASCJHandlingCost : PX.Data.BQL.BqlDecimal.Field<usrASCJHandlingCost> { }
        #endregion

        #region UsrASCJFreightCost
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Freight Cost", Visible = false)]
        public decimal? UsrASCJFreightCost { get; set; }
        public abstract class usrASCJFreightCost : PX.Data.BQL.BqlDecimal.Field<usrASCJFreightCost> { }
        #endregion

        #region UsrASCJDutyCost
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Duty Cost", Visible = false)]
        public decimal? UsrASCJDutyCost { get; set; }
        public abstract class usrASCJDutyCost : PX.Data.BQL.BqlDecimal.Field<usrASCJDutyCost> { }
        #endregion

        #region UsrASCJOtherCost
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Other Cost", Enabled = false, Visible = false)]
        public decimal? UsrASCJOtherCost { get; set; }
        public abstract class usrASCJOtherCost : PX.Data.BQL.BqlDecimal.Field<usrASCJOtherCost> { }
        #endregion

        #region UnitCost
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Unit Cost", IsReadOnly = true)]
        [PXFormula(typeof(Add<Add<Add<Add<usrASCJPreciousMetalCost, usrASCJOtherMaterialsCost>, usrASCJFabricationCost>, usrASCJPackagingCost>, usrASCJPackagingLaborCost>))]
        public decimal? UsrASCJUnitCost { get; set; }
        public abstract class usrASCJUnitCost : PX.Data.BQL.BqlDecimal.Field<usrASCJUnitCost> { }
        #endregion

        #region UsrASCJEstLandedCost
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.00", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Est. Landed Cost", Visibility = PXUIVisibility.Visible, Enabled = false)]
        [PXFormula(typeof(Add<Add<Add<Add<usrASCJUnitCost, usrASCJHandlingCost>, usrASCJFreightCost>, usrASCJLaborCost>, usrASCJDutyCost>))]
        public decimal? UsrASCJEstLandedCost { get; set; }
        public abstract class usrASCJEstLandedCost : PX.Data.BQL.BqlDecimal.Field<usrASCJEstLandedCost> { }
        #endregion

        #region UsrASCJFabricationWeight
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fabrication / Weight", Visibility = PXUIVisibility.Visible)]
        public decimal? UsrASCJFabricationWeight { get; set; }
        public abstract class usrASCJFabricationWeight : PX.Data.BQL.BqlDecimal.Field<usrASCJFabricationWeight> { }
        #endregion

        #region UsrASCJFabricationPiece
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fabrication / Piece", Visibility = PXUIVisibility.Visible)]
        public decimal? UsrASCJFabricationPiece { get; set; }
        public abstract class usrASCJFabricationPiece : PX.Data.BQL.BqlDecimal.Field<usrASCJFabricationPiece> { }
        #endregion

        #region Implementation Unneeded Interface's fields

        [PXDecimal(4)]
        public decimal? UsrASCJActualGRAMGold { get; set; }
        public abstract class usrASCJActualGRAMGold : PX.Data.BQL.BqlDecimal.Field<usrASCJActualGRAMGold> { }

        [PXDecimal(4)]
        public decimal? UsrASCJPricingGRAMSilver { get; set; }
        public abstract class usrASCJActualGRAMSilver : PX.Data.BQL.BqlDecimal.Field<usrASCJActualGRAMSilver> { }

        [PXDecimal(4)]
        public decimal? UsrASCJPricingGRAMGold { get; set; }
        public abstract class usrASCJPricingGRAMGold : PX.Data.BQL.BqlDecimal.Field<usrASCJPricingGRAMGold> { }

        [PXDecimal(4)]
        public decimal? UsrASCJActualGRAMSilver { get; set; }
        public abstract class usrASCJPricingGRAMSilver : PX.Data.BQL.BqlDecimal.Field<usrASCJPricingGRAMSilver> { }

        [PXDecimal(4)]
        public decimal? UsrASCJDutyCostPct { get; set; }
        public abstract class usrASCJDutyCostPct : PX.Data.BQL.BqlDecimal.Field<usrASCJDutyCostPct> { }

        [PXString]
        public string UsrASCJCostingType { get; set; }
        public abstract class usrASCJCostingType : PX.Data.BQL.BqlDecimal.Field<usrASCJCostingType> { }

        [PXString]
        public string UsrASCJCommodityType { get; set; }
        public abstract class usrASCJCommodityType : PX.Data.BQL.BqlDecimal.Field<usrASCJCommodityType> { }

        #endregion
    }
}