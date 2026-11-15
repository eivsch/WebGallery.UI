using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Common;
using Infrastructure.MinimalApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebGallery.UI.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class HistoryController : Controller
    {
        private readonly MinimalApiProxy _minimalApiProxy;
        private readonly string _username;

        public HistoryController(MinimalApiProxy minimalApiProxy, UsernameResolver usernameResolver)
        {
            _minimalApiProxy = minimalApiProxy;
            _username = usernameResolver.Username;
        }

        private const int PageSize = 50;

        public async Task<IActionResult> Index(int from = 0)
        {
            ViewBag.Current = "History";
            from = System.Math.Clamp(from, 0, int.MaxValue - PageSize);
            List<WatchHistoryItemDTO> items = await _minimalApiProxy.GetWatchHistory(_username, from, PageSize + 1);
            ViewBag.HasMore = items.Count > PageSize;
            ViewBag.HasPrevious = from > 0;
            ViewBag.PreviousFrom = System.Math.Max(0, from - PageSize);
            ViewBag.NextFrom = from + PageSize;
            if (items.Count > PageSize) items = items.GetRange(0, PageSize);

            return View(items);
        }

        [HttpPost("remove")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(string album, string mediaName)
        {
            await _minimalApiProxy.DeleteWatchHistoryEntry(_username, album, mediaName);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("clear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            await _minimalApiProxy.ClearWatchHistory(_username);
            return RedirectToAction(nameof(Index));
        }
    }
}
