using System.Collections.Generic;

namespace WebGallery.UI.ViewModels
{
    public class StatsInfoCardViewModel
    {
        public string Header { get; set; }
        public IReadOnlyCollection<string> InfoItems { get; set; }
    }
}
