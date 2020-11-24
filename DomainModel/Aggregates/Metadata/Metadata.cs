using DomainModel.Aggregates.Metadata.Details;
using DomainModel.Aggregates.Metadata.Interfaces;
using DomainModel.Common.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DomainModel.Aggregates.Metadata
{
    public class Metadata : IAggregateRoot
    {
        private string _shortDescription;
        private IMetadataDetails _details;

        public virtual string ShortDescription => _shortDescription;
        public virtual IMetadataDetails Details => _details;

        private Metadata()
        {

        }

        public static Metadata Create(string shortDescription, MetadataType type, Dictionary<string, string> metrics)
        {
            IMetadataDetails details;
            if (type == MetadataType.Picture)
                details = new MetadataDetailsPicture(metrics);
            else
                throw new ArgumentException($"Unknown metadata type '{type}'");

            return new Metadata
            {
                _shortDescription = shortDescription,
                _details = details
            };
        }
    }
}
