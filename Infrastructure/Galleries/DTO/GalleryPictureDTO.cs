using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Infrastructure.Galleries.DTO
{
    internal class GalleryPictureDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("appPath")]
        public string AppPath { get; set; }

        [JsonPropertyName("indexGlobal")]
        public int IndexGlobal { get; set; }

        [JsonPropertyName("mediaType")]
        public string MediaType { get; set; }

        [JsonPropertyName("tags")]
        public IEnumerable<string> Tags { get; set; }
    }
}
