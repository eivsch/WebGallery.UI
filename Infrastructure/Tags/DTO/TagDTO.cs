using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Infrastructure.Tags.DTO
{
    internal class TagDTO
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("itemCount")]
        public int ItemCount { get; set; }
        [JsonPropertyName("mediaItems")]
        public IEnumerable<TagItemDTO> MediaItems { get; set; }
    }
}
