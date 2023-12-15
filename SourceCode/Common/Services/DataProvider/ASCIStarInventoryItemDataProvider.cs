using ASCJewelryLibrary.Common.Services.DataProvider.Interfaces;
using PX.Data;
using PX.Objects.IN;
using System.Collections.Generic;

namespace ASCJewelryLibrary.Common.Services.DataProvider
{
    public class ASCJInventoryItemDataProvider : IASCJInventoryItemDataProvider
    {
        private readonly PXGraph _graph;

        public ASCJInventoryItemDataProvider(PXGraph graph)
        {
            _graph = graph;
        }

        public INItemClass GetItemClassByID(int? itemClassID)
        {
            return PXSelect<
                INItemClass, 
                Where<INItemClass.itemClassID, Equal<Required<INItemClass.itemClassID>>>>
                .Select(_graph, itemClassID);
        }

        public InventoryItem GetInventoryItemByCD(string inventoryCD)
        {
            return PXSelect<
                InventoryItem,
                Where<InventoryItem.inventoryCD, Equal<Required<InventoryItem.inventoryCD>>>>
                .Select(_graph, inventoryCD);
        }

        public InventoryItem GetInventoryItemByID(int? inventoryID)
        {
            return PXSelect<
                InventoryItem,
                Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                .Select(_graph, inventoryID);
        }

        public IEnumerable<InventoryItem> GetInventoryItems()
        {
            return PXSelect<InventoryItem>.Select(_graph).FirstTableItems;
        }
    }
}
