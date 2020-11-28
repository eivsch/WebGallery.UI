using DomainModel.Aggregates.Metadata.Interfaces;
using DomainModel.Common;
using System;
using System.Collections.Generic;

namespace DomainModel.Aggregates.Metadata.Details
{
    public class MetadataDetailsMedia : ValueObject, IMetadataDetails
    {
        private List<string> _infoItems = new List<string>();

        public virtual IReadOnlyCollection<string> InfoItems => _infoItems;

        private MetadataDetailsMedia(int totalCount, IDictionary<string, object> details)
        {
            _infoItems.Add($"Total: {totalCount}");
            _infoItems.Add($"Most Recent: '{details["mostRecentName"]}' - {details["mostRecentTimestamp"].ToString().Substring(0, 10)}");
            _infoItems.Add($"Most Liked: '{details["mostLikedName"]}' - {details["mostLikedCount"]} likes");
        }

        public static MetadataDetailsMedia Create(int totalCount, IDictionary<string, object> details)
        {
            return new MetadataDetailsMedia(totalCount, details);
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            throw new NotImplementedException();
        }
    }
}
