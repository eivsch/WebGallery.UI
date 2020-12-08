using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebGallery.UI.ViewModels.Uploads;

namespace WebGallery.UI.Controllers
{
    [Route("[controller]")]
    public class UploadsController : Controller
    {
        public UploadsController()
        {
        }

        public async Task<IActionResult> Index()
        {
            // var vm = new UploadsViewModel();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Post(IEnumerable<IFormFile> filesToUpload)
        {
            var rootPath = "/var/www/pics";
            var newFolder = "folder1";
            
            var newDir = Path.Combine(rootPath, newFolder);
            Directory.CreateDirectory(newDir);

            foreach (var file in filesToUpload)
            {
                var filePath = Path.Combine(newDir, file.FileName);

                file.CopyTo(new FileStream(filePath, FileMode.Create));
            }

            return View();
        }
    }
}
