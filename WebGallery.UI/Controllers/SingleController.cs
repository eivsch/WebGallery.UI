using System.Threading.Tasks;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using WebGallery.UI.Generators;

namespace WebGallery.UI.Controllers
{
    [Route("[controller]")]
    public class SingleController : Controller
    {
        private readonly IGalleryService _galleryService;

        public SingleController(IGalleryService galleryService)
        {
            _galleryService = galleryService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Current = "Random";

            var uri = await _galleryService.GenerateGalleryUri(24);
            var gallery = await _galleryService.Get(uri);

            var vm = SinglePageGenerator.Generate(gallery);
            vm.GalleryTitle = "Randomized";

            return View("Index", vm);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Index(string id, int offset)
        {
            ViewBag.Current = "Single";

            var galleryResponse = await _galleryService.Get(galleryId: id, itemIndexStart: offset + 1, numberOfItems: 48);
            
            var vm = SinglePageGenerator.Generate(galleryResponse);
            vm.GalleryTitle = "Gallery";

            return View(vm);
        }

        [HttpGet("custom")]
        public async Task<IActionResult> Custom(int nbr, string tags, string tagFilterMode, string mediaFilterMode)
        {
            ViewBag.Current = "Random";

            var uri = await _galleryService.GenerateGalleryUri(nbr, tags, tagFilterMode, mediaFilterMode);
            var gallery = await _galleryService.Get(uri);

            var vm = SinglePageGenerator.Generate(gallery);
            if (!string.IsNullOrWhiteSpace(tags) && tagFilterMode == "custominclusive")
                vm.GalleryTitle = tags;
            else
                vm.GalleryTitle = "Randomized";

            return View("Index", vm);
        }
    }
}