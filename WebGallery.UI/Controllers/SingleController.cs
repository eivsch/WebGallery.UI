using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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
        readonly MinimalApiProxy _minimalApiProxy;
        readonly string _username;

        public SingleController(MinimalApiProxy minimalApiProxy, IHttpContextAccessor httpContext)
        {
            _minimalApiProxy = minimalApiProxy;
            Claim claim = httpContext.HttpContext.User.Claims.FirstOrDefault(f => f.Type == ClaimTypes.Sid);
            _username = claim.Value;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Current = "Random";
            int totalCount = 32, currentCount = 0;
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
            vm.CurrentDisplayCount = 12;

            return View("Index", vm);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string albums = null, string tags = null, string fileExtensions = null, string mediaNameContains = null, int? maxSize = null)
        {
            // Note: tags are searched for "exclusive", i.e. logical AND. Albums are inclusive, i.e. logical OR.
            ViewBag.Current = "Search";
            // TODO: decide how to scroll over many search hits
            const int sizeLimit = 256;
            //const int viewDisplayLimit = 32;
            maxSize = maxSize > sizeLimit ? sizeLimit : maxSize;

            List<SearchHitDTO> hits = await _minimalApiProxy.GetSearch(_username, albums, tags, fileExtensions, mediaNameContains, maxSize);
            List<SingleGalleryImageViewModel> items = [];
            foreach (SearchHitDTO hit in hits)
            {
                SingleGalleryImageViewModel imageVm = new()
                {
                    Id = hit.MediaItem.Id,
                    AppPath = Path.Combine(hit.AlbumName, hit.MediaItem.Name),
                    MediaType = Utils.DetermineMediaType(hit.MediaItem.Name),
                };

                items.Add(imageVm);
            }

            SingleGalleryViewModel vm = SinglePageGenerator.SetDisplayProperties(items);
            vm.GalleryTitle = "Search results";
            vm.TotalImageCount = hits.Count;
            vm.CurrentOffset = 0;
            vm.CurrentDisplayCount = hits.Count;

            return View("Index", vm);
        }

        [HttpGet("custom-js")]
        public async Task<IActionResult> CustomJs(int mediaCount, string tags, string tagFilterMode, string mediaFilterMode)
        {
            return View("CustomJs");
        }
    }
}