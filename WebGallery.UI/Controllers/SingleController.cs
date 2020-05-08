using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using WebGallery.UI.Generators;

namespace WebGallery.UI.Controllers
{
    [Route("[controller]")]
    public class SingleController : Controller
    {
        private readonly IPictureService _pictureService;

        public SingleController(IPictureService pictureService)
        {
            _pictureService = pictureService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Index(string id, int offset)
        {
            var pics = await _pictureService.GetPictures(id, offset);

            var vm = SinglePageGenerator.GenerateRandom_ByFolderOrder(id, pics);

            return View(vm);
        }
    }
}