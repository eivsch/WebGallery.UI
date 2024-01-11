using System.Linq;
using System.Threading.Tasks;
using Application.Services.Interfaces;
using Application.Tags;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebGallery.UI.Models;
using WebGallery.UI.ViewModels.Bio;

namespace WebGallery.UI.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class BioController : Controller
    {
        private readonly IPictureService _pictureService;
        private readonly ITagService _tagService;
        private readonly IMapper _mapper;

        public BioController(IPictureService pictureService, ITagService tagService, IMapper mapper)
        {
            _pictureService = pictureService;
            _tagService = tagService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Current = "Bio";

            var picture = await _pictureService.Get(-1);    // Random
            var tags = await _tagService.GetAll();

            var vm = new BioViewModel
            {
                AllTags = tags.Select(s => s.TagName).ToList(),
                BioPictureViewModel = _mapper.Map<BioPictureViewModel>(picture)
            };

            return View(vm);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Index(int id)
        {
            var picture = await _pictureService.Get(id);
            var tags = await _tagService.GetAll();

            var vm = new BioViewModel
            {
                AllTags = tags.Select(s => s.TagName).ToList(),
                BioPictureViewModel = _mapper.Map<BioPictureViewModel>(picture)
            };

            return View(vm);
        }

        [HttpGet("id/{id}")]
        public async Task<IActionResult> Index(string id)
        {
            var picture = await _pictureService.Get(id);
            var tags = await _tagService.GetAll();

            var vm = new BioViewModel
            {
                AllTags = tags.Select(s => s.TagName).ToList(),
                BioPictureViewModel = _mapper.Map<BioPictureViewModel>(picture)
            };

            return View(vm);
        }

        [HttpGet("switch/{id}")]
        public async Task<IActionResult> Switch(int id)
        {
            if (id < 1)
                id = -1;    // random

            var picture = await _pictureService.Get(id);

            var vm = _mapper.Map<BioPictureViewModel>(picture);

            return PartialView("_Picture", vm);
        }

        [HttpPost("tag")]
        public async Task<IActionResult> AddTag(TagAjaxRequest data)
        {
            var request = new TagRequest
            {
                PictureId = data.PictureId,
                TagName = data.Tag
            };

            await _tagService.Add(request);

            return Ok();
        }

        [HttpPost("tag/delete")]
        public async Task<IActionResult> DeleteTag(TagAjaxRequest data)
        {
            await _tagService.DeleteTag(data.PictureId, data.Tag);

            return Ok();
        }

        [HttpGet("picture/delete/{id}")]
        public async Task<IActionResult> DeletePicture(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            await _pictureService.Delete(id);

            return View("Deleted", new BioPictureViewModel { GlobalSortOrder = 0});
        }
    }
}