using System.Collections.Generic;
using DomainModel.Common;

namespace DomainModel.Aggregates.Upload
{
    public class UploadFile : ValueObject
    {
        private string _fileName;
        private string _uploadDestinationPath;
        private long _fileSize;
        private int _globalIndex;

        public virtual string FileName => _fileName;
        public virtual string UploadDestinationPath => _uploadDestinationPath;
        public virtual long FileSizeInBytes => _fileSize;
        public virtual int GlobalIndex => _globalIndex;

        public UploadFile()
        {
            
        }

        public static UploadFile Create(string fileName, string uploadDestinationPath, long fileSizeInBytes, int globalIndex)
        {
            return new UploadFile
            {
                _fileName = fileName,
                _uploadDestinationPath = uploadDestinationPath,
                _fileSize = fileSizeInBytes,
                _globalIndex = globalIndex
            };
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return FileName;
            yield return UploadDestinationPath;
            yield return UploadDestinationPath;
            yield return GlobalIndex;
        }
    }
}