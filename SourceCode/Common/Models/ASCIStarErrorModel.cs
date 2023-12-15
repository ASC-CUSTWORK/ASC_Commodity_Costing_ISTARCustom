using Newtonsoft.Json;

namespace ASCJewelryLibrary.Common.Models
{
    public class ASCJErrorModel : IASCJModel
    {
        [JsonProperty("data")]
        public Data Data { get; set; }

        /// <summary>
        /// Overrides the ToString method to return a string representation of the error object.
        /// The string contains the error code, type, and info stored in the Data.Error object.
        /// </summary>
        /// <returns>A string representation of the error object.</returns>
        override public string ToString()
        {
            return $"Error Code: {Data.Error.Code} \nError Type: {Data.Error.Type} \nError Info: {Data.Error.Info}";
        }
    }

    public partial class Data
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error")]
        public Error Error { get; set; }
    }

    public partial class Error
    {
        [JsonProperty("code")]
        public long Code { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("info")]
        public string Info { get; set; }
    }
}
