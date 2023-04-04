using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.PO;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using System;
using System.Linq;
using System.Collections.Generic;
/* INFO SOURCE TEMPORARY */
//using InfoSmartSearch;
using ASCISTARCustom.Inventory.DAC;

using POVendorInventoryPriceUpdate = PX.Objects.PO.POItemCostManager.POVendorInventoryPriceUpdate;
using ItemCost = PX.Objects.PO.POItemCostManager.ItemCost;
using PX.Common;
using ASCISTARCustom.Cost.Descriptor;

namespace ASCISTARCustom
{
    [Serializable]
    public static class ASCIStarMarketCostHelper
    {
        //#region Contract

        //public class Contract
        //{

        //    string _Source;
        //    string _Commodity;
        //    decimal _CommodityPrice;
        //    decimal _MatrixPrice;
        //    decimal _SurchargePct;
        //    decimal _LossPct;
        //    decimal _Increment;

        //    public string Source { get { return _Source; } set { if (value == "I" || value == "V" || value == "S") _Source = value; else throw new PXException($"Contract Source Must be I for Item, V for Vendor or S for System Default"); } }
        //    public string Commodity { get { return _Commodity; } set { if (value == "24K" || value == "SSS" || value == "PLT") _Commodity = value; else throw new PXException($"Contract Commodity Must be 24K, SSS, or PLT"); } }
        //    public decimal CommodityPrice { get { return _CommodityPrice; } set { _CommodityPrice = value; } }
        //    //public decimal MatrixPrice { get { return _CommodityPrice; } set { _CommodityPrice = value; } }
        //    public decimal SurchargePct { get { return _SurchargePct; } set { _SurchargePct = value; } }
        //    public decimal LossPct { get { return _LossPct; } set { _LossPct = value; } }
        //    public decimal Increment { get { return _Increment; } set { _Increment = value; } }

        //    public Contract(string source, string commodity, decimal commodityPrice, decimal surchargePct, decimal lossPct, decimal increment, decimal? marketPrice = null)
        //    {
        //        Source = source;
        //        Commodity = commodity;
        //        CommodityPrice = commodityPrice;
        //        SurchargePct = surchargePct;
        //        LossPct = lossPct;
        //        Increment = increment;
        //        if (marketPrice != null && Commodity == "SSS")
        //            MatrixPrice((decimal)marketPrice);
        //    }

        //    public decimal MatrixPrice(decimal MarketPrice)
        //    {
        //        string msg = "";
        //        if (Increment == 0)
        //        {
        //            msg += $"Commodity:{Commodity}{Environment.NewLine};CommodityPrice:{CommodityPrice}{Environment.NewLine}Increment:{Increment}{Environment.NewLine}MarketPrice:{MarketPrice}{Environment.NewLine}Matrix:{MarketPrice}";
        //            return MarketPrice;
        //        }
        //        decimal matrix = MarketPrice;
        //        int steps = (int)Math.Floor((MarketPrice / Increment) - (CommodityPrice / Increment));
        //        decimal floor = CommodityPrice + ((decimal)steps * Increment);
        //        decimal ceiling = floor + Increment;
        //        matrix = (floor + ceiling) / 2.000000m;
        //        msg += $"Commodity:{Commodity}{Environment.NewLine}";
        //        msg += $"CommodityPrice:{CommodityPrice}{Environment.NewLine}";
        //        msg += $"Increment:{Increment}{Environment.NewLine}";
        //        msg += $"MarketPrice:{MarketPrice}{Environment.NewLine}";
        //        msg += $"Floor:{floor}{Environment.NewLine}";
        //        msg += $"Ceiling:{ceiling}{Environment.NewLine}";
        //        msg += $"Matrix:{matrix}";
        //        PXTrace.WriteInformation(msg);
        //        _MatrixPrice = matrix;
        //        return matrix;

        //    }

        //}

        //#endregion Contract


        #region Basis
        public class Basis
        {
            #region xctor
            public Basis(PXGraph graph, int VendorID, int? MarketID, string marketCommodity, DateTime effectiveDate, string fineness = null)
            {
                _graph = graph;
                ItemVendor = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(_graph, VendorID);
                if (ItemVendor.VendorClassID != "MARKET")
                    BasisType = CostBasisType.Vendor;
                if (ItemVendor.VendorClassID == "MARKET")
                    BasisType = CostBasisType.Market;

                ItemVendorExt = ItemVendor.GetExtension<ASCIStarVendorExt>();
                if (MarketID == null)
                    MarketID = ItemVendorExt.UsrMarketID;
                Market = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(_graph, MarketID);
                MarketCommodity = marketCommodity;
                Fineness = fineness ?? MarketCommodity;
                Increment = 0.0000m;
                Floor = 0.0000m;
                Ceiling = 0.0000m;
                SurchargePct = 0.0000m;
                LossPct = 0.0000m;
                EffectiveDate = DateTime.Today;
                MarketPerFineOz = new Dictionary<string, decimal>();
                BasisPerFineOz = new Dictionary<string, decimal>();
                CommodityItem = PXSelect<InventoryItem, Where<InventoryItem.inventoryCD, Equal<Required<InventoryItem.inventoryCD>>>>.Select(_graph, MarketCommodity);

                EffectivePerFineOz = new Dictionary<string, decimal>();
                MarketPerFineOz = new Dictionary<string, decimal>();
                BasisPerFineOz = new Dictionary<string, decimal>();


            }
            #endregion xctor

            #region properties

            public InventoryItem CommodityItem { get; set; }
            public string MarketCommodity { get; set; }
            public Vendor Market { get; set; }
            public Vendor ItemVendor { get; set; }
            public ASCIStarVendorExt ItemVendorExt { get; set; }
            public string Fineness { get; set; }
            public string BasisType { get; set; }
            public PXGraph _graph { get; set; }

            public decimal EffectiveBasisPerOz { get; set; }
            public decimal EffectiveBasisPerGram { get; set; }
            public decimal EffectiveBasisPerDWT { get; set; }
            public decimal EffectiveMarketPerOz { get; set; }
            public decimal EffectiveMarketPerGram { get; set; }
            public decimal EffectiveMarketPerDWT { get; set; }



            public decimal EffectivePerOz(string fineness = null) { return MarketPerFineOz[fineness]; }
            public decimal EffectivePerGram(string fineness = null) { Fineness = Fineness ?? MarketCommodity; return EffectivePerFineOz[Fineness] / 31.103480m; }
            public decimal EffectivePerDWT(string fineness = null) { Fineness = Fineness ?? MarketCommodity; return EffectivePerFineOz[Fineness] / 20.00000m; }

            #region Market
            public decimal MarketPerOz(string fineness = null) { return MarketPerFineOz[fineness]; }
            public decimal MarketPerGram(string fineness = null) { Fineness = Fineness ?? MarketCommodity; return MarketPerFineOz[Fineness] / 31.103480m; }
            public decimal MarketPerDWT(string fineness = null) { Fineness = Fineness ?? MarketCommodity; return MarketPerFineOz[Fineness] / 20.00000m; }
            //{
            //    set
            //    {
            //        if (MarketCommodity == null || _graph == null)
            //            throw new PXException("Set the Commodity before assigning the Price Per Oz");
            //        if(MarketPerFineOz == null || MarketPerFineOz.Count == 0)
            //        BuildFinePrices();
            //    }
            //    get { return MarketPerFineOz[MarketCommodity]; }
            //}


            #endregion Market

            #region Basis
            public decimal BasisPerOz
            {
                set
                {
                    if (MarketCommodity == null || _graph == null)
                        throw new PXException("Set the Commodity before assigning the Price Per Oz");
                    //BuildFinePrices();
                }
                get { return BasisPerFineOz[MarketCommodity]; }
            }
            public decimal BasisPerGram(string fineness = null) { Fineness = Fineness ?? MarketCommodity; return BasisPerFineOz[Fineness] / 31.103480m; }
            public decimal BasisPerDWT(string fineness = null) { Fineness = Fineness ?? MarketCommodity; return BasisPerFineOz[Fineness] / 20.00000m; }
            #endregion Basis


            public decimal Increment { get; set; }
            public decimal Floor { get; set; }
            public decimal Ceiling { get; set; }
            public decimal SurchargePct { get; set; }
            public decimal LossPct { get; set; }


            public DateTime EffectiveDate { get; set; }

            public Dictionary<string, decimal> EffectivePerFineOz { get; set; }
            public Dictionary<string, decimal> MarketPerFineOz { get; set; }
            public Dictionary<string, decimal> BasisPerFineOz { get; set; }
            #endregion properties

            #region Public Methods

            public void BuildFinePrices()
            {
                string msg = "";
                if (MarketCommodity != "24K" && MarketCommodity != "SSS")
                    return;
                if (BasisPerFineOz == null || BasisPerFineOz.Count == 0)
                {
                    BasisPerFineOz.Clear();
                    APVendorPrice vendorPrice = FindMarketPrice(_graph, ItemVendor, CommodityItem, "TOZ", Fineness, EffectiveDate);
                    if (vendorPrice == null)
                    {
                        PXTrace.WriteWarning($"FindMarketPrice returned null using ItemVendor:{ItemVendor.BAccountID} CommodityItem:{CommodityItem.InventoryCD} EffectiveDate:{EffectiveDate}");
                        return;
                    }
                    ASCIStarAPVendorPriceExt vendorPriceExt = vendorPrice.GetExtension<ASCIStarAPVendorPriceExt>();
                    decimal basisPerOz = (decimal)vendorPrice.SalesPrice;
                    Increment = (decimal)vendorPriceExt.UsrCommodityIncrement;
                    LossPct = (decimal)vendorPriceExt.UsrCommodityLossPct / 100.0000m;
                    SurchargePct = (decimal)vendorPriceExt.UsrCommoditySurchargePct / 100.0000m; ;
                    EffectiveBasisPerOz = basisPerOz;// * (1.0000m + SurchargePct) * (1.00000m + LossPct);
                    EffectiveBasisPerGram = EffectiveBasisPerOz / 31.10348m;
                    EffectiveBasisPerDWT = EffectiveBasisPerOz / 20.00000m;

                    msg += $"BasisPerOz:{basisPerOz}{Environment.NewLine}Increment:{Increment}{Environment.NewLine}LossPct:{LossPct}{Environment.NewLine}SurchargePct:{SurchargePct}{Environment.NewLine}";

                    foreach (INUnit conv in PXSelect<INUnit,
                        Where<INUnit.toUnit, Equal<Required<INUnit.toUnit>>>>
                        .Select(_graph, MarketCommodity))
                    {
                        decimal prc = 0.0000m;
                        if (conv.UnitMultDiv == "M")
                            prc = (decimal)basisPerOz * (decimal)conv.UnitRate;
                        if (conv.UnitMultDiv == "D")
                            prc = (decimal)basisPerOz / (decimal)conv.UnitRate;
                        msg += $"BasisPerFineOz[{conv.FromUnit}]:{prc}{Environment.NewLine}";

                        BasisPerFineOz.Add(conv.FromUnit, prc);
                    }
                    PXTrace.WriteWarning($"{msg}");
                }
                if (MarketPerFineOz == null || MarketPerFineOz.Count == 0)
                {
                    MarketPerFineOz.Clear();
                    APVendorPrice marketPrice = FindMarketPrice(_graph, Market, CommodityItem, "TOZ", Fineness, EffectiveDate);
                    if (marketPrice == null)
                    {
                        PXTrace.WriteWarning($"FindMarketPrice returned null using Market:{Market.BAccountID} CommodityItem:{CommodityItem.InventoryCD} EffectiveDate:{EffectiveDate}");
                        return;
                    }
                    ASCIStarAPVendorPriceExt marketPriceExt = marketPrice.GetExtension<ASCIStarAPVendorPriceExt>();
                    decimal marketPerOz = (decimal)marketPrice.SalesPrice;
                    Increment = (decimal)marketPriceExt.UsrCommodityIncrement;
                    LossPct = (decimal)marketPriceExt.UsrCommodityLossPct / 100.0000m; ;
                    SurchargePct = (decimal)marketPriceExt.UsrCommoditySurchargePct / 100.0000m; ;
                    msg += $"MarketPerOz:{marketPerOz}{Environment.NewLine}Increment:{Increment}{Environment.NewLine}LossPct:{LossPct}{Environment.NewLine}SurchargePct:{SurchargePct}{Environment.NewLine}";
                    EffectiveMarketPerOz = marketPerOz;// * (1 + SurchargePct) * (1 + LossPct);
                    EffectiveMarketPerGram = EffectiveMarketPerOz / 31.10348m;
                    EffectiveMarketPerDWT = EffectiveMarketPerOz / 20.00000m;


                    foreach (INUnit conv in PXSelect<INUnit,
                        Where<INUnit.toUnit, Equal<Required<INUnit.toUnit>>>>
                        .Select(_graph, MarketCommodity))
                    {
                        decimal prc = 0.0000m;
                        if (conv.UnitMultDiv == "M")
                            prc = (decimal)marketPerOz * (decimal)conv.UnitRate;
                        if (conv.UnitMultDiv == "D")
                            prc = (decimal)marketPerOz / (decimal)conv.UnitRate;
                        msg += $"MarketPerFineOz[{conv.FromUnit}]:{prc}{Environment.NewLine}";
                        MarketPerFineOz.Add(conv.FromUnit, prc);
                    }
                    PXTrace.WriteWarning($"{msg}");

                }

            }


            #endregion Public Methods

            #region Private Methods


            private APVendorPrice MarketSilverPrice(PXGraph graph, int? MarketID, string UOM = "TOZ", string Fineness = "SSS", DateTime? effectiveDate = null)
            {
                InventoryItem commodity = GetCommodityItem(graph, "SSS");
                Vendor market = GetMarketVendor(graph, MarketID);
                return FindMarketPrice(graph, market, commodity, UOM, Fineness, effectiveDate ?? DateTime.Today);
            }

            private APVendorPrice MarketGoldPrice(PXGraph graph, int? MarketID, string UOM = "TOZ", string Fineness = "24K", DateTime? effectiveDate = null)
            {
                InventoryItem commodity = GetCommodityItem(graph, "24K");
                Vendor market = GetMarketVendor(graph, MarketID);
                return FindMarketPrice(graph, market, commodity, UOM, Fineness, effectiveDate ?? DateTime.Today);
            }

            private APVendorPrice MarketPlatinumPrice(PXGraph graph, int? MarketID, string UOM = "TOZ", string Fineness = "9995", DateTime? effectiveDate = null)
            {
                InventoryItem commodity = GetCommodityItem(graph, "PLT");
                Vendor market = GetMarketVendor(graph, MarketID);
                return FindMarketPrice(graph, market, commodity, UOM, Fineness, effectiveDate ?? DateTime.Today);
            }

            private APVendorPrice FindMarketPrice(PXGraph graph, Vendor Market, InventoryItem commodity, string UOM, string Fineness, DateTime effectiveDate)
            {
                string msg = "";
                APVendorPrice marketPrice = new APVendorPrice();
                VendorClass vc = PXSelect<VendorClass, Where<VendorClass.vendorClassID, Equal<Required<Vendor.vendorClassID>>>>.Select(graph, Market.VendorClassID);
                if (vc.VendorClassID == "MARKET")
                    msg += $"Market   :{Market.AcctCD}{System.Environment.NewLine}";
                else
                    msg += $"Basis    :{Market.AcctCD}{System.Environment.NewLine}";
                msg += $"Commodity:{commodity.InventoryCD}{System.Environment.NewLine}";
                msg += $"UOM      :{UOM}{System.Environment.NewLine}";
                msg += $"Fineness :{Fineness}{System.Environment.NewLine}";
                msg += $"Date     :{effectiveDate.ToString("MM/dd/yyyy")}{System.Environment.NewLine}";

                marketPrice =
                new PXSelect<APVendorPrice,
                Where<APVendorPrice.vendorID, Equal<Required<APVendorPrice.vendorID>>,
                    And<APVendorPrice.inventoryID, Equal<Required<APVendorPrice.inventoryID>>,
                    And<APVendorPrice.uOM, Equal<Required<APVendorPrice.uOM>>,
                    And<APVendorPrice.effectiveDate, LessEqual<Required<APVendorPrice.effectiveDate>>,
                    And<APVendorPrice.effectiveDate, LessEqual<Required<APVendorPrice.effectiveDate>>>>>>>,
                OrderBy<Desc<APVendorPrice.effectiveDate>>>(graph).SelectSingle(
                    Market.BAccountID,
                    commodity.InventoryID,
                    "TOZ",
                    effectiveDate,
                    effectiveDate);

                if (marketPrice == null)
                {
                    throw new PXException($"{Market.AcctCD} does not contain a valid price for {commodity.InventoryCD} on {effectiveDate.ToString("MM/dd/yyyy")}.");
                }

                msg += $"Price    :{marketPrice.SalesPrice}{System.Environment.NewLine}";
                msg += $"Effective:{effectiveDate.ToString("MM/dd/yyyy")}{System.Environment.NewLine}";

                if (UOM != "TOZ") //Convert To UNIT OF MEASURE
                {
                    INUnit conv = ConvertPrice(graph, UOM, "TOZ");
                    PXTrace.WriteInformation($"marketPrice.SalesPrice:{marketPrice.SalesPrice}");
                    PXTrace.WriteInformation($"conv.UnitRate:{conv.UnitRate}");

                    if (conv.UnitMultDiv == "M")
                        marketPrice.SalesPrice = marketPrice.SalesPrice * conv.UnitRate;
                    else
                        marketPrice.SalesPrice = marketPrice.SalesPrice / conv.UnitRate;
                    marketPrice.UOM = UOM;

                }
                PXTrace.WriteInformation(msg);
                return marketPrice;
            }


            private InventoryItem GetCommodityItem(PXGraph graph, string InventoryCD)
            {
                InventoryItem commodity = new PXSelect<InventoryItem, Where<InventoryItem.inventoryCD, Equal<Required<InventoryItem.inventoryCD>>>>(graph).SelectSingle(InventoryCD);
                if (commodity == null)
                    throw new PXException($"{InventoryCD} commodity not found in Stock Items");
                return commodity;

            }

            private Vendor GetMarketVendor(PXGraph graph, string AcctCD = "LONDON PM")
            {
                Vendor market = new PXSelect<Vendor, Where<Vendor.acctCD, Equal<Required<Vendor.acctCD>>>>(graph).SelectSingle(AcctCD);
                if (market == null)
                    throw new PXException($"Market {AcctCD} not found in Vendors");
                return market;

            }

            private Vendor GetMarketVendor(PXGraph graph, int? VendorID)
            {
                if (VendorID == null)
                {
                    PXTrace.WriteWarning("No Market Selected, Using LONDON PM");
                    return GetMarketVendor(graph);
                }
                Vendor market = new PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>(graph).SelectSingle(VendorID);
                if (market == null)
                    throw new PXException($"Market not found in Vendors");
                return market;

            }


            private static INUnit ConvertPrice(PXGraph graph, string FromUnit, string ToUnit)
            {
                INUnit conv =
                new PXSelect<INUnit,
                Where<INUnit.fromUnit, Equal<Required<INUnit.fromUnit>>,
                    And<INUnit.toUnit, Equal<Required<INUnit.toUnit>>>>,
                OrderBy<Desc<APVendorPrice.effectiveDate>>>(graph).SelectSingle(
                    FromUnit,
                    ToUnit);
                if (conv == null)
                    throw new PXException($"No Conversion Found - CHeck Units Of Measure for {FromUnit} to {ToUnit}");
                return conv;
            }

            #endregion Private Methods
        }

        #endregion Basis

        #region costBasis

        public class costBasis
        {
            #region xctor
            public costBasis(PXGraph graph, InventoryItem item, DateTime EffectiveDate)
            {
                if (_graph == null && graph != null)
                    _graph = graph;
                if (item != null)
                {
                    Item = item;
                    ItemExt = this.Item.GetExtension<ASCIStarINInventoryItemExt>();
                }
                /* INFO SOURCE TEMPORARY */
                ASCIStarINJewelryItem jewelryItemData = PXSelect<ASCIStarINJewelryItem, Where<ASCIStarINJewelryItem.inventoryID, Equal<Required<ASCIStarINJewelryItem.inventoryID>>>>.Select(graph, item.InventoryID);
                Fineness = jewelryItemData.MetalType;
                _itemVendor = new Vendor();
                _itemVendorBasis = new ASCIStarVendorExt();
                _market = new Vendor();
                _vendorItem = new POVendorInventory();
                _vendorItemBasis = new ASCIStarPOVendorInventoryExt();
                _EffectiveDate = EffectiveDate;

            }
            #endregion xctor

            #region properties
            private PXGraph _graph;


            public InventoryItem Item;
            public ASCIStarINInventoryItemExt ItemExt;

            public string Fineness;

            public Basis GoldBasis;
            public Basis SilverBasis;



            private POVendorInventory _vendorItem;
            private ASCIStarPOVendorInventoryExt _vendorItemBasis;
            private Vendor _itemVendor;
            private ASCIStarVendorExt _itemVendorBasis;
            private Vendor _market;
            private ASCIStarVendorExt _marketBasis;
            private DateTime _EffectiveDate;

            public Vendor ItemVendor
            {
                get { return _itemVendor; }
                set
                {
                    string msg = "";
                    _itemVendor = value;

                    msg += $"Vendor   :{_itemVendor.AcctCD}{System.Environment.NewLine}";
                    _itemVendorBasis = _itemVendor.GetExtension<ASCIStarVendorExt>();



                    _market = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>
                        .Select(_graph, _itemVendorBasis.UsrMarketID);
                    _marketBasis = _market.GetExtension<ASCIStarVendorExt>();

                    if (ItemExt.UsrPricingGRAMGold > 0)
                    {
                        GoldBasis = new Basis(_graph, (int)_itemVendor.BAccountID, _itemVendorBasis.UsrMarketID, "24K", _EffectiveDate);
                    }
                    if (ItemExt.UsrPricingGRAMSilver > 0)
                    {
                        SilverBasis = new Basis(_graph, (int)_itemVendor.BAccountID, _itemVendorBasis.UsrMarketID, "SSS", _EffectiveDate);
                    }

                    #region depracated
                    //if (Item != null)
                    //{

                    //    var record = PXSelectJoin<Vendor,
                    //        LeftJoin<POVendorInventory, On<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>,
                    //        LeftJoin<InventoryItemCurySettings, On<InventoryItem.inventoryID, Equal<InventoryItemCurySettings.inventoryID>,
                    //        And<InventoryItemCurySettings.preferredVendorID, Equal<Required<POVendorInventory.vendorID>>>>>>,
                    //        Where<POVendorInventory.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                    //        .Select(_graph, _itemVendor.BAccountID, _itemVendor.BAccountID, Item.InventoryID).ToArray()
                    //        .Select(r => new 
                    //                        { 
                    //                          vendorSelect = r.GetItem<Vendor>(),
                    //                          vendorInventorySelect = r.GetItem<POVendorInventory>(),
                    //                          itemSettingsSelect = r.GetItem<InventoryItemCurySettings>() 
                    //                        }
                    //        )
                    //        .FirstOrDefault();

                    //    if (record.vendorInventorySelect != null 
                    //        && _itemVendor != null 
                    //        && record.vendorInventorySelect.VendorID != _itemVendor.BAccountID)
                    //    {

                    //        _vendorItem = record.vendorInventorySelect;
                    //        _vendorItemBasis = _vendorItem.GetExtension<ASCIStarPOVendorInventoryExt>();

                    //        PXTrace.WriteInformation($"{Market.AcctCD} set according to {ItemVendor.AcctCD} Configuration");

                    //    }


                    //}


                    //APVendorPrice goldPrice = Get(_graph, _itemVendor.BAccountID, "TOZ", "24K", DateTime.Today);
                    //ASCIStarAPVendorPriceExt goldPriceBasis = goldPrice.GetExtension<ASCIStarAPVendorPriceExt>();
                    //APVendorPrice silverPrice = MarketSilverPrice(_graph, _itemVendor.BAccountID, "TOZ", "24K", DateTime.Today);
                    //ASCIStarAPVendorPriceExt silverPriceBasis = silverPrice.GetExtension<ASCIStarAPVendorPriceExt>();


                    //GoldBasis.MarketPerOz = (decimal)goldPrice.SalesPrice;
                    //GoldBasis.Fineness = Fineness;
                    //GoldBasis.Increment = (decimal)goldPriceBasis.UsrCommodityIncrement;
                    //GoldBasis.LossPct = (decimal)goldPriceBasis.UsrCommodityLossPct;
                    //GoldBasis.SurchargePct = (decimal)goldPriceBasis.UsrCommoditySurchargePct;
                    //GoldBasis.M = (decimal)goldPriceBasis.UsrCommoditySurchargePct;

                    //GoldBasis.PricePerOz = (decimal)goldPrice.SalesPrice;
                    //GoldBasis.Fineness = Fineness;
                    //GoldBasis.Increment = (decimal)goldPriceBasis.UsrCommodityIncrement;
                    //GoldBasis.LossPct = (decimal)goldPriceBasis.UsrCommodityLossPct;
                    //GoldBasis.SurchargePct = (decimal)goldPriceBasis.UsrCommoditySurchargePct;

                    #endregion depracated



                    //if(_vendorItemBasis.UsrVendorDefault == true)
                    //{
                    //    GoldBasis.Increment = (decimal)_vendorItemBasis.UsrCommodityIncrement;
                    //    GoldBasis.LossPct = (decimal)_vendorItemBasis.UsrCommodityLossPct;
                    //    GoldBasis.SurchargePct = (decimal)_vendorItemBasis.UsrCommoditySurchargePct;

                    //}

                    //msg += $"Market   :{_market.AcctCD}{System.Environment.NewLine}";
                    //msg += $"Price    :{_market.AcctCD}{System.Environment.NewLine}";
                    //msg += $"Increment:{_market.AcctCD}{System.Environment.NewLine}";
                    //msg += $"Loss Pct :{_market.AcctCD}{System.Environment.NewLine}";
                    //msg += $"Surcharge:{_market.AcctCD}{System.Environment.NewLine}";
                    //msg += $"Gold     :{_market.AcctCD}{System.Environment.NewLine}";
                    //msg += $"Silver   :{_market.AcctCD}{System.Environment.NewLine}";
                }
            }

            public Vendor Market;
            public ASCIStarVendorExt MarketBasis;

            public APVendorPrice Cost;
            public ASCIStarAPVendorPriceExt Basis;
            #endregion properties

            //#region Public Methods

            //public void SetItem(InventoryItem item, ASCIStarINInventoryItemExt itemExt = null)
            //{
            //    Item = item;
            //    if (itemExt != null)
            //        ItemExt = itemExt;
            //    else
            //        ItemExt = Item.GetExtension<ASCIStarINInventoryItemExt>();

            //}


            //public void SetVendor(Vendor vendor = null, InventoryItem item = null, ASCIStarVendorExt vendorExt = null, DateTime? PricingDate = null)
            //{
            //    if (PricingDate == null)
            //        PricingDate = DateTime.Today;
            //    if (vendor != null && vendor.BAccountID != null)
            //        _itemVendor = vendor;
            //    if (ItemVendor != null)
            //        _itemVendorBasis = _itemVendor.GetExtension<ASCIStarVendorExt>();
            //    if (Item != null && Item.InventoryID != null &&
            //        _itemVendor == null && _itemVendor.BAccountID == null)
            //    {
            //        foreach (var record in PXSelectJoin<POVendorInventory,
            //            InnerJoin<InventoryItemCurySettings, On<InventoryItem.inventoryID, Equal<Required<InventoryItemCurySettings.inventoryID>>>>,
            //            Where<POVendorInventory.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(_graph, Item.InventoryID).ToArray()
            //            .Select(r => new { vendorInventory = r.GetItem<POVendorInventory>(), itemSettings = r.GetItem<InventoryItemCurySettings>() }))

            //        {
            //            if (vendor == null && record.vendorInventory != null
            //                && record.vendorInventory.VendorID == record.itemSettings.PreferredVendorID)
            //            {
            //                _itemVendor = new PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>(_graph).SelectSingle(record.itemSettings.PreferredVendorID);
            //                _itemVendorBasis = _itemVendor.GetExtension<ASCIStarVendorExt>();
            //                _vendorItem = record.vendorInventory;
            //                _vendorItemBasis = _vendorItem.GetExtension<ASCIStarPOVendorInventoryExt>();
            //            }

            //        }
            //    }

            //}

            ////public costBasis(PXGraph graph, InventoryItem item, Vendor vendor = null)
            ////{
            ////    if (Item == null && item != null)
            ////        Item = item;
            ////    if (ItemVendor == null && vendor != null)
            ////        ItemVendor = vendor;
            ////    if (VendorItem == null && vendor != null)
            ////    {
            ////        VendorItem = PXSelect<POVendorInventory, Where<POVendorInventory.vendorID, Equal<Required<Vendor.bAccountID>>,
            ////            And<POVendorInventory.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>>.Select(graph, this.vendor.BAccountID, item.InventoryID);
            ////        VendorItemBasis = VendorItem.GetExtension<ASCIStarPOVendorInventoryExt>();
            ////    }
            ////    if (VendorItem == null && vendor == null)
            ////    {

            ////        foreach (POVendorInventory vitem in PXSelect<POVendorInventory, Where<POVendorInventory.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(graph, item.InventoryID))
            ////        {
            ////            PXTrace.WriteInformation($"{vitem.VendorID}:{vitem.IsDefault}");

            ////            if (vitem.IsDefault == true)
            ////                VendorItem = vitem;
            ////        }

            ////    }



            ////}
            //#endregion Public Methods

            //#region Private Methods

            ////private void Load()
            ////{
            ////    Validate();
            ////}
            ////private void Validate()
            ////{
            ////    string err = "";
            ////    try
            ////    {
            ////        if (Item == null)
            ////            err += $"Item is NULL{System.Environment.NewLine}";
            ////        if (ItemExt == null)
            ////            err += $"ItemExt is NULL{System.Environment.NewLine}";
            ////        if (_itemVendor == null)
            ////            err += $"ItemVendor is NULL{System.Environment.NewLine}";
            ////        if (_itemVendorBasis == null)
            ////            err += $"ItemVendorBasis is NULL{System.Environment.NewLine}";
            ////        if (_market == null)
            ////            err += $"Market is NULL{System.Environment.NewLine}";
            ////        if (_marketBasis == null)
            ////            err += $"MarketBasis is NULL{System.Environment.NewLine}";
            ////        if (_vendorItem == null)
            ////            err += $"VendorItem is NULL{System.Environment.NewLine}";
            ////        if (_vendorItemBasis == null)
            ////            err += $"VendorItemBasis is NULL{System.Environment.NewLine}";
            ////        if (err.Length > 0)
            ////            throw new PXException(err);
            ////    }
            ////    catch (Exception e)
            ////    {
            ////        throw e;
            ////    }
            ////}

            //#endregion Private Methods

            //public static INUnit ConvertPrice(PXGraph graph, string FromUnit, string ToUnit)
            //{
            //    INUnit conv =
            //    new PXSelect<INUnit,
            //    Where<INUnit.fromUnit, Equal<Required<INUnit.fromUnit>>,
            //        And<INUnit.toUnit, Equal<Required<INUnit.toUnit>>>>,
            //    OrderBy<Desc<APVendorPrice.effectiveDate>>>(graph).SelectSingle(
            //        FromUnit,
            //        ToUnit);
            //    if (conv == null)
            //        throw new PXException($"No Conversion Found - CHeck Units Of Measure for {FromUnit} to {ToUnit}");
            //    return conv;
            //}


            //public InventoryItem GetCommodityItem(PXGraph graph, string InventoryCD)
            //{
            //    InventoryItem commodity = new PXSelect<InventoryItem, Where<InventoryItem.inventoryCD, Equal<Required<InventoryItem.inventoryCD>>>>(graph).SelectSingle(InventoryCD);
            //    if (commodity == null)
            //        throw new PXException($"{InventoryCD} commodity not found in Stock Items");
            //    return commodity;

            //}

            //public Vendor GetMarketVendor(PXGraph graph, string AcctCD = "LONDON PM")
            //{
            //    Vendor market = new PXSelect<Vendor, Where<Vendor.acctCD, Equal<Required<Vendor.acctCD>>>>(graph).SelectSingle(AcctCD);
            //    if (market == null)
            //        throw new PXException($"Market {AcctCD} not found in Vendors");
            //    return market;

            //}

            //public Vendor GetMarketVendor(PXGraph graph, int? VendorID)
            //{
            //    if (VendorID == null)
            //    {
            //        PXTrace.WriteWarning("No Market Selected, Using LONDON PM");
            //        return GetMarketVendor(graph);
            //    }
            //    Vendor market = new PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>(graph).SelectSingle(VendorID);
            //    if (market == null)
            //        throw new PXException($"Market not found in Vendors");
            //    return market;

            //}

            //public APVendorPrice MarketSilverPrice(PXGraph graph, int? MarketID, string UOM = "TOZ", string Fineness = "SSS")
            //public APVendorPrice MarketSilverPrice(PXGraph graph, int? MarketID, string UOM = "TOZ", string Fineness = "SSS", DateTime? effectiveDate = null)
            //{
            //    InventoryItem commodity = GetCommodityItem(graph, "SSS");
            //    Vendor market = GetMarketVendor(graph, MarketID);
            //    return FindMarketPrice(graph, market, commodity, UOM, Fineness, effectiveDate ?? DateTime.Today);
            //}

            //public APVendorPrice MarketGoldPrice(PXGraph graph, int? MarketID, string UOM = "TOZ", string Fineness = "24K", DateTime? effectiveDate = null)
            //{
            //    InventoryItem commodity = GetCommodityItem(graph, "24K");
            //    Vendor market = GetMarketVendor(graph, MarketID);
            //    return FindMarketPrice(graph, market, commodity, UOM, Fineness, effectiveDate ?? DateTime.Today);
            //}

            //public APVendorPrice MarketPlatinumPrice(PXGraph graph, int? MarketID, string UOM = "TOZ", string Fineness = "9995", DateTime? effectiveDate = null)
            //{
            //    InventoryItem commodity = GetCommodityItem(graph, "PLT");
            //    Vendor market = GetMarketVendor(graph, MarketID);
            //    return FindMarketPrice(graph, market, commodity, UOM, Fineness, effectiveDate ?? DateTime.Today);
            //}

            //public APVendorPrice FindMarketPrice(PXGraph graph, Vendor Market, InventoryItem commodity, string UOM, string Fineness, DateTime effectiveDate)
            //{
            //    APVendorPrice marketPrice = new APVendorPrice();
            //    string msg = "";
            //    msg += $"Market   :{Market.AcctCD}{System.Environment.NewLine}";
            //    msg += $"Commodity:{commodity.InventoryCD}{System.Environment.NewLine}";
            //    msg += $"UOM      :{UOM}{System.Environment.NewLine}";
            //    msg += $"Fineness :{Fineness}{System.Environment.NewLine}";
            //    msg += $"Date     :{effectiveDate.ToString("MM/dd/yyyy")}{System.Environment.NewLine}";




            //    marketPrice =
            //    new PXSelect<APVendorPrice,
            //    Where<APVendorPrice.vendorID, Equal<Required<APVendorPrice.vendorID>>,
            //        And<APVendorPrice.inventoryID, Equal<Required<APVendorPrice.inventoryID>>,
            //        And<APVendorPrice.uOM, Equal<Required<APVendorPrice.uOM>>,
            //        And<APVendorPrice.effectiveDate, LessEqual<Required<APVendorPrice.effectiveDate>>,
            //        And<APVendorPrice.effectiveDate, LessEqual<Required<APVendorPrice.effectiveDate>>>>>>>,
            //    OrderBy<Desc<APVendorPrice.effectiveDate>>>(graph).SelectSingle(
            //        Market.BAccountID,
            //        commodity.InventoryID,
            //        "TOZ",
            //        effectiveDate,
            //        effectiveDate);

            //    if (marketPrice == null)
            //    {
            //        throw new PXException($"{Market.AcctCD} does not contain a valid price for {commodity.InventoryCD} on {effectiveDate.ToString("MM/dd/yyyy")}.");
            //    }
            //    PXTrace.WriteInformation(msg);
            //    msg += $"Price    :{marketPrice.SalesPrice} on {marketPrice.EffectiveDate} for effective date {effectiveDate.ToString("MM/dd/yyyy")}."{ System.Environment.NewLine}
            //    ";
            //    msg += $"Effective:{marketPrice.SalesPrice} on {marketPrice.EffectiveDate} for effective date {effectiveDate.ToString("MM/dd/yyyy")}."{ System.Environment.NewLine}
            //    ";
            //    PXTrace.WriteInformation($"{Market.AcctCD} priced {commodity.InventoryCD} at {marketPrice.SalesPrice} on {marketPrice.EffectiveDate} for effective date {effectiveDate.ToString("MM/dd/yyyy")}.");
            //    if (UOM != "TOZ") //Convert To UNIT OF MEASURE
            //    {
            //        INUnit conv = ConvertPrice(graph, UOM, "TOZ");
            //        PXTrace.WriteInformation($"marketPrice.SalesPrice:{marketPrice.SalesPrice}");
            //        PXTrace.WriteInformation($"conv.UnitRate:{conv.UnitRate}");

            //        if (conv.UnitMultDiv == "M")
            //            marketPrice.SalesPrice = marketPrice.SalesPrice * conv.UnitRate;
            //        else
            //            marketPrice.SalesPrice = marketPrice.SalesPrice / conv.UnitRate;
            //        marketPrice.UOM = UOM;

            //    }
            //    return marketPrice;
            //    #endregion Public Methods
            //
        }

        #endregion costBasis

        public class JewelryCost
        {
            #region Selects
            public PXSelect<APVendorPrice, Where<APVendorPrice.inventoryID, Equal<Required<APVendorPrice.inventoryID>>,
                    And<APVendorPrice.vendorID, Equal<Required<APVendorPrice.vendorID>>,
                    And<APVendorPrice.effectiveDate, LessEqual<Required<APVendorPrice.effectiveDate>>>>>> CommodityPrice;

            //public PXSelect<ASCIStarVendorExt, Where<Vendor.acctCD, Equal<MarketList.londonPM>>> VendorDefaultMarket;
            //public PXSelect<BAccount, Where<BAccount.acctCD, Equal<DEFAULTMARKET>>> DefaultMarket;
            //public class DEFAULTMARKET : PX.Data.BQL.BqlString.Constant<DEFAULTMARKET>
            //{
            //    public static readonly string value = "LONDON PM";
            //    public DEFAULTMARKET() : base(value) { }
            //}
            public PXSelectJoin<INKitSpecHdr, InnerJoin<INKitSpecStkDet, On<INKitSpecHdr.kitInventoryID, Equal<INKitSpecStkDet.kitInventoryID>>>,
                    Where<INKitSpecStkDet.compInventoryID, Equal<Required<INKitSpecStkDet.compInventoryID>>>> StockLines;

            public PXSelectJoin<INKitSpecHdr, InnerJoin<INKitSpecNonStkDet, On<INKitSpecHdr.kitInventoryID, Equal<INKitSpecNonStkDet.kitInventoryID>>>,
                    Where<INKitSpecNonStkDet.compInventoryID, Equal<Required<INKitSpecNonStkDet.compInventoryID>>>> NonStockLines;
            #endregion Selects

            #region Declarations

            //private readonly string uom;

            public int level { get; set; }
            public string costingType { get; set; }
            public string costRollupType { get; set; }
            public string commodityType { get; set; }
            public string market { get; set; }

            public decimal GoldWgt { get; set; }
            public decimal fineGoldWgt { get; set; }
            public decimal SilverWgt { get; set; }
            public decimal fineSilverWgt { get; set; }
            public decimal finePlatinumWgt { get; set; }
            public decimal marketCommodityCost { get; set; }

            public costBasis CostBasis { get; set; }

            public APVendorPrice GoldMarket { get; set; }
            public APVendorPrice SilverMarket { get; set; }
            public APVendorPrice PlatinumMarket { get; set; }
            public DateTime? pricingDate { get; set; }



            public List<JewelryCost> SubItem { get; set; }
            public Dictionary<string, decimal> CostRollup { get; set; }
            public Dictionary<string, decimal> CostRollupTotal { get; set; }

            public ASCIStarAPVendorPriceExt vendorPrice { get; set; } //What the Vendor is Charging for the item
            public ASCIStarAPVendorPriceExt commodityPrice { get; set; }



            #endregion Declarations

            //public JewelryCost(InventoryItem item, DateTime? PricingDate) 
            //{
            //    if (PricingDate == null)
            //        PricingDate = DateTime.Today;
            //    JewelryCost(item, null, null, 0.000000m, 0.000000m, true);
            //}
            //public JewelryCost(InventoryItem item, decimal cost) 
            //{
            //}
            //public JewelryCost(InventoryItem item, string uom, string curyID, decimal cost, bool convertCury)
            //{
            //}
            public JewelryCost(PXGraph graph, InventoryItem item, decimal cost = 0.000000m, decimal baseCost = 0.000000m, int? vendorID = null, int? marketID = null
                            , DateTime? PricingDate = null, string uom = "EA", string curyID = "USD", bool convertCury = false)
            {
                string msg = "";
                //try
                //{
                if (PricingDate == null || PricingDate > DateTime.Today)//.AddDays(-1))
                {
                    PricingDate = DateTime.Today;//.AddDays(-1);
                }

                msg = $"cost       :{cost}{Environment.NewLine}";
                msg += $"baseCost   :{baseCost}{Environment.NewLine}";
                msg += $"vendorID   :{vendorID}{Environment.NewLine}";
                msg += $"marketID   :{marketID}{Environment.NewLine}";
                msg += $"PricingDate:{PricingDate}{Environment.NewLine}";
                msg += $"uom        :{uom}{Environment.NewLine}";
                msg += $"curyID     :{curyID}{Environment.NewLine}";


                ASCIStarINInventoryItemExt itemExt = item.GetExtension<ASCIStarINInventoryItemExt>();
                this.costingType = itemExt.UsrCostingType ?? ASCIStarCostingType.StandardCost;
                this.costRollupType = itemExt.UsrCostRollupType ?? ASCIStarCostRollupType.Other;

                this.SubItem = new List<JewelryCost>();
                this.CostRollup = new Dictionary<string, decimal>();
                this.CostRollupTotal = new Dictionary<string, decimal>();


                switch (itemExt.UsrCostingType ?? ASCIStarCostingType.StandardCost)
                {
                    case ASCIStarCostingType.MarketCost:
                        msg += $"Costing    :Market{Environment.NewLine}";
                        break;
                    case ASCIStarCostingType.ContractCost:
                        msg += $"Costing    :Contract{Environment.NewLine}";
                        break;
                    case ASCIStarCostingType.WeightCost:
                        msg += $"Costing    :Weight{Environment.NewLine}";
                        break;
                    //case CostingType.PercentageCost:
                    //    msg += $"Costing    :Percentage{Environment.NewLine}";
                    //    break;
                    default: /* CostingType.StandardCost */
                        msg += $"Costing    :Standard{Environment.NewLine}";
                        break;
                }


                if (marketID == null)
                {
                    msg += $"Market Def :LONDON PM{Environment.NewLine}";
                    //  BAccount market = DefaultMarket.Select();
                    marketID = SelectFrom<Vendor>.Where<Vendor.acctCD.IsEqual<MarketList.defaultMarket>>.View.Select(graph)?.TopFirst.BAccountID;
                }

                /*
                   public SelectFrom<APVendorPrice>.
                    InnerJoin<POVendorInventory>.
                    On<POVendorInventory.inventoryID.IsEqual<INKitSpecHdr.kitInventoryID.FromCurrent>.
                        And<APVendorPrice.vendorID.IsEqual<POVendorInventory.vendorID>>>.
                    InnerJoin<InventoryItem>.
                        On<InventoryItem.inventoryID.IsEqual<APVendorPrice.inventoryID>>.
                    InnerJoin<INItemClass>.On<InventoryItem.itemClassID.IsEqual<INItemClass.itemClassID>.
                        And<INItemClass.itemClassCD.IsEqual<CommodityClass>>>.
                    Where<APVendorPrice.effectiveDate.IsLessEqual<AccessInfo.businessDate>.
                        And<Brackets<APVendorPrice.expirationDate.IsGreaterEqual<AccessInfo.businessDate>.
                            Or<APVendorPrice.expirationDate.IsNull>>>>.
                            OrderBy<APVendorPrice.effectiveDate.Asc> 
                */

                //POVendorInventory itemVendor = SelectFrom<POVendorInventory>
                //    .InnerJoin<InventoryItem>
                //    .On<POVendorInventory.inventoryID.IsEqual<InventoryItem.inventoryID>>
                //    .InnerJoin<InventoryItemCurySettings>.On<POVendorInventory.inventoryID.IsEqual<InventoryItemCurySettings.inventoryID>
                //    .And<POVendorInventory.vendorID.IsEqual<InventoryItemCurySettings.preferredVendorID>>>.View.Select(graph)
                //    .FirstOrDefault();


                POVendorInventory itemVendor = SelectFrom<POVendorInventory>
                    .Where<POVendorInventory.inventoryID.IsEqual<@P.AsInt>
                    .And<POVendorInventory.vendorID.IsEqual<@P.AsInt>>>.View.Select(graph, item.InventoryID, vendorID);


                if (itemVendor == null || itemVendor.VendorID == null)
                {
                    var poVendorInventory = SelectFrom<POVendorInventory>
                        .Where<POVendorInventory.inventoryID.IsEqual<@P.AsInt>>.View.Select(graph, item.InventoryID).FirstTableItems;

                    if (poVendorInventory.Count() == 0)
                    {
                        throw new PXSetPropertyException("The item has not Vendor prices, please, setup it on Vendor Prices screen", PXErrorLevel.RowError);
                        //  PXUIFieldAttribute.SetError<INKitSpecStkDet.compInventoryID>(graph.Caches[] , "Error Message");
                    }

                    foreach (POVendorInventory vitem in poVendorInventory)
                    {
                        if (vitem.IsDefault == true)
                            itemVendor = vitem;
                    }
                }
                msg += $"Item Vendor:{itemVendor?.VendorID} Default: {itemVendor?.IsDefault}{Environment.NewLine}";


                if (itemExt.UsrPricingGRAMGold > 0.0m)
                {

                    CostBasis = new costBasis(graph, item, (DateTime)PricingDate);

                    CostBasis.ItemVendor = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(graph, itemVendor.VendorID);

                    CostBasis.GoldBasis.BuildFinePrices();

                    //decimal basisPerGram = CostBasis.GoldBasis.BasisPerGram();
                    //cost = (itemExt.UsrPricingGRAMGold ?? 0.00m) * basisPerGram * (1.0000m + CostBasis.GoldBasis.LossPct) * (1.0000m + CostBasis.GoldBasis.SurchargePct);
                    //   marketCommodityCost = cost * (CostBasis.GoldBasis.MarketPerFineOz["24K"] / CostBasis.GoldBasis.BasisPerFineOz["24K"]);

                    msg += $"Found {itemExt.UsrPricingGRAMGold} Gold Grams to Price{Environment.NewLine}";
                    msg += $"PricingDate:{pricingDate}{Environment.NewLine}";
                    msg += $"Gold Eff   :{CostBasis.GoldBasis.EffectiveDate}{Environment.NewLine}";

                    //pricingDate = CostBasis.GoldBasis.EffectiveDate; //goldMarket.EffectiveDate;

                    decimal costPerGram = 0;

                    switch (itemExt.UsrCostingType)
                    {
                        case ASCIStarCostingType.MarketCost:
                            // costPerGram = CostBasis.GoldBasis.BasisPerGram(); // CostBasis.GoldBasis.EffectiveMarketPerGram;
                            costPerGram = CostBasis.GoldBasis.MarketPerGram();
                            break;
                        case ASCIStarCostingType.ContractCost:
                            costPerGram = CostBasis.GoldBasis.BasisPerGram();
                            break;
                        case ASCIStarCostingType.WeightCost:
                            costPerGram = CostBasis.GoldBasis.MarketPerGram();
                            // labor cost * gold grams
                            break;

                        case ASCIStarCostingType.StandardCost:
                            break;
                        default:

                            break;
                    }



                    cost = (itemExt.UsrPricingGRAMGold ?? 0.00m) * (costPerGram);// * (1.0000m + CostBasis.GoldBasis.LossPct) * (1.0000m + CostBasis.GoldBasis.SurchargePct);
                                                                                 //  marketCommodityCost = cost * (CostBasis.GoldBasis.MarketPerFineOz["24K"] / CostBasis.GoldBasis.BasisPerFineOz["24K"]);
                    msg += $"Gold Loss  : {CostBasis.GoldBasis.LossPct}{Environment.NewLine}";
                    msg += $"Gold Sur   : {CostBasis.GoldBasis.SurchargePct}{Environment.NewLine}";
                }

                if (itemExt.UsrPricingGRAMSilver > 0.0m)
                {
                    PXTrace.WriteInformation($"Found {itemExt.UsrPricingGRAMSilver} Silver Grams to Price");

                    CostBasis = new costBasis(graph, item, (DateTime)PricingDate);

                    CostBasis.ItemVendor = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(graph, itemVendor.VendorID);

                    CostBasis.SilverBasis.BuildFinePrices();

                    decimal metalCostPerOz = 0.0m;
                    //switch (itemExt.UsrCostingType)
                    //{
                    //    case ASCIStarCostingType.MarketCost:
                    //        metalCostPerOz = GetSilverMetalCostPerOZ(CostBasis.SilverBasis.EffectiveMarketPerOz, CostBasis.SilverBasis.EffectiveMarketPerOz, CostBasis.ItemExt.UsrContractIncrement);
                    //        break;
                    //    case ASCIStarCostingType.ContractCost:
                    //        metalCostPerOz = GetSilverMetalCostPerOZ(CostBasis.SilverBasis.BasisPerOz, CostBasis.SilverBasis.EffectiveMarketPerOz, CostBasis.ItemExt.UsrContractIncrement);
                    //        break;
                    //    case ASCIStarCostingType.WeightCost:  // labor cost * silver grams
                    //        metalCostPerOz = GetSilverMetalCostPerOZ(CostBasis.SilverBasis.BasisPerOz, CostBasis.SilverBasis.EffectiveMarketPerOz, CostBasis.ItemExt.UsrContractIncrement);
                    //        break;

                    //    case ASCIStarCostingType.StandardCost: break;
                    //    default: break;
                    //}

                    metalCostPerOz = GetSilverMetalCostPerOZ(CostBasis.SilverBasis.BasisPerOz, CostBasis.SilverBasis.EffectiveMarketPerOz, CostBasis.ItemExt.UsrContractIncrement);

                    //  var metalCostPerOz = GetSilverMetalCostPerOZ(CostBasis.SilverBasis.BasisPerOz, CostBasis.SilverBasis.EffectiveMarketPerOz, CostBasis.ItemExt.UsrContractIncrement);
                    cost += (itemExt.UsrPricingGRAMSilver ?? 0.00m) * metalCostPerOz / 31.10348m;// * (1.0000m + CostBasis.SilverBasis.LossPct) * (1.0000m + CostBasis.SilverBasis.SurchargePct);
                    //   marketCommodityCost += cost * (CostBasis.SilverBasis.MarketPerFineOz["SSS"] / CostBasis.SilverBasis.BasisPerFineOz["SSS"]);

                    msg += $"Silver Eff :{CostBasis.SilverBasis.EffectiveDate}{Environment.NewLine}";
                    msg += $"Silver Loss: {CostBasis.SilverBasis.LossPct}{Environment.NewLine}";
                    msg += $"Silver Sur : {CostBasis.SilverBasis.SurchargePct}{Environment.NewLine}";
                }
                this.CostRollupTotal[ASCIStarCostRollupType.Commodity] = cost;
                this.CostRollupTotal[ASCIStarCostRollupType.Materials] = itemExt.UsrOtherMaterialCost ?? 0.00m;
                this.CostRollupTotal[ASCIStarCostRollupType.Fabrication] = itemExt.UsrFabricationCost ?? 0.00m;
                this.CostRollupTotal[ASCIStarCostRollupType.Handling] = itemExt.UsrHandlingCost ?? 0.00m;
                this.CostRollupTotal[ASCIStarCostRollupType.Packaging] = itemExt.UsrPackagingCost ?? 0.00m;
                this.CostRollupTotal[ASCIStarCostRollupType.Other] = itemExt.UsrOtherCost ?? 0.00m;

                this.CostRollupTotal[ASCIStarCostRollupType.Shipping] = itemExt.UsrFreightCost ?? 0.00m;
                this.CostRollupTotal[ASCIStarCostRollupType.Labor] = itemExt.UsrLaborCost ?? 0.00m;
                this.CostRollupTotal[ASCIStarCostRollupType.Duty] = itemExt.UsrDutyCost ?? 0.00m;

                //decimal totalNoDuty = 0.00m;
                //foreach (string Key in CostRollupTotal.Keys)
                //{
                //    if (Key != CostRollupType.Duty && Key != CostRollupType.Labor && Key != CostRollupType.Shipping)
                //    {
                //        totalNoDuty += CostRollupTotal[Key];
                //    }
                //}
                //this.CostRollupTotal[CostRollupType.Duty] = (itemExt.UsrDutyCostPct ?? 0.0000m) / 100.0000m * totalNoDuty;
                PXTrace.WriteInformation(msg);
                //}
                //catch (Exception err)
                //{
                //    PXTrace.WriteInformation(msg);
                //    throw err;
                //}
            }


            #region Helpers Methods
            public decimal GetSilverMetalCostPerOZ(decimal basisCost, decimal marketCost, decimal? incrementNullable)
            {
                string msg = "";
                if (incrementNullable == 0.0m || incrementNullable == null)
                {
                    msg += $"basis    :{basisCost}{Environment.NewLine}increment:{incrementNullable}{Environment.NewLine}market    :{market}{Environment.NewLine}matrix    :{market}";
                    return marketCost;
                }
                decimal increment = (decimal)incrementNullable;
                decimal costPerOz = marketCost;
                decimal temp = (marketCost / increment) - (basisCost / increment);

                //decimal steps1 = Math.Round(temp, 0);
                //decimal steps2 = Math.Floor(temp);
                //decimal steps3 = Math.Ceiling(temp);
                decimal steps = Math.Truncate(temp);

                //  decimal steps = temp % 1 == 0 && temp > 1 && temp % 2 == 0 ? temp - 1 : Math.Truncate(temp);
                decimal floor = basisCost + (steps * increment);
                decimal ceiling = floor + increment;
                costPerOz = (floor + ceiling) / 2.000000m;
                msg += $"basis    :{basisCost}{Environment.NewLine}increment:{increment}{Environment.NewLine}market    :{marketCost}{Environment.NewLine}Steps     :{steps}{Environment.NewLine}floor     :{floor}{Environment.NewLine}ceiling   :{ceiling}{Environment.NewLine}matrix    :{market}";

                PXTrace.WriteInformation(msg);

                return costPerOz;
            }

            public decimal GetPurchaseCost(string costingType)
            {
                var itemExt = CostBasis.ItemExt;
                decimal value = 0m;
                if (CostRollupTotal.Any())
                {
                    CostRollupTotal.Keys.ForEach(key =>
                    {
                        //if (costingType == ASCIStarCostingType.WeightCost && key == ASCIStarCostRollupType.Labor)
                        //{
                        //    value += (decimal)itemExt.UsrActualGRAMGold * CostRollupTotal[key] + (decimal)itemExt.UsrActualGRAMSilver * CostRollupTotal[key];
                        //}
                        //else
                        //{
                        //    value += CostRollupTotal[key];
                        //}
                        if (key == "C")
                        {
                            decimal loss = 1.0m + itemExt.UsrContractLossPct.Value / 100;
                            decimal surcharge = 1.0m + itemExt.UsrContractSurcharge.Value / 100;

                            value += CostRollupTotal[key] * surcharge * loss;
                        }
                        else
                        {
                            value += CostRollupTotal[key];
                        }
                        //if (costingType == ASCIStarCostingType.WeightCost && key == ASCIStarCostRollupType.Labor && itemExt.UsrActualGRAMGold > 0)
                        //{
                        //    value += (decimal)itemExt.UsrActualGRAMGold * CostRollupTotal[ASCIStarCostRollupType.Labor];
                        //}
                        //else if (costingType == ASCIStarCostingType.WeightCost && key == ASCIStarCostRollupType.Labor && itemExt.UsrActualGRAMSilver > 0)
                        //{
                        //    value += (decimal)itemExt.UsrActualGRAMSilver * CostRollupTotal[ASCIStarCostRollupType.Labor];
                        //}
                        //else
                        //{
                        //    value += CostRollupTotal[key];
                        //}
                    });
                }
                return value;
            }
            #endregion

            ////public APVendorPrice MarketSilverPrice(PXGraph graph, int? MarketID, string UOM = "TOZ", string Fineness = "SSS")
            //public APVendorPrice MarketSilverPrice(PXGraph graph, int? MarketID, string UOM = "TOZ", string Fineness = "SSS", DateTime? effectiveDate = null)
            //{
            //    InventoryItem commodity = GetCommodityItem(graph, "SSS");              
            //    Vendor market = GetMarketVendor(graph, MarketID);
            //    return FindMarketPrice(graph, market, commodity, UOM, Fineness, effectiveDate ?? DateTime.Today);
            //}

            //public APVendorPrice MarketGoldPrice(PXGraph graph, int? MarketID, string UOM = "TOZ", string Fineness = "24K", DateTime? effectiveDate = null)
            //{
            //    InventoryItem commodity = GetCommodityItem(graph, "24K");
            //    Vendor market = GetMarketVendor(graph, MarketID);
            //    return FindMarketPrice(graph, market, commodity, UOM, Fineness, effectiveDate ?? DateTime.Today);
            //}

            //public APVendorPrice MarketPlatinumPrice(PXGraph graph, int? MarketID, string UOM = "TOZ", string Fineness = "9995", DateTime? effectiveDate = null)
            //{
            //    InventoryItem commodity = GetCommodityItem(graph, "PLT");
            //    Vendor market = GetMarketVendor(graph, MarketID);
            //    return FindMarketPrice(graph, market, commodity, UOM, Fineness, effectiveDate ?? DateTime.Today);
            //}

            //public APVendorPrice FindMarketPrice(PXGraph graph, Vendor Market, InventoryItem commodity, string UOM, string Fineness, DateTime effectiveDate)
            //{
            //    APVendorPrice marketPrice = new APVendorPrice();
            //    string msg = "";
            //    msg += $"Market   :{Market.AcctCD}{System.Environment.NewLine}";
            //    msg += $"Commodity:{commodity.InventoryCD}{System.Environment.NewLine}";
            //    msg += $"UOM      :{UOM}{System.Environment.NewLine}";
            //    msg += $"Fineness :{Fineness}{System.Environment.NewLine}";
            //    msg += $"Date     :{effectiveDate.ToString("MM/dd/yyyy")}{System.Environment.NewLine}";




            //    marketPrice =
            //    new PXSelect<APVendorPrice,
            //    Where<APVendorPrice.vendorID, Equal<Required<APVendorPrice.vendorID>>,
            //        And<APVendorPrice.inventoryID, Equal<Required<APVendorPrice.inventoryID>>,
            //        And<APVendorPrice.uOM, Equal<Required<APVendorPrice.uOM>>,
            //        And<APVendorPrice.effectiveDate, LessEqual<Required<APVendorPrice.effectiveDate>>,
            //        And<APVendorPrice.effectiveDate, LessEqual<Required<APVendorPrice.effectiveDate>>>>>>>,
            //    OrderBy<Desc<APVendorPrice.effectiveDate>>>(graph).SelectSingle(
            //        Market.BAccountID,
            //        commodity.InventoryID,
            //        "TOZ",
            //        effectiveDate,
            //        effectiveDate);

            //    if (marketPrice == null)
            //    {
            //        throw new PXException($"{Market.AcctCD} does not contain a valid price for {commodity.InventoryCD} on {effectiveDate.ToString("MM/dd/yyyy")}.");
            //    }
            //    PXTrace.WriteInformation(msg);
            //    msg += $"Price    :{marketPrice.SalesPrice} on {marketPrice.EffectiveDate} for effective date {effectiveDate.ToString("MM/dd/yyyy")}."{ System.Environment.NewLine}
            //    ";
            //    msg += $"Effective:{marketPrice.SalesPrice} on {marketPrice.EffectiveDate} for effective date {effectiveDate.ToString("MM/dd/yyyy")}."{ System.Environment.NewLine}
            //    ";
            //    PXTrace.WriteInformation($"{Market.AcctCD} priced {commodity.InventoryCD} at {marketPrice.SalesPrice} on {marketPrice.EffectiveDate} for effective date {effectiveDate.ToString("MM/dd/yyyy")}.");
            //    if (UOM != "TOZ") //Convert To UNIT OF MEASURE
            //    {
            //        INUnit conv = ConvertPrice(graph, UOM, "TOZ");
            //        PXTrace.WriteInformation($"marketPrice.SalesPrice:{marketPrice.SalesPrice}");
            //        PXTrace.WriteInformation($"conv.UnitRate:{conv.UnitRate}");

            //        if (conv.UnitMultDiv == "M")
            //            marketPrice.SalesPrice = marketPrice.SalesPrice * conv.UnitRate;
            //        else
            //            marketPrice.SalesPrice = marketPrice.SalesPrice / conv.UnitRate;
            //        marketPrice.UOM = UOM;

            //    }
            //    return marketPrice;

            //}

            //public static INUnit ConvertPrice(PXGraph graph, string FromUnit, string ToUnit)
            //{
            //    INUnit conv =
            //    new PXSelect<INUnit,
            //    Where<INUnit.fromUnit, Equal<Required<INUnit.fromUnit>>,
            //        And<INUnit.toUnit, Equal<Required<INUnit.toUnit>>>>,
            //    OrderBy<Desc<APVendorPrice.effectiveDate>>>(graph).SelectSingle(
            //        FromUnit,
            //        ToUnit);
            //    if (conv == null)
            //        throw new PXException($"No Conversion Found - CHeck Units Of Measure for {FromUnit} to {ToUnit}");
            //    return conv;
            //}

            //public static INUnit GetFinenessConversionRate(PXGraph graph, string FromUnit, string ToUnit)
            //{
            //    INUnit conv =
            //    new PXSelect<INUnit,
            //    Where<INUnit.fromUnit, Equal<Required<INUnit.fromUnit>>,
            //        And<INUnit.toUnit, Equal<Required<INUnit.toUnit>>>>,
            //    OrderBy<Desc<APVendorPrice.effectiveDate>>>(graph).SelectSingle(
            //        FromUnit,
            //        ToUnit);
            //    return conv;
            //}
            //public static APVendorPrice FindMarketPrice(PXGraph graph, APVendorPrice price)

            //{
            //    if (price.EffectiveDate == null)
            //        price.EffectiveDate = DateTime.Today;

            //    ASCIStarAPVendorPriceExt MarketPriceExt = price.GetExtension<ASCIStarAPVendorPriceExt>();
            //    InventoryItem Commodity = InventoryItem.PK.Find(graph, price.InventoryID);

            //    APVendorPrice MarketPrice =
            //        new PXSelect<APVendorPrice,
            //        Where<APVendorPrice.vendorID, Equal<Required<APVendorPrice.vendorID>>,
            //            And<APVendorPrice.inventoryID, Equal<Required<APVendorPrice.inventoryID>>,
            //            And<APVendorPrice.uOM, Equal<Required<APVendorPrice.uOM>>>>>,
            //        OrderBy<Desc<APVendorPrice.effectiveDate>>>(graph).SelectSingle(
            //            price.VendorID,
            //            price.InventoryID,
            //            price.UOM);
            //    return MarketPrice;
            //}

            //public static JewelryCost Fetch(PXGraph graph, int? vendorID, int? vendorLocationID, DateTime? docDate, string curyID, string baseCuryID, int? inventoryID, int? subItemID, int? siteID, string uom, bool onlyVendor = false)
            //{
            //    PXSelectBase<InventoryItem> vendorCostSelect =
            //        new PXSelectReadonly2<InventoryItem,
            //            LeftJoin<INItemCost, On<INItemCost.inventoryID, Equal<InventoryItem.inventoryID>,
            //                And<INItemCost.curyID, Equal<Required<INItemCost.curyID>>>>,
            //        LeftJoin<POVendorInventory, On<
            //            POVendorInventory.inventoryID, Equal<InventoryItem.inventoryID>,
            //            And<POVendorInventory.active, Equal<True>,
            //                        And<POVendorInventory.vendorID, Equal<Required<Vendor.bAccountID>>,
            //                        And<POVendorInventory.curyID, Equal<Required<POVendorInventory.curyID>>,
            //            And2<Where<POVendorInventory.subItemID, Equal<Required<POVendorInventory.subItemID>>,
            //                                    Or<POVendorInventory.subItemID, Equal<InventoryItem.defaultSubItemID>,
            //                                    Or<POVendorInventory.subItemID, IsNull,
            //                                    Or<Where<Required<POVendorInventory.subItemID>, IsNull,
            //                                         And<POVendorInventory.subItemID, Equal<True>>>>>>>,
            //                     And2<Where<POVendorInventory.purchaseUnit, Equal<Required<POVendorInventory.purchaseUnit>>>,
            //                     And<Where<POVendorInventory.vendorLocationID, Equal<Required<POVendorInventory.vendorLocationID>>,
            //                                    Or<POVendorInventory.vendorLocationID, IsNull>>>>>>>>>>>,
            //            Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>,
            //            OrderBy<
            //            Asc<Switch<Case<Where<POVendorInventory.purchaseUnit, Equal<InventoryItem.purchaseUnit>>, True>, False>,
            //            Asc<Switch<Case<Where<POVendorInventory.subItemID, Equal<InventoryItem.defaultSubItemID>>, True>, False>,
            //            Asc<Switch<Case<Where<POVendorInventory.vendorLocationID, IsNull>, True>, False>,
            //            Asc<InventoryItem.inventoryCD>>>>>>(graph);

            //    Func<string, PXResult<InventoryItem, INItemCost, POVendorInventory>> selectVendorCostByUOM =
            //        uomParam => vendorCostSelect
            //            .Select(baseCuryID, vendorID, curyID, subItemID, subItemID, uomParam, vendorLocationID, inventoryID).AsEnumerable()
            //            .FirstOrDefault(r => r.GetItem<POVendorInventory>() != null)
            //            as PXResult<InventoryItem, INItemCost, POVendorInventory>;

            //    var vendorCostRow = selectVendorCostByUOM(uom);
            //    var item = vendorCostRow.GetItem<InventoryItem>();

            //    Func<POVendorInventory, JewelryCost> fetchVendorLastCost =
            //        vendorPrice => vendorPrice.LastPrice != null && vendorPrice.LastPrice != 0m
            //            ? new JewelryCost(graph, item, vendorPrice.LastPrice.Value, vendorPrice.LastPrice.Value, vendorPrice.VendorID, null, DateTime.Today
            //            , vendorPrice.PurchaseUnit, curyID, false)
            //            : null;

            //    return fetchVendorLastCost(vendorCostRow.GetItem<POVendorInventory>())
            //        ?? fetchVendorLastCost(selectVendorCostByUOM(item.BaseUnit).GetItem<POVendorInventory>())
            //        ?? (onlyVendor ? null : FetchStdCost(graph, item, baseCuryID, docDate))
            //        ?? (onlyVendor ? null : FetchSiteLastCost(graph, item, siteID, baseCuryID))
            //        ?? new JewelryCost(graph, item, vendorCostRow.GetItem<INItemCost>()?.LastCost ?? 0);
            //}



            //private static JewelryCost FetchStdCost(PXGraph graph, InventoryItem item, string baseCuryID, DateTime? docDate)
            //{
            //    if (item.StkItem == false || item.ValMethod == INValMethod.Standard)
            //    {
            //        InventoryItemCurySettings curySettings =
            //            InventoryItemCurySettings.PK.Find(graph, item.InventoryID, baseCuryID ?? CurrencyCollection.GetBaseCurrency()?.CuryID);

            //        if (curySettings == null) return null;

            //        if (!docDate.HasValue || (curySettings.StdCostDate.HasValue && curySettings.StdCostDate.Value <= docDate.Value))
            //        {
            //            return new JewelryCost(graph, item, curySettings.StdCost.Value);
            //        }
            //    }
            //    return null;
            //}

            //private static JewelryCost FetchSiteLastCost(PXGraph graph, InventoryItem item, int? siteID, string baseCuryID)
            //{
            //    INItemStats itemStats;
            //    if (siteID != null)
            //    {
            //        itemStats = PXSelect<INItemStats,
            //             Where<INItemStats.inventoryID, Equal<Required<INItemStats.inventoryID>>,
            //            And<INItemStats.siteID, Equal<Required<INItemStats.siteID>>>>>
            //            .Select(graph, item.InventoryID, siteID);
            //    }
            //    else
            //    {
            //        itemStats = PXSelectJoin<INItemStats,
            //            InnerJoin<INSite, On<INItemStats.FK.Site>>,
            //            Where<INItemStats.inventoryID, Equal<Required<INItemStats.inventoryID>>,
            //                And<INSite.baseCuryID, Equal<Required<INSite.baseCuryID>>>>,
            //            OrderBy<Desc<INItemStats.lastCostDate>>>
            //            .Select(graph, item.InventoryID, baseCuryID);
            //    }

            //    if (itemStats?.LastCost != null)
            //        return new JewelryCost(graph, item, itemStats.LastCost.Value);

            //    return null;
            //}

            //public static int? FetchLocation(PXGraph graph, int? vendorID, int? itemID, int? subItemID, int? siteID)
            //{
            //    BAccountR company = PXSelectJoin<BAccountR,
            //        InnerJoin<Branch, On<Branch.bAccountID, Equal<BAccountR.bAccountID>>>,
            //        Where<BAccountR.bAccountID, Equal<Required<BAccountR.bAccountID>>>>.Select(graph, vendorID);
            //    if (company != null)
            //    {
            //        return company.DefLocationID;
            //    }

            //    Vendor vendor = PXSelectReadonly<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(graph, vendorID);

            //    var record =
            //                PXSelectJoin<INItemSiteSettings,
            //                        LeftJoin<POVendorInventory,
            //                            On<POVendorInventory.inventoryID, Equal<INItemSiteSettings.inventoryID>,
            //                            And<POVendorInventory.active, Equal<boolTrue>,
            //                            And<POVendorInventory.vendorID, Equal<Required<Vendor.bAccountID>>,
            //                            And<Where<POVendorInventory.subItemID, Equal<Required<POVendorInventory.subItemID>>,
            //                                    Or<POVendorInventory.subItemID, Equal<INItemSiteSettings.defaultSubItemID>,
            //                                    Or<POVendorInventory.subItemID, IsNull,
            //                                    Or<Where<Required<POVendorInventory.subItemID>, IsNull,
            //                                         And<POVendorInventory.subItemID, Equal<True>>>>>>>>>>>>,
            //                    Where<INItemSiteSettings.inventoryID, Equal<Required<INItemSiteSettings.inventoryID>>,
            //                        And<INItemSiteSettings.siteID, Equal<Required<INItemSiteSettings.siteID>>>>>
            //                        .Select(graph, vendorID, subItemID, subItemID, itemID, siteID).ToArray()
            //                        .Select(r => new { Item = r.GetItem<POVendorInventory>(), Site = r.GetItem<INItemSiteSettings>() })
            //                        .Where(r => r.Item != null && r.Site != null)
            //                        .OrderBy(r => r.Item.LastPrice)
            //                        .ThenByDescending(r => r.Item.SubItemID == r.Site.DefaultSubItemID)
            //                        .ThenByDescending(r => r.Item.VendorLocationID != null)
            //                        .ThenByDescending(r => r.Item.IsDefault == true)
            //                        .ThenByDescending(r => r.Item.VendorLocationID == vendor?.DefLocationID)
            //                        .FirstOrDefault();

            //    if (record == null)
            //        return null;
            //    if (record.Item.VendorLocationID != null)
            //        return record.Item.VendorLocationID;
            //    if (record.Site.PreferredVendorID == vendorID)
            //        return record.Site.PreferredVendorLocationID ?? vendor?.DefLocationID;
            //    if (vendor != null && vendor.BAccountID == vendorID)
            //        return vendor.DefLocationID;
            //    return null;
            //}

            //public static void Update(PXGraph graph, int? vendorID, int? vendorLocationID, string curyID, int? inventoryID, int? subItemID, string uom, decimal curyCost)
            //{
            //    if (curyCost <= 0 || string.IsNullOrEmpty(uom) ||
            //        vendorID == null ||
            //        vendorLocationID == null) return;

            //    PXCache cache = graph.Caches[typeof(POVendorInventoryPriceUpdate)];

            //    foreach (PXResult<InventoryItem, Vendor, Company> r in
            //        PXSelectReadonly2<InventoryItem,
            //            LeftJoinSingleTable<Vendor,
            //                         On<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>,
            //            CrossJoin<Company>>,
            //            Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.
            //            Select(graph, vendorID, inventoryID))
            //    {
            //        InventoryItem item = r;
            //        Vendor vendor = r;
            //        Company company = r;
            //        if (item.InventoryID == null || vendor.BAccountID == null ||
            //            (item.StkItem == true && subItemID == null)) continue;
            //        INSetup setup = PXSetupOptional<INSetup>.Select(graph);

            //        int? savedSubItemID = item.StkItem == true ? subItemID : null;

            //        POVendorInventoryPriceUpdate vendorPrice = (POVendorInventoryPriceUpdate)cache.CreateInstance();
            //        vendorPrice.InventoryID = inventoryID;
            //        vendorPrice.SubItemID = savedSubItemID;
            //        vendorPrice.VendorID = vendorID;
            //        vendorPrice.VendorLocationID = vendorLocationID;
            //        vendorPrice.PurchaseUnit = uom;
            //        vendorPrice = (POVendorInventoryPriceUpdate)cache.Insert(vendorPrice);
            //        if (item.StkItem != true) vendorPrice.SubItemID = savedSubItemID;
            //        vendorPrice.CuryID = curyID;
            //        cache.Normalize();
            //        vendorPrice.Active = true;
            //        vendorPrice.LastPrice = curyCost;
            //    }
            //}



            //public void UpdateDependents(PXCache cache, int? kitInventoryID)
            //{

            //    InventoryItem item = InventoryItem.PK.Find(cache.Graph, kitInventoryID);
            //    if (item == null)
            //        return;


            //    INKitSpecHdr kitItem = INKitSpecHdr.PK.Find(cache.Graph, kitInventoryID);


            //    INKitSpecStkDet stkDet = StockLines.Select(cache.Graph, kitInventoryID);

            //    APVendorPrice retPrice = new APVendorPrice();
            //    if (item == null)
            //        return;



            //}


            //public APVendorPrice GetCommodityPrice(PXCache cache, int? inventoryID, string UOM, int? vendorID, DateTime? effectiveDate = null)
            //{
            //    APVendorPrice retPrice = new APVendorPrice();
            //    InventoryItem item = InventoryItem.PK.Find(cache.Graph, inventoryID);
            //    if (item == null)
            //        return retPrice;
            //    if (effectiveDate == null)
            //        effectiveDate = DateTime.Today;
            //    PXTrace.WriteInformation($"{item.InventoryCD} Exists, Checking Commodity");
            //    INItemClass itemClass = INItemClass.PK.Find(cache.Graph, item.ItemClassID);
            //    PXTrace.WriteInformation($"Class {itemClass.ItemClassCD} Found");
            //    if (itemClass.ItemClassCD.Trim() == "COMMODITY")
            //    {
            //        PXTrace.WriteInformation("Item is Class Commodity");

            //        Vendor vendor;
            //        if (vendorID == null)
            //        {
            //            PXTrace.WriteInformation("No Market Selected, defaulting LONDON PM");
            //            vendor = PXSelectReadonly<Vendor, Where<Vendor.acctCD, Equal<Required<Vendor.acctCD>>>>.Select(cache.Graph, "LONDON PM");
            //            if (vendor == null)
            //                throw new PXException("LONDON PM Vendor missing and no vendor provided");
            //            vendorID = vendor.BAccountID;

            //        }
            //        else
            //        {
            //            vendor = PXSelectReadonly<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(cache.Graph, vendorID);
            //        }
            //        PXTrace.WriteInformation($"item/ID:{item.InventoryCD}/{inventoryID}, vendor/ID:{vendor}/{vendorID}, effectiveDate:{effectiveDate}");
            //        APVendorPrice price = (APVendorPrice)CommodityPrice.SelectSingle(cache.Graph, inventoryID, vendorID, effectiveDate);
            //        if (price == null)
            //        {
            //            PXTrace.WriteInformation($"No Price found for InventoryID:{inventoryID}, vendorID:{vendorID}, effectiveDate:{effectiveDate}");
            //        }
            //        else
            //        {
            //            PXTrace.WriteInformation($"{price.EffectiveDate}: {price.SalesPrice} per {price.UOM}");
            //            ASCIStarAPVendorPriceExt priceext = cache.GetExtension<ASCIStarAPVendorPriceExt>(price);
            //            retPrice = price;
            //        }

            //    }
            //    else
            //    {
            //        PXTrace.WriteInformation("Item is Not a Commodity");
            //    }
            //    return retPrice;
            //}

        }
    }
}