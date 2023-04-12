using ASCISTARCustom.Common.Services.DataProvider.Interfaces;
using PX.Data;
using PX.Objects.IN;
using System.Collections.Generic;

namespace ASCISTARCustom.Common.Services.DataProvider
{
    public class ASCIStarInventoryItemDataProvider : IASCIStarInventoryItemDataProvider
    {
        private readonly PXGraph _graph;

        public ASCIStarInventoryItemDataProvider(PXGraph graph)
        {
            _graph = graph;
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
