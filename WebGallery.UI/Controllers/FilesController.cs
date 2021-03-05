using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Enums;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using WebGallery.UI.Attributes;
using WebGallery.UI.Helpers;
using WebGallery.UI.ViewModels.Uploads;

namespace WebGallery.UI.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class FilesController : Controller
    {
        private readonly IFileService _fileService;

        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpGet("image/{base64EncodedAppPath}")]
        public async Task<IActionResult> ShowImage(string base64EncodedAppPath)
        {
            var file = await _fileService.DownloadFile(base64EncodedAppPath, MediaType.Image);
            FileContentResult result = new FileContentResult(file, "image/jpeg"); 

            return result;
        }

        [HttpGet("video/{base64EncodedAppPath}")]
        public async Task<IActionResult> ShowVideo(string base64EncodedAppPath)
        {
            var file = await _fileService.DownloadFile(base64EncodedAppPath, MediaType.Video);
            FileContentResult result = new FileContentResult(file, "video/mp4"); 

            return result;
        }
    }
}