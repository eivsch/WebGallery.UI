using System.Collections.Generic;
using Infrastructure.MinimalApi;

namespace WebGallery.UI.ViewModels.Customizer
{
    public class SearchesViewModel
    {
        public List<SavedSearchDTO> SavedSearches { get; set; } = [];
        public IEnumerable<TagStatsViewModel> AllTags { get; set; } = [];
        public IEnumerable<AlbumStatsViewModel> AllAlbums { get; set; } = [];
        public int TotalItems { get; set; }
    }

    public class TagStatsViewModel
    {
        public string TagName { get; set; }
        public int Count { get; set; }
    }

    public class AlbumStatsViewModel
    {
        public string AlbumName { get; set; }
        public int Count { get; set; }
    }

    public class SaveSearchRequest
    {
        public string Albums { get; set; }
        public string Tags { get; set; }
        public string FileExtensions { get; set; }
        public string MediaNameContains { get; set; }
        public int? MaxSize { get; set; }
        public long? MinFileSize { get; set; }
        public long? MaxFileSize { get; set; }
        public bool? AllTagsMustMatch { get; set; }
        public string SearchName { get; set; }
        public string CreatedAfter { get; set; }
        public string CreatedBefore { get; set; }
    }
}