using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using WebGallery.UI.ViewModels.Customizer;

namespace WebGallery.UI.Controllers
{
    [Route("[controller]")]
    public class CustomizerController : Controller
    {
        private readonly ITagService _tagService;

        public CustomizerController(ITagService tagService)
        {
            _tagService = tagService;
        }
        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var tags = await _tagService.GetAll();

            var vm = new CustomizerViewModel
            {
                Tags = tags.Select(t => new Models.Tag { Name = t }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Generate(CustomizerViewModel vm)
        {


            return View(new CustomizerViewModel());
        }
    }
}
