using DomainModel.Aggregates.Metadata.Interfaces;
using DomainModel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomainModel.Aggregates.Metadata.Details
{
    public class MetadataDetailsPicture : ValueObject, IMetadataDetails
    {
        private List<string> _infoItems = new List<string>();

        public virtual IReadOnlyCollection<string> InfoItems => _infoItems;

        public MetadataDetailsPicture(IReadOnlyDictionary<string, string> metrics)
        {
            _infoItems.Add($"Total: {metrics["count"]}");
            _infoItems.Add($"Most Recent: '{metrics["mostRecentName"]}' - {metrics["mostRecentTs"]}");
            _infoItems.Add($"Most Liked: '{metrics["mostLikedName"]}' - {metrics["mostLikedCount"]}");
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            throw new NotImplementedException();
        }
    }
}
