using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using WebGallery.UI.ViewModels.Bio;

namespace WebGallery.UI.Controllers
{
    [Route("[controller]")]
    public class BioController : Controller
    {
        private readonly IPictureService _pictureService;

        public BioController(IPictureService pictureService)
        {
            _pictureService = pictureService;
        }

        public async Task<IActionResult> Index()
        {
            var picture = await _pictureService.Get(-1); // Random

            var vm = new BioViewModel
            {
                PictureId = picture.Id,
                GlobalSortOrder = picture.GlobalSortOrder,
            };

            return View(vm);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Index(int id)
        {
            var picture = await _pictureService.Get(id);

            var vm = new BioViewModel
            {
                PictureId = picture.Id,
                GlobalSortOrder = picture.GlobalSortOrder,
            };

            return View(vm);
        }
    }
}