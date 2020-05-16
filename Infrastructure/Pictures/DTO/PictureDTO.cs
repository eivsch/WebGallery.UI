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

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("appPath")]
        public string AppPath { get; set; }

        [JsonPropertyName("originalPath")]
        public string OriginalPath { get; set; }

        [JsonPropertyName("folderName")]
        public string FolderName { get; set; }

        [JsonPropertyName("folderId")]
        public string FolderId { get; set; }

        [JsonPropertyName("globalSortOrder")]
        public int GlobalSortOrder { get; set; }

        [JsonPropertyName("folderSortOrder")]
        public int FolderSortOrder { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("createTimestamp")]
        public DateTime CreateTimestamp { get; set; }

        [JsonPropertyName("tags")]
        public IEnumerable<string> Tags { get; set; }
    }
}
