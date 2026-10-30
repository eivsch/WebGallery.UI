using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Infrastructure.Common;
using Infrastructure.MinimalApi;
using Infrastructure.FileServer;
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
        private readonly MinimalApiProxy _minimalApiProxy;
        private readonly IFileServerProxy _fileSystemService;
        private readonly string _username;

        private List<AlbumMetaDTO> _albumsCache;

        public BioController(
            MinimalApiProxy minimalApiProxy,
            IFileServerProxy fileSystemService,
            UsernameResolver usernameResolver)
        {
            _minimalApiProxy = minimalApiProxy;
            _fileSystemService = fileSystemService;
            _username = usernameResolver.Username;
        }

        private async Task<List<AlbumMetaDTO>> GetAlbumsAsync()
        {
            if (_albumsCache == null)
                _albumsCache = await _minimalApiProxy.GetAllAlbums(_username);
            return _albumsCache;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Current = "Bio";

            List<AlbumMetaDTO> albums = await GetAlbumsAsync();
            if (albums == null) return null;

            Random rnd = new();
            int albumIndex = rnd.Next(0, albums.Count);
            AlbumMetaDTO album = albums[albumIndex];
            int albumMediaIndex = rnd.Next(0, album.TotalCount);

            BioViewModel vm = await BuildBioViewModelAsync(album.AlbumName, albumMediaIndex);
            return View(vm);
        }

        [HttpGet("{album}/{index}")]
        public async Task<IActionResult> Index(string album, int index)
        {
            ViewBag.Current = "Bio";
            BioViewModel vm = await BuildBioViewModelAsync(album, index);
            return View(vm);
        }

        [HttpGet("id/{id}")]
        public async Task<IActionResult> Index(string id)
        {
            ViewBag.Current = "Bio";

            List<SearchHitDTO> result = await _minimalApiProxy.GetSearch(username: _username, albums: null, tags: null, fileExtension: null, mediaNameContains: id, maxResults: 1, allTagsMustMatch: false);
            if (result.Count == 0) return NoContent();
            SearchHitDTO searchHit = result[0];

            List<AlbumMetaDTO> albums = await GetAlbumsAsync();
            IEnumerable<string> allTags = albums.SelectMany(s => s.Tags).Select(s => s.TagName).Distinct();

            BioViewModel vm = new()
            {
                AllTags = allTags.ToList(),
                BioPictureViewModel = new BioPictureViewModel
                {
                    Id = searchHit.MediaItem.Id,
                    Name = searchHit.MediaItem.Name,
                    AppPath = $"{searchHit.AlbumName}/{searchHit.MediaItem.Name}",
                    Tags = searchHit.MediaItem.Tags.Select(s => s.TagName).ToList(),
                    AlbumMediaIndex = searchHit.MediaAlbumIndex,
                    Album = searchHit.AlbumName,
                }
            };

            return View(vm);
        }

        private async Task<BioViewModel> BuildBioViewModelAsync(string album, int index)
        {
            AlbumContentsDTO albumContents = await _minimalApiProxy.GetAlbumContents(_username, album, index, 1);
            MediaDTO media = albumContents.Items[0];

            List<AlbumMetaDTO> albums = await GetAlbumsAsync();
            IEnumerable<string> allTags = albums.SelectMany(s => s.Tags).Select(s => s.TagName).Distinct();

            return new BioViewModel
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
        }

        [HttpGet("switch/{album}/{index}")]
        public async Task<IActionResult> Switch(string album, int index)
        {
            AlbumContentsDTO albumContents = await _minimalApiProxy.GetAlbumContents(_username, album, index, 1);
            MediaDTO media = albumContents.Items[0];

            BioPictureViewModel vm = new()
            {
                Album = album,
                AlbumMediaIndex = index,
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
            await _fileSystemService.DeleteFileFromFileServer(album, media); // Ensure the file is deleted from the file system as well

            return View("Deleted", new BioPictureViewModel { AlbumMediaIndex = 0 });
        }

        [HttpPost("played")]
        public async Task<IActionResult> RecordPlayed([FromBody] PlayedRequest request)
        {
            bool isMp4 = request?.MediaName != null && request.MediaName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase);
            if (!isMp4 || string.IsNullOrWhiteSpace(request.Album)) return BadRequest();

            await _minimalApiProxy.PostWatchHistory(_username, request.Album, request.MediaName, request.MediaId);
            return NoContent();
        }

        [HttpGet("resume")]
        public async Task<IActionResult> Resume(string album, string mediaName)
        {
            WatchHistoryItemDTO entry = await _minimalApiProxy.GetWatchHistoryEntry(_username, album, mediaName);
            double position = entry == null || entry.Watched ? 0 : entry.PositionSeconds;

            return Json(new { positionSeconds = position });
        }

        [HttpPost("position")]
        public async Task<IActionResult> SavePosition([FromBody] PositionRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Album) || string.IsNullOrWhiteSpace(request.MediaName)
                || !double.IsFinite(request.PositionSeconds) || request.PositionSeconds < 0) return BadRequest();

            double? duration = request.DurationSeconds is double d && double.IsFinite(d) && d > 0 ? d : null;
            await _minimalApiProxy.PutWatchPosition(_username, request.Album, request.MediaName, request.PositionSeconds, duration);
            return NoContent();
        }

        public class PositionRequest
        {
            public string Album { get; set; }
            public string MediaName { get; set; }
            public double PositionSeconds { get; set; }
            public double? DurationSeconds { get; set; }
        }

        // Exact lookup by media id; avoids the ambiguity of name-contains search.
        [HttpGet("open/{album}/{mediaId}")]
        public async Task<IActionResult> Open(string album, string mediaId)
        {
            MediaDTO media = await _minimalApiProxy.GetMedia(_username, album, mediaId);
            if (media?.Index == null) return NotFound();

            return Redirect($"/Bio/{Uri.EscapeDataString(album)}/{media.Index.Value}");
        }

        // Fallback for history entries without a media id: exact name match within the album.
        [HttpGet("open")]
        public async Task<IActionResult> OpenByName(string album, string mediaName)
        {
            if (string.IsNullOrWhiteSpace(album) || string.IsNullOrWhiteSpace(mediaName)) return BadRequest();

            List<SearchHitDTO> hits = await _minimalApiProxy.GetSearch(username: _username, albums: album, tags: null, fileExtension: null, mediaNameContains: mediaName, maxResults: 100, allTagsMustMatch: false);
            SearchHitDTO hit = hits?.FirstOrDefault(h =>
                string.Equals(h.AlbumName, album, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(h.MediaItem?.Name, mediaName, StringComparison.OrdinalIgnoreCase));
            if (hit == null) return NotFound();

            return Redirect($"/Bio/{Uri.EscapeDataString(hit.AlbumName)}/{hit.MediaAlbumIndex}");
        }

        public class PlayedRequest
        {
            public string Album { get; set; }
            public string MediaName { get; set; }
            public string MediaId { get; set; }
        }

        [HttpPost("SetVideoThumbnail")]
        public async Task<IActionResult> SetVideoThumbnail([FromBody] SetVideoThumbnailRequest request)
        {
            await _fileSystemService.GenerateVideoThumbnailAsync(request.AppPathB64, request.CurrentTime ?? "00:00:01");
            return Ok();
        }

        [HttpPost("GenerateVideoImage")]
        public async Task<IActionResult> GenerateVideoImage([FromBody] SetVideoThumbnailRequest request)
        {
            SavedFileInfo savedFile = await _fileSystemService.GenerateVideoImageAsync(request.AppPathB64, request.CurrentTime ?? "00:00:01.000");
            string fullDirName = Path.GetDirectoryName(savedFile.FilePathFull);
            string albumName = Path.GetFileName(fullDirName);
            List<AlbumMetaDTO> albums = await GetAlbumsAsync();
            if (albums.Any(a => a.AlbumName == albumName) == false)
            {
                // Create the album if it doesn't exist
                await _minimalApiProxy.CreateAlbum(_username, albumName);
            }

            await _minimalApiProxy.PostMediaItem(_username, albumName, savedFile);
            await _minimalApiProxy.PostTag(_username, albumName, savedFile.FileName, "screencap");

            return Ok();
        }

        public class SetVideoThumbnailRequest
        {
            public string AppPathB64 { get; set; }
            public string CurrentTime { get; set; }
        }
    }
}