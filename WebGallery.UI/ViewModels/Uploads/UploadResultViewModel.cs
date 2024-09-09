using System.Collections.Generic;
using Application.Uploads;
using Infrastructure.Services;

namespace WebGallery.UI.ViewModels.Uploads
{
    public class UploadResultViewModel
    {
        public string UploadAlbumName { get; set; }
        public int UploadFileCount { get; set; }
        public List<SavedFileInfo> UploadedFiles { get; set;}
    }
}