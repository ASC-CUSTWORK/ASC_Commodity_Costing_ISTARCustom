using ASCJSMCustom.Common.Services.DataProvider.Interfaces;
using PX.Data;
using PX.Objects.IN;
using System.Collections.Generic;

namespace ASCJSMCustom.Common.Services.DataProvider
{
    public class ASCJSMInventoryItemDataProvider : IASCJSMInventoryItemDataProvider
    {
        private readonly PXGraph _graph;

        public ASCJSMInventoryItemDataProvider(PXGraph graph)
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
