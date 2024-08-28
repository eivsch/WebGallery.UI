using System.Diagnostics;
using System.Threading.Tasks;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebGallery.UI.ViewModels;


namespace WebGallery.UI.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IMetadataService _statisticsService;

        public HomeController(IMetadataService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        public IActionResult Index()
        {
            //if (!HttpContext.User.Identity.IsAuthenticated)
            //    return RedirectToAction("Index", "Login");

            ViewBag.Current = "Home";

            return View();
        }

        [HttpGet("home/stats")]
        public async Task<IActionResult> GetStatistics(string itemType)
        {
            var stats = await _statisticsService.GetStatistics(itemType);

            var headerLink = itemType switch
            {
                "picture" => "/single",
                "album" => "/albums",
                "tag" => "tags",
                _ => "/single"
            };

            // TODO: Automapper
            var vm = new StatsInfoCardViewModel
            {
                Header = stats.ShortDescription,
                Headerlink = headerLink,
                InfoItems = stats.InfoItems
            };

            return PartialView("_StatsInfoCard", vm);
        }

        [HttpGet("home/search")]
        public async Task<IActionResult> Search(string query)
        {
            return null;
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
