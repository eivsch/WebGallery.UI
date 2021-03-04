using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebGallery.UI.Generators;

namespace WebGallery.UI.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class AlbumsController : Controller
    {
        private readonly ILogger<AlbumsController> _logger;
        private readonly IGalleryService _galleryService;
        private readonly IPictureService _pictureService;

        public AlbumsController(ILogger<AlbumsController> logger, IGalleryService galleryService, IPictureService pictureService)
        {
            _logger = logger;
            _galleryService = galleryService;
            _pictureService = pictureService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Current = "Albums";

            var allAlbums = await _galleryService.GetAllGalleriesWithoutItems();
            var allAlbumsVm = AlbumsPageGenerator.GenerateAllRandom(allAlbums.ToList());
            
            foreach (var albumVm in allAlbumsVm.Albums)
            {
                var picture = await _pictureService.GetRandom(albumVm.GalleryId);
                albumVm.CoverImageMediaType = picture.MediaType;
                albumVm.CoverImageId = picture.Id;
                albumVm.CoverImageAppPath = picture.AppPath;
                albumVm.CoverImageIndex = picture.FolderSortOrder;
            }

            return View(allAlbumsVm);
        }
    }
}
