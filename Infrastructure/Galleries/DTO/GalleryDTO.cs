using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Infrastructure.Galleries.DTO
{
    internal class GalleryDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("imageCount")]
        public int ImageCount { get; set; }

        [JsonPropertyName("galleryPictures")]
        public IEnumerable<GalleryPictureDTO> GalleryPictures { get; set; }

        [JsonPropertyName("tags")]
        public IEnumerable<string> Tags { get; set; }
    }
}
