using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Infrastructure.FileServer;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace WebGallery.UI.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class FilesController : Controller
    {
        private readonly IFileServerProxy _fileSystemService;

        public FilesController(IFileServerProxy fileSystemService)
        {
            _fileSystemService = fileSystemService;
        }

        [HttpGet("image/{base64EncodedAppPath}")]
        public async Task<IActionResult> ShowImage(string base64EncodedAppPath)
        {
            var file = await _fileSystemService.DownloadImageFromFileServer(base64EncodedAppPath);
            FileContentResult result = new(file, "image/jpeg"); 

            return result;
        }

        [HttpGet("video/{base64EncodedAppPath}")]
        public async Task<IActionResult> ShowVideo(string base64EncodedAppPath)
        {
            var rangeHeader = Request.Headers.Range.ToString();
            var ifRangeHeader = Request.Headers.IfRange.ToString();
            using var response = await _fileSystemService.DownloadVideoFromFileServer(base64EncodedAppPath, rangeHeader, ifRangeHeader);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode);
            }

            Response.StatusCode = (int)response.StatusCode;
            Response.ContentType = response.Content.Headers.ContentType?.ToString() ?? "video/mp4";

            if (response.Content.Headers.ContentLength.HasValue)
                Response.ContentLength = response.Content.Headers.ContentLength.Value;

            var contentRange = response.Content.Headers.ContentRange?.ToString();
            if (string.IsNullOrWhiteSpace(contentRange) == false)
                Response.Headers["Content-Range"] = contentRange;

            var acceptRanges = response.Headers.AcceptRanges;
            if (acceptRanges != null && acceptRanges.Count > 0)
                Response.Headers["Accept-Ranges"] = string.Join(",", acceptRanges);

            var etag = response.Headers.ETag?.ToString();
            if (string.IsNullOrWhiteSpace(etag) == false)
                Response.Headers["ETag"] = etag;

            if (response.Content.Headers.LastModified.HasValue)
                Response.Headers["Last-Modified"] = response.Content.Headers.LastModified.Value.ToString("R");

            await using var stream = await response.Content.ReadAsStreamAsync();
            await stream.CopyToAsync(Response.Body, 81920, HttpContext.RequestAborted);

            return new EmptyResult();
        }
    }
}