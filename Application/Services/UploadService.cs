using Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Services;
using DomainModel.Aggregates.Picture;
using System.IO;
using DomainModel.Aggregates.Picture.Interfaces;
using DomainModel.Aggregates.Upload;
using Application.Uploads;
using AutoMapper;
using System.Linq;
using DomainModel.Aggregates.Metadata.Interfaces;

namespace Application.Services
{
    public class UploadService : IUploadService
    {
        private readonly IFileSystemService _fileSystemService;
        private readonly IPictureRepository _pictureRepository;
        private readonly IMapper _mapper;
        private readonly List<Picture> _uploadedPictures;
        
        private int currentGlobalMax;

        public UploadService(IFileSystemService fileSystemService, IPictureRepository pictureRepository, IMetadataRepository metadataRepository, IMapper mapper)
        {
            _fileSystemService = fileSystemService;
            _pictureRepository = pictureRepository;
            _mapper = mapper;
            _uploadedPictures = new List<Picture>();

            currentGlobalMax = metadataRepository.GetMaxGlobalIndex();
        }

        public UploadResponse GetUploadRequestResult()
        {
            if (_uploadedPictures.Count == 0)
                throw new Exception("0 files have been uploaded.");

            var aggregate = Upload.Create(_uploadedPictures.First().FolderName);

            foreach (var uploadedPicture in _uploadedPictures)
                aggregate.AddUploadedFile(uploadedPicture.Name, uploadedPicture.OriginalPath, uploadedPicture.Size, uploadedPicture.GlobalSortOrder);

            return _mapper.Map<UploadResponse>(aggregate);
        }

        public async Task UploadFile(string folderName, string fileName, int folderSortOrder, Stream file)
        {
            var savedFileInfo = await _fileSystemService.CopyFileToDisk(folderName, fileName, file);
            
            var picture = Picture.Create(
                id: "placeholder",  // Handled by the API
                name: fileName,
                appPath: Path.Combine(folderName, fileName),
                originalPath: savedFileInfo.FilePathFull,
                folderName: folderName,
                folderId: "",   // Handled by the API
                folderSortOrder: folderSortOrder,   // TODO: Should be handled better
                globalSortOrder: ++currentGlobalMax,
                size: (int) savedFileInfo.FileSize,
                created: DateTime.UtcNow);

            picture = await _pictureRepository.Save(picture);
            _uploadedPictures.Add(picture);
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