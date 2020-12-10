using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Services.Interfaces;
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
    [Route("[controller]")]
    public class UploadsController : Controller
    {
        private const long MaxFileSize = 10L * 1024L * 1024L * 1024L; // 10GB, adjust to your need
        
        private static readonly FormOptions _defaultFormOptions = new FormOptions
            {
                MultipartBodyLengthLimit = MaxFileSize,
            };
        
        private readonly IUploadService _uploadService;

        public UploadsController(IUploadService uploadService)
        {
            _uploadService = uploadService;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new UploadsViewModel();

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Post(UploadsViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                // TODO
                throw new Exception("Invalid ModelState");
            }

            await _uploadService.UploadFiles(vm.AlbumName, vm.FilesToUpload);
            
            return View("success");
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

            string albumName = "";
            int fileCounter = 0;

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
                    if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
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
                        }
                    }
                    else if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                    {
                        fileCounter++;
                        var fileName = contentDisposition.FileNameStar.ToString();
                        if (string.IsNullOrEmpty(fileName))
                        {
                            fileName = contentDisposition.FileName.ToString();
                        }

                        if (string.IsNullOrEmpty(fileName))
                            throw new Exception("No filename defined.");

                        using (var fileStream = section.Body)
                        {
                            await _uploadService.UploadFile(albumName, fileName, fileCounter, fileStream);
                        }
                    }
                }

                section = await reader.ReadNextSectionAsync();
            }

            return View("success");
        }
    }
}
