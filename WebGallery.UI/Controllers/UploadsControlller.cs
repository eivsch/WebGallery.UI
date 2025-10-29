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
        private const long MaxFileSize = 10L * 1024L * 1024L * 1024L; // 10GB, adjust to your need
        
        private static readonly FormOptions _defaultFormOptions = new FormOptions
            {
                MultipartBodyLengthLimit = MaxFileSize,
            };
        
        private readonly IFileServerProxy _fileSystemService;
        private readonly MinimalApiProxy _minimalApiProxy;
        private readonly string _username;

        public UploadsController(IFileServerProxy fileSystemService, MinimalApiProxy minimalApiProxy, UsernameResolver usernameResolver)
        {
            _fileSystemService = fileSystemService;
            _minimalApiProxy = minimalApiProxy;
            _username = usernameResolver.Username;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new UploadsViewModel();

            return View(vm);
        }

        [DisableFormValueModelBinding]
        [RequestSizeLimit(MaxFileSize)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSize)]
        [HttpPost("large")]
        public async Task<IActionResult> ReceiveFile()
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
                throw new Exception("Not a multipart request");

            var boundary = MultipartRequestHelper.GetBoundary(
                MediaTypeHeaderValue.Parse(Request.ContentType), 
                _defaultFormOptions.MultipartBoundaryLengthLimit
            );

            List<SavedFileInfo> uploadedFiles = [];
            string albumName = "";
            List<AlbumMetaDTO> albums = await _minimalApiProxy.GetAlbums(_username);

            var reader = new MultipartReader(boundary, Request.Body);
            var section = await reader.ReadNextSectionAsync();
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
                            var value = await streamReader.ReadToEndAsync();
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

                section = await reader.ReadNextSectionAsync();
            }

            var vm = CreateUploadResult(uploadedFiles, albumName);

            return View("success", vm);
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
