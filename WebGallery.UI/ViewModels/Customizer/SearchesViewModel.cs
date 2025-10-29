using System.Collections.Generic;
using Infrastructure.MinimalApi;

namespace WebGallery.UI.ViewModels.Customizer
{
    public class SearchesViewModel
    {
        public List<SavedSearchDTO> SavedSearches { get; set; } = [];
    }

    public class SaveSearchRequest
    {
        public string Albums { get; set; }
        public string Tags { get; set; }
        public string FileExtensions { get; set; }
        public string MediaNameContains { get; set; }
        public int? MaxSize { get; set; }
        public bool? AllTagsMustMatch { get; set; }
        public string SearchName { get; set; }
    }
}