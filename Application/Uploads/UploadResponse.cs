using System.Collections.Generic;

namespace Application.Uploads
{    
    public class UploadResponse
    {
        public string UploadAlbumName { get; set; }
        public int UploadFileCount { get; set; }
        public virtual List<UploadFile> UploadedFiles { get; set;}
    }
}