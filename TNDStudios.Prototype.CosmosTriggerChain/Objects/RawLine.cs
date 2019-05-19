using Newtonsoft.Json;
using System;

namespace TNDStudios.Prototype.CosmosTriggerChain
{
    [JsonObject]
    public class RawLine
    {
        [JsonProperty(PropertyName = "id")]
        public String Id { get; set; } = String.Empty;

        [JsonProperty(PropertyName = "timesheetId")]
        public String TimesheetId { get; set; } = String.Empty;

        [JsonProperty(PropertyName = "user")]
        public String User { get; set; } = String.Empty;

        [JsonProperty(PropertyName = "totalItems")]
        public Int32 TotalItems { get; set; } = (Int32)0;

        [JsonProperty(PropertyName = "sequenceNumber")]
        public Int32 SequenceNumber { get; set; } = (Int32)0;

        [JsonProperty(PropertyName = "rateCode")]
        public String RateCode { get; set; } = String.Empty;

        [JsonProperty(PropertyName = "units")]
        public Double Units { get; set; } = (Double)0;

        [JsonProperty(PropertyName = "day")]
        public DateTime Day { get; set; } = DateTime.MinValue;
    }
}
