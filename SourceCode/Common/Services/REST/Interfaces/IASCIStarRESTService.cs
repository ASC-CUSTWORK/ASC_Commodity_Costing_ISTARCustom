using ASCISTARCustom.Common.Models;
using ASCISTARCustom.Cost.DAC;
using System.Collections.Generic;

namespace ASCISTARCustom.Common.Services.REST.Interfaces
{
    public interface IASCIStarRESTService
    {
        /// <summary>
        /// Sends a GET request to the specified endpoint and returns the response as an object of type TModel.
        /// </summary>
        /// <typeparam name="TModel">The type of the object to deserialize the response into.</typeparam>
        /// <param name="endpoint">The endpoint to send the request to.</param>
        /// <returns>An instance of TModel deserialized from the response.</returns>
        /// <exception cref="JsonSerializationException">Thrown when deserialization fails.</exception>
        /// <exception cref="ASCIStarStatusCodeException">Thrown when the response has a non-OK status code.</exception>
        /// <exception cref="PXException">Thrown when an ASCIStarStatusCodeException is caught, to indicate a remote server error.</exception>
        TModel Get<TModel>(string endpoint, params KeyValuePair<string, string>[] parameters) where TModel : IASCIStarModel;

        ASCIStarSetup GetASCIStarSetup();
    }
}
