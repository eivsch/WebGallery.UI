using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Interfaces;
using Application.Tags;
using Microsoft.AspNetCore.Mvc;
using WebGallery.UI.Models;
using WebGallery.UI.ViewModels.Bio;

namespace WebGallery.UI.Controllers
{
    [Route("[controller]")]
    public class BioController : Controller
    {
        private readonly IPictureService _pictureService;
        private readonly ITagService _tagService;

        static string _currentPictureId = string.Empty;

        public BioController(IPictureService pictureService, ITagService tagService)
        {
            _pictureService = pictureService;
            _tagService = tagService;
        }

        public async Task<IActionResult> Index()
        {
            var picture = await _pictureService.Get(-1);    // Random

            var vm = new BioViewModel
            {
                AllTags = new List<string> { "About", "Base", "Blog", "Contact", "Custom", "Support", "Tools" },
                BioPictureViewModel = new BioPictureViewModel
                {
                    PictureId = picture.Id,
                    GlobalSortOrder = picture.GlobalSortOrder,
                    Tags = picture.Tags.ToList()
                }
            };

            _currentPictureId = picture.Id;

            return View(vm);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Index(int id)
        {
            var picture = await _pictureService.Get(id);

            var vm = new BioViewModel
            {
                AllTags = new List<string> { "About", "Base", "Blog", "Contact", "Custom", "Support", "Tools" },    // TODO
                BioPictureViewModel = new BioPictureViewModel
                {
                    PictureId = picture.Id,
                    GlobalSortOrder = picture.GlobalSortOrder,
                    Tags = picture.Tags.ToList()
                }
            };

            _currentPictureId = picture.Id;

            return View(vm);
        }

        [HttpGet("switch/{id}")]
        public async Task<IActionResult> Switch(int id)
        {
            var picture = await _pictureService.Get(id);

            var vm = new BioPictureViewModel
            {
                PictureId = picture.Id,
                GlobalSortOrder = picture.GlobalSortOrder,
                Tags = picture.Tags.ToList()
            };

            _currentPictureId = picture.Id;

            return PartialView("_Picture", vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddTag(AddTagAjaxRequest data)
        {
            var request = new TagRequest
            {
                PictureId = _currentPictureId,
                TagName = data.Tag
            };

            await _tagService.Add(request);

            return Ok();
        }
    }
}