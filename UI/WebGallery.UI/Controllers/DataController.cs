using Infrastructure.MinimalApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using WebGallery.UI.Configuration;
using WebGallery.UI.Helpers;

namespace WebGallery.UI.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class DataController : Controller
    {
        private const string SingleSearchCacheKeyPrefix = "single_search_";
        private static readonly TimeSpan SingleSearchCacheExpiry = TimeSpan.FromMinutes(10);

        private readonly MinimalApiProxy _minimalApiProxy;
        private readonly DisplayOptions _displayOptions;
        private readonly IMemoryCache _cache;
        readonly string _username;

        public DataController(MinimalApiProxy minimalApiProxy, IHttpContextAccessor httpContext, IOptions<DisplayOptions> displayOptions, IMemoryCache cache)
        {
            _minimalApiProxy = minimalApiProxy;
            _displayOptions = displayOptions.Value;
            _cache = cache;
            Claim claim = httpContext.HttpContext.User.Claims.FirstOrDefault(f => f.Type == ClaimTypes.Sid);
            _username = claim.Value;
        }

        [HttpGet("albums")]
        public async Task<IActionResult> GetAlbums()
        {
            List<AlbumMetaDTO> result = await _minimalApiProxy.GetAllAlbums(_username);
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

            List<AlbumMetaDTO> albums = await _minimalApiProxy.GetAllAlbums(_username);
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
            List<AlbumMetaDTO> a = await _minimalApiProxy.GetAllAlbums(_username);
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

            List<AlbumMetaDTO> albums = await _minimalApiProxy.GetAllAlbums(_username);
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

        [HttpGet("single/search")]
        public async Task<IActionResult> SearchSingle(string q, int page = 1)
        {
            int minQueryLength = Math.Max(1, _displayOptions.SingleSearchMinQueryLength);
            int pageSize = Math.Max(1, _displayOptions.PageSize);
            int maxResultsCap = Math.Max(pageSize, _displayOptions.SingleSearchMaxResultsCap);
            int scanBatchSize = Math.Clamp(_displayOptions.SingleSearchScanBatchSize, 1, 500);
            int maxHitsToScan = Math.Max(scanBatchSize, _displayOptions.SingleSearchMaxHitsToScan);

            if (string.IsNullOrWhiteSpace(q))
            {
                return Ok(new SingleSearchResponse
                {
                    Page = 1,
                    PageSize = pageSize,
                    TotalMatches = 0,
                    TotalPages = 1,
                    MinimumQueryLength = minQueryLength,
                    QueryTooShort = true,
                    ResultCap = maxResultsCap,
                    ResultCapReached = false,
                    SearchWasTruncated = false,
                    Items = []
                });
            }

            string query = q.Trim();
            if (query.Length < minQueryLength)
            {
                return Ok(new SingleSearchResponse
                {
                    Page = 1,
                    PageSize = pageSize,
                    TotalMatches = 0,
                    TotalPages = 1,
                    MinimumQueryLength = minQueryLength,
                    QueryTooShort = true,
                    ResultCap = maxResultsCap,
                    ResultCapReached = false,
                    SearchWasTruncated = false,
                    Items = []
                });
            }

            int safePage = Math.Max(1, page);
            string cacheKey = BuildSingleSearchCacheKey(query, maxResultsCap, scanBatchSize, maxHitsToScan);
            if (!_cache.TryGetValue(cacheKey, out SingleSearchCacheEntry cacheEntry))
            {
                cacheEntry = await BuildSingleSearchCacheEntryAsync(query, maxResultsCap, scanBatchSize, maxHitsToScan);
                _cache.Set(cacheKey, cacheEntry, SingleSearchCacheExpiry);
            }

            int totalMatches = cacheEntry.TotalMatches;
            int totalPages = Math.Max(1, (int)Math.Ceiling(totalMatches / (double)pageSize));
            int clampedPage = Math.Min(safePage, totalPages);
            List<SingleSearchItem> items = cacheEntry.Items
                .Skip((clampedPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new SingleSearchResponse
            {
                Page = clampedPage,
                PageSize = pageSize,
                TotalMatches = totalMatches,
                TotalPages = totalPages,
                MinimumQueryLength = minQueryLength,
                QueryTooShort = false,
                ResultCap = maxResultsCap,
                ResultCapReached = cacheEntry.ResultCapReached,
                SearchWasTruncated = cacheEntry.SearchWasTruncated,
                Items = items
            });
        }

        private async Task<SingleSearchCacheEntry> BuildSingleSearchCacheEntryAsync(string query, int maxResultsCap, int scanBatchSize, int maxHitsToScan)
        {
            int hitsToSkip = 0;
            int scannedHits = 0;
            List<SingleSearchItem> matchedItems = [];

            while (matchedItems.Count <= maxResultsCap && scannedHits < maxHitsToScan)
            {
                List<SearchHitDTO> batch = await _minimalApiProxy.GetSearch(_username, null, null, null, null, scanBatchSize, false, hitsToSkip);
                if (batch.Count == 0)
                {
                    break;
                }

                scannedHits += batch.Count;
                hitsToSkip += batch.Count;

                foreach (SearchHitDTO hit in batch)
                {
                    if (!MatchesQuery(hit, query))
                    {
                        continue;
                    }

                    matchedItems.Add(MapToSingleSearchItem(hit));
                    if (matchedItems.Count > maxResultsCap)
                    {
                        break;
                    }
                }

                if (batch.Count < scanBatchSize)
                {
                    break;
                }
            }

            bool resultCapReached = matchedItems.Count > maxResultsCap;
            if (resultCapReached)
            {
                matchedItems = matchedItems.Take(maxResultsCap).ToList();
            }

            bool searchWasTruncated = resultCapReached || scannedHits >= maxHitsToScan;
            return new SingleSearchCacheEntry
            {
                Items = matchedItems,
                TotalMatches = matchedItems.Count,
                ResultCapReached = resultCapReached,
                SearchWasTruncated = searchWasTruncated,
            };
        }

        private string BuildSingleSearchCacheKey(string query, int maxResultsCap, int scanBatchSize, int maxHitsToScan)
        {
            return string.Join('|',
                SingleSearchCacheKeyPrefix + _username,
                query.Trim().ToLowerInvariant(),
                maxResultsCap,
                scanBatchSize,
                maxHitsToScan);
        }

        private static bool MatchesQuery(SearchHitDTO hit, string queryValue)
        {
            if (hit is null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(hit.MediaItem?.Name) && hit.MediaItem.Name.Contains(queryValue, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(hit.AlbumName) && hit.AlbumName.Contains(queryValue, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return hit.MediaItem?.Tags?.Any(a => !string.IsNullOrWhiteSpace(a.TagName) && a.TagName.Contains(queryValue, StringComparison.OrdinalIgnoreCase)) == true;
        }

        private static SingleSearchItem MapToSingleSearchItem(SearchHitDTO hit)
        {
            MediaDTO media = hit.MediaItem;
            return new SingleSearchItem
            {
                Id = media.Id,
                Name = media.Name,
                AlbumName = hit.AlbumName,
                AppPath = Path.Combine(hit.AlbumName, media.Name),
                TagSearchText = BuildTagSearchText(media.Tags),
                MediaType = Utils.DetermineMediaType(media.Name).ToString()
            };
        }

        private static string BuildTagSearchText(List<TagDTO> tags)
        {
            if (tags is null || tags.Count == 0)
            {
                return string.Empty;
            }

            return string.Join(',', tags
                .Where(w => !string.IsNullOrWhiteSpace(w.TagName))
                .Select(s => s.TagName));
        }

        private class SingleSearchResponse
        {
            public int Page { get; set; }
            public int PageSize { get; set; }
            public int TotalMatches { get; set; }
            public int TotalPages { get; set; }
            public int MinimumQueryLength { get; set; }
            public bool QueryTooShort { get; set; }
            public int ResultCap { get; set; }
            public bool ResultCapReached { get; set; }
            public bool SearchWasTruncated { get; set; }
            public List<SingleSearchItem> Items { get; set; } = [];
        }

        private class SingleSearchItem
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string AlbumName { get; set; }
            public string AppPath { get; set; }
            public string TagSearchText { get; set; }
            public string MediaType { get; set; }
        }

        private class SingleSearchCacheEntry
        {
            public List<SingleSearchItem> Items { get; set; } = [];
            public int TotalMatches { get; set; }
            public bool ResultCapReached { get; set; }
            public bool SearchWasTruncated { get; set; }
        }
    }
}
