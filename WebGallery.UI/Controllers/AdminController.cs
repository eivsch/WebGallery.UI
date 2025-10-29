using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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
