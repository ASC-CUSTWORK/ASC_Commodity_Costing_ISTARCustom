using Newtonsoft.Json;
using System;

namespace ASCISTARCustom.Common.Helper
{
    public class ASCIStarParseStringConverter : JsonConverter
    {
        public static readonly ASCIStarParseStringConverter Singleton = new ASCIStarParseStringConverter();

        /// <summary>
        /// Overrides the CanConvert method of JsonConverter to determine if the specified type can be converted to/from JSON.
        /// Returns true if the specified type is a long or nullable long type, and false otherwise.
        /// </summary>
        /// <param name="type">The type to check for compatibility with this converter.</param>
        /// <returns>True if the type can be converted to/from JSON, false otherwise.</returns>
        public override bool CanConvert(Type type) =>
            type == typeof(long) || type == typeof(long?);

        /// <summary>
        /// Overrides the base method for reading JSON in the Json.NET serializer to read and deserialize a JSON value to a long integer.
        /// If the JSON token is null, returns null. Throws an exception if the JSON value cannot be parsed as a long integer.
        /// </summary>
        /// <param name="reader">The JsonReader to read from.</param>
        /// <param name="t">The type of the object to deserialize to.</param>
        /// <param name="existingValue">The existing value of the object being deserialized.</param>
        /// <param name="serializer">The JsonSerializer to use for deserialization.</param>
        /// <returns>The deserialized long integer value.</returns>
        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        /// <summary>
        /// Overrides the WriteJson method of JsonConverter to write a long integer value to a JSON string.
        /// If the value is null, writes a null value to the JSON string. Otherwise, serializes the long integer value as a string and writes it to the JSON string.
        /// </summary>
        /// <param name="writer">The JsonWriter to write to.</param>
        /// <param name="untypedValue">The value to write to the JSON string.</param>
        /// <param name="serializer">The JsonSerializer to use for serialization.</param>
        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }
    }
}
