using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebGallery.UI.Generators;
using WebGallery.UI.ViewModels;

namespace WebGallery.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IGalleryService _galleryService;
        private readonly IPictureService _pictureService;

        public HomeController(ILogger<HomeController> logger, IGalleryService galleryService, IPictureService pictureService)
        {
            _logger = logger;
            _galleryService = galleryService;
            _pictureService = pictureService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Current = "Home";

            var allGalleries = await _galleryService.GetAllGalleriesWithoutItems();
            var allGalleriesVm = HomePageGenerator.GenerateAllRandom(allGalleries);
            
            foreach (var galleryVm in allGalleriesVm.Galleries)
            {
                var picture = await _pictureService.Get(galleryVm.GalleryId, galleryVm.CoverImageIndex);
                galleryVm.CoverImageGlobalIndex = picture.GlobalSortOrder;
            }

            return View(allGalleriesVm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
