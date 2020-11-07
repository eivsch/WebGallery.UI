using DomainModel.Common;
using DomainModel.Common.Enums;
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

        public virtual void AddGalleryItem(string galleryItemId, int indexGlobal, string mediaType, string tags = "")
        {
            var galleryItem = GalleryItem.Create(galleryItemId, indexGlobal, ParseMediaType(mediaType));

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
