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

        public virtual int ImageCount => _imageCount;

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
    }
}
