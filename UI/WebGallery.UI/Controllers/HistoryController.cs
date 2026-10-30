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

        public async Task<IActionResult> Index(int size = PageSize)
        {
            ViewBag.Current = "History";
            size = System.Math.Clamp(size, PageSize, 500);
            List<WatchHistoryItemDTO> items = await _minimalApiProxy.GetWatchHistory(_username, 0, size + 1);
            ViewBag.HasMore = items.Count > size && size < 500;
            ViewBag.NextSize = size + PageSize;
            if (items.Count > size) items = items.GetRange(0, size);

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
