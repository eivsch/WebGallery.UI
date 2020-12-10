using Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Services;
using DomainModel.Aggregates.Picture;
using System.IO;
using DomainModel.Aggregates.Picture.Interfaces;

namespace Application.Services
{
    public class UploadService : IUploadService
    {
        private readonly IFileSystemService _fileSystemService;
        private readonly IPictureRepository _pictureRepository;

        public UploadService(IFileSystemService fileSystemService, IPictureRepository pictureRepository)
        {
            _fileSystemService = fileSystemService;
            _pictureRepository = pictureRepository;
        }

        public async Task UploadFiles(string folderName, IEnumerable<IFormFile> folderFiles)
        {
            int folderSortOrder = 1;
            foreach (var file in folderFiles)
            {
                var picture = Picture.Create(
                    id: "placeholder",  // Handled by the API
                    name: file.FileName,
                    appPath: Path.Combine(folderName, file.FileName),
                    originalPath: $"/uploads/{file.FileName}",
                    folderName: folderName,
                    folderId: "",   // Handled by the API
                    folderSortOrder: folderSortOrder++,
                    globalSortOrder: 0, // Handled by the API
                    size: (int) file.Length,
                    created: DateTime.UtcNow);

                await _fileSystemService.CopyFileToDisk(folderName, file);
                await _pictureRepository.Save(picture);
            }
        }
    }
}
