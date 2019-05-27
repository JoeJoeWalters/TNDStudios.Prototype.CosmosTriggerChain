using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TNDStudios.Prototype.CosmosTriggerChain
{
    [JsonObject]
    public class ProcessedObject
    {
        [JsonProperty(PropertyName = "id")]
        public String Id { get; set; } = String.Empty;

        [JsonProperty(PropertyName = "user")]
        public String User { get; set; } = String.Empty;

        [JsonProperty(PropertyName = "lines")]
        public List<ProcessedObjectLine> Lines { get; set; } = new List<ProcessedObjectLine>() { };

    }

    [JsonObject]
    public class ProcessedObjectLine
    {
        [JsonProperty(PropertyName = "rateCode")]
        public String RateCode { get; set; } = String.Empty;

        [JsonProperty(PropertyName = "units")]
        public Double Units { get; set; } = (Double)0;

        [JsonProperty(PropertyName = "day")]
        public DateTime Day { get; set; } = DateTime.MinValue;
    }
}
