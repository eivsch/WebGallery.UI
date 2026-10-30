namespace WebGallery.FileServer
{
    public class FileUploadResponse
    {
        public string FileName { get; set; }
        public string FilePathFull { get; set; }
        public long FileSize { get; set; }
        public string ThumbnailPath { get; set; } 
    }
}