using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebGallery.UI.ViewModels.Customizer;

namespace WebGallery.UI.Controllers
{
    [Route("[controller]")]
    public class CustomizerController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var vm = new CustomizerViewModel();

            return View(vm);
        }
    }
}
