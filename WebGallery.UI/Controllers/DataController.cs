using Infrastructure.MinimalApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
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

        [HttpGet("albums/search")]
        public async Task<IActionResult> SearchAlbums(string q, int maxResults = 200)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Ok(new List<AlbumMetaDTO>());
            }

            string query = q.Trim();
            int clampedMaxResults = Math.Min(Math.Max(maxResults, 1), 500);

            List<AlbumMetaDTO> albums = await _minimalApiProxy.GetAlbums(_username, size: clampedMaxResults);
            List<AlbumMetaDTO> filtered = albums
                .Where(w => !string.IsNullOrWhiteSpace(w.AlbumName)
                    && w.AlbumName.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderBy(o => o.AlbumName)
                .Take(clampedMaxResults)
                .ToList();

            return Ok(filtered);
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

        [HttpGet("tags/search")]
        public async Task<IActionResult> SearchTags(string q, int maxResults = 200)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Ok(new List<TagMetaDTO>());
            }

            string query = q.Trim();
            int clampedMaxResults = Math.Min(Math.Max(maxResults, 1), 500);

            List<AlbumMetaDTO> albums = await _minimalApiProxy.GetAlbums(_username);
            IEnumerable<TagMetaDTO> allTags = albums.SelectMany(s => s.Tags);
            List<TagMetaDTO> grouped = allTags
                .Where(w => !string.IsNullOrWhiteSpace(w.TagName)
                    && w.TagName.Contains(query, StringComparison.OrdinalIgnoreCase))
                .GroupBy(g => g.TagName, StringComparer.OrdinalIgnoreCase)
                .Select(sl => new TagMetaDTO
                {
                    TagName = sl.First().TagName,
                    Count = sl.Sum(c => c.Count)
                })
                .OrderBy(o => o.TagName)
                .Take(clampedMaxResults)
                .ToList();

            return Ok(grouped);
        }
    }
}
