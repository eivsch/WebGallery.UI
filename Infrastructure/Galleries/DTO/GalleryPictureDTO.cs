using System.Text.Json.Serialization;

namespace Infrastructure.Galleries.DTO
{
    internal class GalleryPictureDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("index")]
        public int Index { get; set; }
    }
}
