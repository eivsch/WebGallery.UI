using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        public HomeController(ILogger<HomeController> logger, IGalleryService galleryService)
        {
            _logger = logger;
            _galleryService = galleryService ?? throw new ArgumentNullException(nameof(galleryService));
        }

        public async Task<IActionResult> Index()
        {
            var galleries = await _galleryService.GetAll();

            var vm = HomePageGenerator.GenerateAllRandom(galleries);

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
