using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Infrastructure.Metadata.DTO
{
    class MetadataDTO
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        [JsonPropertyName("details")]
        public IDictionary<string, object> Details { get; set; }
    }
}
