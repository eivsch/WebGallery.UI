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
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using WebGallery.UI.Configuration;
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
        public string CreatedAfter { get; set; }
        public string CreatedBefore { get; set; }
        public bool HasMoreResults { get; set; }
    }

    [Authorize]
    [Route("[controller]")]
    public class SingleController : Controller
    {
        private static readonly TimeSpan SearchCacheExpiry = TimeSpan.FromMinutes(30);
        private const string SearchCacheKeyPrefix = "search_";
        private const int SearchBatchLimit = 200;

        readonly MinimalApiProxy _minimalApiProxy;
        readonly IMemoryCache _cache;
        readonly DisplayOptions _displayOptions;
        readonly string _username;

        public SingleController(MinimalApiProxy minimalApiProxy, IHttpContextAccessor httpContext, IMemoryCache cache, IOptions<DisplayOptions> displayOptions)
        {
            _minimalApiProxy = minimalApiProxy;
            _cache = cache;
            _displayOptions = displayOptions.Value;
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
            while (currentCount < _displayOptions.PageSize)
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
            vm.TotalImageCount = _displayOptions.PageSize;
            vm.CurrentOffset = 0;
            vm.DisplayCount = _displayOptions.PageSize;
            vm.IsRandomized = true;

            return View("Index", vm);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string albums = null, string tags = null, string fileExtensions = null, string mediaNameContains = null, int? maxSize = 200, bool? allTagsMustMatch = false, int? hitsToSkip = null, bool shuffle = false, string createdAfter = null, string createdBefore = null)
        {
            // Note: tags are searched for "exclusive", i.e. logical AND. Albums are inclusive, i.e. logical OR.
            ViewBag.Current = "Search";

            int requestedSize = GetRequestedSearchSize(maxSize);
            int startingOffset = hitsToSkip ?? 0;
            List<SearchHitDTO> searchHits = await GetSearchHitsAsync(albums, tags, fileExtensions, mediaNameContains, requestedSize, allTagsMustMatch ?? true, startingOffset, createdAfter, createdBefore);
            bool hasMoreResults = searchHits.Count == requestedSize;

            SearchDetails searchDetails = new()
            {
                Hits = searchHits,
                Albums = albums,
                Tags = tags,
                FileExtensions = fileExtensions,
                MediaNameContains = mediaNameContains,
                MaxSize = requestedSize,
                AllTagsMustMatch = allTagsMustMatch,
                CreatedAfter = createdAfter,
                CreatedBefore = createdBefore,
                HasMoreResults = hasMoreResults,
            };

            _cache.Set(SearchCacheKeyPrefix + _username, searchDetails, SearchCacheExpiry);

            if (shuffle)
            {
                ShuffleSearchHits();
            }
            
            List<SingleGalleryImageViewModel> items = PopulateItemList(0, searchHits);

            SingleGalleryViewModel vm = SinglePageGenerator.SetDisplayProperties(items);
            vm.GalleryTitle = "Search results";
            vm.TotalImageCount = searchDetails.HasMoreResults ? searchHits.Count + 1 : searchHits.Count;
            vm.CurrentOffset = hitsToSkip ?? 0;
            vm.DisplayCount = _displayOptions.PageSize;
            vm.IsRandomized = shuffle;

            return View("Index", vm);

            void ShuffleSearchHits()
            {
                Random rnd = new();
                for (int i = searchHits.Count - 1; i > 0; i--)
                {
                    int j = rnd.Next(0, i + 1);
                    SearchHitDTO temp = searchHits[i];
                    searchHits[i] = searchHits[j];
                    searchHits[j] = temp;
                }
            }
        }

        private static int GetRequestedSearchSize(int? maxSize)
        {
            int requestedSize = maxSize ?? SearchBatchLimit;
            return requestedSize > 0 ? requestedSize : SearchBatchLimit;
        }

        private async Task<List<SearchHitDTO>> GetSearchHitsAsync(string albums, string tags, string fileExtensions, string mediaNameContains, int requestedSize, bool allTagsMustMatch, int hitsToSkip, string createdAfter, string createdBefore)
        {
            List<SearchHitDTO> searchHits = [];
            int currentOffset = hitsToSkip;

            while (searchHits.Count < requestedSize)
            {
                int remaining = requestedSize - searchHits.Count;
                int batchSize = Math.Min(SearchBatchLimit, remaining);
                List<SearchHitDTO> batch = await _minimalApiProxy.GetSearch(_username, albums, tags, fileExtensions, mediaNameContains, batchSize, allTagsMustMatch, currentOffset, createdAfter, createdBefore);

                if (batch.Count == 0)
                {
                    break;
                }

                searchHits.AddRange(batch);
                currentOffset += batch.Count;

                if (batch.Count < batchSize)
                {
                    break;
                }
            }

            if (searchHits.Count > requestedSize)
            {
                searchHits = searchHits.Take(requestedSize).ToList();
            }

            return searchHits;
        }

        [HttpGet("search/scroll")]
        public async Task <IActionResult> ScrollSearch(int from)
        {
            if (!_cache.TryGetValue(SearchCacheKeyPrefix + _username, out SearchDetails cachedResults)) return RedirectToAction("Index", "Customizer");
            if (from >= cachedResults.Hits.Count && cachedResults.HasMoreResults)
            {
                return await Search(cachedResults.Albums, cachedResults.Tags, cachedResults.FileExtensions, cachedResults.MediaNameContains, cachedResults.MaxSize, cachedResults.AllTagsMustMatch, hitsToSkip: from, createdAfter: cachedResults.CreatedAfter, createdBefore: cachedResults.CreatedBefore);
            }

            List<SingleGalleryImageViewModel> items = PopulateItemList(from, cachedResults.Hits);
            
            SingleGalleryViewModel vm = SinglePageGenerator.SetDisplayProperties(items);
            vm.GalleryTitle = "Search results";
            vm.TotalImageCount = cachedResults.Hits.Count;
            vm.CurrentOffset = from;
            vm.DisplayCount = _displayOptions.PageSize;

            return View("Index", vm);
        }

        [HttpGet("custom-js")]
        public async Task<IActionResult> CustomJs(int mediaCount, string tags, string tagFilterMode, string mediaFilterMode)
        {
            return View("CustomJs");
        }

        private List<SingleGalleryImageViewModel> PopulateItemList(int from, List<SearchHitDTO> searchHits)
        {
            List<SingleGalleryImageViewModel> items = [];
            int i = 0;
            foreach (SearchHitDTO hit in searchHits.Skip(from))
            {
            if (i >= _displayOptions.PageSize) break;
                else i++;

                SingleGalleryImageViewModel imageVm = new()
                {
                    Id = hit.MediaItem.Id,
                    AppPath = Path.Combine(hit.AlbumName, hit.MediaItem.Name),
                    MediaType = Utils.DetermineMediaType(hit.MediaItem.Name),
                    Name = hit.MediaItem.Name,
                };

                items.Add(imageVm);
            }

            return items;
        }
    }
}