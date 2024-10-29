using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Services.Interfaces;
using Infrastructure.MinimalApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NuGet.LibraryModel;
using WebGallery.UI.Generators;
using WebGallery.UI.Generators.Helpers;
using WebGallery.UI.Helpers;
using WebGallery.UI.ViewModels.Albums;

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
    }
}
