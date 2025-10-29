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

namespace WebGallery.UI.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class AdminController : Controller
    {
        readonly string _username;
        readonly MinimalApiProxy _minimalApiProxy;
        readonly IFileServerProxy _fileSystemService;

        public AdminController(MinimalApiProxy minimalApiProxy, IHttpContextAccessor httpContext, IFileServerProxy fileSystemService)
        {
            _minimalApiProxy = minimalApiProxy;
            Claim claim = httpContext.HttpContext.User.Claims.FirstOrDefault(f => f.Type == ClaimTypes.Sid);
            _username = claim?.Value;
            _fileSystemService = fileSystemService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var albums = await _minimalApiProxy.GetAlbums(_username);
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
                    // Check if album already exists (case-insensitive)
                    var existingAlbums = await _minimalApiProxy.GetAlbums(_username);
                    if (existingAlbums != null && existingAlbums.Any(a => string.Equals(a.AlbumName, newTargetAlbum, StringComparison.OrdinalIgnoreCase)))
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
                    return Ok(new { success = true, message = $"Merged {sourceAlbums.Count} album(s) into '{actualTarget}'." });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = "API merge failed: " + ex.Message + ". Files may have been moved on the file server." });
                }
            }
        }

        [HttpGet("albums/{albumName}")]
        public async Task<IActionResult> Album(string albumName)
        {
            var albumContents = await _minimalApiProxy.GetAlbumContents(_username, albumName, 0, 1000); // adjust size as needed
            ViewBag.AlbumName = albumName;
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
    }
}
