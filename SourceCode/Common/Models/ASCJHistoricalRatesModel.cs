using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ASCJewelryLibrary.Common.Models
{
    public class ASCJHistoricalRatesModel : IASCJModel
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("historical")]
        public bool Historical { get; set; }

        [JsonProperty("date")]
        public DateTimeOffset Date { get; set; }

        [JsonProperty("base")]
        public string Base { get; set; }

        [JsonProperty("rates")]
        public Dictionary<string, double> Rates { get; set; }
    }
}
