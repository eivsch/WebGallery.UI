using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace WebGallery.UI.ViewModels.Uploads
{
    public class UploadsViewModel
    {
        public string AlbumName { get; set; }
        public IEnumerable<IFormFile> FilesToUpload { get; set; }
    }
}