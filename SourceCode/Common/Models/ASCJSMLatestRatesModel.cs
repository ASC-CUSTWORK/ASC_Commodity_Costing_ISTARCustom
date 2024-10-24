﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ASCJSMCustom.Common.Models
{
    public class ASCJSMLatestRatesModel : IASCJSMModel
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("date")]
        public DateTimeOffset Date { get; set; }

        [JsonProperty("base")]
        public string Base { get; set; }

        [JsonProperty("rates")]
        public Dictionary<string, double> Rates { get; set; }
    }
}