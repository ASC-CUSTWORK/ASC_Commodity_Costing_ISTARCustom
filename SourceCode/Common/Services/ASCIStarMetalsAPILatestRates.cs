using ASCISTARCustom.Common.Descriptor;
using ASCISTARCustom.Common.Models;
using ASCISTARCustom.Common.Services.Interfaces;
using PX.Data;
using System.Collections.Generic;
using static ASCISTARCustom.Preferences.Descriptor.ASCIStarSymbols;

namespace ASCISTARCustom.Common.Services
{
    /// <summary>
    /// A service class that provides methods for retrieving inverse rate values for different metals from the ASCIStar API.
    /// Implements the IASCIStarMetalsAPILatestRates interface.
    /// Uses an instance of the ASCIStarRESTService class to send requests to the API.
    /// Validates that the symbol parameter is specified before sending a request, and throws a PXException if it is not.
    /// Implements a virtual GetInverseRateValue method that retrieves the inverse rate value for a specified currency and symbol from the API by sending a GET request to the Latest Rates endpoint with the specified parameters.
    /// Uses the TryGetValue method to get the rate value for the specified symbol from the Rates collection in the response, and sets the value to zero if the rate is not found.
    /// Returns the inverse of the rate value as a decimal if the value is not zero, and returns 0 if the value is zero.
    /// Implements private static Parameters and IsSymbolSpecified methods that respectively create an array of key-value pairs representing API request parameters and check whether the specified symbol is included in the symbols list in the ASCIStar setup details.
    /// </summary>
    public class ASCIStarMetalsAPILatestRates : IASCIStarMetalsAPILatestRates
    {
        private readonly IASCIStarRESTService _starRESTService;
        private readonly PXGraph _graph;

        public ASCIStarMetalsAPILatestRates(PXGraph graph)
        {
            _graph = graph;
            _starRESTService = new ASCIStarRESTService(_graph);
        }

        public decimal GetLBXAGRate(string currency)
        {
            return GetInverseRateValue(currency, LBXAG);
        }

        public decimal GetLBXAUAMRate(string currency)
        {
            return GetInverseRateValue(currency, LBXAUAM);
        }

        public decimal GetLBXAUPMRate(string currency)
        {
            return GetInverseRateValue(currency, LBXAUPM);
        }

        public decimal GetXAGRate(string currency)
        {
            return GetInverseRateValue(currency, XAG);
        }

        public decimal GetXAURate(string currency)
        {
            return GetInverseRateValue(currency, XAU);
        }

        /// <summary>
        /// Virtual method that gets the inverse rate value of the specified currency and symbol from the ASCIStar API.
        /// Validates that the symbol parameter is specified, and throws a PXException if it is not.
        /// Sends a GET request to the Latest Rates endpoint with the specified currency and symbol parameters, and retrieves the result as an instance of the ASCIStarLatestRatesModel class.
        /// Tries to get the rate value for the specified symbol from the Rates collection in the response using the TryGetValue method, and sets the value to zero if the rate is not found.
        /// If the rate value is not zero, returns the inverse of the value as a decimal.
        /// If the value is zero, returns 0.
        /// </summary>
        /// <param name="currency">The currency to retrieve the inverse rate value for.</param>
        /// <param name="symbol">The symbol to retrieve the inverse rate value for.</param>
        /// <returns>The inverse rate value as a decimal.</returns>
        /// <exception cref="PXException">Thrown when the symbol parameter is not specified.</exception>
        public virtual decimal GetInverseRateValue(string currency, string symbol)
        {
            if (!IsSymbolSpecified(symbol))
            {
                throw new PXException(ASCIStarMessages.Error.SymbolNotSpecified);
            }

            var result = _starRESTService.Get<ASCIStarLatestRatesModel>(ASCIStarEndpoints.LatestRates, Parameters(currency, symbol));
            var value = result.Rates.TryGetValue(symbol, out double rateValue) ? rateValue : 0;

            return value != 0 ? 1 / (decimal)value : 0;
        }

        /// <summary>
        /// Private static method that creates an array of key-value pairs representing the parameters for an ASCIStar API request.
        /// The method creates two parameters: Base and Symbols, with the specified currency and symbol values as their respective values.
        /// </summary>
        /// <param name="currency">The currency to use as the base currency parameter.</param>
        /// <param name="symbol">The symbol to use as the symbols parameter.</param>
        /// <returns>An array of key-value pairs representing the API request parameters.</returns>
        private static KeyValuePair<string, string>[] Parameters(string currency, string symbol) =>
            new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>(ASCIStarQueryParams.Base, currency),
                new KeyValuePair<string, string>(ASCIStarQueryParams.Symbols, symbol),
            };

        private bool IsSymbolSpecified(string symbol)
        {
            var result = _starRESTService.GetASCIStarSetup();
            if (string.IsNullOrEmpty(symbol) || result?.Symbols == null)
            {
                return false;
            }

            return result.Symbols.Contains(symbol);
        }
    }
}
