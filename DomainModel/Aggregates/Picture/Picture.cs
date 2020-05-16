using DomainModel.Common;
using DomainModel.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomainModel.Aggregates.Picture
{
    public class Picture : Entity, IAggregateRoot
    {
        private string _appPath;
        private string _originalPath;
        private string _name;
        private string _folderName;
        private string _folderId;
        private int _globalSortOrder;
        private int _folderSortOrder;
        private int _size;
        private DateTime _createTimestamp;
        private List<string> _tags = new List<string>();

        public virtual string Name => _name;
        public virtual string AppPath => _appPath;
        public virtual string OriginalPath => _originalPath;
        public virtual string FolderName => _folderName;
        public virtual string FolderId => _folderId;
        public virtual int GlobalSortOrder => _globalSortOrder;
        public virtual int FolderSortOrder => _folderSortOrder;
        public virtual int Size => _size;
        public virtual DateTime CreateTimestamp => _createTimestamp;
        public virtual IReadOnlyCollection<string> Tags => _tags;

        private Picture(string id)
        {
            Id = id;
        }

        public static Picture Create(
            string id,
            string name,
            string appPath,
            string originalPath,
            string folderName,
            string folderId,
            int folderSortOrder,
            int globalSortOrder,
            int size,
            DateTime created
        )
        {
            if (string.IsNullOrWhiteSpace(id) && globalSortOrder < 1)
                throw new ArgumentException("Either a valid id or global index must be provided");

            return new Picture(id) 
            {
                _name = name,
                _appPath = appPath,
                _originalPath = originalPath,
                _folderName = folderName,
                _folderId = folderId,
                _folderSortOrder = folderSortOrder,
                _globalSortOrder = globalSortOrder,
                _size = size,
                _createTimestamp = created
            };
        }

        public void AddTag(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                throw new ArgumentNullException(nameof(tagName));

            if (!_tags.Contains(tagName))
                _tags.Add(tagName);
        }
    }
}
