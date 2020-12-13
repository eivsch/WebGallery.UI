using System.Collections.Generic;
using Application.Uploads;

namespace WebGallery.UI.ViewModels.Uploads
{
    public class UploadResultViewModel
    {
        public string UploadAlbumName { get; set; }
        public int UploadFileCount { get; set; }
        public List<UploadFile> UploadedFiles { get; set;}
    }
}