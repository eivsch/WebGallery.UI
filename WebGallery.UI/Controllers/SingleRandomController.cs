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
    public class SingleRandomController : Controller
    {
        private readonly IGalleryService _galleryService;

        public SingleRandomController(IGalleryService galleryService)
        {
            _galleryService = galleryService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Current = "Random";

            var uri = await _galleryService.GenerateGalleryUri(24);
            var gallery = await _galleryService.Get(uri);

            var vm = SinglePageGenerator.Generate(gallery);

            return View("Index", vm);
        }

        [HttpGet("custom")]
        public async Task<IActionResult> Custom(int nbr, string tags, string tagFilterMode)
        {
            ViewBag.Current = "Random";

            var uri = await _galleryService.GenerateGalleryUri(nbr, tags, tagFilterMode);
            var gallery = await _galleryService.Get(uri);

            var vm = SinglePageGenerator.Generate(gallery);

            return View("Index", vm);
        }
    }
}