using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Infrastructure.Pictures.DTO
{
    internal class PictureDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("globalSortOrder")]
        public int GlobalSortOrder { get; set; }
    }
}
