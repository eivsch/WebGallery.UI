using System.Collections.Generic;
using Infrastructure.FileServer;

namespace WebGallery.UI.ViewModels.Uploads
{
    public class UploadResultViewModel
    {
        public string UploadAlbumName { get; set; }
        public int UploadFileCount { get; set; }
        public List<SavedFileInfo> UploadedFiles { get; set;}
    }
}