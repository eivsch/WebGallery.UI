using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using Infrastructure.MinimalApi;
using Infrastructure.FileServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebGallery.UI.Configuration;

namespace WebGallery.UI.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class AdminController : Controller
    {
        readonly string _username;
        readonly MinimalApiProxy _minimalApiProxy;
        readonly IFileServerProxy _fileSystemService;
        readonly DisplayOptions _displayOptions;

        public AdminController(MinimalApiProxy minimalApiProxy, IHttpContextAccessor httpContext, IFileServerProxy fileSystemService, IOptions<DisplayOptions> displayOptions)
        {
            _minimalApiProxy = minimalApiProxy;
            Claim claim = httpContext.HttpContext.User.Claims.FirstOrDefault(f => f.Type == ClaimTypes.Sid);
            _username = claim?.Value;
            _fileSystemService = fileSystemService;
            _displayOptions = displayOptions.Value;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            ViewBag.Current = "Admin";

            var albums = await _minimalApiProxy.GetAllAlbums(_username);
            return View(albums);
        }

        [HttpPost("delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string albumName)
        {
            if (!string.IsNullOrWhiteSpace(albumName))
            {
                // Get all files in the album
                var albumContents = await _minimalApiProxy.GetAlbumContents(_username, albumName, 0, 10000);
                if (albumContents?.Items != null)
                {
                    foreach (var item in albumContents.Items)
                    {
                        await _fileSystemService.DeleteFileFromFileServer(albumName, item.Name);
                    }
                }

                await _minimalApiProxy.DeleteAlbum(_username, albumName);
            }
            return RedirectToAction(nameof(Index));
        }

        // Modified: Merge albums returns JSON result so the UI can parse messages without TempData.
        // Supports creating a new target album via newTargetAlbum. If new target is requested, create it first.
        // Also handles companion video_images albums to maintain consistency across merges.
        [HttpPost("merge")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Merge(string targetAlbum, [FromForm] List<string> sourceAlbums, string newTargetAlbum)
        {
            if (string.IsNullOrWhiteSpace(targetAlbum) && string.IsNullOrWhiteSpace(newTargetAlbum))
            {
                return BadRequest(new { success = false, message = "Please select or specify a target album." });
            }

            if (sourceAlbums == null || sourceAlbums.Count == 0)
            {
                return BadRequest(new { success = false, message = "Please select source album(s) to merge." });
            }

            // Choose actual target: prefer newTargetAlbum when provided
            var actualTarget = !string.IsNullOrWhiteSpace(newTargetAlbum) ? newTargetAlbum : targetAlbum;

            // Ensure target is not one of the sources
            if (sourceAlbums.Contains(actualTarget))
            {
                return BadRequest(new { success = false, message = "Target album must be different from the source album(s)." });
            }

            // If creating a new target, validate existence and create it first in the Minimal API
            if (!string.IsNullOrWhiteSpace(newTargetAlbum))
            {
                try
                {
                    if (await _minimalApiProxy.AlbumExists(_username, newTargetAlbum))
                    {
                        return BadRequest(new { success = false, message = "An album with that name already exists. Please choose a different name or select the existing album as the target." });
                    }

                    await _minimalApiProxy.CreateAlbum(_username, newTargetAlbum);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = "Failed to create target album: " + ex.Message });
                }

                // After creating target in metadata, call file server to move files
                try
                {
                    await _fileSystemService.MergeFolders(actualTarget, sourceAlbums);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = "File server merge failed: " + ex.Message });
                }

                // Then inform Minimal API to update metadata for the merge
                try
                {
                    await _minimalApiProxy.MergeAlbums(_username, actualTarget, sourceAlbums);
                    
                    // Handle companion video_images albums if any source has them
                    await MergeCompanionVideoImagesAsync(actualTarget, sourceAlbums);
                    
                    return Ok(new { success = true, message = $"Merged {sourceAlbums.Count} album(s) into '{actualTarget}'." });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = "API merge failed: " + ex.Message + ". Files may have been moved on the file server." });
                }
            }
            else
            {
                // No new target: original flow - file server first then Minimal API
                try
                {
                    await _fileSystemService.MergeFolders(actualTarget, sourceAlbums);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = "File server merge failed: " + ex.Message });
                }

                try
                {
                    await _minimalApiProxy.MergeAlbums(_username, actualTarget, sourceAlbums);
                    
                    // Handle companion video_images albums if any source has them
                    await MergeCompanionVideoImagesAsync(actualTarget, sourceAlbums);
                    
                    return Ok(new { success = true, message = $"Merged {sourceAlbums.Count} album(s) into '{actualTarget}'." });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = "API merge failed: " + ex.Message + ". Files may have been moved on the file server." });
                }
            }
        }

        [HttpPost("rename")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenameAlbum(string albumName, string newAlbumName)
        {
            if (string.IsNullOrWhiteSpace(albumName))
            {
                TempData["AdminAlbumError"] = "Album name is required.";
                return RedirectToAction(nameof(Index));
            }

            var trimmedNewAlbumName = (newAlbumName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(trimmedNewAlbumName))
            {
                TempData["AdminAlbumError"] = "A new album name is required.";
                return RedirectToAction(nameof(Index));
            }

            if (string.Equals(albumName, trimmedNewAlbumName, StringComparison.OrdinalIgnoreCase))
            {
                TempData["AdminAlbumMessage"] = "Album name is already set to that value.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (await _minimalApiProxy.AlbumExists(_username, trimmedNewAlbumName))
                {
                    TempData["AdminAlbumError"] = $"An album named '{trimmedNewAlbumName}' already exists.";
                    return RedirectToAction(nameof(Index));
                }

                await _fileSystemService.RenameFolder(albumName, trimmedNewAlbumName);

                var companionVideoImagesFolder = GetCompanionAlbumName(albumName);
                var renamedCompanionVideoImagesFolder = GetCompanionAlbumName(trimmedNewAlbumName);
                if (!string.Equals(companionVideoImagesFolder, renamedCompanionVideoImagesFolder, StringComparison.OrdinalIgnoreCase) &&
                    await _minimalApiProxy.AlbumExists(_username, companionVideoImagesFolder))
                {
                    await _fileSystemService.RenameFolder(companionVideoImagesFolder, renamedCompanionVideoImagesFolder);
                    await _minimalApiProxy.RenameAlbum(_username, companionVideoImagesFolder, renamedCompanionVideoImagesFolder);
                }

                await _minimalApiProxy.RenameAlbum(_username, albumName, trimmedNewAlbumName);
                TempData["AdminAlbumMessage"] = $"Renamed album '{albumName}' to '{trimmedNewAlbumName}'.";
            }
            catch (Exception ex)
            {
                TempData["AdminAlbumError"] = "Failed to rename album: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("albums/{albumName}")]
        public async Task<IActionResult> Album(string albumName)
        {
            var albumContents = await _minimalApiProxy.GetAlbumContents(_username, albumName, 0, 1000); // adjust size as needed
            var allAlbums = await _minimalApiProxy.GetAllAlbums(_username);
            ViewBag.AlbumName = albumName;
            ViewBag.AllAlbums = allAlbums;
            return View("Album", albumContents);
        }

        [HttpPost("albums/{albumName}/delete-file")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFile(string albumName, string fileName)
        {
            if (!string.IsNullOrWhiteSpace(albumName) && !string.IsNullOrWhiteSpace(fileName))
            {
                await _minimalApiProxy.DeleteMedia(_username, albumName, fileName);
                await _fileSystemService.DeleteFileFromFileServer(albumName, fileName); // Ensure the file is deleted from the file system as well
            }

            return RedirectToAction(nameof(Album), new { albumName });
        }

        [HttpPost("albums/{albumName}/delete-files")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFiles(string albumName, [FromForm] List<string> fileNames)
        {
            if (string.IsNullOrWhiteSpace(albumName) || fileNames == null || fileNames.Count == 0)
            {
                TempData["AdminAlbumError"] = "Please select at least one file to delete.";
                return RedirectToAction(nameof(Album), new { albumName });
            }

            var failedDeletes = new List<string>();
            var distinctFileNames = fileNames
                .Where(name => string.IsNullOrWhiteSpace(name) == false)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var fileName in distinctFileNames)
            {
                try
                {
                    await _minimalApiProxy.DeleteMedia(_username, albumName, fileName);
                    await _fileSystemService.DeleteFileFromFileServer(albumName, fileName);
                }
                catch
                {
                    failedDeletes.Add(fileName);
                }
            }

            if (failedDeletes.Count == 0)
            {
                TempData["AdminAlbumMessage"] = $"Deleted {distinctFileNames.Count} file(s).";
            }
            else
            {
                TempData["AdminAlbumError"] = $"Deleted {distinctFileNames.Count - failedDeletes.Count} file(s), but failed to delete {failedDeletes.Count} file(s).";
            }

            return RedirectToAction(nameof(Album), new { albumName });
        }

        [HttpPost("albums/{albumName}/rename-file")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenameFile(string albumName, string fileName, string newFileName, string mediaLocator)
        {
            if (string.IsNullOrWhiteSpace(albumName) || string.IsNullOrWhiteSpace(fileName))
            {
                TempData["AdminAlbumError"] = "Source album and file name are required.";
                return RedirectToAction(nameof(Album), new { albumName });
            }

            var trimmedNewFileName = (newFileName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(trimmedNewFileName))
            {
                TempData["AdminAlbumError"] = "A new file name is required.";
                return RedirectToAction(nameof(Album), new { albumName });
            }

            if (string.Equals(fileName, trimmedNewFileName, StringComparison.OrdinalIgnoreCase))
            {
                TempData["AdminAlbumMessage"] = "File name is already set to that value.";
                return RedirectToAction(nameof(Album), new { albumName });
            }

            try
            {
                var albumContents = await _minimalApiProxy.GetAlbumContents(_username, albumName, 0, 1000);
                if (albumContents?.Items != null && albumContents.Items.Any(item => string.Equals(item.Name, trimmedNewFileName, StringComparison.OrdinalIgnoreCase)))
                {
                    TempData["AdminAlbumError"] = $"A file named '{trimmedNewFileName}' already exists in this album.";
                    return RedirectToAction(nameof(Album), new { albumName });
                }

                var locator = string.IsNullOrWhiteSpace(mediaLocator) ? fileName : mediaLocator;

                var sourceExtension = System.IO.Path.GetExtension(fileName);
                var targetExtension = System.IO.Path.GetExtension(trimmedNewFileName);
                if (IsVideoFileName(fileName) && !string.Equals(sourceExtension, targetExtension, StringComparison.OrdinalIgnoreCase))
                {
                    TempData["AdminAlbumError"] = "Video file renames must keep the same extension.";
                    return RedirectToAction(nameof(Album), new { albumName });
                }

                await TryRenameMediaAsync(albumName, fileName, trimmedNewFileName, locator);

                TempData["AdminAlbumMessage"] = $"Renamed '{fileName}' to '{trimmedNewFileName}'.";
            }
            catch (Exception ex)
            {
                TempData["AdminAlbumError"] = "Failed to rename file: " + ex.Message;
            }

            return RedirectToAction(nameof(Album), new { albumName });
        }

        [HttpPost("albums/{albumName}/move-file")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveFile(string albumName, string fileName, string targetAlbum, string mediaLocator)
        {
            if (string.IsNullOrWhiteSpace(albumName) || string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(targetAlbum))
            {
                TempData["AdminAlbumError"] = "Source album, file name and target album are required.";
                return RedirectToAction(nameof(Album), new { albumName });
            }

            if (string.Equals(albumName, targetAlbum, StringComparison.OrdinalIgnoreCase))
            {
                TempData["AdminAlbumError"] = "Target album must be different from source album.";
                return RedirectToAction(nameof(Album), new { albumName });
            }

            try
            {
                var locator = string.IsNullOrWhiteSpace(mediaLocator) ? fileName : mediaLocator;
                await TryMoveMediaAsync(albumName, locator, targetAlbum, fileName);
                TempData["AdminAlbumMessage"] = "File moved successfully.";
            }
            catch (Exception ex)
            {
                TempData["AdminAlbumError"] = "Failed to move file: " + ex.Message;
            }

            return RedirectToAction(nameof(Album), new { albumName });
        }

        [HttpPost("albums/{albumName}/move-files")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveFiles(string albumName, [FromForm] List<string> fileNames, [FromForm] List<string> mediaLocators, string targetAlbum)
        {
            if (string.IsNullOrWhiteSpace(albumName) || fileNames == null || fileNames.Count == 0 || string.IsNullOrWhiteSpace(targetAlbum))
            {
                TempData["AdminAlbumError"] = "Source album, selected file(s) and target album are required.";
                return RedirectToAction(nameof(Album), new { albumName });
            }

            if (string.Equals(albumName, targetAlbum, StringComparison.OrdinalIgnoreCase))
            {
                TempData["AdminAlbumError"] = "Target album must be different from source album.";
                return RedirectToAction(nameof(Album), new { albumName });
            }

            var movedCount = 0;
            var failedMoves = new List<string>();

            for (var i = 0; i < fileNames.Count; i++)
            {
                var fileName = fileNames[i];
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    continue;
                }

                var mediaLocator = (mediaLocators != null && mediaLocators.Count > i)
                    ? mediaLocators[i]
                    : fileName;
                var locator = string.IsNullOrWhiteSpace(mediaLocator) ? fileName : mediaLocator;

                try
                {
                    await TryMoveMediaAsync(albumName, locator, targetAlbum, fileName);
                    movedCount++;
                }
                catch
                {
                    failedMoves.Add(fileName);
                }
            }

            if (failedMoves.Count == 0)
            {
                TempData["AdminAlbumMessage"] = $"Moved {movedCount} file(s) to '{targetAlbum}'.";
            }
            else if (movedCount > 0)
            {
                TempData["AdminAlbumError"] = $"Moved {movedCount} file(s), but failed to move {failedMoves.Count} file(s).";
            }
            else
            {
                TempData["AdminAlbumError"] = "Failed to move selected file(s).";
            }

            return RedirectToAction(nameof(Album), new { albumName });
        }

        private static bool IsVideoFileName(string fileName)
        {
            var extension = System.IO.Path.GetExtension(fileName);
            return string.Equals(extension, ".mp4", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".mov", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".mkv", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".webm", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetCompanionAlbumName(string albumName)
        {
            return $"{albumName}_video_images";
        }

        private async Task<List<SearchHitDTO>> GetVideoScreencapsAsync(string albumName, string videoFileName)
        {
            var oldVideoStem = System.IO.Path.GetFileNameWithoutExtension(videoFileName);
            var companionAlbumName = GetCompanionAlbumName(albumName);
            var screencapSearchLimit = Math.Max(1, _displayOptions.SingleSearchMaxResultsCap);
            var screencaps = await _minimalApiProxy.GetSearch(_username, companionAlbumName, "screencap", null, oldVideoStem, screencapSearchLimit, false);
            if (screencaps != null && screencaps.Count >= screencapSearchLimit)
            {
                throw new InvalidOperationException($"The screencap search cap of {screencapSearchLimit} was reached for this video. Increase 'Display:SingleSearchMaxResultsCap' in appsettings.json to allow more screencaps to be processed.");
            }

            return screencaps ?? new List<SearchHitDTO>();
        }

        private async Task EnsureCompanionAlbumExistsAsync(string albumName)
        {
            var companionAlbumName = GetCompanionAlbumName(albumName);
            if (!await _minimalApiProxy.AlbumExists(_username, companionAlbumName))
            {
                await _minimalApiProxy.CreateAlbum(_username, companionAlbumName);
            }
        }

        private async Task HandleVideoScreencapsForRenameAsync(string albumName, string fileName, string newFileName)
        {
            var oldVideoStem = System.IO.Path.GetFileNameWithoutExtension(fileName);
            var newVideoStem = System.IO.Path.GetFileNameWithoutExtension(newFileName);
            var companionAlbumName = GetCompanionAlbumName(albumName);
            var screencaps = await GetVideoScreencapsAsync(albumName, fileName);

            foreach (var screencap in screencaps)
            {
                var screencapName = screencap.MediaItem?.Name;
                if (string.IsNullOrWhiteSpace(screencapName))
                {
                    continue;
                }

                var screencapNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(screencapName);
                var screencapExtension = System.IO.Path.GetExtension(screencapName);
                var suffix = string.Empty;

                if (screencapNameWithoutExtension.StartsWith(oldVideoStem + "_", StringComparison.OrdinalIgnoreCase))
                {
                    suffix = screencapNameWithoutExtension.Substring(oldVideoStem.Length);
                }
                else if (string.Equals(screencapNameWithoutExtension, oldVideoStem, StringComparison.OrdinalIgnoreCase))
                {
                    suffix = string.Empty;
                }
                else
                {
                    continue;
                }

                var newScreencapName = newVideoStem + suffix + screencapExtension;
                if (string.Equals(screencapName, newScreencapName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                await _minimalApiProxy.RenameMedia(_username, companionAlbumName, screencap.MediaItem.Id, newScreencapName);
                await _fileSystemService.MoveFile(companionAlbumName, companionAlbumName, screencapName, newScreencapName);
            }
        }

        private async Task HandleVideoScreencapsForMoveAsync(string sourceAlbumName, string sourceFileName, string targetAlbumName, string targetFileName)
        {
            var sourceVideoStem = System.IO.Path.GetFileNameWithoutExtension(sourceFileName);
            var targetVideoStem = System.IO.Path.GetFileNameWithoutExtension(targetFileName);
            var sourceCompanionAlbumName = GetCompanionAlbumName(sourceAlbumName);
            var targetCompanionAlbumName = GetCompanionAlbumName(targetAlbumName);
            var screencaps = await GetVideoScreencapsAsync(sourceAlbumName, sourceFileName);

            if (!await _minimalApiProxy.AlbumExists(_username, targetCompanionAlbumName))
            {
                await _minimalApiProxy.CreateAlbum(_username, targetCompanionAlbumName);
            }

            foreach (var screencap in screencaps)
            {
                var screencapName = screencap.MediaItem?.Name;
                if (string.IsNullOrWhiteSpace(screencapName))
                {
                    continue;
                }

                var screencapNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(screencapName);
                var screencapExtension = System.IO.Path.GetExtension(screencapName);
                var suffix = string.Empty;

                if (screencapNameWithoutExtension.StartsWith(sourceVideoStem + "_", StringComparison.OrdinalIgnoreCase))
                {
                    suffix = screencapNameWithoutExtension.Substring(sourceVideoStem.Length);
                }
                else if (string.Equals(screencapNameWithoutExtension, sourceVideoStem, StringComparison.OrdinalIgnoreCase))
                {
                    suffix = string.Empty;
                }
                else
                {
                    continue;
                }

                var newScreencapName = targetVideoStem + suffix + screencapExtension;
                var moveResponse = await _minimalApiProxy.TryMoveMedia(_username, sourceCompanionAlbumName, screencap.MediaItem.Id, targetCompanionAlbumName, screencapName);
                if (!moveResponse.Success)
                {
                    throw new InvalidOperationException("Failed to move screencap: " + moveResponse.ErrorMessage);
                }

                await _fileSystemService.MoveFile(sourceCompanionAlbumName, targetCompanionAlbumName, screencapName, newScreencapName);
            }
        }

        private async Task TryRenameMediaAsync(string albumName, string fileName, string newFileName, string mediaLocator)
        {
            if (IsVideoFileName(fileName))
            {
                await HandleVideoScreencapsForRenameAsync(albumName, fileName, newFileName);
            }

            await _minimalApiProxy.RenameMedia(_username, albumName, mediaLocator, newFileName);
            await _fileSystemService.MoveFile(albumName, albumName, fileName, newFileName);
        }

        private async Task TryMoveMediaAsync(string sourceAlbum, string mediaLocator, string targetAlbum, string fileName)
        {
            MoveMediaResponseDTO moveMediaResponseDTO = await _minimalApiProxy.TryMoveMedia(_username, sourceAlbum, mediaLocator, targetAlbum, fileName);
            if (!moveMediaResponseDTO.Success)
            {
                throw new InvalidOperationException("Move failed with error: " + moveMediaResponseDTO.ErrorMessage);
            }

            if (IsVideoFileName(fileName))
            {
                await HandleVideoScreencapsForMoveAsync(sourceAlbum, fileName, targetAlbum, fileName);
            }

            await _fileSystemService.MoveFile(sourceAlbum, targetAlbum, fileName);
        }

        private async Task MergeCompanionVideoImagesAsync(string targetAlbum, List<string> sourceAlbums)
        {
            var targetCompanionAlbumName = GetCompanionAlbumName(targetAlbum);
            var sourceCompanionAlbumNames = sourceAlbums
                .Select(album => GetCompanionAlbumName(album))
                .ToList();

            var sourcesWithCompanionAlbums = new List<string>();
            foreach (var companionAlbumName in sourceCompanionAlbumNames)
            {
                if (await _minimalApiProxy.AlbumExists(_username, companionAlbumName))
                {
                    sourcesWithCompanionAlbums.Add(companionAlbumName);
                }
            }

            if (sourcesWithCompanionAlbums.Count == 0)
            {
                return;
            }

            await EnsureCompanionAlbumExistsAsync(targetAlbum);

            await _fileSystemService.MergeFolders(targetCompanionAlbumName, sourcesWithCompanionAlbums);
            await _minimalApiProxy.MergeAlbums(_username, targetCompanionAlbumName, sourcesWithCompanionAlbums);
        }

        [HttpGet("albums/{albumName}/rebuild-index")]
        public async Task<IActionResult> RebuildIndex(string albumName, [FromQuery] string type)
        {
            if (!string.IsNullOrWhiteSpace(albumName))
            {
                await _minimalApiProxy.PatchRebuildIndex(_username, albumName, type);
            }

            return RedirectToAction(nameof(Album), new { albumName });
        }

        [HttpGet("albums/{albumName}/rebuild-tags")]
        public async Task<IActionResult> RebuildTags(string albumName, [FromQuery] string type)
        {
            if (!string.IsNullOrWhiteSpace(albumName))
            {
                // TODO: Implement api call
                // Minimal API signature: app.MapPatch("/users/{username}/albums/{albumName}/rebuild-tags", (string username, string albumName) => { ... });
                //await _minimalApiProxy.PatchRebuildTags(_username, albumName, type);
            }

            return RedirectToAction(nameof(Album), new { albumName });
        }
    }
}
