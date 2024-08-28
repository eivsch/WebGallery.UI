using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Services.Interfaces;
using Application.Tags;
using Infrastructure.MinimalApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebGallery.UI.Generators;

namespace WebGallery.UI.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class SingleController : Controller
    {
        private readonly IGalleryService _galleryService;
        private readonly ITagService _tagService;
        readonly MinimalApiProxy _minimalApiProxy;
        readonly string _username;

        public SingleController(IGalleryService galleryService, ITagService tagService, MinimalApiProxy minimalApiProxy, IHttpContextAccessor httpContext)
        {
            _galleryService = galleryService;
            _tagService = tagService;
            _minimalApiProxy = minimalApiProxy;
            Claim claim = httpContext.HttpContext.User.Claims.FirstOrDefault(f => f.Type == ClaimTypes.Sid);
            _username = claim.Value;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Current = "Random";

            List<AlbumMetaDTO> albums = await _minimalApiProxy.GetAlbums(_username);
            


            var gallery = await _galleryService.Get(uri);

            var vm = SinglePageGenerator.Generate(gallery);
            vm.GalleryTitle = "Randomized";

            return View("Index", vm);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Index(string id, int offset)
        {
            ViewBag.Current = "Single";

            var galleryResponse = await _galleryService.Get(galleryId: id, itemIndexStart: offset + 1, numberOfItems: 48);
            
            var vm = SinglePageGenerator.Generate(galleryResponse);

            return View(vm);
        }

        [HttpGet("custom")]
        public async Task<IActionResult> Custom(int nbr, string tags, string tagFilterMode, string mediaFilterMode)
        {
            ViewBag.Current = "Random";

            var uri = await _galleryService.GenerateGalleryUri(nbr, tags, tagFilterMode, mediaFilterMode);
            var gallery = await _galleryService.Get(uri);

            var vm = SinglePageGenerator.Generate(gallery);
            if (!string.IsNullOrWhiteSpace(tags) && tagFilterMode == "custominclusive")
                vm.GalleryTitle = tags;
            else
                vm.GalleryTitle = "Randomized";

            return View("Index", vm);
        }

        [HttpPost("add-like/{id}")]
        public async Task<IActionResult> AddLike(string id)
        {
            var request = new TagRequest
            {
                PictureId = id,
                TagName = "like",
            };

            await _tagService.Add(request);

            return Ok();
        }
    }
}