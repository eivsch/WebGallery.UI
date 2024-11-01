using Infrastructure.MinimalApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebGallery.UI.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class DataController : Controller
    {
        private readonly MinimalApiProxy _minimalApiProxy;
        readonly string _username;

        public DataController(MinimalApiProxy minimalApiProxy, IHttpContextAccessor httpContext)
        {
            _minimalApiProxy = minimalApiProxy;
            Claim claim = httpContext.HttpContext.User.Claims.FirstOrDefault(f => f.Type == ClaimTypes.Sid);
            _username = claim.Value;
        }

        [HttpGet("albums")]
        public async Task<IActionResult> GetAlbums()
        {
            List<AlbumMetaDTO> result = await _minimalApiProxy.GetAlbums(_username);
            return Ok(result);
        }

        [HttpGet("albums/{album}")]
        public async Task<IActionResult> GetAlbumItems(string album, int from = 0, int itemCount = 32)
        {
            AlbumContentsDTO result = await _minimalApiProxy.GetAlbumContents(_username, album, from, itemCount);
            return Ok(result);
        }

        [HttpGet("tags")]
        public async Task<IActionResult> GetTags()
        {
            List<AlbumMetaDTO> a = await _minimalApiProxy.GetAlbums(_username);
            IEnumerable<TagMetaDTO> allTags = a.SelectMany(s => s.Tags);
            List<TagMetaDTO> grouped = allTags.GroupBy(g => g.TagName)
                .Select(sl => new TagMetaDTO
                {
                    TagName = sl.First().TagName,
                    Count = sl.Sum(c => c.Count)
                })
                .OrderBy(o => o.TagName)
                .ToList();

            return Ok(grouped);
        }
    }
}
