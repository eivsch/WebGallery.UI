using Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Services;

namespace Application.Services
{
    public class UploadService : IUploadService
    {
        private readonly IFileSystemService _fileSystemService;

        public UploadService(IFileSystemService fileSystemService)
        {
            _fileSystemService = fileSystemService;
        }

        public async Task UploadFiles(string folderName, IEnumerable<IFormFile> folderFiles)
        {
            // TODO: File validation. Check for disallowed file types.
            await _fileSystemService.CopyFilesToDisk(folderName, folderFiles);
        }
    }
}
