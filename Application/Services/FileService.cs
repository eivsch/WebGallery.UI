using Application.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.FileServer;
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
    public class FileService : IFileService
    {
        private readonly IFileServerProxy _fileSystemService;
        private readonly IPictureRepository _pictureRepository;
        private readonly IMapper _mapper;
        private readonly List<Picture> _uploadedPictures;
        
        public FileService(IFileServerProxy fileSystemService, IPictureRepository pictureRepository, IMetadataRepository metadataRepository, IMapper mapper)
        {
            _fileSystemService = fileSystemService;
            _pictureRepository = pictureRepository;
            _mapper = mapper;
            _uploadedPictures = new List<Picture>();
        }

        public async Task UploadFile(string folderName, string fileName, Stream file)
        {
            var savedFileInfo = await _fileSystemService.UploadFileToFileServer(folderName, fileName, file);
            
            var picture = Picture.Create(
                name: savedFileInfo.FileName,
                appPath: Path.Combine(folderName, savedFileInfo.FileName),
                originalPath: savedFileInfo.FilePathFull,
                folderName: folderName,
                size: (int) savedFileInfo.FileSize,
                created: DateTime.UtcNow);

            picture = await _pictureRepository.Save(picture);
            _uploadedPictures.Add(picture);
        }

        //public async Task<byte[]> DownloadFile(string identifier, MediaType mediaType)
        //{
        //    switch (mediaType){
        //        case MediaType.Gif:
        //        case MediaType.Image:
        //            return await _fileSystemService.DownloadImageFromFileServer(identifier);
        //        case MediaType.Video:
        //            return await _fileSystemService.DownloadVideoFromFileServer(identifier);
        //        default:
        //            return await _fileSystemService.DownloadImageFromFileServer(identifier);
        //    }
        //}

        public UploadResponse GetUploadRequestResult()
        {
            if (_uploadedPictures.Count == 0)
                throw new Exception("0 files have been uploaded.");

            var aggregate = Upload.Create(_uploadedPictures.First().FolderName);

            foreach (var uploadedPicture in _uploadedPictures)
                aggregate.AddUploadedFile(uploadedPicture.Name, uploadedPicture.OriginalPath, uploadedPicture.Size);

            return _mapper.Map<UploadResponse>(aggregate);
        }
    }
}
