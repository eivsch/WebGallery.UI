using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using WebGallery.UI.ViewModels;

namespace WebGallery.UI.Controllers
{
    [Route("[controller]")]
    public class LandingController : Controller
    {
        private readonly IMetadataService _statisticsService;

        public LandingController(IMetadataService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("stats")]
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
    }
}
