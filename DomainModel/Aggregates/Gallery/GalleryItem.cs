using DomainModel.Common;
using DomainModel.Common.Enums;
using System;
using System.Collections.Generic;

namespace DomainModel.Aggregates.Gallery
{
    public class GalleryItem : Entity
    {
        private string _appPath;
        private int? _indexGlobal;
        private List<string> _tags = new List<string>();
        private MediaType _mediaType;

        public virtual int? IndexGlobal => _indexGlobal;
        public virtual string AppPath => _appPath;
        public virtual IReadOnlyCollection<string> Tags => _tags;
        public virtual MediaType MediaType => _mediaType;

        private GalleryItem()
        { }

        internal static GalleryItem Create(string id, string appPath, int? indexGlobal, MediaType mediaType)
        {
            var item = new GalleryItem()
            {
                Id = id,
                _indexGlobal = indexGlobal,
                _mediaType = mediaType,
                _appPath = appPath
            };

            return item;
        }

        internal virtual void AddTag(string tag)
        {
            if (!_tags.Contains(tag))
                _tags.Add(tag.ToUpper());
        }
    }
}
