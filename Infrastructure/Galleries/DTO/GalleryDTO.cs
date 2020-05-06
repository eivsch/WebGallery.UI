using System.Text.Json.Serialization;

namespace Infrastructure.Galleries.DTO
{
    internal class GalleryDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("imageCount")]
        public int ImageCount { get; set; }
    }
}
