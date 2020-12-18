﻿using System.Linq;
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

        public BioController(IPictureService pictureService, ITagService tagService)
        {
            _pictureService = pictureService;
            _tagService = tagService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Current = "Bio";

            var picture = await _pictureService.Get(-1);    // Random
            var tags = await _tagService.GetAll();

            var vm = new BioViewModel
            {
                AllTags = tags.Select(s => s.TagName).ToList(),
                BioPictureViewModel = new BioPictureViewModel
                {
                    PictureId = picture.Id,
                    AppPath = picture.AppPath,
                    GlobalSortOrder = picture.GlobalSortOrder,
                    Tags = picture.Tags.ToList()
                }
            };

            return View(vm);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Index(int id)
        {
            var picture = await _pictureService.Get(id);
            var tags = await _tagService.GetAll();

            var vm = new BioViewModel
            {
                AllTags = tags.Select(s => s.TagName).ToList(),
                BioPictureViewModel = new BioPictureViewModel
                {
                    PictureId = picture.Id,
                    AppPath = picture.AppPath,
                    GlobalSortOrder = picture.GlobalSortOrder,
                    Tags = picture.Tags.ToList()
                }
            };

            return View(vm);
        }

        [HttpGet("switch/{id}")]
        public async Task<IActionResult> Switch(int id)
        {
            var picture = await _pictureService.Get(id);

            var vm = new BioPictureViewModel
            {
                PictureId = picture.Id,
                AppPath = picture.AppPath,
                GlobalSortOrder = picture.GlobalSortOrder,
                Tags = picture.Tags.ToList()
            };

            return PartialView("_Picture", vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddTag(AddTagAjaxRequest data)
        {
            var request = new TagRequest
            {
                PictureId = data.PictureId,
                TagName = data.Tag
            };

            await _tagService.Add(request);

            return Ok();
        }
    }
}