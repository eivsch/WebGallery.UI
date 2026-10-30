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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using WebGallery.UI.Configuration;
using WebGallery.UI.Generators;
using WebGallery.UI.Generators.Helpers;
using WebGallery.UI.Helpers;
using WebGallery.UI.ViewModels.Albums;
using WebGallery.UI.ViewModels.Single;

namespace WebGallery.UI.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class AlbumsController : Controller
    {
        private readonly ILogger<AlbumsController> _logger;
        private readonly MinimalApiProxy _minimalApiProxy;
        private readonly DisplayOptions _displayOptions;
        readonly string _username;

        public AlbumsController(ILogger<AlbumsController> logger, MinimalApiProxy minimalApiProxy, IHttpContextAccessor httpContext, IOptions<DisplayOptions> displayOptions)
        {
            _logger = logger;
            _minimalApiProxy = minimalApiProxy;
            _displayOptions = displayOptions.Value;
            Claim claim = httpContext.HttpContext.User.Claims.FirstOrDefault(f => f.Type == ClaimTypes.Sid);
            _username = claim.Value;
        }

        public async Task<IActionResult> Index(int offset = 0, int? displayCount = null, bool randomAlbumOrder = false, bool randomCoverImage = false)
        {
            ViewBag.Current = "Albums";
            Random rnd = new();

            int resolvedDisplayCount = displayCount.GetValueOrDefault(_displayOptions.PageSize);
            if (resolvedDisplayCount <= 0)
            {
                resolvedDisplayCount = _displayOptions.PageSize;
            }

            int resolvedOffset = Math.Max(0, offset);
            PagedAlbumMetaDTO albumPage = randomAlbumOrder
                ? await GetRandomAlbumsAsync(resolvedDisplayCount)
                : await _minimalApiProxy.GetAlbumsPage(_username, resolvedOffset, resolvedDisplayCount);

            if (albumPage?.Albums == null) return null;

            List<AlbumViewModel> albumVms = new();
            foreach (AlbumMetaDTO album in albumPage.Albums)
            {
                int i = randomCoverImage ? rnd.Next(0, album.TotalCount) : 0;
                AlbumContentsDTO c = await _minimalApiProxy.GetAlbumContents(_username, album.AlbumName, from: i, numberOfItems: 1);
                if (c.Items.Count == 0) continue;
                MediaDTO coverImg = c.Items[0];

                AlbumViewModel albumVm = new()
                {
                    GalleryId = album.AlbumName,
                    Title = album.AlbumName,
                    ItemCount = album.TotalCount,
                    CoverImageMediaType = Utils.DetermineMediaType(coverImg.Name),
                    CoverImageId = coverImg.Id,
                    CoverImageAppPath = Path.Combine(album.AlbumName, coverImg.Name),
                    CoverImageIndex = i,
                };
                
                albumVms.Add(albumVm);
            }

            var vm = AlbumsPageGenerator.SetDisplayProperties(albumVms);

            vm.TotalAlbumCount = albumPage.TotalCount;
            vm.CurrentOffset = randomAlbumOrder ? 0 : albumPage.From;
            vm.DisplayCount = resolvedDisplayCount;
            vm.IsRandomized = randomAlbumOrder;
            vm.RandomCoverImage = randomCoverImage;

            return View(vm);

            async Task<PagedAlbumMetaDTO> GetRandomAlbumsAsync(int pageSize)
            {
                List<AlbumMetaDTO> allAlbums = [];
                int currentOffset = 0;

                while (true)
                {
                    PagedAlbumMetaDTO batch = await _minimalApiProxy.GetAlbumsPage(_username, currentOffset, pageSize);
                    if (batch.Albums.Count == 0)
                    {
                        break;
                    }

                    allAlbums.AddRange(batch.Albums);
                    currentOffset = batch.From + batch.CurrentSize;

                    if (currentOffset >= batch.TotalCount || batch.CurrentSize < pageSize)
                    {
                        break;
                    }
                }

                allAlbums.ShuffleList();
                List<AlbumMetaDTO> randomizedPage = allAlbums.Take(pageSize).ToList();

                return new PagedAlbumMetaDTO
                {
                    Albums = randomizedPage,
                    TotalCount = allAlbums.Count,
                    From = 0,
                    CurrentSize = randomizedPage.Count,
                };
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAlbum(string id, int offset = 0, int? displayCount = null)
        {
            ViewBag.Current = "Albums";

            int resolvedDisplayCount = displayCount.GetValueOrDefault(_displayOptions.PageSize);
            if (resolvedDisplayCount <= 0)
            {
                resolvedDisplayCount = _displayOptions.PageSize;
            }

            AlbumContentsDTO data = await _minimalApiProxy.GetAlbumContents(_username, id, offset, numberOfItems: resolvedDisplayCount);

            List<SingleGalleryImageViewModel> items = new();
            int indexCounter = offset;
            foreach (var media in data.Items)
            {
                SingleGalleryImageViewModel imageVm = new()
                {
                    Id = media.Id,
                    AppPath = Path.Combine(id, media.Name),
                    GalleryIndex = indexCounter++,
                    IndexGlobal = -1,
                    MediaType = Utils.DetermineMediaType(media.Name),
                    Name = media.Name,
                    AlbumName = id,
                };
                items.Add(imageVm);
            }

            var vm = SinglePageGenerator.SetDisplayProperties(items);
            vm.Id = id;
            vm.GalleryTitle = id;
            vm.TotalImageCount = data.TotalCount;
            vm.CurrentOffset = offset;
            vm.DisplayCount = resolvedDisplayCount;

            return View("Album", vm);
        }

        [HttpPost("{album}/{media}/add-like")]
        public async Task<IActionResult> AddLike(string album, string media)
        {
            await _minimalApiProxy.PatchAddLike(_username, album, media);

            return Ok();
        }
    }
}
