using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Services;
using Application.Services.Interfaces;
using Application.Tags;
using Infrastructure.MinimalApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly IGalleryService _galleryService;
        private readonly IPictureService _pictureService;
        private readonly MinimalApiProxy _minimalApiProxy;
        readonly string _username;

        public AlbumsController(ILogger<AlbumsController> logger, IGalleryService galleryService, IPictureService pictureService, MinimalApiProxy minimalApiProxy, IHttpContextAccessor httpContext)
        {
            _logger = logger;
            _galleryService = galleryService;
            _pictureService = pictureService;
            _minimalApiProxy = minimalApiProxy;
            Claim claim = httpContext.HttpContext.User.Claims.FirstOrDefault(f => f.Type == ClaimTypes.Sid);
            _username = claim.Value;
        }

        public async Task<IActionResult> Index(bool randomAlbumOrder = false, bool randomCoverImage = false)
        {
            ViewBag.Current = "Albums";
            Random rnd = new();
            
            List<AlbumMetaDTO> albums = await _minimalApiProxy.GetAlbums(_username);
            if (albums == null) return null;

            List<AlbumViewModel> albumVms = new();
            foreach (AlbumMetaDTO album in albums)
            {
                int i = randomCoverImage ? rnd.Next(0, album.TotalCount) : 0;
                AlbumContentsDTO c = await _minimalApiProxy.GetAlbumContents(_username, album.AlbumName, from: i, numberOfItems: 1);
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
            if (randomAlbumOrder)
                albumVms.ShuffleList();

            return View(vm);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAlbum(string id, int offset = 0, int displayCount = 12)
        {
            AlbumContentsDTO data = await _minimalApiProxy.GetAlbumContents(_username, id, offset, numberOfItems: displayCount);

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
                };
                items.Add(imageVm);
            }

            var vm = SinglePageGenerator.SetDisplayProperties(items);
            vm.Id = id;
            vm.GalleryTitle = id;
            vm.TotalImageCount = data.TotalCount;
            vm.CurrentOffset = offset;
            vm.CurrentDisplayCount = displayCount;

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
