using System;
using System.Text.Json.Serialization;

namespace Infrastructure.Tags.DTO
{
    internal class TagItemDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("globalIndex")]
        public int? GlobalIndex { get; set; }
        [JsonPropertyName("created")]
        public DateTime Created { get; set; }
    }
}
