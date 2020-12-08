using System;
using System.Collections.Generic;
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
            return View();
        }
    }
}
