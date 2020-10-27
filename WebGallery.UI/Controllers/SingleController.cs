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

        [HttpGet("{id}")]
        public async Task<IActionResult> Index(string id, int offset)
        {
            ViewBag.Current = "Single";

            var galleryResponse = await _galleryService.Get(galleryId: id, itemIndexStart: offset + 1, numberOfItems: 48);
            
            var vm = SinglePageGenerator.Generate(galleryResponse);
            vm.GalleryType = "Gallery";

            return View(vm);
        }
    }
}