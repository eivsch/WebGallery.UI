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
    class SearchDetails
    {
        public List<SearchHitDTO> Hits { get; set; }

        public string Albums { get; set; } 
        public string Tags { get; set; }
        public string FileExtensions { get; set; }
        public string MediaNameContains { get; set; }
        public int? MaxSize { get; set; }
        public bool? AllTagsMustMatch { get; set; }
        public bool HasMoreResults { get; set; }
    }

    [Authorize]
    [Route("[controller]")]
    public class SingleController : Controller
    {
        private static Dictionary<string, SearchDetails> _searchCache = [];
        
        readonly MinimalApiProxy _minimalApiProxy;
        readonly string _username;

        const int DISPLAY_COUNT_MAX = 32;

        public SingleController(MinimalApiProxy minimalApiProxy, IHttpContextAccessor httpContext)
        {
            _minimalApiProxy = minimalApiProxy;
            Claim claim = httpContext.HttpContext.User.Claims.FirstOrDefault(f => f.Type == ClaimTypes.Sid);
            _username = claim.Value;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Current = "Random";
            int currentCount = 0;
            Random rnd = new();

            List<AlbumMetaDTO> albums = await _minimalApiProxy.GetAlbums(_username);
            if (albums == null) return null;

            List<SingleGalleryImageViewModel> items = new();
            while (currentCount < DISPLAY_COUNT_MAX)
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
            vm.TotalImageCount = DISPLAY_COUNT_MAX;
            vm.CurrentOffset = 0;
            vm.DisplayCount = DISPLAY_COUNT_MAX;
            vm.IsRandomized = true;

            return View("Index", vm);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string albums = null, string tags = null, string fileExtensions = null, string mediaNameContains = null, int? maxSize = 200, bool? allTagsMustMatch = false, int? hitsToSkip = null)
        {
            // Note: tags are searched for "exclusive", i.e. logical AND. Albums are inclusive, i.e. logical OR.
            ViewBag.Current = "Search";

            List<SearchHitDTO> searchHits = await _minimalApiProxy.GetSearch(_username, albums, tags, fileExtensions, mediaNameContains, maxSize, allTagsMustMatch ?? true, hitsToSkip);
            SearchDetails searchDetails = new()
            {
                Hits = searchHits,
                Albums = albums,
                Tags = tags,
                FileExtensions = fileExtensions,
                MediaNameContains = mediaNameContains,
                MaxSize = maxSize,
                AllTagsMustMatch = allTagsMustMatch,
                HasMoreResults = maxSize == searchHits.Count,
            };

            _searchCache.Remove(_username);
            _searchCache.Add(_username, searchDetails);

            List<SingleGalleryImageViewModel> items = PopulateItemList(0, searchHits);

            SingleGalleryViewModel vm = SinglePageGenerator.SetDisplayProperties(items);
            vm.GalleryTitle = "Search results";
            vm.TotalImageCount = searchDetails.HasMoreResults ? searchHits.Count + 1 : searchHits.Count;
            vm.CurrentOffset = hitsToSkip ?? 0;
            vm.DisplayCount = DISPLAY_COUNT_MAX;

            return View("Index", vm);
        }

        [HttpGet("search/scroll")]
        public async Task <IActionResult> ScrollSearch(int from)
        {
            if (_searchCache.ContainsKey(_username) == false) RedirectToAction("Index", "Customizer");
            
            SearchDetails cachedResults = _searchCache[_username];
            if (from >= cachedResults.Hits.Count && cachedResults.HasMoreResults)
            {
                return await Search(cachedResults.Albums, cachedResults.Tags, cachedResults.FileExtensions, cachedResults.MediaNameContains, cachedResults.MaxSize, cachedResults.AllTagsMustMatch, hitsToSkip: from);
            }

            List<SingleGalleryImageViewModel> items = PopulateItemList(from, cachedResults.Hits);
            
            SingleGalleryViewModel vm = SinglePageGenerator.SetDisplayProperties(items);
            vm.GalleryTitle = "Search results";
            vm.TotalImageCount = cachedResults.Hits.Count;
            vm.CurrentOffset = from;
            vm.DisplayCount = DISPLAY_COUNT_MAX;

            return View("Index", vm);
        }

        [HttpGet("custom-js")]
        public async Task<IActionResult> CustomJs(int mediaCount, string tags, string tagFilterMode, string mediaFilterMode)
        {
            return View("CustomJs");
        }

        private static List<SingleGalleryImageViewModel> PopulateItemList(int from, List<SearchHitDTO> searchHits)
        {
            List<SingleGalleryImageViewModel> items = [];
            int i = 0;
            foreach (SearchHitDTO hit in searchHits.Skip(from))
            {
                if (i >= DISPLAY_COUNT_MAX) break;
                else i++;

                SingleGalleryImageViewModel imageVm = new()
                {
                    Id = hit.MediaItem.Id,
                    AppPath = Path.Combine(hit.AlbumName, hit.MediaItem.Name),
                    MediaType = Utils.DetermineMediaType(hit.MediaItem.Name),
                };

                items.Add(imageVm);
            }

            return items;
        }
    }
}