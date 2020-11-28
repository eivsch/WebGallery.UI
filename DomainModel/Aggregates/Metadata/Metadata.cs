using DomainModel.Aggregates.Metadata.Details;
using DomainModel.Aggregates.Metadata.Interfaces;
using DomainModel.Common.Interfaces;
using System;
using System.Collections.Generic;

namespace DomainModel.Aggregates.Metadata
{
    public class Metadata : IAggregateRoot
    {
        private string _shortDescription;
        private IMetadataDetails _metadataDetails;

        public virtual string ShortDescription => _shortDescription;
        public virtual IMetadataDetails MetadataDetails => _metadataDetails;

        private Metadata()
        {

        }

        public static Metadata Create(string shortDescription, MetadataType type, int totalCount, IDictionary<string, object> details)
        {
            IMetadataDetails metaDetails;
            switch (type)
            {
                case MetadataType.Album:
                case MetadataType.Gif:
                case MetadataType.Picture:
                case MetadataType.Video:
                    metaDetails = MetadataDetailsMedia.Create(totalCount, details);
                    break;
                case MetadataType.Tag:
                    metaDetails = MetadataDetailsTag.Create(totalCount, details);
                    break;
                default:
                    throw new ArgumentException();
            }

            return new Metadata
            {
                _shortDescription = shortDescription,
                _metadataDetails = metaDetails,
            };
        }
    }
}
