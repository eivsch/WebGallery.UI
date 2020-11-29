using DomainModel.Common;
using System;

namespace DomainModel.Aggregates.Tags
{
    public class TagMediaItem : Entity
    {
        private DateTime _created;
        private int? _globalIndex;

        public virtual DateTime Created => _created;
        public virtual int? GlobalIndex => _globalIndex;

        private TagMediaItem(string id)
        {
            Id = id;
        }

        internal static TagMediaItem Create(string itemId, DateTime? created, int? globalIndex)
        {
            if (string.IsNullOrWhiteSpace(itemId) && !globalIndex.HasValue)
                throw new ArgumentException($"Either a {nameof(itemId)} or {nameof(globalIndex)} is required to create a Tag");

            return new TagMediaItem(itemId)
            {
                _created = created ?? DateTime.UtcNow,
                _globalIndex = globalIndex
            };
        }
    }
}
