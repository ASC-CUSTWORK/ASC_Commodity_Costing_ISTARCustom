using ASCJewelryLibrary.AP.CacheExt;
using ASCJewelryLibrary.Common.Builder;
using ASCJewelryLibrary.Common.Descriptor;
using ASCJewelryLibrary.Common.Helper;
using ASCJewelryLibrary.Common.Helper.Extensions;
using ASCJewelryLibrary.Common.Services.DataProvider.Interfaces;
using ASCJewelryLibrary.IN.CacheExt;
using ASCJewelryLibrary.IN.DAC;
using ASCJewelryLibrary.IN.Descriptor.Constants;
using ASCJewelryLibrary.INKit.CacheExt;
using ASCJewelryLibrary.INKit.DAC;
using ASCJewelryLibrary.INKit.Descriptor;
using ASCJewelryLibrary.PO.CacheExt;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.EP;
using PX.Objects.AP;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.SM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ASCJewelryLibrary.INKit
{
    public class ASCJINKitSpecMaintExt : PXGraphExtension<INKitSpecMaint>
    {
        private const decimal One_Gram = 1m;

        public static bool IsActive() => true;

        #region DataView
        [PXCopyPasteHiddenView]
        public PXSelect<INKitSpecHdr, Where<INKitSpecHdr.kitInventoryID, Equal<Optional<INKitSpecHdr.kitInventoryID>>>> ASCJHdr;

        [PXCopyPasteHiddenView]
        public PXSelect<POVendorInventory, Where<POVendorInventory.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>> ASCJVendorItems;

        public SelectFrom<ASCJINKitSpecJewelryItem>
                 .Where<ASCJINKitSpecJewelryItem.kitInventoryID.IsEqual<INKitSpecHdr.kitInventoryID.FromCurrent>
                    .And<ASCJINKitSpecJewelryItem.revisionID.IsEqual<INKitSpecHdr.revisionID.FromCurrent>>>
                        .View ASCJJewelryItemView;


        [PXCopyPasteHiddenView]
        public PXSetup<INSetup> ASCJINSetup;

        [PXCopyPasteHiddenView]
        public PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>> ASCJInventoryItem;

        [PXCopyPasteHiddenView]
        public PXSelect<ASCJINJewelryItem, Where<ASCJINJewelryItem.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>> ASCJJewelryItem;

        [PXCopyPasteHiddenView]
        public FbqlSelect<SelectFromBase<InventoryItemCurySettings,
            TypeArrayOf<IFbqlJoin>.Empty>.Where<BqlChainableConditionBase<TypeArrayOf<IBqlBinary>.FilledWith<And<Compare<InventoryItemCurySettings.inventoryID,
                Equal<P.AsInt>>>>>.And<BqlOperand<InventoryItemCurySettings.curyID, IBqlString>.IsEqual<BqlField<AccessInfo.baseCuryID, IBqlString>.AsOptional>>>,
            InventoryItemCurySettings>.View ASCJItemCurySettings;

        [PXCopyPasteHiddenView]
        public FbqlSelect<SelectFromBase<InventoryItemCurySettings,
            TypeArrayOf<IFbqlJoin>.Empty>.Where<BqlOperand<InventoryItemCurySettings.inventoryID, IBqlInt>.IsEqual<P.AsInt>>,
            InventoryItemCurySettings>.View ASCJAllItemCurySettings;
        #endregion

        #region Dependency Injection
        [InjectDependency]
        public IASCJInventoryItemDataProvider _itemDataProvider { get; set; }

        [InjectDependency]
        public IASCJVendorDataProvider _vendorDataProvider { get; set; }
        #endregion

        #region Overrides
        public override void Initialize()
        {
            base.Initialize();
            var setup = ASCJINSetup.Current;
            if (setup != null)
            {
                var setupExt = PXCache<INSetup>.GetExtension<ASCJINSetupExt>(setup);
                ASCJCreateProdItem.SetVisible(!setupExt.UsrASCJIsActiveKitVersion ?? false);
                ASCJCreateProdItem.SetEnabled(!setupExt.UsrASCJIsActiveKitVersion ?? false);
            }
        }

        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            CopyFieldsValueToStockItem(Base.Hdr.Current);
            CopyFieldsValueToPOVendorInventory(Base.Hdr.Current);

            var setup = ASCJINSetup.Current;
            if (setup != null)
            {
                var setupExt = PXCache<INSetup>.GetExtension<ASCJINSetupExt>(setup);
                if (setupExt.UsrASCJIsActiveKitVersion == false) // Has to be specified on IN101000 screen
                {
                    CopyJewelryItemFieldsToStockItem(Base.Hdr.Current);
                }
            }
            baseMethod();
        }
        #endregion

        #region CacheAttached

        [PXRemoveBaseAttribute(typeof(PXDBStringAttribute))]
        [PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">##")]
        [PXDefault("01")]
        [PXUIField(DisplayName = "Variant")]
        protected void _(Events.CacheAttached<INKitSpecHdr.revisionID> cacheAttached) { }

        [PXRemoveBaseAttribute(typeof(PXUIFieldAttribute))]
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXUIField(DisplayName = "Default", Enabled = true)]
        protected virtual void _(Events.CacheAttached<POVendorInventory.isDefault> cacheAttached) { }

        [PXRemoveBaseAttribute(typeof(PXDBDefaultAttribute))]
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDBDefault(typeof(INKitSpecHdr.kitInventoryID))]
        protected virtual void _(Events.CacheAttached<POVendorInventory.inventoryID> cacheAttached) { }

        #endregion

        #region Actions
        public PXAction<INKitSpecHdr> ASCJSendEmailToVendor;
        [PXUIField(DisplayName = "Send Email to Vendor", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void aSCJSendEmailToVendor()
        {
            if (this.Base.Hdr.Current == null) return;

            PXLongOperation.StartOperation(this.Base, () =>
            {
                var defaultVendorInventory = GetDefaultPOVendorInventory();
                if (defaultVendorInventory == null)
                    throw new PXException(ASCJINKitMessages.ASCJError.NoDefaultVendor);

                SendEmailNotification(this.Base.Hdr.Current);
            });
        }

        public PXAction<INKitSpecHdr> ASCJCreateProdItem;
        [PXUIField(DisplayName = "Create Production Item", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable aSCIStarCreateProdItem(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXAction<INKitSpecHdr> ASCJUpdateMetalCost;
        [PXUIField(DisplayName = "Update Metal Cost", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(CommitChanges = true)]
        public virtual IEnumerable aSCIStarUpdateMetalCost(PXAdapter adapter)
        {
            var currentHdr = Base.Hdr.Current;
            if (currentHdr != null)
            {
                ASCJVendorItems.Select().RowCast<POVendorInventory>().ForEach(row =>
                {

                    // var jewelryItem = GetASCJINJewelryItem(currentHdr.KitInventoryID);
                    if (ASCJJewelryItemView.Current == null)
                        ASCJJewelryItemView.Current = ASCJJewelryItemView.Select()?.TopFirst;

                    var metalType = ASCJJewelryItemView.Current?.MetalType;

                    if (ASCJMetalType.IsGold(metalType))
                    {
                        var item = _itemDataProvider.GetInventoryItemByCD(ASCJConstants.MetalType.Type_24K);
                        SetOrUpdatePreciousMetalCost(row, item, metalType);
                    }
                    else if (ASCJMetalType.IsSilver(metalType))
                    {
                        var item = _itemDataProvider.GetInventoryItemByCD(ASCJConstants.MetalType.Type_SSS);
                        SetOrUpdatePreciousMetalCost(row, item, metalType);
                    }
                });
                Base.Save.PressButton();
            }

            return adapter.Get();
        }
        #endregion

        #region Event Handlers

        #region INKitSpecHdr Events
        protected virtual void _(Events.RowInserted<INKitSpecHdr> e)
        {
            var row = e.Row;
            if (row == null || Base.Hdr.Current == null) return;

            CopyJewelryItemFields(Base.Hdr.Current);
            CopyFieldsValueFromStockItem(this.Base.Hdr.Current);
        }

        protected virtual void _(Events.RowSelected<INKitSpecHdr> e)
        {
            if (e.Row is INKitSpecHdr row)
            {
                var setup = ASCJINSetup.Current;
                if (setup != null)
                {
                    var inSetupExt = setup?.GetExtension<ASCJINSetupExt>();
                    PXUIFieldAttribute.SetVisible<INKitSpecHdr.revisionID>(Base.Hdr.Cache, Base.Hdr.Current, inSetupExt?.UsrASCJIsActiveKitVersion == true);
                }

                SetVisibleItemWeightFields(e.Cache, row);
            }
        }
        //protected virtual void _(Events.FieldSelecting<INKitSpecHdr, ASCJINKitSpecHdrExt.usrASCJUnitCost> e)
        //{
        //    if (e.Row is INKitSpecHdr row)
        //    {
        //        var rowExt = PXCache<INKitSpecHdr>.GetExtension<ASCJINKitSpecHdrExt>(row);

        //        var result = ASCJCostBuilder.CalculateUnitCost(rowExt);
        //        e.ReturnValue = result;
        //    }
        //}
        //protected virtual void _(Events.FieldSelecting<INKitSpecHdr, ASCJINKitSpecHdrExt.usrASCJEstLandedCost> e)
        //{
        //    if (e.Row is INKitSpecHdr row)
        //    {
        //        var rowExt = PXCache<INKitSpecHdr>.GetExtension<ASCJINKitSpecHdrExt>(row);

        //        var result = ASCJCostBuilder.CalculateUnitCost(rowExt) + ASCJCostBuilder.CalculateEstLandedCost(rowExt);
        //        e.ReturnValue = result;
        //    }
        //}

        protected virtual void _(Events.FieldSelecting<INKitSpecHdr, ASCJINKitSpecHdrExt.usrASCJBasisValue> e)
        {
            if (e.Row is INKitSpecHdr row)
            {
                var defaultVendor = ASCJVendorItems.Select().RowCast<POVendorInventory>().FirstOrDefault(_ => _.IsDefault == true);

                if (defaultVendor != null)
                {
                    decimal? value = 0m;
                    // var jewelryItem = GetASCJINJewelryItem(row.KitInventoryID);
                    if (ASCJJewelryItemView.Current == null)
                        ASCJJewelryItemView.Current = ASCJJewelryItemView.Select()?.TopFirst;

                    var metalType = ASCJJewelryItemView.Current?.MetalType;

                    if (ASCJMetalType.IsGold(metalType) || ASCJMetalType.IsSilver(metalType))
                    {
                        var baseItemCd = ASCJMetalType.IsGold(metalType) ? ASCJConstants.MetalType.Type_24K : ASCJConstants.MetalType.Type_SSS;
                        var baseItem = _itemDataProvider.GetInventoryItemByCD(baseItemCd);

                        if (baseItem != null)
                        {
                            var vendorPrice = ASCJCostBuilder.GetAPVendorPrice(Base, defaultVendor.VendorID, baseItem.InventoryID, ASCJConstants.TOZ.value, PXTimeZoneInfo.Now);

                            if (vendorPrice != null)
                            {
                                if (ASCJMetalType.IsGold(metalType))
                                {
                                    value = vendorPrice.SalesPrice;
                                }
                                else
                                {
                                    var roxExt = PXCache<INKitSpecHdr>.GetExtension<ASCJINKitSpecHdrExt>(row);
                                    value = (vendorPrice.SalesPrice + (vendorPrice.SalesPrice + (roxExt.UsrASCJMatrixStep ?? 0.5m))) / 2m;
                                }
                            }
                        }
                    }
                    e.ReturnValue = value;
                }
            }
        }

        protected virtual void _(Events.FieldVerifying<INKitSpecHdr, ASCJINKitSpecHdrExt.usrASCJBasisValue> e)
        {
            if (e.Row is INKitSpecHdr row)
            {
                if (!IsBaseItemsExists())
                {
                    var rowExt = PXCache<INKitSpecHdr>.GetExtension<ASCJINKitSpecHdrExt>(row);
                    e.Cache.RaiseExceptionHandling<ASCJINKitSpecHdrExt.usrASCJBasisValue>(row, rowExt.UsrASCJBasisValue,
                        new PXSetPropertyException(ASCJINKitMessages.ASCJWarning.BaseItemNotSpecifyed, PXErrorLevel.Warning));
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecHdr, ASCJINKitSpecHdrExt.usrASCJContractSurcharge> e)
        {
            var row = e.Row;
            if (row == null) return;

            UpdateInKitStkComponents(row);
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecHdr, ASCJINKitSpecHdrExt.usrASCJContractLossPct> e)
        {
            var row = e.Row;
            if (row == null) return;

            UpdateInKitStkComponents(row);
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecHdr, ASCJINKitSpecHdrExt.usrASCJDutyCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            var rowExt = PXCache<INKitSpecHdr>.GetExtension<ASCJINKitSpecHdrExt>(row);

            if (rowExt.UsrASCJUnitCost == null || rowExt.UsrASCJUnitCost == 0.0m)
            {
                rowExt.UsrASCJDutyCostPct = decimal.Zero;
                return;
            }
            decimal? newCostPctValue = (decimal?)e.NewValue / rowExt.UsrASCJUnitCost * 100.0m;
            if (newCostPctValue == rowExt.UsrASCJDutyCostPct) return;
            rowExt.UsrASCJDutyCostPct = newCostPctValue;
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecHdr, ASCJINKitSpecHdrExt.usrASCJDutyCostPct> e)
        {
            var row = e.Row;
            if (row == null) return;

            ASCJINKitSpecHdrExt rowExt = PXCache<INKitSpecHdr>.GetExtension<ASCJINKitSpecHdrExt>(row);

            decimal? newDutyCostValue = rowExt.UsrASCJUnitCost * (decimal?)e.NewValue / 100.00m;
            if (newDutyCostValue == rowExt.UsrASCJDutyCost) return;
            e.Cache.SetValueExt<ASCJINKitSpecHdrExt.usrASCJDutyCost>(row, newDutyCostValue);
        }

        #endregion

        #region INKitSpecStkDet Events
        protected virtual void _(Events.RowSelected<INKitSpecStkDet> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetVisibleINKitSpecStkDet(e.Cache, row);
        }

        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCJINKitSpecStkDetExt.usrASCJCostingType> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                var jewelryItem = GetASCJINJewelryItem(row.CompInventoryID);
                if (IsCommodityItem(row))
                {
                    e.NewValue = ASCJConstants.CostingType.MarketCost;
                }
                else
                {
                    var inventoryItem = _itemDataProvider.GetInventoryItemByID(row.CompInventoryID);
                    if (inventoryItem != null && jewelryItem != null && (ASCJMetalType.IsGold(jewelryItem.MetalType) || ASCJMetalType.IsSilver(jewelryItem.MetalType)))
                    {

                        var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(inventoryItem);
                        e.NewValue = inventoryItemExt.UsrASCJCostingType;
                    }
                    else
                    {
                        e.NewValue = ASCJConstants.CostingType.StandardCost;
                    }
                }
            }
        }

        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCJINKitSpecStkDetExt.usrASCJUnitCost> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                var jewelryItem = GetASCJINJewelryItem(row.CompInventoryID);
                if (IsCommodityItem(row))
                {
                    e.NewValue = GetUnitCostForCommodityItem(row);

                }
                else
                {
                    var inventoryItem = _itemDataProvider.GetInventoryItemByID(row.CompInventoryID);
                    if (inventoryItem != null && jewelryItem != null && (ASCJMetalType.IsGold(jewelryItem?.MetalType) || ASCJMetalType.IsSilver(jewelryItem?.MetalType)))
                    {
                        //e.NewValue = ASCJCostBuilder.CalculateUnitCost(inventoryItem);
                        var inventoryItemExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(inventoryItem);
                        e.NewValue = inventoryItemExt.UsrASCJPreciousMetalCost;
                    }
                    else
                    {
                        var result = INItemCost.PK.Find(Base, row.CompInventoryID, Base.Accessinfo.BaseCuryID);
                        e.NewValue = result?.AvgCost ?? 0m;
                    }
                }
            }
        }

        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCJINKitSpecStkDetExt.usrASCJSalesPrice> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                decimal? salesPrice = 0m;
                var defaultVendor = GetItemVendor(row);
                if (defaultVendor != null)
                {
                    var rowExt = PXCache<INKitSpecStkDet>.GetExtension<ASCJINKitSpecStkDetExt>(row);
                    if (IsCommodityItem(row))
                    {
                        salesPrice = ASCJCostBuilder.GetAPVendorPrice(Base, defaultVendor.VendorID, row.CompInventoryID, ASCJConstants.TOZ.value, PXTimeZoneInfo.Now)?.SalesPrice;
                    }
                    else
                    {
                        var jewelryCostBuilder = CreateCostBuilder(rowExt, row);
                        if (jewelryCostBuilder == null)
                        {
                            e.NewValue = salesPrice;
                            return;
                        }
                        else
                        {
                            if (rowExt.UsrASCJCostingType == ASCJConstants.CostingType.MarketCost)
                            {
                                salesPrice = jewelryCostBuilder.PreciousMetalMarketCostPerTOZ;
                            }
                            else if (rowExt.UsrASCJCostingType == ASCJConstants.CostingType.ContractCost)
                            {
                                salesPrice = jewelryCostBuilder.PreciousMetalContractCostPerTOZ;
                            }
                        }
                    }
                }
                e.NewValue = salesPrice;
            }
        }

        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCJINKitSpecStkDetExt.usrASCJBasisPrice> e)
        {
            var row = e.Row;
            if (row == null) return;

            var defaultVendor = GetItemVendor(row);

            if (defaultVendor == null) return;
            var defaultVendorExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(defaultVendor);
            e.NewValue = defaultVendorExt?.UsrASCJBasisPrice;
        }

        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCJINKitSpecStkDetExt.usrASCJBasisValue> e)
        {
            var row = e.Row;
            if (row == null) return;

            var defaultVendor = GetItemVendor(row);

            if (defaultVendor == null) return;
            var defaultVendorExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(defaultVendor);
            e.NewValue = defaultVendorExt?.UsrASCJBasisValue;
        }

        protected virtual void _(Events.FieldDefaulting<INKitSpecStkDet, ASCJINKitSpecStkDetExt.usrASCJIsMetal> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                var jewelryItem = GetASCJINJewelryItem(row.CompInventoryID);
                e.NewValue = jewelryItem != null && (ASCJMetalType.IsGold(jewelryItem?.MetalType) || ASCJMetalType.IsSilver(jewelryItem?.MetalType));
            }
        }

        protected virtual void _(Events.FieldVerifying<INKitSpecStkDet, INKitSpecStkDet.compInventoryID> e)
        {
            var row = e.Row;
            if (row == null) return;

            var newValue = (int?)e.NewValue;
            if (newValue == null) return;

            if (ASCJHdr.Current?.KitInventoryID == newValue)
            {
                var invItem = _itemDataProvider.GetInventoryItemByID(ASCJHdr.Current?.KitInventoryID);
                e.Cancel = true;
                throw new PXSetPropertyException(ASCJINKitMessages.ASCJError.CannotCreateItself, invItem.InventoryCD, invItem.InventoryCD);
            }


            if (ASCJJewelryItemView.Current == null)
                ASCJJewelryItemView.Current = ASCJJewelryItemView.Select()?.TopFirst;



        }

        protected virtual void _(Events.FieldUpdated<INKitSpecStkDet, INKitSpecStkDet.compInventoryID> e)
        {
            var row = e.Row;
            if (row == null) return;

            e.Cache.RaiseFieldDefaulting<ASCJINKitSpecStkDetExt.usrASCJCostingType>(row, out object _costType);
            e.Cache.SetValueExt<ASCJINKitSpecStkDetExt.usrASCJCostingType>(row, _costType);

            e.Cache.RaiseFieldDefaulting<ASCJINKitSpecStkDetExt.usrASCJUnitCost>(row, out object _costUnit);
            e.Cache.SetValueExt<ASCJINKitSpecStkDetExt.usrASCJUnitCost>(row, _costUnit);

            e.Cache.RaiseFieldDefaulting<ASCJINKitSpecStkDetExt.usrASCJSalesPrice>(row, out object _salesPrice);
            e.Cache.SetValueExt<ASCJINKitSpecStkDetExt.usrASCJSalesPrice>(row, _salesPrice);

            e.Cache.RaiseFieldDefaulting<ASCJINKitSpecStkDetExt.usrASCJBasisPrice>(row, out object _basisPrice);
            e.Cache.SetValueExt<ASCJINKitSpecStkDetExt.usrASCJSalesPrice>(row, _basisPrice);

            e.Cache.RaiseFieldDefaulting<ASCJINKitSpecStkDetExt.usrASCJBasisValue>(row, out object _basisValue);
            e.Cache.SetValueExt<ASCJINKitSpecStkDetExt.usrASCJSalesPrice>(row, _basisValue);

            if (IsCommodityItem(row))
            {
                DfltGramsForCommodityItemType(e.Cache, row);

                var inKitHdrExt = PXCache<INKitSpecHdr>.GetExtension<ASCJINKitSpecHdrExt>(this.Base.Hdr.Current);
                this.Base.StockDet.Cache.SetValueExt<ASCJINKitSpecStkDetExt.usrASCJContractLossPct>(row, inKitHdrExt.UsrASCJContractLossPct);
                this.Base.StockDet.Cache.SetValueExt<ASCJINKitSpecStkDetExt.usrASCJContractSurcharge>(row, inKitHdrExt.UsrASCJContractSurcharge);
            }

            var newValue = (int?)e.NewValue;
            var inJewelryItemDB = GetASCJINJewelryItem(newValue);
            if (inJewelryItemDB != null
                    && (inJewelryItemDB.MetalType != null && inJewelryItemDB.MetalType != ASCJJewelryItemView.Current?.MetalType
                    && (ASCJMetalType.IsGold(inJewelryItemDB.MetalType) || ASCJMetalType.IsSilver(inJewelryItemDB.MetalType))))
            {
                ASCJJewelryItemView.Current.MetalType = null;
                ASCJJewelryItemView.Update(ASCJJewelryItemView.Current);
            }

            var itemVendor = GetItemVendor(row);
            if (itemVendor != null && !ASCJVendorItems.Select().FirstTableItems.Any(x => x.VendorID == itemVendor.VendorID))
            {
                itemVendor.RecordID = null;
                itemVendor.IsDefault = false;
                itemVendor.InventoryID = row.KitInventoryID;

                var inventoryID = ASCJMetalType.GetBaseInventoryID(this.Base, inJewelryItemDB.MetalType);
                if (inventoryID != null)
                {
                    var apVendorPrice = ASCJCostBuilder.GetAPVendorPrice(this.Base, itemVendor.VendorID, inventoryID, ASCJConstants.TOZ.value, PXTimeZoneInfo.Today);
                    var apVendorPriceExt = PXCache<APVendorPrice>.GetExtension<ASCJAPVendorPriceExt>(apVendorPrice);
                    var newVendorExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(itemVendor);
                    newVendorExt.UsrASCJBasisPrice = apVendorPriceExt.UsrASCJBasisValue;
                }

                ASCJVendorItems.Insert(itemVendor);
            }
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecStkDet, ASCJINKitSpecStkDetExt.usrASCJCostingType> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                var rowExt = PXCache<INKitSpecStkDet>.GetExtension<ASCJINKitSpecStkDetExt>(row);
                if (IsCommodityItem(row))
                {
                    var value = GetUnitCostForCommodityItem(row);
                    e.Cache.SetValueExt<ASCJINKitSpecStkDetExt.usrASCJUnitCost>(row, value);
                }
                else
                {
                    if (rowExt.UsrASCJCostingType == ASCJConstants.CostingType.StandardCost)
                    {
                        var result = INItemCost.PK.Find(Base, row.CompInventoryID, Base.Accessinfo.BaseCuryID);
                        e.Cache.SetValueExt<ASCJINKitSpecStkDetExt.usrASCJUnitCost>(row, result?.AvgCost ?? 0m);
                    }
                    else if (rowExt.UsrASCJCostingType == ASCJConstants.CostingType.MarketCost || rowExt.UsrASCJCostingType == ASCJConstants.CostingType.ContractCost)
                    {


                        UpdateVendorPrice(e, row, rowExt);
                    }
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecStkDet, ASCJINKitSpecStkDetExt.usrASCJCostRollupType> e)
        {
            var row = e.Row;
            if (row == null) return;

            if (!IsCommodityItem(row))
            {
                UpdateTotalSurchargeAndLoss();
            }
        }

        protected virtual void _(Events.FieldUpdated<INKitSpecStkDet, ASCJINKitSpecStkDetExt.usrASCJExtCost> e)
        {
            var row = e.Row;
            if (row == null) return;

            if (!IsCommodityItem(row))
            {
                UpdateTotalSurchargeAndLoss();
            }
        }

        protected virtual void _(Events.RowPersisting<INKitSpecStkDet> e)
        {
            if (e.Row is INKitSpecStkDet row)
            {
                var rowExt = PXCache<INKitSpecStkDet>.GetExtension<ASCJINKitSpecStkDetExt>(row);
                if (rowExt.UsrASCJCostRollupType == null)
                {
                    e.Cache.RaiseExceptionHandling<ASCJINKitSpecStkDetExt.usrASCJCostRollupType>(row, rowExt.UsrASCJCostRollupType,
                        new PXSetPropertyException(ASCJINKitMessages.ASCJError.CostRollupTypeNotSet, PXErrorLevel.Error));
                    e.Cancel = true;
                    throw new PXException(ASCJINKitMessages.ASCJError.CostRollupTypeNotSet);
                }
            }
        }

        #endregion

        #region INKitSpecNonStkDet Events
        protected virtual void _(Events.FieldDefaulting<INKitSpecNonStkDet, ASCJINKitSpecNonStkDetExt.usrASCJUnitCost> e)
        {
            if (e.Row is INKitSpecNonStkDet row)
            {
                var result = InventoryItemCurySettings.PK.Find(Base, row.CompInventoryID, Base.Accessinfo.BaseCuryID);
                e.NewValue = result?.StdCost ?? 0m;
            }
        }

        protected virtual void _(Events.RowPersisting<INKitSpecNonStkDet> e)
        {
            if (e.Row is INKitSpecNonStkDet row)
            {
                var rowExt = PXCache<INKitSpecNonStkDet>.GetExtension<ASCJINKitSpecNonStkDetExt>(row);
                if (rowExt.UsrASCJCostRollupType == null)
                {
                    e.Cache.RaiseExceptionHandling<ASCJINKitSpecNonStkDetExt.usrASCJCostRollupType>(row, rowExt.UsrASCJCostRollupType,
                        new PXSetPropertyException(ASCJINKitMessages.ASCJError.CostRollupTypeNotSet, PXErrorLevel.Error));
                    e.Cancel = true;
                    throw new PXException(ASCJINKitMessages.ASCJError.CostRollupTypeNotSet);
                }
            }
        }
        #endregion

        #region POVendorInventory Events

        protected virtual void _(Events.RowSelected<POVendorInventory> e)
        {
            var row = e.Row;
            if (row == null) return;

            SetReadOnlyPOVendorInventoryFields(e.Cache, row);
            SetVisiblePOVendorInventoryFields(e.Cache, row);
        }

        protected virtual void _(Events.FieldVerifying<POVendorInventory, POVendorInventory.isDefault> e)
        {
            var row = e.Row;
            if (row == null || (bool)e.NewValue != true) return;

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(row);
            if (rowExt.UsrASCJMarketID == null)
            {
                e.Cache.RaiseExceptionHandling<ASCJPOVendorInventoryExt.usrASCJMarketID>(e.Row, false, new PXSetPropertyException(ASCJINConstants.ASCJErrors.MarketEmpty, PXErrorLevel.RowError));
            }

            var inventoryID = ASCJMetalType.GetBaseInventoryID(this.Base, this.ASCJJewelryItemView.Current?.MetalType);

            var apVendorPrice = ASCJCostBuilder.GetAPVendorPrice(this.Base, row.VendorID, inventoryID, ASCJConstants.TOZ.value, PXTimeZoneInfo.Today);

            if (apVendorPrice == null && PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(row).UsrASCJIsOverrideVendor != true)
            {
                e.Cache.RaiseExceptionHandling<POVendorInventory.isDefault>(row, false,
                    new PXSetPropertyException(ASCJMessages.ASCJError.VendorPriceNotFound, PXErrorLevel.RowWarning));
            }

            List<POVendorInventory> selectPOVendors = ASCJVendorItems.Select()?.FirstTableItems.ToList();
            foreach (var vendorInventory in selectPOVendors)
            {
                if (vendorInventory.IsDefault == true && vendorInventory != row)
                {
                    this.ASCJVendorItems.Cache.SetValue<POVendorInventory.isDefault>(vendorInventory, false);
                    this.ASCJVendorItems.View.RequestRefresh();
                    break;
                }
            }
        }

        protected virtual void _(Events.FieldVerifying<POVendorInventory, ASCJPOVendorInventoryExt.usrASCJBasisPrice> e)
        {
            var row = e.Row;
            if (row == null) return;

            if ((decimal?)e.NewValue == decimal.Zero)
            {
                e.Cache.RaiseExceptionHandling<ASCJPOVendorInventoryExt.usrASCJBasisPrice>(row, e.NewValue,
                    new PXSetPropertyException(ASCJINConstants.ASCJWarnings.BasisOrMarketPriceEmpty, PXErrorLevel.Warning));
            }
        }

        protected virtual void _(Events.FieldUpdated<POVendorInventory, ASCJPOVendorInventoryExt.usrASCJCommodityID> e)
        {
            var row = e.Row;
            if (row == null || !e.ExternalCall) return;
            var rowExt = row.GetExtension<ASCJPOVendorInventoryExt>();
            var inventoryID = (int?)e.NewValue;
            var apVendorPrice = ASCJCostBuilder.GetAPVendorPrice(this.Base, rowExt?.UsrASCJMarketID, inventoryID, ASCJConstants.TOZ.value, PXTimeZoneInfo.Today);
            var apVendorPriceExt = PXCache<APVendorPrice>.GetExtension<ASCJAPVendorPriceExt>(apVendorPrice);
            e.Cache.SetValueExt<ASCJPOVendorInventoryExt.usrASCJBasisValue>(row, apVendorPriceExt.UsrASCJBasisValue);
        }

        protected virtual void _(Events.RowPersisting<POVendorInventory> e)
        {
            if (e.Row is POVendorInventory row)
            {
                if (e.Row.IsDefault == true)
                {
                    var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(e.Row);
                    if (rowExt.UsrASCJMarketID == null)
                    {
                        e.Cache.RaiseExceptionHandling<ASCJPOVendorInventoryExt.usrASCJMarketID>(row, row.IsDefault, new PXSetPropertyException(ASCJINKitMessages.ASCJError.MarketNotFound, PXErrorLevel.Error));
                        e.Cancel = true;
                        throw new PXException(ASCJINKitMessages.ASCJError.MarketNotFound);
                    }
                }
            }
        }

        protected virtual void _(Events.RowUpdated<POVendorInventory> e)
        {
            if (e.OldRow == null || e.Row == null || !e.Row.VendorID.HasValue)
            {
                return;
            }

            var itemCorySettings = ASCJAllItemCurySettings.Select(e.Row.InventoryID).RowCast<InventoryItemCurySettings>();
            var vendor = Vendor.PK.Find(Base, e.Row.VendorID);
            bool flag = false;
            foreach (var item in itemCorySettings)
            {
                var prefferedVendor = Vendor.PK.Find(Base, item.PreferredVendorID);
                if (vendor.BaseCuryID == null || string.Equals(item.CuryID, vendor.BaseCuryID, StringComparison.OrdinalIgnoreCase))
                {
                    var conditionResult = (e.Row.IsDefault == true && (item.PreferredVendorID != e.Row.VendorID || item.PreferredVendorLocationID != e.Row.VendorLocationID)) ||
                                          (e.Row.IsDefault != true && item.PreferredVendorID == e.Row.VendorID && item.PreferredVendorLocationID == e.Row.VendorLocationID);
                    if (conditionResult)
                    {
                        item.PreferredVendorID = ((e.Row.IsDefault == true) ? e.Row.VendorID : null);
                        item.PreferredVendorLocationID = ((e.Row.IsDefault == true) ? e.Row.VendorLocationID : null);
                        ASCJItemCurySettings.Update(item);
                        flag = true;
                    }
                }
                else if (prefferedVendor != null && prefferedVendor.BaseCuryID == null)
                {
                    item.PreferredVendorID = null;
                    item.PreferredVendorLocationID = null;
                    ASCJItemCurySettings.Update(item);
                }
            }

            if (!(e.Row.IsDefault == true && flag))
            {
                return;
            }

            foreach (var currentRow in ASCJVendorItems.Select().RowCast<POVendorInventory>())
            {
                if (currentRow.RecordID != e.Row.RecordID && currentRow.IsDefault == true)
                {
                    ASCJVendorItems.Cache.SetValue<POVendorInventory.isDefault>(currentRow, false);
                }
            }

            ASCJVendorItems.Cache.ClearQueryCacheObsolete();
            ASCJVendorItems.View.RequestRefresh();

            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(e.Row);
            SetBasisValueOnStockComp(rowExt);
        }
        #endregion

        #endregion

        #region ServiceMethods
        protected virtual void SetReadOnlyPOVendorInventoryFields(PXCache cache, POVendorInventory row)
        {
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrASCJContractIncrement>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrASCJContractLossPct>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrASCJContractSurcharge>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrASCJPreciousMetalCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrASCJOtherMaterialsCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrASCJFabricationCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrASCJPackagingCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrASCJLaborCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrASCJPackagingLaborCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrASCJHandlingCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrASCJFreightCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrASCJDutyCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrASCJUnitCost>(cache, row, true);
            PXUIFieldAttribute.SetReadOnly<ASCJPOVendorInventoryExt.usrASCJMatrixStep>(cache, row, true);
        }

        protected virtual void SetVisiblePOVendorInventoryFields(PXCache cache, POVendorInventory row)
        {
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrASCJContractIncrement>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrASCJContractLossPct>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrASCJContractSurcharge>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrASCJPreciousMetalCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrASCJOtherMaterialsCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrASCJFabricationCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrASCJPackagingCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrASCJLaborCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrASCJPackagingLaborCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrASCJHandlingCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrASCJFreightCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrASCJDutyCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrASCJUnitCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrASCJMatrixStep>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrASCJEstLandedCost>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrASCJCeiling>(cache, null, false);
            PXUIFieldAttribute.SetVisible<ASCJPOVendorInventoryExt.usrASCJFloor>(cache, null, false);
        }

        protected virtual void SetVisibleItemWeightFields(PXCache cache, INKitSpecHdr row)
        {
            if (ASCJJewelryItemView.Current == null)
                ASCJJewelryItemView.Current = ASCJJewelryItemView.Select()?.TopFirst;

            var mixedType = ASCJMetalType.GetMixedTypeValue(ASCJJewelryItemView.Current?.MetalType);
            bool isVisibleMixed = mixedType == ASCJConstants.MixedMetalType.Type_MixedDefault ||
                ASCJJewelryItemView.Current?.MetalType == null;
            PXUIFieldAttribute.SetVisible<ASCJINKitSpecHdrExt.usrASCJActualGRAMSilverRight>(cache, row, isVisibleMixed);
            PXUIFieldAttribute.SetVisible<ASCJINKitSpecHdrExt.usrASCJPricingGRAMSilverRight>(cache, row, isVisibleMixed);

            bool isVisibleGold = mixedType == ASCJConstants.MixedMetalType.Type_MixedGold ||
                ASCJMetalType.IsGold(ASCJJewelryItemView.Current?.MetalType);
            PXUIFieldAttribute.SetVisible<ASCJINKitSpecHdrExt.usrASCJActualGRAMGold>(cache, row, isVisibleMixed || isVisibleGold);
            PXUIFieldAttribute.SetVisible<ASCJINKitSpecHdrExt.usrASCJPricingGRAMGold>(cache, row, isVisibleMixed || isVisibleGold);
            PXUIFieldAttribute.SetVisible<ASCJINKitSpecHdrExt.usrASCJContractSurcharge>(cache, row, isVisibleMixed || isVisibleGold);

            bool isVisibleSilver = ASCJMetalType.IsSilver(ASCJJewelryItemView.Current?.MetalType);
            PXUIFieldAttribute.SetVisible<ASCJINKitSpecHdrExt.usrASCJActualGRAMSilver>(cache, row, isVisibleSilver);
            PXUIFieldAttribute.SetVisible<ASCJINKitSpecHdrExt.usrASCJPricingGRAMSilver>(cache, row, isVisibleSilver);
            PXUIFieldAttribute.SetVisible<ASCJINKitSpecHdrExt.usrASCJMatrixStep>(cache, row, isVisibleMixed || isVisibleSilver);
        }

        protected virtual void SetVisibleINKitSpecStkDet(PXCache cache, INKitSpecStkDet row)
        {
            //if (JewelryItemView.Current == null)
            //    JewelryItemView.Current = JewelryItemView.Select()?.TopFirst;

            //var mixedType = ASCJMetalType.GetMixedTypeValue(JewelryItemView.Current?.MetalType);
            //bool isVisibleMixed = mixedType == ASCJConstants.MixedMetalType.Type_MixedDefault ||
            //    JewelryItemView.Current?.MetalType == null;

            //bool isVisibleGold = mixedType == ASCJConstants.MixedMetalType.Type_MixedGold ||
            //    ASCJMetalType.IsGold(JewelryItemView.Current?.MetalType);
            //PXUIFieldAttribute.SetVisible<ASCJINKitSpecStkDetExt.usrASCJActualGRAMGold>(cache, null, isVisibleMixed || isVisibleGold);
            //PXUIFieldAttribute.SetVisible<ASCJINKitSpecStkDetExt.usrASCJPricingGRAMGold>(cache, null, isVisibleMixed || isVisibleGold);

            //bool isVisibleSilver = ASCJMetalType.IsSilver(JewelryItemView.Current?.MetalType);
            //PXUIFieldAttribute.SetVisible<ASCJINKitSpecStkDetExt.usrASCJActualGRAMSilver>(cache, null, isVisibleMixed || isVisibleSilver);
            //PXUIFieldAttribute.SetVisible<ASCJINKitSpecStkDetExt.usrASCJPricingGRAMSilver>(cache, null, isVisibleMixed || isVisibleSilver);
            //PXUIFieldAttribute.SetVisible<ASCJINKitSpecStkDetExt.usrASCJMatrixStep>(cache, null, isVisibleMixed || isVisibleSilver);
        }

        protected virtual void CopyJewelryItemFields(INKitSpecHdr kitSpecHdr)
        {
            var jewelItem = SelectFrom<ASCJINJewelryItem>.Where<ASCJINJewelryItem.inventoryID.IsEqual<PX.Data.BQL.P.AsInt>>.View.Select(Base, kitSpecHdr?.KitInventoryID)?.TopFirst;

            if (jewelItem == null) return;

            ASCJINKitSpecJewelryItem jewelryKitItem = new ASCJINKitSpecJewelryItem()
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

            ASCJJewelryItemView.Insert(jewelryKitItem);
        }

        protected virtual void CopyJewelryItemFieldsToStockItem(INKitSpecHdr kitSpecHdr)
        {
            var jewelItem = SelectFrom<ASCJINJewelryItem>.Where<ASCJINJewelryItem.inventoryID.IsEqual<PX.Data.BQL.P.AsInt>>.View.Select(Base, kitSpecHdr?.KitInventoryID)?.TopFirst;

            if (jewelItem == null) return;

            jewelItem.ShortDesc = ASCJJewelryItemView.Current?.ShortDesc;
            jewelItem.LongDesc = ASCJJewelryItemView.Current?.LongDesc;
            jewelItem.StyleStatus = ASCJJewelryItemView.Current?.StyleStatus;
            jewelItem.CustomerCode = ASCJJewelryItemView.Current?.CustomerCode;
            jewelItem.InvCategory = ASCJJewelryItemView.Current?.InvCategory;
            jewelItem.ItemType = ASCJJewelryItemView.Current?.ItemType;
            jewelItem.ItemSubType = ASCJJewelryItemView.Current?.ItemSubType;
            jewelItem.Collection = ASCJJewelryItemView.Current?.Collection;
            jewelItem.MetalType = ASCJJewelryItemView.Current?.MetalType;
            jewelItem.MetalNote = ASCJJewelryItemView.Current?.MetalNote;
            jewelItem.MetalColor = ASCJJewelryItemView.Current?.MetalColor;
            jewelItem.Plating = ASCJJewelryItemView.Current?.Plating;
            jewelItem.Finishes = ASCJJewelryItemView.Current?.Finishes;
            jewelItem.VendorMaker = ASCJJewelryItemView.Current?.VendorMaker;
            jewelItem.OrgCountry = ASCJJewelryItemView.Current?.OrgCountry;
            jewelItem.StoneType = ASCJJewelryItemView.Current?.StoneType;
            jewelItem.WebNotesComment = ASCJJewelryItemView.Current?.WebNotesComment;
            jewelItem.StoneComment = ASCJJewelryItemView.Current?.StoneComment;
            jewelItem.StoneColor = ASCJJewelryItemView.Current?.StoneColor;
            jewelItem.StoneShape = ASCJJewelryItemView.Current?.StoneShape;
            jewelItem.StoneCreation = ASCJJewelryItemView.Current?.StoneCreation;
            jewelItem.GemstoneTreatment = ASCJJewelryItemView.Current?.GemstoneTreatment;
            jewelItem.SettingType = ASCJJewelryItemView.Current?.SettingType;
            jewelItem.Findings = ASCJJewelryItemView.Current?.Findings;
            jewelItem.FindingsSubType = ASCJJewelryItemView.Current?.FindingsSubType;
            jewelItem.ChainType = ASCJJewelryItemView.Current?.ChainType;
            jewelItem.RingLength = ASCJJewelryItemView.Current?.RingLength;
            jewelItem.RingSize = ASCJJewelryItemView.Current?.RingSize;
            jewelItem.OD = ASCJJewelryItemView.Current?.OD;
            ASCJJewelryItem.Update(jewelItem);
        }

        protected virtual void CopyFieldsValueFromStockItem(INKitSpecHdr kitSpecHdr)
        {
            var item = _itemDataProvider.GetInventoryItemByID(kitSpecHdr?.KitInventoryID);
            if (item != null && kitSpecHdr != null)
            {
                //        kitSpecHdrExt.UsrASCJTotalGoldGrams = itemExt.UsrASCJActualGRAMGold;
                //        kitSpecHdrExt.UsrASCJTotalFineGoldGrams = itemExt.UsrASCJPricingGRAMGold;
                //        kitSpecHdrExt.UsrASCJTotalSilverGrams = itemExt.UsrASCJActualGRAMSilver;
                //        kitSpecHdrExt.UsrASCJTotalFineSilverGrams = itemExt.UsrASCJPricingGRAMSilver;
                //        kitSpecHdrExt.UsrASCJPreciousMetalCost = itemExt.UsrASCJCommodityCost;
                //        kitSpecHdrExt.UsrASCJFabricationCost = itemExt.UsrASCJFabricationCost;
                //        kitSpecHdrExt.UsrASCJOtherCost = itemExt.UsrASCJOtherCost;
                //        kitSpecHdrExt.UsrASCJPackagingCost = itemExt.UsrASCJPackagingCost;
                //        kitSpecHdrExt.UsrASCJLaborCost = itemExt.UsrASCJLaborCost;
                //        kitSpecHdrExt.UsrASCJHandlingCost = itemExt.UsrASCJHandlingCost;
                //        kitSpecHdrExt.UsrASCJFreightCost = itemExt.UsrASCJFreightCost;
                //        kitSpecHdrExt.UsrASCJDutyCost = itemExt.UsrASCJDutyCost;
                //        kitSpecHdrExt.UsrASCJDutyCostPct = itemExt.UsrASCJDutyCostPct;
                //        kitSpecHdrExt.UsrASCJLegacyID = itemExt.UsrASCJLegacyID;
                //        kitSpecHdrExt.UsrASCJLegacyShortRef = itemExt.UsrASCJLegacyShortRef;
                kitSpecHdr.Descr = item.Descr;
                Base.Hdr.Update(kitSpecHdr);
            }
        }

        protected virtual void CopyFieldsValueToStockItem(INKitSpecHdr kitSpecHdr)
        {
            var item = _itemDataProvider.GetInventoryItemByID(kitSpecHdr?.KitInventoryID);
            if (item != null && kitSpecHdr != null)
            {
                var itemExt = PXCache<InventoryItem>.GetExtension<ASCJINInventoryItemExt>(item);
                var kitSpecHdrExt = PXCache<INKitSpecHdr>.GetExtension<ASCJINKitSpecHdrExt>(kitSpecHdr);
                itemExt.UsrASCJActualGRAMGold = kitSpecHdrExt.UsrASCJActualGRAMGold;
                itemExt.UsrASCJPricingGRAMGold = kitSpecHdrExt.UsrASCJPricingGRAMGold;
                itemExt.UsrASCJActualGRAMSilver = kitSpecHdrExt.UsrASCJActualGRAMSilver;
                itemExt.UsrASCJPricingGRAMSilver = kitSpecHdrExt.UsrASCJPricingGRAMSilver;
                itemExt.UsrASCJPreciousMetalCost = kitSpecHdrExt.UsrASCJPreciousMetalCost;
                itemExt.UsrASCJContractLossPct = kitSpecHdrExt.UsrASCJContractLossPct;
                itemExt.UsrASCJContractSurcharge = kitSpecHdrExt.UsrASCJContractSurcharge;
                itemExt.UsrASCJContractIncrement = kitSpecHdrExt.UsrASCJContractIncrement;
                itemExt.UsrASCJFabricationCost = kitSpecHdrExt.UsrASCJFabricationCost;
                itemExt.UsrASCJOtherCost = kitSpecHdrExt.UsrASCJOtherCost;
                itemExt.UsrASCJOtherMaterialsCost = kitSpecHdrExt.UsrASCJOtherMaterialsCost;
                itemExt.UsrASCJPackagingCost = kitSpecHdrExt.UsrASCJPackagingCost;
                itemExt.UsrASCJPackagingLaborCost = kitSpecHdrExt.UsrASCJPackagingLaborCost;
                itemExt.UsrASCJLaborCost = kitSpecHdrExt.UsrASCJLaborCost;
                itemExt.UsrASCJHandlingCost = kitSpecHdrExt.UsrASCJHandlingCost;
                itemExt.UsrASCJFreightCost = kitSpecHdrExt.UsrASCJFreightCost;
                itemExt.UsrASCJDutyCost = kitSpecHdrExt.UsrASCJDutyCost;
                itemExt.UsrASCJDutyCostPct = kitSpecHdrExt.UsrASCJDutyCostPct;
                itemExt.UsrASCJLegacyID = kitSpecHdrExt.UsrASCJLegacyID;
                itemExt.UsrASCJLegacyShortRef = kitSpecHdrExt.UsrASCJLegacyShortRef;
                ASCJInventoryItem.Update(item);
            }
        }

        protected virtual void CopyFieldsValueToPOVendorInventory(INKitSpecHdr kitSpecHdr)
        {
            var poVendorInventory = ASCJVendorItems.Select().RowCast<POVendorInventory>().FirstOrDefault(_ => _.IsDefault == true);
            if (poVendorInventory != null && kitSpecHdr != null)
            {
                var poVendorInventoryExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(poVendorInventory);
                var kitSpecHdrExt = PXCache<INKitSpecHdr>.GetExtension<ASCJINKitSpecHdrExt>(kitSpecHdr);
                poVendorInventoryExt.UsrASCJPreciousMetalCost = kitSpecHdrExt.UsrASCJPreciousMetalCost;
                poVendorInventoryExt.UsrASCJContractLossPct = kitSpecHdrExt.UsrASCJContractLossPct;
                poVendorInventoryExt.UsrASCJContractSurcharge = kitSpecHdrExt.UsrASCJContractSurcharge;
                poVendorInventoryExt.UsrASCJContractIncrement = kitSpecHdrExt.UsrASCJContractIncrement;
                poVendorInventoryExt.UsrASCJFabricationCost = kitSpecHdrExt.UsrASCJFabricationCost;
                poVendorInventoryExt.UsrASCJOtherCost = kitSpecHdrExt.UsrASCJOtherCost;
                poVendorInventoryExt.UsrASCJOtherMaterialsCost = kitSpecHdrExt.UsrASCJOtherMaterialsCost;
                poVendorInventoryExt.UsrASCJPackagingCost = kitSpecHdrExt.UsrASCJPackagingCost;
                poVendorInventoryExt.UsrASCJPackagingLaborCost = kitSpecHdrExt.UsrASCJPackagingLaborCost;
                poVendorInventoryExt.UsrASCJLaborCost = kitSpecHdrExt.UsrASCJLaborCost;
                poVendorInventoryExt.UsrASCJHandlingCost = kitSpecHdrExt.UsrASCJHandlingCost;
                poVendorInventoryExt.UsrASCJFreightCost = kitSpecHdrExt.UsrASCJFreightCost;
                poVendorInventoryExt.UsrASCJDutyCost = kitSpecHdrExt.UsrASCJDutyCost;
                poVendorInventoryExt.UsrASCJDutyCostPct = kitSpecHdrExt.UsrASCJDutyCostPct;
                ASCJVendorItems.Update(poVendorInventory);
            }
        }

        protected virtual ASCJCostBuilder CreateCostBuilder(ASCJINKitSpecStkDetExt currentRow, INKitSpecStkDet row)
        {
            var defaultVendor = GetItemVendor(row);

            if (defaultVendor != null)
            {
                return new ASCJCostBuilder(Base)
                            .WithInventoryItem(currentRow)
                            .WithPOVendorInventory(defaultVendor)
                            .WithPricingData(PXTimeZoneInfo.Today)
                            .Build();
            }

            throw new PXSetPropertyException(ASCJINKitMessages.ASCJError.NoDefaultVendor);
        }

        protected virtual POVendorInventory GetItemVendor(INKitSpecStkDet row) =>
            SelectFrom<POVendorInventory>.
                    Where<POVendorInventory.inventoryID.
                        IsEqual<@P.AsInt>>.View.Select(Base, row.CompInventoryID).FirstTableItems.FirstOrDefault(x => x.IsDefault == true);



        protected virtual void UpdateVendorPrice(Events.FieldUpdated<INKitSpecStkDet, ASCJINKitSpecStkDetExt.usrASCJCostingType> e,
            INKitSpecStkDet row, ASCJINKitSpecStkDetExt rowExt)
        {
            var inventoryItem = _itemDataProvider.GetInventoryItemByID(row.CompInventoryID);
            var jewelryCostBuilder = CreateCostBuilder(rowExt, row);
            if (jewelryCostBuilder == null) return;

            var result = jewelryCostBuilder.CalculatePreciousMetalCost(e.NewValue?.ToString());
            e.Cache.SetValueExt<ASCJINKitSpecStkDetExt.usrASCJUnitCost>(row, result);

            var salesPrice = rowExt.UsrASCJCostingType == ASCJConstants.CostingType.MarketCost ? jewelryCostBuilder.PreciousMetalMarketCostPerTOZ : jewelryCostBuilder.PreciousMetalContractCostPerTOZ;
            e.Cache.SetValueExt<ASCJINKitSpecStkDetExt.usrASCJSalesPrice>(row, salesPrice);
            e.Cache.SetValueExt<ASCJINKitSpecStkDetExt.usrASCJBasisPrice>(row, jewelryCostBuilder.PreciousMetalContractCostPerTOZ);
        }

        protected virtual decimal? GetUnitCostForCommodityItem(INKitSpecStkDet row)
        {
            var value = 0m;
            var defaultVendor = GetItemVendor(row);
            if (defaultVendor == null) return value;

            var defaultVendorExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(defaultVendor);


            if (ASCJJewelryItemView.Current == null)
                ASCJJewelryItemView.Current = ASCJJewelryItemView.Select();

            if (ASCJJewelryItemView.Current == null) return value;

            int? metalInventoryID = null;
            decimal multCoef = 24;

            bool isGold = ASCJMetalType.IsGold(ASCJJewelryItemView.Current.MetalType);
            if (isGold)
            {
                metalInventoryID = _itemDataProvider.GetInventoryItemByCD("24K").InventoryID;
                multCoef = ASCJMetalType.GetGoldTypeValue(ASCJJewelryItemView.Current.MetalType) / 24;
            }

            bool isSilver = ASCJMetalType.IsSilver(ASCJJewelryItemView.Current.MetalType);
            if (isSilver)
            {
                metalInventoryID = _itemDataProvider.GetInventoryItemByCD("SSS").InventoryID;
                multCoef = ASCJMetalType.GetSilverTypeValue(ASCJJewelryItemView.Current.MetalType);
            }

            var vendorPrice = ASCJCostBuilder.GetAPVendorPrice(Base, defaultVendorExt.UsrASCJMarketID, metalInventoryID, ASCJConstants.TOZ.value, PXTimeZoneInfo.Now);

            if (vendorPrice == null) return value;

            var rowExt = PXCache<INKitSpecStkDet>.GetExtension<ASCJINKitSpecStkDetExt>(row);
            var vendorPriceExt = PXCache<APVendorPrice>.GetExtension<ASCJAPVendorPriceExt>(vendorPrice);
            if (row.UOM == ASCJConstants.GRAM.value)
            {
                if (isSilver)
                {
                    var jewelryCostBuilder = CreateCostBuilder(rowExt, row);
                    if (jewelryCostBuilder == null) return value;

                    var tempValue = jewelryCostBuilder.CalculatePreciousMetalCost(jewelryCostBuilder.ItemCostSpecification.UsrASCJCostingType);
                    value = (jewelryCostBuilder.PreciousMetalAvrSilverMarketCostPerTOZ ?? 0.0m) / ASCJConstants.TOZ2GRAM_31_10348.value * multCoef;
                    // return value;
                }
                if (isGold)
                {
                    value = (vendorPriceExt?.UsrASCJCommodityPerGram ?? 0m) * multCoef;
                    //return value;
                }
            }
            else if (row.UOM == ASCJConstants.TOZ.value)
            {
                return vendorPrice?.SalesPrice ?? 0m;
            }
            decimal? surchargeValue = (100.0m + (rowExt.UsrASCJContractSurcharge ?? 0.0m)) / 100.0m;
            decimal? metalLossValue = (100.0m + (rowExt.UsrASCJContractLossPct ?? 0.0m)) / 100.0m;
            return value * surchargeValue * metalLossValue;
        }

        protected virtual bool IsCommodityItem(INKitSpecStkDet row)
        {
            var inventoryItem = _itemDataProvider.GetInventoryItemByID(row.CompInventoryID);
            var itemClass = _itemDataProvider.GetItemClassByID(inventoryItem?.ItemClassID);
            return itemClass?.ItemClassCD.NormalizeCD() == ASCJConstants.CommodityClass.value;
        }

        protected virtual void DfltGramsForCommodityItemType(PXCache cache, INKitSpecStkDet row)
        {
            var jewelryItem = GetASCJINJewelryItem(row.CompInventoryID);

            if (!string.IsNullOrEmpty(jewelryItem?.MetalType))
            {

                if (ASCJMetalType.IsGold(jewelryItem?.MetalType))
                {
                    var multFactor = ASCJMetalType.GetGoldTypeValue(jewelryItem?.MetalType);
                    var fineGrams = (One_Gram * multFactor) / 24;
                    cache.SetValueExt<ASCJINKitSpecStkDetExt.usrASCJBaseGoldGrams>(row, One_Gram);
                    cache.SetValueExt<ASCJINKitSpecStkDetExt.usrASCJBaseFineGoldGrams>(row, fineGrams);
                }
                if (ASCJMetalType.IsSilver(jewelryItem?.MetalType))
                {
                    var multFactor = ASCJMetalType.GetSilverTypeValue(jewelryItem?.MetalType);
                    var fineGrams = One_Gram * multFactor;
                    cache.SetValueExt<ASCJINKitSpecStkDetExt.usrASCJBaseSilverGrams>(row, One_Gram);
                    cache.SetValueExt<ASCJINKitSpecStkDetExt.usrASCJBaseFineSilverGrams>(row, fineGrams);
                }
            }
            else
            {
                cache.RaiseExceptionHandling<INKitSpecStkDet.compInventoryID>(row, row.CompInventoryID,
                    new PXSetPropertyException(ASCJINKitMessages.ASCJWarning.MissingMetalType, PXErrorLevel.RowWarning));
            }
        }

        protected virtual void UpdateInKitStkComponents(INKitSpecHdr inKitSpecHdr)
        {
            var inKitSpecHdrExt = PXCache<INKitSpecHdr>.GetExtension<ASCJINKitSpecHdrExt>(inKitSpecHdr);

            var stkComponets = this.Base.StockDet.Select()?.FirstTableItems.ToList();

            foreach (var stkComponent in stkComponets)
            {
                if (IsCommodityItem(stkComponent))
                {
                    var stkComponentExt = PXCache<INKitSpecStkDet>.GetExtension<ASCJINKitSpecStkDetExt>(stkComponent);

                    if (stkComponentExt.UsrASCJCostRollupType == ASCJConstants.CostRollupType.PreciousMetal)
                    {
                        stkComponentExt.UsrASCJContractLossPct = inKitSpecHdrExt.UsrASCJContractLossPct;
                        stkComponentExt.UsrASCJContractSurcharge = inKitSpecHdrExt.UsrASCJContractSurcharge;

                        decimal? preciousMetalCost = GetUnitCostForCommodityItem(stkComponent);
                        stkComponentExt.UsrASCJUnitCost = preciousMetalCost;
                        this.Base.StockDet.Cache.SetValueExt<ASCJINKitSpecStkDetExt.usrASCJUnitCost>(stkComponent, stkComponentExt.UsrASCJUnitCost);
                        this.Base.StockDet.Update(stkComponent);
                    }
                }
            }
        }

        private void SetBasisValueOnStockComp(ASCJPOVendorInventoryExt rowExt)
        {
            var stockComponets = this.Base.StockDet.Select()?.FirstTableItems.ToList();
            foreach (var stockComponet in stockComponets)
            {
                var stockComponentExt = PXCache<INKitSpecStkDet>.GetExtension<ASCJINKitSpecStkDetExt>(stockComponet);
                if (stockComponentExt.UsrASCJCostRollupType == ASCJConstants.CostRollupType.PreciousMetal)
                {
                    stockComponentExt.UsrASCJBasisValue = rowExt.UsrASCJBasisValue;
                    this.Base.StockDet.Update(stockComponet);
                }
            }
        }

        private void SetOrUpdatePreciousMetalCost(POVendorInventory row, InventoryItem item, string metalType)
        {
            var rowExt = PXCache<POVendorInventory>.GetExtension<ASCJPOVendorInventoryExt>(row);
            var marketID = GetVendorMarketID(row, rowExt);

            var vendorPrice = ASCJCostBuilder.GetAPVendorPrice(Base, marketID, item.InventoryID, ASCJConstants.TOZ.value, PXTimeZoneInfo.Today);
            if (vendorPrice != null)
            {
                var result = vendorPrice.SalesPrice * ASCJMetalType.GetMultFactorConvertTOZtoGram(metalType);
                rowExt.UsrASCJPreciousMetalCost = result;
                ASCJVendorItems.Update(row);
            }
        }

        private void UpdateTotalSurchargeAndLoss()
        {
            if (this.Base.Hdr.Current == null) return;

            var newTotalLoss = GetFieldTotalPersentage<ASCJINKitSpecStkDetExt.usrASCJContractLossPct>(this.Base.StockDet.Cache);
            var newTotalSurcharge = GetFieldTotalPersentage<ASCJINKitSpecStkDetExt.usrASCJContractSurcharge>(this.Base.StockDet.Cache);
            var newIncrement = GetIncrementTotalValue();

            this.Base.Hdr.SetValueExt<ASCJINKitSpecHdrExt.usrASCJContractLossPct>(this.Base.Hdr.Current, newTotalLoss);
            this.Base.Hdr.SetValueExt<ASCJINKitSpecHdrExt.usrASCJContractSurcharge>(this.Base.Hdr.Current, newTotalSurcharge);
            this.Base.Hdr.SetValueExt<ASCJINKitSpecHdrExt.usrASCJContractIncrement>(this.Base.Hdr.Current, newIncrement);
        }

        private decimal? GetFieldTotalPersentage<TField>(PXCache cache) where TField : IBqlField
        {
            List<INKitSpecStkDet> stkLineList = this.Base.StockDet.Select()?.FirstTableItems?
                .Where(x => x.GetExtension<ASCJINKitSpecStkDetExt>().UsrASCJCostRollupType == ASCJConstants.CostRollupType.PreciousMetal).ToList();

            decimal? totalLossAbsValue = stkLineList.Sum(row =>
            {
                var rowExt = row.GetExtension<ASCJINKitSpecStkDetExt>();
                decimal? lineFieldValue = (decimal?)cache.GetValue<TField>(row);
                return rowExt.UsrASCJExtCost * lineFieldValue;
            });

            decimal? totalExtCost = stkLineList.Sum(line => line.GetExtension<ASCJINKitSpecStkDetExt>().UsrASCJExtCost);

            return totalExtCost == 0.0m || totalExtCost == null ? decimal.Zero : totalLossAbsValue / totalExtCost;
        }

        private decimal? GetIncrementTotalValue()
        {
            List<INKitSpecStkDet> stkLineList = this.Base.StockDet.Select()?.FirstTableItems?.ToList();

            decimal? totalPerMetalType = decimal.Zero;
            decimal? totalPerPreciousMetalType = decimal.Zero;
            foreach (var row in stkLineList)
            {
                var rowExt = row.GetExtension<ASCJINKitSpecStkDetExt>();
                string metalType = GetASCJINJewelryItem(row.CompInventoryID)?.MetalType;
                totalPerMetalType += ASCJMetalType.GetMultFactorConvertTOZtoGram(metalType) * (rowExt.UsrASCJActualGRAMGold + rowExt.UsrASCJActualGRAMSilver);
                totalPerPreciousMetalType += ASCJMetalType.GetMultFactorConvertTOZtoGram(ASCJJewelryItemView.Select().TopFirst?.MetalType) * (rowExt.UsrASCJActualGRAMGold + rowExt.UsrASCJActualGRAMSilver); ;
            }


            return totalPerPreciousMetalType == 0.0m || totalPerPreciousMetalType == null
                ? decimal.Zero
                : ASCJMetalType.GetMultFactorConvertTOZtoGram(ASCJJewelryItemView.Select().TopFirst?.MetalType) * totalPerMetalType / totalPerPreciousMetalType;
        }

        private int? GetVendorMarketID(POVendorInventory row, ASCJPOVendorInventoryExt rowExt)
        {
            int? marketID = null;
            if (rowExt.UsrASCJMarketID == null)
            {
                var vendor = _vendorDataProvider.GetVendor(row.VendorID);
                if (vendor != null)
                {
                    marketID = PXCache<Vendor>.GetExtension<ASCJVendorExt>(vendor)?.UsrASCJMarketID;
                }
            }
            else
            {
                marketID = rowExt.UsrASCJMarketID;
            }

            return marketID;
        }

        private bool IsBaseItemsExists()
        {
            return _itemDataProvider.GetInventoryItemByCD(ASCJConstants.MetalType.Type_24K) != null &&
                   _itemDataProvider.GetInventoryItemByCD(ASCJConstants.MetalType.Type_SSS) != null;
        }

        private ASCJINJewelryItem GetASCJINJewelryItem(int? inventoryID) =>
            SelectFrom<ASCJINJewelryItem>.Where<ASCJINJewelryItem.inventoryID.IsEqual<P.AsInt>>.View.Select(Base, inventoryID)?.TopFirst;

        private POVendorInventory GetDefaultPOVendorInventory() => this.ASCJVendorItems.Select()?.FirstTableItems.FirstOrDefault(x => x.IsDefault == true);

        #region Emails Methods
        protected virtual void SendEmailNotification(INKitSpecHdr inKitSpecHdr)
        {

            if (this.ASCJVendorItems.Current == null)
                this.ASCJVendorItems.Current = this.ASCJVendorItems.Select()?.RowCast<POVendorInventory>().FirstOrDefault(x => x.IsDefault == true);
            if (this.ASCJVendorItems.Current == null)
                throw new PXException(ASCJINKitMessages.ASCJError.NoDefaultVendor);

            var bAccount = BAccount.PK.Find(Base, this.ASCJVendorItems.Current.VendorID);

            var inventoryItem = InventoryItem.PK.Find(this.Base, inKitSpecHdr.KitInventoryID);

            var sender = new NotificationGenerator
            {
                To = GetVendorEmail(bAccount),
                Subject = string.Format(ASCJINKitMessages.EMailSubject, inventoryItem?.InventoryCD),
                Body = CreateEmailBody(inventoryItem, bAccount),
                BodyFormat = EmailFormatListAttribute.Html,
            };

            AddAttachmentsToEmail(sender);

            sender.Send();
        }

        private string GetVendorEmail(BAccount bAccount) =>
           SelectFrom<Contact>.Where<Contact.contactID.IsEqual<P.AsInt>>.View.Select(Base, bAccount.PrimaryContactID)?.TopFirst?.EMail;

        private string CreateEmailBody(InventoryItem inventoryItem, BAccount bAccount)
        {
            Location location = SelectFrom<Location>.Where<Location.bAccountID.IsEqual<P.AsInt>>.View.Select(Base, bAccount.BAccountID)?.TopFirst;

            string companyName = PX.Data.PXLogin.ExtractCompany(PX.Common.PXContext.PXIdentity.IdentityName);

            string returnBodyString = string.Format(ASCJINKitMessages.EMailBody,
                bAccount.AcctName, inventoryItem.InventoryCD, inventoryItem.Descr, location?.VLeadTime?.ToString(), companyName, PXAccess.GetUserDisplayName());

            return returnBodyString;
        }

        private void AddAttachmentsToEmail(NotificationGenerator sender)
        {
            UploadFileMaintenance fileUpload = PXGraph.CreateInstance<UploadFileMaintenance>();
            Guid[] savedFileOnScreenGuids = PXNoteAttribute.GetFileNotes(Base.Hdr.Cache, Base.Hdr.Current);

            foreach (var eachGuid in savedFileOnScreenGuids)
            {
                FileInfo file = fileUpload.GetFile(eachGuid);

                sender.AddAttachment(file.Name, file.BinData);
            }
        }
        #endregion

        #endregion
    }
}