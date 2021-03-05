using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebGallery.UI.ViewModels.Tags;

namespace WebGallery.UI.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class TagsController : Controller
    {
        private readonly ITagService _tagService;
        private readonly IGalleryService _galleryService;

        public TagsController(ITagService tagService, IGalleryService galleryService)
        {
            _tagService = tagService;
            _galleryService = galleryService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Current = "Tags";

            var tags = await _tagService.GetAll();

            var tagsVm = new List<TagsViewModel>();
            foreach (var tag in tags.OrderBy(o => o.TagName))
            {
                var coverImageUri = await _galleryService.GenerateGalleryUri(1, tag.TagName, "custominclusive", "include");
                var coverImageGallery = await _galleryService.Get(coverImageUri);
                var coverImage = coverImageGallery.GalleryItems.Single();
                var vm = new TagsViewModel
                {
                    CategoryName = tag.TagName,
                    CoverImageAppPath = coverImage.AppPath,
                    CoverImageMediaType = coverImage.MediaType,
                    ItemCount = tag.ItemCount,
                };

                tagsVm.Add(vm);
            }

            return View(tagsVm);
        }

        [HttpGet("{tag}")]
        public async Task<IActionResult> Get(string tag)
        {
            return Redirect($"/Single/Custom?nbr=48&tags={tag}&tagFilterMode=custominclusive&mediaFilterMode=include");
        }
    }
}
