using ASCJewelryLibrary.Common.Descriptor;
using ASCJewelryLibrary.Common.Helper;
using ASCJewelryLibrary.Common.Helper.Exceptions;
using ASCJewelryLibrary.Common.Helper.Extensions;
using ASCJewelryLibrary.Common.Models;
using ASCJewelryLibrary.Common.Services.REST.Interfaces;
using ASCJewelryLibrary.AP.DAC;
using PX.Data;
using RestSharp;
using System.Collections.Generic;
using System.Linq;

namespace ASCJewelryLibrary.Common.Services.REST
{
    /// <summary>
    /// Implementation of the IASCJRESTService interface that provides methods for sending HTTP requests to the ASCJ API using RestSharp.
    /// The class provides a Request method for sending HTTP requests with the specified endpoint, method, body, and parameters.
    /// The class also includes methods for appending a request body and parameters, and a method for executing the request and handling the response.
    /// The Get method sends a GET request with the specified endpoint and parameters, and returns the response as an instance of the specified model class.
    /// The class also includes a GetASCJSetup method for retrieving the ASCJ setup details from the database.
    /// </summary>
    public class ASCJRESTService : IASCJRESTService
    {
        #region Constants
        private const string Accept = "Accept";
        private const string Application = "application/json";
        #endregion

        #region fields
        private readonly ASCJAPMetalRatesSetup _setup;
        private readonly PXGraph _graph;
        #endregion

        #region ctor
        public ASCJRESTService(PXGraph graph)
        {
            _graph = graph;
            _setup = GetASCJSetup();
        }
        #endregion

        #region Core
        /// <summary>
        /// Sends an HTTP request to the specified endpoint with the specified method and body, using the credentials and base URL configured in the ACPR setup.
        /// </summary>
        /// <param name="endpoint">The endpoint to send the request to.</param>
        /// <param name="httpMethod">The HTTP method to use for the request, with GET as the default.</param>
        /// <param name="body">The body to include in the request, as an object that will be serialized as JSON.</param>
        /// <param name="parameters">The parameters to include in the request, as an array of dictionaries containing key-value pairs. Default is an empty array.</param>
        /// <returns>A string containing the content of the response.</returns>
        /// <exception cref="ASCJStatusCodeException">Thrown when the response has a non-OK status code.</exception>
        /// <exception cref="PXException">Thrown when an ASCJStatusCodeException is caught, to indicate a remote server error.</exception>
        private string Request(string endpoint, Method httpMethod = Method.Get, object body = null, params Dictionary<string, string>[] parameters)
        {
            var client = new RestClient(_setup.BaseURL);
            var request = new RestRequest(endpoint);
            request.AddHeader(Accept, Application);

            AppendBody(body, request);
            AppendParameters(parameters, request);

            return ExecuteRequest(httpMethod, client, request);
        }
        #endregion

        #region ServiceMethods
        private static string ExecuteRequest(Method httpMethod, RestClient client, RestRequest request)
        {
            try
            {
                var responce = client.Execute(request, httpMethod);
                if (responce.StatusCode != System.Net.HttpStatusCode.OK || responce.Content.Contains("error"))
                {
                    throw new ASCJStatusCodeException(responce.StatusCode, responce.Content.ToErrorString<ASCJErrorModel>());
                }
                return responce.Content;
            }
            catch (ASCJStatusCodeException ex)
            {
                PXTrace.WriteError($"Error: {ex.Message}");
                PXTrace.WriteError($"Status Code: {ex.StatusCode}");
                PXTrace.WriteError($"Content: {ex.Content}");
                throw new PXException(ASCJMessages.StatusCode.RemoteServerError);
            }
        }

        private static void AppendBody(object body, RestRequest request)
        {
            if (body != null)
            {
                request.AddJsonBody(body, Application);
            }
        }

        private static void AppendParameters(Dictionary<string, string>[] parameters, RestRequest request)
        {
            foreach (var parameter in parameters)
            {
                foreach (var item in parameter)
                {
                    request.AddQueryParameter(item.Key, item.Value);
                }
            }
        }

        public TModel Get<TModel>(string endpoint, params KeyValuePair<string, string>[] parameters) where TModel : IASCJModel
        {
            Dictionary<string, string> mergedDic = null;
            var paramKeyValue = new Dictionary<string, string>()
            {
                { ASCJQueryParams.Access_key, _setup.AccessKey }
            };

            if (parameters.Any())
            {
                parameters.ToDictionary(x => x.Key, x => x.Value);
                mergedDic = paramKeyValue.Concat(parameters).ToDictionary(x => x.Key, x => x.Value);
            }

            var responce = Request(endpoint, Method.Get, null, mergedDic ?? paramKeyValue);
            return ASCJJsonConverter<TModel>.FromJson(responce);
        }
        #endregion

        #region ServiceQueries
        /// <summary>
        /// Gets the ASCJ setup details from the database using a graph and a query.
        /// </summary>
        /// <returns>The ASCJ setup details as an instance of the ASCJSetup class.</returns>
        public ASCJAPMetalRatesSetup GetASCJSetup() =>
              PXSelectBase<
                  ASCJAPMetalRatesSetup, PXSelect<
                  ASCJAPMetalRatesSetup>.Config>
                  .Select(_graph);
        #endregion
    }
}
