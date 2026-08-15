using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Common;
using Infrastructure.MinimalApi;
using Infrastructure.FileServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using WebGallery.UI.Attributes;
using WebGallery.UI.Helpers;
using WebGallery.UI.ViewModels.Uploads;

namespace WebGallery.UI.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class UploadsController : Controller
    {
        private const long MaxFileSize = 20L * 1024L * 1024L * 1024L; // 20GB, adjust to your need
        
        private static readonly FormOptions _defaultFormOptions = new FormOptions
            {
                MultipartBodyLengthLimit = MaxFileSize,
            };
        
        private readonly IFileServerProxy _fileSystemService;
        private readonly MinimalApiProxy _minimalApiProxy;
        private readonly ILogger<UploadsController> _logger;
        private readonly string _username;

        public UploadsController(IFileServerProxy fileSystemService, MinimalApiProxy minimalApiProxy, UsernameResolver usernameResolver, ILogger<UploadsController> logger)
        {
            _fileSystemService = fileSystemService;
            _minimalApiProxy = minimalApiProxy;
            _logger = logger;
            _username = usernameResolver.Username;
        }

        public async Task<IActionResult> Index([FromQuery] string albumName)
        {
            var vm = new UploadsViewModel { AlbumName = albumName };

            return View(vm);
        }

        [DisableFormValueModelBinding]
        [RequestSizeLimit(MaxFileSize)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSize)]
        [HttpPost("large")]
        public async Task<IActionResult> ReceiveFile()
        {
            try
            {
                if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
                    throw new Exception("Not a multipart request");

                var boundary = MultipartRequestHelper.GetBoundary(
                    MediaTypeHeaderValue.Parse(Request.ContentType), 
                    _defaultFormOptions.MultipartBoundaryLengthLimit
                );

                List<SavedFileInfo> uploadedFiles = [];
                string albumName = "";
                List<AlbumMetaDTO> albums = await _minimalApiProxy.GetAllAlbums(_username);

                var reader = new MultipartReader(boundary, Request.Body);
                var section = await reader.ReadNextSectionAsync(HttpContext.RequestAborted);
                while (section != null)
                {
                    var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(
                        section.ContentDisposition, 
                        out var contentDisposition
                    );

                    if (hasContentDispositionHeader)
                    {
                        if (string.IsNullOrWhiteSpace(albumName) && MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                        {
                            using (var streamReader = new StreamReader(
                                section.Body,
                                Encoding.UTF8,
                                detectEncodingFromByteOrderMarks: true,
                                bufferSize: 1024,
                                leaveOpen: false))
                            {
                                var value = await streamReader.ReadToEndAsync(HttpContext.RequestAborted);
                                albumName = value;
                                if (!albums.Any(a => a.AlbumName == albumName)) await _minimalApiProxy.CreateAlbum(_username, albumName);
                            }
                        }
                        else if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                        {
                            var fileName = contentDisposition.FileNameStar.ToString();
                            if (string.IsNullOrEmpty(fileName))
                            {
                                fileName = contentDisposition.FileName.ToString();
                            }

                            if (string.IsNullOrEmpty(fileName))
                                throw new Exception("No filename defined.");

                            using (var fileStream = section.Body)
                            {
                                SavedFileInfo savedFileInfo = await _fileSystemService.UploadFileToFileServer(albumName, fileName, fileStream);
                                uploadedFiles.Add(savedFileInfo);
                                await _minimalApiProxy.PostMediaItem(_username, albumName, savedFileInfo);
                            }
                        }
                    }

                    section = await reader.ReadNextSectionAsync(HttpContext.RequestAborted);
                }

                var vm = CreateUploadResult(uploadedFiles, albumName);

                return View("success", vm);
            }
            catch (OperationCanceledException ex) when (HttpContext.RequestAborted.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Upload request was canceled by the client. User: {Username}", _username);
                return BadRequest("Upload was canceled before completion.");
            }
            catch (IOException ex)
            {
                _logger.LogWarning(ex, "Upload request stream terminated unexpectedly. User: {Username}", _username);
                return BadRequest("Upload stream ended unexpectedly. Please try again, preferably with smaller batches.");
            }
        }

        private UploadResultViewModel CreateUploadResult(List<SavedFileInfo> savedFiles, string albumName)
        {
            return new()
            {
                UploadAlbumName = albumName,
                UploadedFiles = savedFiles,
                UploadFileCount = savedFiles.Count
            };
        }
    }
}
