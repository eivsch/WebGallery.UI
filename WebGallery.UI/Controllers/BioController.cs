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
            int albumMediaIndex;
            
            List<AlbumMetaDTO> albums = await _minimalApiProxy.GetAlbums(_username);
            if (albums == null) return null;

            int albumIndex = rnd.Next(0, albums.Count); 
            album = albums[albumIndex];
            albumMediaIndex = rnd.Next(0, album.TotalCount);

            return RedirectToAction("Index", "Bio", new { album = album.AlbumName, index = albumMediaIndex });
        }

        [HttpGet("{album}/{index}")]
        public async Task<IActionResult> Index(string album, int index)
        {
            AlbumContentsDTO albumContents = await _minimalApiProxy.GetAlbumContents(_username, album, index, 1);
            MediaDTO media = albumContents.Items[0];

            List<AlbumMetaDTO> albums = await _minimalApiProxy.GetAlbums(_username);
            IEnumerable<TagMetaDTO> tags = albums.SelectMany(s => s.Tags);
            IEnumerable<string> allTags = tags.Select(s => s.TagName).Distinct();

            BioViewModel vm = new()
            {
                AllTags = allTags.ToList(),
                BioPictureViewModel = new BioPictureViewModel
                {
                    Id = media.Id,
                    Name = media.Name,
                    AppPath = $"{album}/{media.Name}",
                    Tags = media.Tags.Select(s => s.TagName).ToList(),
                    AlbumMediaIndex = index,
                    Album = album,
                }
            };

            return View(vm);
        }

        [HttpGet("id/{id}")]
        public async Task<IActionResult> Index(string id)
        {
            throw new NotImplementedException();

            var picture = await _pictureService.Get(id);
            var tags = await _tagService.GetAll();

            var vm = new BioViewModel
            {
                AllTags = tags.Select(s => s.TagName).ToList(),
                BioPictureViewModel = _mapper.Map<BioPictureViewModel>(picture)
            };

            return View(vm);
        }

        [HttpGet("switch/{album}/{id}")]
        public async Task<IActionResult> Switch(string album, int id)
        {
            AlbumContentsDTO albumContents = await _minimalApiProxy.GetAlbumContents(_username, album, id, 1);
            MediaDTO media = albumContents.Items[0];

            BioPictureViewModel vm = new()
            {
                Album = album,
                AlbumMediaIndex = id,
                AppPath = $"{album}/{media.Name}",
                Id = media.Id,
                Name = media.Name,
                Tags = media.Tags.Select(s => s.TagName).ToList(),
            };

            return PartialView("_Picture", vm);
        }

        [HttpPost("tag")]
        public async Task<IActionResult> AddTag(TagAjaxRequest data)
        {
            await _minimalApiProxy.PostTag(_username, data.Album, data.PictureId, data.Tag);

            return Ok();
        }

        [HttpPost("tag/delete")]
        public async Task<IActionResult> DeleteTag(TagAjaxRequest data)
        {
            bool success = await _minimalApiProxy.DeleteTag(_username, data.Album, data.PictureId, data.Tag);

            if (success) return Ok();
            else throw new Exception("Deletion failed.");
        }

        [HttpGet("picture/delete/{album}/{media}")]
        public async Task<IActionResult> DeletePicture(string album, string media)
        {
            await _minimalApiProxy.DeleteMedia(_username, album, media);

            return View("Deleted", new BioPictureViewModel { AlbumMediaIndex = 0});
        }
    }
}