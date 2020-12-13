using DomainModel.Common;
using DomainModel.Common.Enums;
using DomainModel.Common.Interfaces;
using System;
using System.Collections.Generic;

namespace DomainModel.Aggregates.Gallery
{
    public class Gallery : Entity, IAggregateRoot
    {
        private int _imageCount;
        private readonly List<GalleryItem> _galleryItems = new List<GalleryItem>();
        private string _galleryName;

        public virtual int ImageCount => _imageCount;
        public virtual IReadOnlyCollection<GalleryItem> GalleryItems => _galleryItems.AsReadOnly();
        public virtual string GalleryName => _galleryName;

        private Gallery(string id)
        {
            Id = id;
        }

        public static Gallery Create(string id, int imageCount, string galleryName = null)
        {
            return new Gallery(id)
            {
                _imageCount = imageCount,
                _galleryName = galleryName
            };
        }

        public virtual void AddGalleryItem(string galleryItemId, string mediaType, string appPath, string tags = "", int? indexGlobal = null)
        {
            if (string.IsNullOrWhiteSpace(galleryItemId))
                throw new ArgumentNullException(nameof(galleryItemId));
            if (string.IsNullOrWhiteSpace(appPath))
                throw new ArgumentNullException(nameof(appPath));

            var galleryItem = GalleryItem.Create(galleryItemId, appPath, indexGlobal, ParseMediaType(mediaType));

            if (!string.IsNullOrWhiteSpace(tags))
            {
                foreach (var tag in tags.Split(','))
                {
                    galleryItem.AddTag(tag);
                }
            }

            _galleryItems.Add(galleryItem);
        }

        private MediaType ParseMediaType(string mediaType)
        {
            switch (mediaType.Trim().ToLower())
            {
                case "image":
                    return MediaType.Image;
                case "video":
                    return MediaType.Video;
                case "gif":
                    return MediaType.Gif;
                default:
                    return MediaType.Image;
            }
        }
    }
}
