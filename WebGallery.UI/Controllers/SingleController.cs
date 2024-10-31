using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Services.Interfaces;
using Application.Tags;
using Infrastructure.MinimalApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebGallery.UI.Generators;
using WebGallery.UI.Helpers;
using WebGallery.UI.ViewModels.Single;

namespace WebGallery.UI.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class SingleController : Controller
    {
        private readonly IGalleryService _galleryService;
        private readonly ITagService _tagService;
        readonly MinimalApiProxy _minimalApiProxy;
        readonly string _username;

        public SingleController(IGalleryService galleryService, ITagService tagService, MinimalApiProxy minimalApiProxy, IHttpContextAccessor httpContext)
        {
            _galleryService = galleryService;
            _tagService = tagService;
            _minimalApiProxy = minimalApiProxy;
            Claim claim = httpContext.HttpContext.User.Claims.FirstOrDefault(f => f.Type == ClaimTypes.Sid);
            _username = claim.Value;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Current = "Random";
            int totalCount = 24, currentCount = 0;
            Random rnd = new();

            List<AlbumMetaDTO> albums = await _minimalApiProxy.GetAlbums(_username);
            if (albums == null) return null;

            List<SingleGalleryImageViewModel> items = new();
            while (currentCount < totalCount)
            {
                int randomAlbumIndex = rnd.Next(0, albums.Count);
                AlbumMetaDTO album = albums[randomAlbumIndex];
                int randomMediaIndex = rnd.Next(0, album.TotalCount);

                AlbumContentsDTO data = await _minimalApiProxy.GetAlbumContents(_username, album.AlbumName, randomMediaIndex, 1);
                MediaDTO media = data.Items[0];
                SingleGalleryImageViewModel imageVm = new()
                {
                    Id = media.Id,
                    AppPath = Path.Combine(album.AlbumName, media.Name),
                    GalleryIndex = randomMediaIndex,
                    IndexGlobal = -1,
                    MediaType = Utils.DetermineMediaType(media.Name),
                };
                items.Add(imageVm);

                currentCount++;
            }

            var vm = SinglePageGenerator.SetDisplayProperties(items);
            vm.GalleryTitle = "Randomized album";
            vm.TotalImageCount = totalCount;
            vm.CurrentOffset = 0;
            vm.CurrentDisplayCount = totalCount;

            return View("Index", vm);
        }

        [HttpGet("custom")]
        public async Task<IActionResult> Custom(int nbr, string tags, string tagFilterMode, string mediaFilterMode)
        {
            ViewBag.Current = "Random";

            var uri = await _galleryService.GenerateGalleryUri(nbr, tags, tagFilterMode, mediaFilterMode);
            var gallery = await _galleryService.Get(uri);

            var vm = SinglePageGenerator.Generate(gallery);
            if (!string.IsNullOrWhiteSpace(tags) && tagFilterMode == "custominclusive")
                vm.GalleryTitle = tags;
            else
                vm.GalleryTitle = "Randomized";

            return View("Index", vm);
        }

        [HttpPost("add-like/{id}")]
        public async Task<IActionResult> AddLike(string id)
        {
            var request = new TagRequest
            {
                PictureId = id,
                TagName = "like",
            };

            await _tagService.Add(request);

            return Ok();
        }
    }
}