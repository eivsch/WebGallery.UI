using System.Diagnostics;
using System.Threading.Tasks;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using WebGallery.UI.ViewModels;

namespace WebGallery.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMetadataService _statisticsService;

        public HomeController(IMetadataService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        public IActionResult Index()
        {
            ViewBag.Current = "Home";

            return View();
        }

        [HttpGet("home/stats")]
        public async Task<IActionResult> GetStatistics(string itemType)
        {
            var stats = await _statisticsService.GetStatistics(itemType);

            // TODO: Automapper
            var vm = new StatsInfoCardViewModel
            {
                Header = stats.ShortDescription,
                InfoItems = stats.InfoItems
            };

            return PartialView("_StatsInfoCard", vm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
