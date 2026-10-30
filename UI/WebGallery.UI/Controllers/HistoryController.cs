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

        public async Task<IActionResult> Index()
        {
            ViewBag.Current = "History";
            List<WatchHistoryItemDTO> items = await _minimalApiProxy.GetWatchHistory(_username);

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
