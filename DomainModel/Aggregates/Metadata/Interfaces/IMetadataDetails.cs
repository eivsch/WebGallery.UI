using System.Collections.Generic;

namespace DomainModel.Aggregates.Metadata.Interfaces
{
    public interface IMetadataDetails
    {
        IReadOnlyCollection<string> InfoItems { get; }
    }
}
