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
        private readonly IUploadService _uploadService;

        public UploadsController(IUploadService uploadService)
        {
            _uploadService = uploadService;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new UploadsViewModel();

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Post(UploadsViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                // TODO
                throw new Exception("Invalid ModelState");
            }

            await _uploadService.UploadFiles(vm.AlbumName, vm.FilesToUpload);
            

            return View();
        }
    }
}
