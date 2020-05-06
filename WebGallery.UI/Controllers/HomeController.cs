using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebGallery.UI.ViewModels;

namespace WebGallery.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IGalleryService _galleryService;

        private readonly string _baseUrl = "http://localhost:5000/pictures";

        public HomeController(ILogger<HomeController> logger, IGalleryService galleryService)
        {
            _logger = logger;
            _galleryService = galleryService ?? throw new ArgumentNullException(nameof(galleryService));
        }

        public async Task<IActionResult> Index()
        {
            // Create view model
            // TODO: Separate into a UI service/manager/factory
            var galleries = await _galleryService.GetAll();
            var list = new List<HomeGalleryViewModel>();
            foreach(var gallery in galleries)
            {
                list.Add(
                    new HomeGalleryViewModel
                    {
                        Id = gallery.Id,
                        ItemCount = gallery.ImageCount,
                        CoverImageUrl = $"{_baseUrl}/{gallery.Id}/1",   // TODO: Randomize cover image
                    });
            }

            var vm = new HomeViewModel
            {
                Galleries = list
            };

            return View(vm);
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
