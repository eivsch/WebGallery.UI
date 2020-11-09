using DomainModel.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DomainModel.Aggregates.Tags
{
    public class Tag : IAggregateRoot
    {
        private string _tagName;
        private int? _itemCount;
        private List<TagMediaItem> _mediaItems = new List<TagMediaItem>();

        public virtual string TagName => _tagName;
        public virtual int ItemCount => _itemCount ?? _mediaItems.Count;
        public virtual IReadOnlyCollection<TagMediaItem> MediaItems => _mediaItems.AsReadOnly();

        public static Tag Create(string tagName, int? itemCount = null)
        {
            return new Tag
            {
                _tagName = tagName.Replace("#", ""),
                _itemCount = itemCount,
            };
        }

        public virtual void AddMediaItem(string itemId, DateTime? created = null, int? globalIndex = null)
        {
            TagMediaItem taggedItem = _mediaItems.FirstOrDefault(i => i.Id == itemId);
            if (taggedItem is null)
            {
                taggedItem = TagMediaItem.Create(itemId, created, globalIndex);

                _mediaItems.Add(taggedItem);
            }
        }
    }
}
