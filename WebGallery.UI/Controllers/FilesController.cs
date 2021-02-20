using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Services.Interfaces;
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
    [Route("[controller]")]
    public class FilesController : Controller
    {
        private readonly IFileService _fileService;

        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        public async Task<IActionResult> Index()
        {
            var file = await _fileService.DownloadFile("todo");
            FileContentResult result = new FileContentResult(file, "image/jpeg"); 

            return result;
        }
    }
}