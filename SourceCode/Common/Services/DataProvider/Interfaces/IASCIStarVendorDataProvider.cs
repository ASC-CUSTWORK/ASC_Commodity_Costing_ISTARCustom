using PX.Data;
using PX.Objects.AP;
using System;

namespace ASCJewelryLibrary.Common.Services.DataProvider.Interfaces
{
    public interface IASCJVendorDataProvider
    {
        /// <summary>
        /// Retrieves the APVendorPrice record for the specified vendor, inventory item, unit of measure, and effective date.
        /// Sends a SELECT query to the APVendorPrice table, filtering the results by the specified parameters.
        /// If the withException parameter is true and the result is null, throws a PXException with the message "Vendor price not found".
        /// </summary>
        /// <param name="bAccountID">The ID of the vendor.</param>
        /// <param name="inventoryID">The ID of the inventory item.</param>
        /// <param name="UOM">The unit of measure.</param>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="withException">Whether to throw an exception if the result is null. Default is false.</param>
        /// <returns>The APVendorPrice record if found, or null if not found and withException is false.</returns>
        /// <exception cref="PXException">Thrown when the specified APVendorPrice record is not found and the withException parameter is set to true.</exception>
        APVendorPrice GetAPVendorPrice(int? bAccountID, int? inventoryID, string UOM, DateTime? effectiveDate, bool withException = false);

        /// <summary>
        /// Retrieves a Vendor record with the specified Business Account ID from the database.
        /// Executes a PXSelect statement to retrieve the record and returns the result as a Vendor object.
        /// If the Vendor record is not found and the withException parameter is set to true, throws a PXException with an appropriate error message.
        /// </summary>
        /// <param name="bAccountID">The Business Account ID of the Vendor to retrieve.</param>
        /// <param name="withException">A boolean parameter that indicates whether to throw an exception if the Vendor record is not found. Defaults to false.</param>
        /// <returns>The Vendor record with the specified Business Account ID.</returns>
        /// <exception cref="PXException">Thrown when the specified Vendor record is not found and the withException parameter is set to true.</exception>
        Vendor GetVendor(int? bAccountID, bool withException = false);
    }
}
