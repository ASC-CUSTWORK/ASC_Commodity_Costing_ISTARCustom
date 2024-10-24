using ASCJSMCustom.Common.Descriptor;
using ASCJSMCustom.Common.Helper;
using ASCJSMCustom.Common.Helper.Exceptions;
using ASCJSMCustom.Common.Helper.Extensions;
using ASCJSMCustom.Common.Models;
using ASCJSMCustom.Common.Services.REST.Interfaces;
using ASCJSMCustom.Cost.DAC;
using PX.Data;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using ASCJSMCustom.Common.Models;

namespace ASCJSMCustom.Common.Services.REST
{
    /// <summary>
    /// Implementation of the IASCIStarRESTService interface that provides methods for sending HTTP requests to the ASCIStar API using RestSharp.
    /// The class provides a Request method for sending HTTP requests with the specified endpoint, method, body, and parameters.
    /// The class also includes methods for appending a request body and parameters, and a method for executing the request and handling the response.
    /// The Get method sends a GET request with the specified endpoint and parameters, and returns the response as an instance of the specified model class.
    /// The class also includes a GetASCIStarSetup method for retrieving the ASCIStar setup details from the database.
    /// </summary>
    public class ASCJSMRESTService : IASCJSMRESTService
    {
        #region Constants
        private const string Accept = "Accept";
        private const string Application = "application/json";
        #endregion

        #region fields
        private readonly ASCJSMSetup _setup;
        private readonly PXGraph _graph;
        #endregion

        #region ctor
        public ASCJSMRESTService(PXGraph graph)
        {
            _graph = graph;
            _setup = GetASCIStarSetup();
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
        /// <exception cref="ASCIStarStatusCodeException">Thrown when the response has a non-OK status code.</exception>
        /// <exception cref="PXException">Thrown when an ASCIStarStatusCodeException is caught, to indicate a remote server error.</exception>
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
                    throw new ASCJSMStatusCodeException(responce.StatusCode, responce.Content.ToErrorString<ASCJSMErrorModel>());
                }
                return responce.Content;
            }
            catch (ASCJSMStatusCodeException ex)
            {
                PXTrace.WriteError($"Error: {ex.Message}");
                PXTrace.WriteError($"Status Code: {ex.StatusCode}");
                PXTrace.WriteError($"Content: {ex.Content}");
                throw new Exception(ex.Message);
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

        public TModel Get<TModel>(string endpoint, params KeyValuePair<string, string>[] parameters) where TModel : IASCJSMModel
        {
            Dictionary<string, string> mergedDic = null;
            var paramKeyValue = new Dictionary<string, string>()
            {
                { ASCJSMQueryParams.Access_key, _setup.AccessKey }
            };

            if (parameters.Any())
            {
                parameters.ToDictionary(x => x.Key, x => x.Value);
                mergedDic = paramKeyValue.Concat(parameters).ToDictionary(x => x.Key, x => x.Value);
            }

            var responce = Request(endpoint, Method.Get, null, mergedDic ?? paramKeyValue);
            return ASCJSMJsonConverter<TModel>.FromJson(responce);
        }
        #endregion

        #region ServiceQueries
        /// <summary>
        /// Gets the ASCIStar setup details from the database using a graph and a query.
        /// </summary>
        /// <returns>The ASCIStar setup details as an instance of the ASCIStarSetup class.</returns>
        public ASCJSMSetup GetASCIStarSetup() =>
              PXSelectBase<
                  ASCJSMSetup, PXSelect<
                  ASCJSMSetup>.Config>
                  .Select(_graph);
        #endregion
    }
}
