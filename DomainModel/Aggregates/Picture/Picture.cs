using DomainModel.Common;
using DomainModel.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomainModel.Aggregates.Picture
{
    public class Picture : Entity, IAggregateRoot
    {
        private string _urlPath;

        public virtual string UrlPath => _urlPath;

        private Picture(string id)
        {
            Id = id;
        }

        public static Picture Create(string id, string urlPath)
        {
            return new Picture(id) { _urlPath = urlPath };
        }
    }
}
