using DomainModel.Common;
using DomainModel.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomainModel.Aggregates.Gallery
{
    public class GalleryItem : Entity
    {
        private int _index;
        private List<string> _tags = new List<string>();
        private MediaType _mediaType;

        public virtual int Index => _index;
        public virtual IReadOnlyCollection<string> Tags => _tags;
        public virtual MediaType MediaType => _mediaType;

        private GalleryItem(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                Id = Guid.NewGuid().ToString();
            else
                Id = id;
        }

        internal static GalleryItem Create(string id, int index, MediaType mediaType)
        {
            var item = new GalleryItem(id)
            {
                _index = index,
                _mediaType = mediaType,
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
