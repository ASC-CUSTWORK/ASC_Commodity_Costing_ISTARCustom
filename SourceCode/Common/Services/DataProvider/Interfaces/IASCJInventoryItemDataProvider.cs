using PX.Objects.IN;
using System.Collections.Generic;

namespace ASCJewelryLibrary.Common.Services.DataProvider.Interfaces
{
    public interface IASCJInventoryItemDataProvider
    {
        InventoryItem GetInventoryItemByID(int? inventoryID);
        InventoryItem GetInventoryItemByCD(string inventoryCD);
        IEnumerable<InventoryItem> GetInventoryItems();
        INItemClass GetItemClassByID(int? itemClassID);
    }
}
