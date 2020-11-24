using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Infrastructure.Metadata.DTO
{
    class MetadataDTO
    {
        [JsonPropertyName("shortDescription")]
        public string ShortDescription { get; set; }
        [JsonPropertyName("metrics")]
        public Dictionary<string, string> Metrics { get; set; }
    }
}
