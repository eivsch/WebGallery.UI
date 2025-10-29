using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Infrastructure.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace WebGallery.UI.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class FilesController : Controller
    {
        private readonly IFileSystemService _fileSystemService;

        public FilesController(IFileSystemService fileSystemService)
        {
            _fileSystemService = fileSystemService;
        }

        [HttpGet("image/{base64EncodedAppPath}")]
        public async Task<IActionResult> ShowImage(string base64EncodedAppPath)
        {
            var file = await _fileSystemService.DownloadImageFromFileServer(base64EncodedAppPath);
            FileContentResult result = new FileContentResult(file, "image/jpeg"); 

            return result;
        }

        [HttpGet("video/{base64EncodedAppPath}")]
        public async Task<IActionResult> ShowVideo(string base64EncodedAppPath)
        {
            var file = await _fileSystemService.DownloadVideoFromFileServer(base64EncodedAppPath);
            FileContentResult result = new FileContentResult(file, "video/mp4"); 

            return result;
        }
    }
}