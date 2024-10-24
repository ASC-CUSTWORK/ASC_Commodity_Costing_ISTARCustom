using PX.Objects.IN;
using System.Collections.Generic;

namespace ASCJSMCustom.Common.Services.DataProvider.Interfaces
{
    public interface IASCJSMInventoryItemDataProvider
    {
        InventoryItem GetInventoryItemByID(int? inventoryID);
        InventoryItem GetInventoryItemByCD(string inventoryCD);
        IEnumerable<InventoryItem> GetInventoryItems();
        INItemClass GetItemClassByID(int? itemClassID);
    }
}
