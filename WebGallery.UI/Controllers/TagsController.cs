using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Services.Interfaces;
using Infrastructure.MinimalApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        private readonly MinimalApiProxy _minimalApiProxy;
        readonly string _username;

        public TagsController(ITagService tagService, IGalleryService galleryService, MinimalApiProxy minimalApiProxy, IHttpContextAccessor httpContext)
        {
            _tagService = tagService;
            _galleryService = galleryService;
            _minimalApiProxy = minimalApiProxy;
            Claim claim = httpContext.HttpContext.User.Claims.FirstOrDefault(f => f.Type == ClaimTypes.Sid);
            _username = claim.Value;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Current = "Tags";

            return View();
            //var tags = await _tagService.GetAll();

            //var tagsVm = new List<TagsViewModel>();
            //foreach (var tag in tags.OrderBy(o => o.TagName))
            //{
            //    var coverImageUri = await _galleryService.GenerateGalleryUri(1, tag.TagName, "custominclusive", "include");
            //    var coverImageGallery = await _galleryService.Get(coverImageUri);
            //    var coverImage = coverImageGallery.GalleryItems.Single();
            //    var vm = new TagsViewModel
            //    {
            //        CategoryName = tag.TagName,
            //        CoverImageAppPath = coverImage.AppPath,
            //        CoverImageMediaType = coverImage.MediaType,
            //        ItemCount = tag.ItemCount,
            //    };

            //    tagsVm.Add(vm);
            //}

            //return View(tagsVm);
        }

        [HttpGet("{tag}")]
        public async Task<IActionResult> Get(string tag)
        {
            return Redirect($"/Single/Custom?nbr=48&tags={tag}&tagFilterMode=custominclusive&mediaFilterMode=include");
        }
    }
}
