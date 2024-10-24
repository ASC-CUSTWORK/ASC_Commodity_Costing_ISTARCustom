using ASCJSMCustom.Common.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Globalization;

namespace ASCJSMCustom.Common.Helper
{
    public class ASCJSMJsonConverter<TModel> where TModel : IASCJSMModel
    {
        /// <summary>
        /// Deserializes a JSON string into a TModel object using the provided JSON serialization settings.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A TModel object deserialized from the JSON string.</returns>
        public static TModel FromJson(string json) => JsonConvert.DeserializeObject<TModel>(json, Settings);

        /// <summary>
        /// Deserializes a JSON array string into an enumerable of TModel objects using the provided JSON serialization settings.
        /// </summary>
        /// <param name="json">The JSON array string to deserialize.</param>
        /// <returns>An enumerable of TModel objects deserialized from the JSON array string.</returns>
        public static IEnumerable<TModel> FromJsonArray(string json) => JsonConvert.DeserializeObject<IEnumerable<TModel>>(json, Settings);

        /// <summary>
        /// Serializes a TModel object into a JSON string using the provided JSON serialization settings.
        /// </summary>
        /// <param name="self">The TModel object to serialize.</param>
        /// <returns>A JSON string representation of the TModel object.</returns>
        public static string ToJson(TModel self) => JsonConvert.SerializeObject(self, Settings);

        /// <summary>
        /// JSON serialization settings used by the application.
        /// </summary>
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
