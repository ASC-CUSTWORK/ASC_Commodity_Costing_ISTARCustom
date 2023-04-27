using PX.Objects.IN;
using System.Collections.Generic;

namespace ASCISTARCustom.Common.Services.DataProvider.Interfaces
{
    public interface IASCIStarInventoryItemDataProvider
    {
        InventoryItem GetInventoryItemByID(int? inventoryID);
        InventoryItem GetInventoryItemByCD(string inventoryCD);
        IEnumerable<InventoryItem> GetInventoryItems();
        INItemClass GetItemClassByID(int? itemClassID);
    }
}
