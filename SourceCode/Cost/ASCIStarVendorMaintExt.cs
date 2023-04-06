using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PX.Api;
using PX.Api.Models;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.WorkflowAPI;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.DR;
using PX.Objects.GL;
using PX.Objects.PO;
using PX.Objects.SO;
using PX.Objects.RUTROT;
using PX.Objects.Common.Discount;
using PX.SM;
using CRLocation = PX.Objects.CR.Standalone.Location;
using ItemStats = PX.Objects.IN.Overrides.INDocumentRelease.ItemStats;
using ItemCost = PX.Objects.IN.Overrides.INDocumentRelease.ItemCost;
using SiteStatus = PX.Objects.IN.Overrides.INDocumentRelease.SiteStatus;
using PX.Objects.Common.GraphExtensions;
using PX.Objects.CM;
using PX.Objects;
using PX.Objects.IN;
//using InfoSmartSearch;

namespace ASCISTARCustom.Cost
{
    public class ASCIStarVendorMaintExt : PXGraphExtension<VendorMaint>
    {
        public static bool IsActive() => true;

        #region Selects

        //public PXSelect<ASCIStarVendorPriceConfig, Where<ASCIStarVendorPriceConfig.bAccountID, Equal<Current<Vendor.bAccountID>>>> PriceConfig;

        //[PXFilterable]
        //public PXSelectJoin<APVendorPrice,
        //        InnerJoin<InventoryItem, On<APVendorPrice.inventoryID, Equal<InventoryItem.inventoryID>>,
        //        InnerJoin<INItemClass, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>>,
        //        Where<APVendorPrice.vendorID, Equal<Current<Vendor.bAccountID>>, 
        //            And<INItemClass.itemClassCD, Equal<CommodityClass>>>> VendorPriceBasis;
        public class today : PX.Data.BQL.BqlDateTime.Constant<today>
        {
            public today() : base(DateTime.Today)
            {
            }
        }

        //[PXFilterable]
        //public PXSelectJoin<APVendorPrice,
        //        InnerJoin<InventoryItem, On<APVendorPrice.inventoryID, Equal<InventoryItem.inventoryID>>,
        //        InnerJoin<POVendorInventory, On<InventoryItem.inventoryID, Equal<POVendorInventory.inventoryID>>,
        //        InnerJoin<InventoryItemCurySettings, On<POVendorInventory.inventoryID, Equal<InventoryItemCurySettings.inventoryID>,
        //            And<POVendorInventory.vendorID, Equal<InventoryItemCurySettings.preferredVendorID>>>,
        //        InnerJoin<INItemClass, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>>>>,
        //        Where<APVendorPrice.vendorID, Equal<Required<APVendorPrice.vendorID>>,
        //            And<INItemClass.itemClassCD, Equal<CommodityClass>,
        //            And<APVendorPrice.effectiveDate, LessEqual<today>,
        //            And<APVendorPrice.expirationDate, GreaterEqual<today>>>>>,
        //        OrderBy<Desc<APVendorPrice.effectiveDate>>> VendorPriceBasis;

        [PXFilterable]
        public PXSelectJoin<APVendorPrice,
        InnerJoin<InventoryItem, On<APVendorPrice.inventoryID, Equal<InventoryItem.inventoryID>>,
        InnerJoin<INItemClass, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>>,
        Where<APVendorPrice.vendorID, Equal<Current<Vendor.bAccountID>>,
            And<INItemClass.itemClassCD, Equal<CommodityClass>>>,
        OrderBy<Desc<APVendorPrice.effectiveDate>>> VendorPriceBasis;

        #endregion Select

    }
}