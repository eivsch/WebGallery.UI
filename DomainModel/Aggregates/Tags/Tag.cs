using DomainModel.Common.Interfaces;
using System;

namespace DomainModel.Aggregates.Tags
{
    public class Tag : IAggregateRoot
    {
        private string _pictureId;
        private string _tagName;
        private int? _pictureIndex;

        public virtual string PictureId => _pictureId;
        public virtual string TagName => _tagName;
        public virtual int? PictureIndex=> _pictureIndex;

        public static Tag  Create(string tagName, string pictureId, int? pictureIndex = null)
        {
            if (string.IsNullOrWhiteSpace(pictureId) && !pictureIndex.HasValue)
                throw new ArgumentException($"Either a {nameof(pictureId)} or {nameof(pictureIndex)} is required to create a Tag");

            return new Tag
            {
                _tagName = tagName.Replace("#", ""),
                _pictureId = pictureId,
                _pictureIndex = pictureIndex
            };
        }
    }
}
