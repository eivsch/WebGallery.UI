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
        private readonly IGalleryService _galleryService;

        public SingleController(IPictureService pictureService, IGalleryService galleryService)
        {
            _pictureService = pictureService;
            _galleryService = galleryService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Index(string id, int offset)
        {
            ViewBag.Current = "Single";

            var pics = await _pictureService.GetPictures(id, offset);

            var vm = SinglePageGenerator.GenerateRandom_ByFolderOrder(id, pics);
            vm.Offset = offset;

            return View(vm);
        }

        //[HttpGet("randomized")]
        //public async Task<IActionResult> Randomized()
        //{
        //    ViewBag.Current = "Randomized";

        //    var gallery = await _galleryService.GetRandom(12);

        //    var vm = HomePageGenerator.GenerateHomePage(gallery, randomizeCoverImage: false);

        //    return View("Index", vm);
        //}
    }
}