using System;
using System.Collections.Generic;

namespace Application.Metadata
{
    public class MetadataResponse
    {
        public string ShortDescription { get; set; }
        public IReadOnlyCollection<string> InfoItems { get; set; }
    }
}
