using PX.Data;

namespace ASCISTARCustom.Common.Services.REST.Interfaces
{
    public interface IASCIStarMetalsAPILatestRateService
    {
        /// <summary>
        /// Retrieves the inverse rate value for the LBXAG(LBMA Silver) symbol and the specified currency.
        /// </summary>
        /// <param name="currency">The currency to retrieve the inverse rate value for.</param>
        /// <returns>The inverse rate value for the LBXAG(LBMA Silver) symbol and the specified currency as a decimal.</returns>
        /// <exception cref="PXException">Thrown when the symbol parameter is not specified.</exception>
        decimal GetLBXAGRate(string currency);

        /// <summary>
        /// Retrieves the inverse rate value for the LBXAUAM(LBMA Gold Am) symbol and the specified currency.
        /// </summary>
        /// <param name="currency">The currency to retrieve the inverse rate value for.</param>
        /// <returns>The inverse rate value for the LBXAUAM(LBMA Gold Am) symbol and the specified currency as a decimal.</returns>
        /// <exception cref="PXException">Thrown when the symbol parameter is not specified.</exception>
        decimal GetLBXAUAMRate(string currency);

        /// <summary>
        /// Retrieves the inverse rate value for the LBXAUPM(LBMA Gold Pm) symbol and the specified currency.
        /// </summary>
        /// <param name="currency">The currency to retrieve the inverse rate value for.</param>
        /// <returns>The inverse rate value for the LBXAUPM(LBMA Gold Pm) symbol and the specified currency as a decimal.</returns>
        /// <exception cref="PXException">Thrown when the symbol parameter is not specified.</exception>
        decimal GetLBXAUPMRate(string currency);

        /// <summary>
        /// Retrieves the inverse rate value for the XAU(Gold) symbol and the specified currency.
        /// </summary>
        /// <param name="currency">The currency to retrieve the inverse rate value for.</param>
        /// <returns>The inverse rate value for the XAU(Gold) symbol and the specified currency as a decimal.</returns>
        /// <exception cref="PXException">Thrown when the symbol parameter is not specified.</exception>
        decimal GetXAURate(string currency);

        /// <summary>
        /// Retrieves the inverse rate value for the XAG(Silver) symbol and the specified currency.
        /// </summary>
        /// <param name="currency">The currency to retrieve the inverse rate value for.</param>
        /// <returns>The inverse rate value for the XAG(Silver) symbol and the specified currency as a decimal.</returns>
        /// <exception cref="PXException">Thrown when the symbol parameter is not specified.</exception>
        decimal GetXAGRate(string currency);
    }
}
