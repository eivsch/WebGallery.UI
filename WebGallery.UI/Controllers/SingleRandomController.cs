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

            var gallery = await _galleryService.GetRandom(24);

            var vm = SinglePageGenerator.Generate(gallery);

            return View("Index", vm);
        }

        [HttpGet("custom")]
        public async Task<IActionResult> Custom(int num, string tags, string tagMode)
        {
            ViewBag.Current = "Random";

            var gallery = await _galleryService.Get(num, tags, tagMode);

            var vm = SinglePageGenerator.Generate(gallery);

            return View("Index", vm);
        }

        private List<string> ParseTags(string tags)
        {
            return tags.Split(',').ToList();
        }
    }
}