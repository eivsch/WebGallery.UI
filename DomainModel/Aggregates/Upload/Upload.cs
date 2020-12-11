using System;
using System.Collections.Generic;
using DomainModel.Common.Interfaces;

namespace DomainModel.Aggregates.Upload
{
    public class Upload : IAggregateRoot
    {
        private string _uploadAlbumName;
        private List<UploadFile> _uploadedFiles;

        public virtual string UploadAlbumName => _uploadAlbumName;
        public virtual int UploadFileCount => _uploadedFiles.Count;
        public virtual IReadOnlyCollection<UploadFile> UploadedFiles => _uploadedFiles.AsReadOnly();

        public Upload()
        {
            _uploadedFiles = new List<UploadFile>();
        }

        public static Upload Create(string uploadAlbumName)
        {
            if (string.IsNullOrWhiteSpace(uploadAlbumName))
                throw new ArgumentNullException(nameof(uploadAlbumName));

            return new Upload
            {
                _uploadAlbumName = uploadAlbumName
            };
        }

        public void AddUploadedFile(string fileName, string uploadDestinationPath, long fileSizeInBytes, int globalIndex)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));
            if (string.IsNullOrWhiteSpace(uploadDestinationPath))
                throw new ArgumentNullException(nameof(uploadDestinationPath));
            if (fileSizeInBytes < 1)
                throw new ArgumentException("A file must be bigger than 0 bytes");

            var uploadFile = UploadFile.Create(fileName, uploadDestinationPath, fileSizeInBytes, globalIndex);
            
            _uploadedFiles.Add(uploadFile);
        }
    }
}