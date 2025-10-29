using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Infrastructure.MinimalApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebGallery.UI.ViewModels;


namespace WebGallery.UI.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly MinimalApiProxy _minimalApiProxy;
        readonly string _username;

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
            List<string> infos = [];
            infos.Add($"Total: {a.Count}");
            
            AlbumMetaDTO lastAdded = a.OrderByDescending(x => x.Created).ToList()[0];
            infos.Add($"Most Recent: '{lastAdded.AlbumName}' - {lastAdded.Created.ToString()[..10]}");

            AlbumMetaDTO mostLikesTotal = a.OrderByDescending(x => x.TotalLikes).ToList()[0];
            infos.Add($"Most likes in total: '{mostLikesTotal.AlbumName}' - {mostLikesTotal.TotalLikes}");

            AlbumMetaDTO mostUniqueLikes = a.OrderByDescending(x => x.TotalUniqueLikes).ToList()[0];
            infos.Add($"Most unique item likes: '{mostUniqueLikes.AlbumName}' - {mostUniqueLikes.TotalUniqueLikes}");

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
            List<string> infos = [];
            int totalTags = a.Select(s => s.Tags.Count).Sum();
            infos.Add($"Total: {totalTags}");

            IEnumerable<TagMetaDTO> allTags = a.SelectMany(s => s.Tags);
            int uniqueTags = allTags.Select(s => s.TagName).Distinct().Count();
            infos.Add($"Total unique: {uniqueTags}");

            IEnumerable<TagMetaDTO> grouped = allTags.GroupBy(g => g.TagName)
                .Select(sl => new TagMetaDTO
                {
                    TagName = sl.First().TagName,
                    Count = sl.Sum(c => c.Count)
                });

            if (grouped.Any())
            {
                TagMetaDTO r = grouped.OrderByDescending(o => o.Count).Take(1).ToList()[0];
                infos.Add($"Most popular tag: {r.Count}");
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
            List<AlbumMetaDTO> a = await _minimalApiProxy.GetAlbums(_username);
            List<string> infos = [];
            int totalItems = a.Select(s => s.TotalCount).Sum();
            infos.Add($"Total: {totalItems}");

            // TODO: most recent

            // TODO: most liked

            StatsInfoCardViewModel vm = new()
            {
                Header = "Media",
                Headerlink = "/single",
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
                "tag" => "tags",
                _ => "/single"
            };

            return headerLink;
        }
    }
}
