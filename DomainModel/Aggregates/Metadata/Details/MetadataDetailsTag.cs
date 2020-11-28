using DomainModel.Aggregates.Metadata.Interfaces;
using DomainModel.Common;
using System;
using System.Collections.Generic;

namespace DomainModel.Aggregates.Metadata.Details
{
    public class MetadataDetailsTag : ValueObject, IMetadataDetails
    {
        private List<string> _infoItems = new List<string>();

        public virtual IReadOnlyCollection<string> InfoItems => _infoItems;

        private MetadataDetailsTag(int totalCount, IDictionary<string, object> details)
        {
            _infoItems.Add($"Total: {totalCount}");
            _infoItems.Add($"Unique: {details["totalUnique"]}");
            _infoItems.Add($"Most popular: '{details["mostPopularName"]}' ({details["mostPopularCount"]})");
            _infoItems.Add($"Most Recent: '{details["mostRecentTagName"]}' ({details["mostRecentMediaName"]})");
        }

        public static MetadataDetailsTag Create(int totalCount, IDictionary<string, object> details)
        {
            return new MetadataDetailsTag(totalCount, details);
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            throw new NotImplementedException();
        }
    }
}
