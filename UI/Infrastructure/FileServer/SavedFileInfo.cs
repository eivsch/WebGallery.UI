using System.Text.Json.Serialization;

namespace Infrastructure.FileServer
{
    public class SavedFileInfo
    {
        [JsonPropertyName("fileName")]
        public string FileName {get;set;} 

        [JsonPropertyName("filePathFull")]
        public string FilePathFull {get;set;} 
        
        [JsonPropertyName("fileSize")]
        public long FileSize {get;set;} 
    }
}