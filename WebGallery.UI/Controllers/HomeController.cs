using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.MinimalApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebGallery.UI.Helpers;
using WebGallery.UI.ViewModels;


namespace WebGallery.UI.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly MinimalApiProxy _minimalApiProxy;
        readonly string _username;
        private const int SearchBatchLimit = 1000;

        public HomeController(MinimalApiProxy minimalApiProxy, IHttpContextAccessor httpContext)
        {
            _minimalApiProxy = minimalApiProxy;
            Claim claim = httpContext.HttpContext.User.Claims.FirstOrDefault(f => f.Type == ClaimTypes.Sid);
            _username = claim.Value;
        }

        public IActionResult Index()
        {
            //if (!HttpContext.User.Identity.IsAuthenticated)
            //    return RedirectToAction("Index", "Login");

            ViewBag.Current = "Home";

            return View();
        }

        [HttpGet("home/random-bio")]
        public async Task<IActionResult> RandomBio(string fileExtensions)
        {
            List<SearchHitDTO> results = await _minimalApiProxy.GetSearch(
                _username,
                albums: null,
                tags: null,
                fileExtension: fileExtensions,
                mediaNameContains: null,
                maxSize: SearchBatchLimit,
                allTagsMustMatch: false);

            if (results == null || results.Count == 0)
                return RedirectToAction("Index");

            Random rnd = new();
            SearchHitDTO selected = results[rnd.Next(0, results.Count)];
            return Redirect($"/bio/id/{selected.MediaItem.Id}");
        }

        [HttpGet("home/banner-image")]
        public async Task<IActionResult> GetBannerImage()
        {
            List<SearchHitDTO> hits = await _minimalApiProxy.GetSearch(
                _username,
                albums: null,
                tags: "banner image",
                fileExtension: "jpg,jpeg,png,gif,webp,bmp",
                mediaNameContains: null,
                maxSize: 1000,
                allTagsMustMatch: true);

            if (hits == null || hits.Count == 0)
            {
                return PartialView("_BannerImage", string.Empty);
            }

            Random rnd = new();
            SearchHitDTO selected = hits[rnd.Next(0, hits.Count)];

            string appPath = Path.Combine(selected.AlbumName, selected.MediaItem.Name);
            byte[] appPathBytes = Encoding.UTF8.GetBytes(appPath);
            string appPathBase64 = System.Convert.ToBase64String(appPathBytes);
            string bannerImageUri = $"/files/image/{appPathBase64}";

            return PartialView("_BannerImage", bannerImageUri);
        }

        [HttpGet("home/stats")]
        public async Task<IActionResult> GetStatistics(string itemType)
        {
            //List<string> supportedTypes = ["picture", "gif", "video", "album", "tag", "media"];
            //if (!supportedTypes.Any(x => x == itemType))
            //    return null;

            StatsInfoCardViewModel vm = new();
            switch (itemType)
            {
                case "album":
                    vm = await GetAlbumStats();
                    break;
                case "tag":
                    vm = await GetTagStats();
                    break;
                case "media":
                    vm = await GetMediaStats();
                    break;
                case "gif":
                    vm = await GetGifStats();
                    break;
                case "video":
                    vm = await GetVideoStats();
                    break;
                default:
                    return null;
            }

            return PartialView("_StatsInfoCard", vm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        async Task<StatsInfoCardViewModel> GetAlbumStats()
        {
            List<AlbumMetaDTO> a = await _minimalApiProxy.GetAlbums(_username);
            List<InfoItemViewModel> infos = [];
            infos.Add(new InfoItemViewModel { Text = $"Total: {a.Count}", Url = null });
            
            AlbumMetaDTO lastAdded = a.OrderByDescending(x => x.Created).ToList()[0];
            infos.Add(new InfoItemViewModel 
            { 
                Text = $"Most Recent: '{lastAdded.AlbumName}' - {lastAdded.Created.ToString()[..10]}", 
                Url = $"/albums/{lastAdded.AlbumName}"
            });

            AlbumMetaDTO mostLikesTotal = a.OrderByDescending(x => x.TotalLikes).ToList()[0];
            infos.Add(new InfoItemViewModel 
            { 
                Text = $"Most likes in total: '{mostLikesTotal.AlbumName}' - {mostLikesTotal.TotalLikes}", 
                Url = $"/albums/{mostLikesTotal.AlbumName}"
            });

            AlbumMetaDTO mostUniqueLikes = a.OrderByDescending(x => x.TotalUniqueLikes).ToList()[0];
            infos.Add(new InfoItemViewModel 
            { 
                Text = $"Most unique item likes: '{mostUniqueLikes.AlbumName}' - {mostUniqueLikes.TotalUniqueLikes}", 
                Url = $"/albums/{mostUniqueLikes.AlbumName}"
            });

            StatsInfoCardViewModel vm = new()
            {
                Header = "Albums",
                Headerlink = "/albums",
                InfoItems = infos
            };

            return vm;
        }

        async Task<StatsInfoCardViewModel> GetTagStats()
        {
            List<AlbumMetaDTO> a = await _minimalApiProxy.GetAlbums(_username);
            List<InfoItemViewModel> infos = [];
            int totalTags = a.Select(s => s.Tags.Count).Sum();
            infos.Add(new InfoItemViewModel { Text = $"Total: {totalTags}", Url = null });

            IEnumerable<TagMetaDTO> allTags = a.SelectMany(s => s.Tags);
            int uniqueTags = allTags.Select(s => s.TagName).Distinct().Count();
            infos.Add(new InfoItemViewModel { Text = $"Total unique: {uniqueTags}", Url = null });

            IEnumerable<TagMetaDTO> grouped = allTags.GroupBy(g => g.TagName)
                .Select(sl => new TagMetaDTO
                {
                    TagName = sl.First().TagName,
                    Count = sl.Sum(c => c.Count)
                });

            if (grouped.Any())
            {
                TagMetaDTO r = grouped.OrderByDescending(o => o.Count).Take(1).ToList()[0];
                infos.Add(new InfoItemViewModel 
                { 
                    Text = $"Most popular tag: '{r.TagName}' - {r.Count}", 
                    Url = $"/single/search?tags={Uri.EscapeDataString(r.TagName)}"
                });
            }

            StatsInfoCardViewModel vm = new()
            {
                Header = "Tags",
                Headerlink = "/tags",
                InfoItems = infos
            };

            return vm;
        }

        async Task<StatsInfoCardViewModel> GetMediaStats()
        {
            int total = await GetTotalCountAsync(null);
            SearchHitDTO mostRecent = await GetMostRecentAsync(null);

            List<InfoItemViewModel> infos = [];
            infos.Add(new InfoItemViewModel { Text = $"Total: {total}", Url = null });

            if (mostRecent != null)
            {
                var bioUrl = $"/bio/id/{mostRecent.MediaItem.Id}";
                infos.Add(new InfoItemViewModel
                {
                    Text = $"Most recent: {mostRecent.MediaItem.Name} - {mostRecent.MediaItem.Created.ToString("yyyy-MM-dd")}",
                    Url = bioUrl
                });
            }

            // TODO: most liked

            StatsInfoCardViewModel vm = new()
            {
                Header = "Media",
                Headerlink = "/single",
                InfoItems = infos
            };

            return vm;
        }

        async Task<StatsInfoCardViewModel> GetGifStats()
        {
            int total = await GetTotalCountAsync("gif");
            SearchHitDTO mostRecent = await GetMostRecentAsync("gif");

            List<InfoItemViewModel> infos = [];
            infos.Add(new InfoItemViewModel { Text = $"Total: {total}", Url = null });

            if (mostRecent != null)
            {
                var bioUrl = $"/bio/id/{mostRecent.MediaItem.Id}";
                infos.Add(new InfoItemViewModel
                {
                    Text = $"Most recent: {mostRecent.MediaItem.Name} - {mostRecent.MediaItem.Created.ToString("yyyy-MM-dd")}",
                    Url = bioUrl
                });
            }

            StatsInfoCardViewModel vm = new()
            {
                Header = "GIFs",
                Headerlink = "/single/search?fileExtensions=gif",
                InfoItems = infos
            };

            return vm;
        }

        async Task<StatsInfoCardViewModel> GetVideoStats()
        {
            int total = await GetTotalCountAsync("mp4,avi,mov,mkv,flv,wmv,webm,m4v");
            SearchHitDTO mostRecent = await GetMostRecentAsync("mp4,avi,mov,mkv,flv,wmv,webm,m4v");

            List<InfoItemViewModel> infos = [];
            infos.Add(new InfoItemViewModel { Text = $"Total: {total}", Url = null });

            if (mostRecent != null)
            {
                var bioUrl = $"/bio/id/{mostRecent.MediaItem.Id}";
                infos.Add(new InfoItemViewModel
                {
                    Text = $"Most recent: {mostRecent.MediaItem.Name} - {mostRecent.MediaItem.Created.ToString("yyyy-MM-dd")}",
                    Url = bioUrl
                });
            }

            StatsInfoCardViewModel vm = new()
            {
                Header = "Videos",
                Headerlink = "/single/search?fileExtensions=mp4,avi,mov,mkv,flv,wmv,webm,m4v",
                InfoItems = infos
            };

            return vm;
        }


        string GetHeaderLink(string itemType)
        {
            string headerLink = itemType switch
            {
                "picture" => "/single",
                "album" => "/albums",
                "tag" => "/tags",
                _ => "/single"
            };

            return headerLink;
        }

        private async Task<int> GetTotalCountAsync(string fileExtension)
        {
            // Fetch a small batch just to count — paginate if needed
            List<SearchHitDTO> allResults = [];
            int currentOffset = 0;
            bool hasMore = true;

            while (hasMore)
            {
                List<SearchHitDTO> batch = await _minimalApiProxy.GetSearch(
                    _username,
                    albums: null,
                    tags: null,
                    fileExtension: fileExtension,
                    mediaNameContains: null,
                    maxSize: SearchBatchLimit,
                    allTagsMustMatch: false,
                    hitsToSkip: currentOffset);

                if (batch == null || batch.Count == 0)
                {
                    hasMore = false;
                }
                else
                {
                    allResults.AddRange(batch);
                    if (batch.Count < SearchBatchLimit)
                        hasMore = false;
                    else
                        currentOffset += batch.Count;
                }
            }

            return allResults.Count;
        }

        private async Task<SearchHitDTO> GetMostRecentAsync(string fileExtension)
        {
            // Use progressively wider date windows to find the most recent item
            // without loading all items into memory.
            int[] windowDays = [7, 30, 90, 365, 365 * 3, 365 * 10, 365 * 50];

            foreach (int days in windowDays)
            {
                string createdAfter = DateTime.UtcNow.AddDays(-days).ToString("yyyy-MM-dd");
                List<SearchHitDTO> batch = await _minimalApiProxy.GetSearch(
                    _username,
                    albums: null,
                    tags: null,
                    fileExtension: fileExtension,
                    mediaNameContains: null,
                    maxSize: SearchBatchLimit,
                    allTagsMustMatch: false,
                    createdAfter: createdAfter);

                if (batch?.Count > 0)
                    return batch.OrderByDescending(x => x.MediaItem.Created).First();
            }

            return null;
        }
    }
}
