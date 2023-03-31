using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using System;
using System.Linq;
using System.Collections.Generic;
using ASCISTARCustom.Cost.Descriptor;

namespace ASCISTARCustom
{
	public static class ASCIStarCostManager
	{
		public class ItemPrice
		{
			public ItemPrice(InventoryItem item, decimal cost)
				: this(item, null, null, cost, cost, true)
			{
			}
			public ItemPrice(InventoryItem item, string uom, string curyID, decimal cost, bool convertCury)
				: this(item, uom, curyID, cost, cost, convertCury)
			{
			}
			public ItemPrice(InventoryItem item, string uom, string curyID, decimal cost, decimal baseCost, bool convertCury)
			{
				this.Item = item;
				this.uom = uom;
				this.CuryID = curyID;
				this.Cost = cost;
				this.BaseCost = baseCost;
				this.ConvertCury = convertCury;

				ASCIStarINInventoryItemExt itemExt = item.GetExtension<ASCIStarINInventoryItemExt>();
				this.costingType = itemExt.UsrCostingType;
				this.costRollupType = itemExt.UsrCostRollupType;

				this.commodityType = CommodityType.Undefined;
				this.market = MarketList.LondonPM;

				this.SubItemPrice = new List<ItemPrice>();
			}




			public string UOM
			{
				get
				{
					return this.Item != null ? this.uom ?? this.Item.BaseUnit : null;
				}
			}

			private readonly string uom;

			public readonly InventoryItem Item;
			public readonly int level;
			public readonly string CuryID;
			public readonly decimal Cost;
			public readonly decimal BaseCost;
			public readonly bool ConvertCury;
			public readonly string costingType;
			public readonly string costRollupType;
			public readonly string commodityType;
			public readonly string market;
			public readonly List<ItemPrice> SubItemPrice;
			public readonly Dictionary<ASCIStarCostRollupType, decimal> CostRollupTotal;

			public readonly ASCIStarAPVendorPriceExt vendorPrice; //What the Vendor is Charging for the item
			public readonly ASCIStarAPVendorPriceExt commodityPrice;






		public decimal Convert<InventoryIDField>(PXGraph graph, object inventoryRow, PX.Objects.CM.Extensions.CurrencyInfo currencyInfo, string uom)
			where InventoryIDField : IBqlField
			{
				ItemPrice price = this;
				if (price == null || price.Cost == 0 || price.Item == null || inventoryRow == null)
					return 0;

				decimal result = (ConvertCury && currencyInfo != null) ? currencyInfo.CuryConvCuryRaw(price.BaseCost) : price.BaseCost;

				if (price.UOM != uom && !string.IsNullOrEmpty(uom))
				{
					if (inventoryRow == null) return 0;

					PXCache invCache = graph.Caches[inventoryRow.GetType()];
					decimal baseUOM =
						price.UOM != price.Item.BaseUnit ?
						INUnitAttribute.ConvertFromBase<InventoryIDField>(invCache, inventoryRow, price.UOM, result, INPrecision.UNITCOST) :
						result;

					result =
						uom != price.Item.BaseUnit ?
						INUnitAttribute.ConvertToBase<InventoryIDField>(invCache, inventoryRow, uom, baseUOM, INPrecision.UNITCOST) :
						baseUOM;
				}

				return result;
			}
		}

	}
}