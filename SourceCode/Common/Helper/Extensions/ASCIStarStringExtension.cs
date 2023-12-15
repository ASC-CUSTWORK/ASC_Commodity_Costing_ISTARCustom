using ASCJewelryLibrary.Common.Models;

namespace ASCJewelryLibrary.Common.Helper.Extensions
{
    public static class ASCJStringExtension
    {
        /// <summary>
        /// Extension method that deserializes a JSON string to an ASCJErrorModel object of the specified type and returns a string representation of the error data.
        /// Uses the ASCJJsonConverter class to deserialize the JSON string to the error model object, then calls the ToString method on the error data object to return a string representation.
        /// </summary>
        /// <typeparam name="TModel">The type of the error model object to deserialize the JSON string to.</typeparam>
        /// <param name="value">The JSON string to deserialize.</param>
        /// <returns>A string representation of the error data object.</returns>
        public static string ToErrorString<TModel>(this string value) where TModel : ASCJErrorModel
        {
            var errorModel = ASCJJsonConverter<TModel>.FromJson(value);
            return errorModel.ToString();
        }

        public static string NormalizeCD(this string value)
        {
            return value.Trim();
        }
    }
}
