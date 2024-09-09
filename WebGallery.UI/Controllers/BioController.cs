using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Interfaces;
using Application.Tags;
using AutoMapper;
using Infrastructure.Common;
using Infrastructure.MinimalApi;
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
        private readonly MinimalApiProxy _minimalApiProxy;
        private readonly string _username;

        public BioController(IPictureService pictureService, ITagService tagService, IMapper mapper, MinimalApiProxy minimalApiProxy, UsernameResolver usernameResolver)
        {
            _pictureService = pictureService;
            _tagService = tagService;
            _mapper = mapper;
            _minimalApiProxy = minimalApiProxy;
            _username = usernameResolver.Username;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Current = "Bio";

            // Get a random picture
            Random rnd = new();
            AlbumMetaDTO album;
            int mediaIndex;
            
            List<AlbumMetaDTO> albums = await _minimalApiProxy.GetAlbums(_username);
            if (albums == null) return null;
            
            int albumIndex  = rnd.Next(0, albums.Count-1);  // creates a number between 0 and albums.Count
            album = albums[albumIndex];
            mediaIndex = rnd.Next(0, album.TotalCount-1);

            AlbumContentsDTO albumContents = await _minimalApiProxy.GetAlbumContents(_username, album.AlbumName, albumIndex, albumIndex+1);
            MediaDTO media = albumContents.Items[0];

            // Get all unique tags for user
            IEnumerable<TagMetaDTO> tags = albums.SelectMany(s => s.Tags);
            IEnumerable<string> allTags = tags.Select(s => s.TagName).Distinct();

            BioViewModel vm = new()
            {
                AllTags = allTags.ToList(),
                BioPictureViewModel = new BioPictureViewModel
                {
                    Id = media.Id,
                    Name = media.Name,
                    AppPath = $"{album.AlbumName}/{media.Name}",
                    Tags = media.Tags.Select(s => s.TagName).ToList(),             
                    GlobalSortOrder = -1,
                }
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