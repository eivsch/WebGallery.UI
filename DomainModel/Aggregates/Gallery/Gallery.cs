using DomainModel.Common;
using DomainModel.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomainModel.Aggregates.Gallery
{
    public class Gallery : Entity, IAggregateRoot
    {
        private int _imageCount;
        private readonly List<GalleryItem> _galleryItems = new List<GalleryItem>();

        public virtual int ImageCount => _imageCount;
        public virtual IReadOnlyCollection<GalleryItem> GalleryItems => _galleryItems.AsReadOnly();

        private Gallery(string id)
        {
            Id = id;
        }

        public static Gallery Create(string id, int imageCount)
        {
            return new Gallery(id)
            {
                _imageCount = imageCount
            };
        }

        public virtual void AddGalleryItem(string galleryItemId, int index, string tags = "")
        {
            var galleryItem = GalleryItem.Create(galleryItemId, index);

            if (!string.IsNullOrWhiteSpace(tags))
            {
                foreach (var tag in tags.Split(','))
                {
                    galleryItem.AddTag(tag);
                }
            }

            _galleryItems.Add(galleryItem);
        }
    }
}
