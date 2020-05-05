using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebGallery.UI.ViewModels;

namespace WebGallery.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string _baseUrl = "http://192.168.1.251:5000/picture";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var galleries = new List<HomeGalleryViewModel>();
            for (int i = 1; i < 13; i++)
            {
                galleries.Add(
                    new HomeGalleryViewModel
                    {
                        CoverImageUrl = _baseUrl + "/" + i
                    });
            }

            var vm = new HomeViewModel
            {
                Galleries = galleries
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
