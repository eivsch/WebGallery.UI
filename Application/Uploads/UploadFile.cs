namespace Application.Uploads
{    
    public class UploadFile
    {
        public string FileName { get; set; }
        public string UploadDestinationPath { get; set; }
        public long FileSizeInBytes { get; set; }
    }
}