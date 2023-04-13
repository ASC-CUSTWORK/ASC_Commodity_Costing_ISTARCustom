using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.IN;
using PX.Objects.AP;
using PX.Objects.PO;
using PX.Objects.CR.Standalone;
using System.Collections;
using ASCISTARCustom.PDS.CacheExt;
using ASCISTARCustom.Inventory.DAC;
using ASCISTARCustom.Inventory.CacheExt;
using ASCISTARCustom.Common.Builder;
using ASCISTARCustom.Common.Services.DataProvider.Interfaces;
using ASCISTARCustom.Cost.Descriptor;
using PX.Common;
using ASCISTARCustom.Common.Helper;
using PX.Data.BQL;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.CS;
using PX.TM;
using System;

namespace ASCISTARCustom.PDS
{
    public class ASCIStarINKitSpecMaintExt : PXGraphExtension<INKitSpecMaint>
    {
        #region Static Functions
        public static bool IsActive() => true;
        #endregion

        #region View
        public PXSelect<POVendorInventory, Where<POVendorInventory.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>> VendorItems;

        public SelectFrom<ASCIStarINKitSpecJewelryItem>
                 .Where<ASCIStarINKitSpecJewelryItem.kitInventoryID.IsEqual<INKitSpecHdr.kitInventoryID.FromCurrent>
                    .And<ASCIStarINKitSpecJewelryItem.revisionID.IsEqual<INKitSpecHdr.revisionID.FromCurrent>>>
                        .View JewelryItemView;

        [PXFilterable]
        public PXSelectJoin<
            APVendorPrice,
            InnerJoin<POVendorInventory, 
                On<POVendorInventory.vendorID, Equal<Current<APVendorPrice.vendorID>>,
                And<POVendorInventory.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>>,
            InnerJoin<InventoryItemCurySettings, 
                On<InventoryItemCurySettings.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
                And<InventoryItemCurySettings.preferredVendorID, Equal<POVendorInventory.vendorID>>>,
            InnerJoin<InventoryItem, 
                On<APVendorPrice.inventoryID, Equal<InventoryItem.inventoryID>>,
            InnerJoin<INItemClass, 
                On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>>>>,
            Where<APVendorPrice.vendorID, Equal<InventoryItemCurySettings.preferredVendorID>,
                And<INItemClass.itemClassCD, Equal<CommodityClass>,
                And<APVendorPrice.effectiveDate, LessEqual<AccessInfo.businessDate>,
                And<APVendorPrice.expirationDate, GreaterEqual<AccessInfo.businessDate>>>>>,
            OrderBy<
                Desc<APVendorPrice.effectiveDate>>> VendorPriceBasis;
        #endregion

        #region Dependency Injection
        [InjectDependency]
        public IASCIStarInventoryItemDataProvider _itemDataProvider { get; set; }

        [InjectDependency]
        public IASCIStarVendorDataProvider _vendorDataProvider { get; set; }
        #endregion

        #region CacheAttached
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXFormula(typeof(Switch<Case<Where<Current<APVendorPrice.uOM>, NotEqual<TOZ>>, Null>, Div<APVendorPrice.salesPrice, TOZ2GRAM>>))]
        protected void _(Events.CacheAttached<ASCIStarAPVendorPriceExt.usrCommodityPerGram> cacheAttached) { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXFormula(typeof(Switch<Case<Where<Current<APVendorPrice.uOM>, NotEqual<TOZ>>, Null>, Div<Div<APVendorPrice.salesPrice, ASCIStarAPVendorPriceExt.usrCommodityPrice>, TOZ2GRAM>>))]
        protected void _(Events.CacheAttached<ASCIStarAPVendorPriceExt.usrIncrementPerGram> cacheAttached) { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDefault(typeof(INKitSpecHdr.kitInventoryID))]
        protected void _(Events.CacheAttached<APVendorPrice.inventoryID> cacheAttached) { }

        [PXRemoveBaseAttribute(typeof(PXDBStringAttribute))]
        [PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">##")]
        [PXDefault("01")]
        protected void _(Events.CacheAttached<INKitSpecHdr.revisionID> cacheAttached) { }
        #endregion

        #region Actions
        public PXAction<INKitSpecHdr> createitem;
        [PXUIField(DisplayName = "Create Production Item", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable createItem(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXAction<INKitSpecHdr> ASCIStarUpdateMetalCost;
        [PXUIField(DisplayName = "Update Metal Cost", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(CommitChanges = true)]
        public virtual IEnumerable aSCIStarUpdateMetalCost(PXAdapter adapter)
        {
            Base.StockDet.Select().RowCast<INKitSpecStkDet>().ForEach(currentLine =>
            {
                //TODO create logic for updateing metal costs
            });

            Base.NonStockDet.Select().RowCast<INKitSpecNonStkDet>().ForEach(currentLine =>
            {
                //TODO create logic for updateing metal costs based on non stock component.
                //NOTE! this loop might not be needed
            });

            return adapter.Get();
        }
        #endregion

        #region Event Handlers

        #region INKitSpecHdr Events
        protected virtual void _(Events.RowInserted<INKitSpecHdr> e)
        {
            var row = e.Row;
            if (row == null || this.Base.Hdr.Current == null) return;

            CopyJewelryItemFields(this.Base.Hdr.Current);
        }
        protected virtual void _(Events.RowSelected<INKitSpecHdr> e)
        {
            if (e.Row is INKitSpecHdr row)
            {
                SetVisibleRevisionID();
                SetEnabledMetalWeightFields(e, row);
            }
        }
        protected virtual void _(Events.RowUpdated<INKitSpecHdr> e)
        {
            if (e.Row is INKitSpecHdr row)
            {
                var rowExt = PXCache<INKitSpecHdr>.GetExtension<ASCIStarINKitSpecHdrExt>(row);
                rowExt.UsrIsManualChanged = true;
            }
        }
        protected virtual void _(Events.FieldSelecting<INKitSpecHdr, ASCIStarINKitSpecHdrExt.usrUnitCost> e)
        {
            if (e.Row is INKitSpecHdr row)
            {
                var result = ASCIStarCostBuilder.CalculateUnitCost(row);
                e.ReturnValue = result;
            }
        }
        protected virtual void _(Events.FieldSelecting<INKitSpecHdr, ASCIStarINKitSpecHdrExt.usrLandedCost> e)
        {
            if (e.Row is INKitSpecHdr row)
            {
                var result = ASCIStarCostBuilder.CalculateUnitCost(row) + ASCIStarCostBuilder.CalculateLandedCost(row);
                e.ReturnValue = result;
            }
        }
        #endregion

        #region INKitSpecStkDet Events
        protected virtual void _(Events.RowSelected<INKitSpecStkDet> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                if (Base.Hdr.Current != null)
                {
                    var specHdrExt = PXCache<INKitSpecHdr>.GetExtension<ASCIStarINKitSpecHdrExt>(Base.Hdr.Current);
                    SetZeroForCustomFields(specHdrExt);

                    Base.StockDet.Select().RowCast<INKitSpecStkDet>().ForEach(currentLine =>
                    {
                        var quantity = GetQuantity(currentLine);
                        var inventoryItem = _itemDataProvider.GetInventoryItemByID(currentLine.CompInventoryID);
                        if (inventoryItem != null)
                        {
                            var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(inventoryItem);
                            var jewelryItem = GetASCIStarINJewelryItem(inventoryItem.InventoryID);

                            if (jewelryItem != null && jewelryItem.MetalType == "24K")
                            {
                                CalculateAndAssignGoldGrams(specHdrExt, quantity, inventoryItemExt);
                                CalculateAndAssignItemCosts(specHdrExt, quantity, inventoryItemExt);

                                DisableRollupType(e.Cache, currentLine);
                            }
                            else if (jewelryItem != null && jewelryItem.MetalType == "SSS")
                            {
                                CalculateAndAssignSilverGrams(specHdrExt, currentLine, inventoryItemExt);
                                CalculateAndAssignItemCosts(specHdrExt, quantity, inventoryItemExt);

                                DisableRollupType(e.Cache, currentLine);

                                //Silver Always Market costing type and ERP Valuation
                            }
                            else
                            {
                                var rowExt = PXCache<INKitSpecStkDet>.GetExtension<ASCIStarINKitSpecStkDetExt>(currentLine);
                                AssignCost(rowExt.UsrCostRollupType, specHdrExt, rowExt.UsrExtCost);

                                // for other component items always costing type should be ERP Valuation. and disabled

                            }
                        }
                    });
                }
            }
        }
        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCIStarINKitSpecStkDetExt.usrCostingType> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                var jewelryItem = GetASCIStarINJewelryItem(row.CompInventoryID);
                if (jewelryItem != null && (ASCIStarMetalType.IsGold(jewelryItem.MetalType) || ASCIStarMetalType.IsSilver(jewelryItem.MetalType)))
                {
                    var inventoryItem = _itemDataProvider.GetInventoryItemByID(row.CompInventoryID);
                    var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCIStarINInventoryItemExt>(inventoryItem);
                    e.NewValue = inventoryItemExt.UsrCostingType;
                }
                else
                {
                    e.NewValue = ASCIStarCostingType.StandardCost;
                }
            }
        }
        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCIStarINKitSpecStkDetExt.usrUnitCost> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                var inventoryItem = _itemDataProvider.GetInventoryItemByID(row.CompInventoryID);
                e.NewValue = ASCIStarCostBuilder.CalculateUnitCost(inventoryItem);
            }
        }
        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCIStarINKitSpecStkDetExt.usrSalesPrice> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                decimal? salesPrice = 0m;
                var defaultVendor = VendorItems.Select().RowCast<POVendorInventory>().FirstOrDefault(_ => _.IsDefault == true);
                if (defaultVendor != null)
                {
                    
                    var jewelryItem = GetASCIStarINJewelryItem(row.CompInventoryID);
                    if (jewelryItem != null && ASCIStarMetalType.IsGold(jewelryItem.MetalType))
                    {
                        var preciousMetalItem = GetInventoryItemByInvenctoryCD("24K");
                        salesPrice = GetAPVendorPrice(defaultVendor.VendorID, preciousMetalItem.InventoryID, PXTimeZoneInfo.Now).SalesPrice;
                    }
                    else if (jewelryItem != null && ASCIStarMetalType.IsSilver(jewelryItem.MetalType))
                    {
                        var preciousMetalItem = GetInventoryItemByInvenctoryCD("SSS");
                        salesPrice = GetAPVendorPrice(defaultVendor.VendorID, preciousMetalItem.InventoryID, PXTimeZoneInfo.Now).SalesPrice;
                    }
                }
                e.NewValue = salesPrice;
            }
        }
        #endregion

        #region INKitSpecNonStkDet Events
        protected virtual void _(Events.RowSelected<INKitSpecNonStkDet> e)
        {
            //TODO Clarify with the product owner what the behavior should be for Non-Stock components.
        }
        #endregion

        #endregion

        #region ServiceMethods
        protected virtual void AssignCost(string rollupType, ASCIStarINKitSpecHdrExt rowExt, decimal? value)
        {
            switch (rollupType)
            {
                case ASCIStarCostRollupType.PreciousMetal:
                    rowExt.UsrPreciousMetalCost += value;
                    break;
                case ASCIStarCostRollupType.Fabrication:
                    rowExt.UsrFabricationCost += value;
                    break;
                case ASCIStarCostRollupType.Packaging:
                    rowExt.UsrPackagingCost += value;
                    break;
                case ASCIStarCostRollupType.Labor:
                    rowExt.UsrLaborCost += value;
                    break;
                case ASCIStarCostRollupType.Materials:
                    rowExt.UsrMaterialCost += value;
                    break;
                case ASCIStarCostRollupType.Blank:
                    rowExt.UsrOtherCost += value;
                    break;
                case ASCIStarCostRollupType.Freight:
                    rowExt.UsrFreightCost += value;
                    break;
                case ASCIStarCostRollupType.Handling:
                    rowExt.UsrHandlingCost += value;
                    break;
                case ASCIStarCostRollupType.Duty:
                    rowExt.UsrDutyCost += value;
                    break;
                default:
                    break;
            }
        }
        protected virtual void CalculateAndAssignSilverGrams(ASCIStarINKitSpecHdrExt specHdrExt, INKitSpecStkDet line, ASCIStarINInventoryItemExt inventoryItemExt)
        {
            specHdrExt.UsrSilverGrams += inventoryItemExt.UsrActualGRAMSilver * line.DfltCompQty;
            specHdrExt.UsrFineSilverGrams += inventoryItemExt.UsrPricingGRAMSilver * line.DfltCompQty;
            specHdrExt.UsrIncrement += inventoryItemExt.UsrContractIncrement * line.DfltCompQty;
        }
        protected virtual void CalculateAndAssignGoldGrams(ASCIStarINKitSpecHdrExt specHdrExt, decimal? quantity, ASCIStarINInventoryItemExt inventoryItemExt)
        {
            specHdrExt.UsrGoldGrams += inventoryItemExt.UsrActualGRAMGold * quantity;
            specHdrExt.UsrFineGoldGrams += inventoryItemExt.UsrPricingGRAMGold * quantity;
            specHdrExt.UsrIncrement += inventoryItemExt.UsrContractIncrement * quantity;
        }
        protected virtual void CalculateAndAssignItemCosts(ASCIStarINKitSpecHdrExt specHdrExt, decimal? quantity, ASCIStarINInventoryItemExt inventoryItemExt)
        {
            specHdrExt.UsrPreciousMetalCost += inventoryItemExt.UsrCommodityCost * quantity;
            specHdrExt.UsrFabricationCost += inventoryItemExt.UsrFabricationCost * quantity;
            specHdrExt.UsrPackagingCost += inventoryItemExt.UsrPackagingCost * quantity;
            specHdrExt.UsrMaterialCost += inventoryItemExt.UsrMaterialsCost * quantity;
            specHdrExt.UsrLaborCost += inventoryItemExt.UsrLaborCost * quantity;
            specHdrExt.UsrFreightCost += inventoryItemExt.UsrFreightCost * quantity;
            specHdrExt.UsrHandlingCost += inventoryItemExt.UsrHandlingCost * quantity;
            specHdrExt.UsrDutyCost += inventoryItemExt.UsrHandlingCost * quantity;
        }
        protected virtual decimal? GetQuantity(INKitSpecStkDet line)
        {
            return line.DfltCompQty == 0 ? 1 : line.DfltCompQty;
        }
        protected virtual void SetZeroForCustomFields(ASCIStarINKitSpecHdrExt specHdrExt)
        {
            specHdrExt.UsrPreciousMetalCost = 0m;
            specHdrExt.UsrFabricationCost = 0m;
            specHdrExt.UsrPackagingCost = 0m;
            specHdrExt.UsrMaterialCost = 0m;
            specHdrExt.UsrGoldGrams = 0m;
            specHdrExt.UsrFineGoldGrams = 0m;
            specHdrExt.UsrSilverGrams = 0m;
            specHdrExt.UsrFineSilverGrams = 0m;
            specHdrExt.UsrIncrement = 0m;
            specHdrExt.UsrLaborCost = 0m;
            specHdrExt.UsrMetalLossPct = 0m;
            specHdrExt.UsrSurchargePct = 0m;
            specHdrExt.UsrFreightCost = 0m;
        }
        protected virtual void CopyJewelryItemFields(INKitSpecHdr kitSpecHdr)
        {
            var jewelItem = SelectFrom<ASCIStarINJewelryItem>.Where<ASCIStarINJewelryItem.inventoryID.IsEqual<PX.Data.BQL.P.AsInt>>.View.Select(this.Base, kitSpecHdr.KitInventoryID)?.TopFirst;

            if (jewelItem == null) return;

            ASCIStarINKitSpecJewelryItem jewelryKitItem = new ASCIStarINKitSpecJewelryItem()
            {
                KitInventoryID = kitSpecHdr.KitInventoryID,
                RevisionID = kitSpecHdr.RevisionID,
                ShortDesc = jewelItem.ShortDesc,
                LongDesc = jewelItem.LongDesc,
                StyleStatus = jewelItem.StyleStatus,
                CustomerCode = jewelItem.CustomerCode,
                InvCategory = jewelItem?.InvCategory,
                ItemType = jewelItem?.ItemType,
                ItemSubType = jewelItem?.ItemSubType,
                Collection = jewelItem.Collection,
                MetalType = jewelItem?.MetalType,
                MetalNote = jewelItem.MetalNote,
                MetalColor = jewelItem?.MetalColor,
                Plating = jewelItem.Plating,
                Finishes = jewelItem.Finishes,
                VendorMaker = jewelItem.VendorMaker,
                OrgCountry = jewelItem?.OrgCountry,
                StoneType = jewelItem.StoneType,
                WebNotesComment = jewelItem.WebNotesComment,
                StoneComment = jewelItem.StoneComment,
                StoneColor = jewelItem?.StoneColor,
                StoneShape = jewelItem.StoneShape,
                StoneCreation = jewelItem.StoneCreation,
                GemstoneTreatment = jewelItem.GemstoneTreatment,
                SettingType = jewelItem.SettingType,
                Findings = jewelItem.Findings,
                FindingsSubType = jewelItem.FindingsSubType,
                ChainType = jewelItem.ChainType,
                RingLength = jewelItem.RingLength,
                RingSize = jewelItem.RingSize,
                OD = jewelItem.OD,
            };

            this.JewelryItemView.Insert(jewelryKitItem);
        }
        protected virtual void SetVisibleRevisionID()
        {
            var inSetup = SelectFrom<INSetup>.View.Select(this.Base)?.TopFirst;
            var inSetupExt = inSetup?.GetExtension<ASCIStarINSetupExt>();
            PXUIFieldAttribute.SetVisible<INKitSpecHdr.revisionID>(this.Base.Hdr.Cache, this.Base.Hdr.Current, inSetupExt?.UsrIsPDSTenant == true);
        }
        protected virtual void DisableRollupType(PXCache cache, INKitSpecStkDet line)
        {
            PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecStkDetExt.usrCostRollupType>(cache, line, false);
        }
        protected virtual void SetEnabledMetalWeightFields(Events.RowSelected<INKitSpecHdr> e, INKitSpecHdr row)
        {
            var jewelryItem = GetASCIStarINJewelryItem(row.KitInventoryID);
            var result = ASCIStarMetalType.GetMetalType(jewelryItem?.MetalType);
            if (result == true)
            {
                PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecHdrExt.usrGoldGrams>(e.Cache, row, true);
                PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecHdrExt.usrFineGoldGrams>(e.Cache, row, true);
                PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecHdrExt.usrMatrixStep>(e.Cache, row, false);
                PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecHdrExt.usrSilverGrams>(e.Cache, row, false);
                PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecHdrExt.usrFineSilverGrams>(e.Cache, row, false);
            }
            else if (result == false)
            {
                PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecHdrExt.usrGoldGrams>(e.Cache, row, false);
                PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecHdrExt.usrFineGoldGrams>(e.Cache, row, false);
                PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecHdrExt.usrMatrixStep>(e.Cache, row, true);
                PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecHdrExt.usrSilverGrams>(e.Cache, row, true);
                PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecHdrExt.usrFineSilverGrams>(e.Cache, row, true);
            }
        }
        #endregion

        #region ServiceQueries
        private ASCIStarINJewelryItem GetASCIStarINJewelryItem(int? inventoryID)
        {
            return PXSelect<
                ASCIStarINJewelryItem, 
                Where<ASCIStarINJewelryItem.inventoryID, Equal<Required<ASCIStarINJewelryItem.inventoryID>>>>
                .Select(Base, inventoryID);
        }
        private InventoryItem GetInventoryItemByInvenctoryCD(string inventiryCD)
           => SelectFrom<InventoryItem>.Where<InventoryItem.inventoryCD.IsEqual<P.AsString>>.View.Select(Base, inventiryCD)?.TopFirst;
        private APVendorPrice GetAPVendorPrice(int? vendorID, int? inventoryID, DateTime effectiveDate)
            => new PXSelect<APVendorPrice,
                Where<APVendorPrice.vendorID, Equal<Required<APVendorPrice.vendorID>>,
                    And<APVendorPrice.inventoryID, Equal<Required<APVendorPrice.inventoryID>>,
                    And<APVendorPrice.uOM, Equal<Required<APVendorPrice.uOM>>,
                    And<APVendorPrice.effectiveDate, LessEqual<Required<APVendorPrice.effectiveDate>>,
                    And<APVendorPrice.expirationDate, GreaterEqual<Required<APVendorPrice.effectiveDate>>>>>>>,
                OrderBy<Desc<APVendorPrice.effectiveDate>>>(Base).SelectSingle(vendorID, inventoryID, "TOZ", effectiveDate, effectiveDate);
        #endregion
    }
}