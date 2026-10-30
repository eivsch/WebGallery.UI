using System;

namespace WebGallery.UI.ViewModels.Paging
{
    public class PagerViewModel
    {
        public string AriaLabel { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public int CurrentOffset { get; set; }
        public int WindowSize { get; set; }
        public Func<int, string> PageUrlFactory { get; set; }

        public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)TotalItems / Math.Max(1, PageSize)));
        public int CurrentPage => Math.Min(TotalPages, (CurrentOffset / Math.Max(1, PageSize)) + 1);
        public int FirstPage
        {
            get
            {
                int safeWindowSize = Math.Max(1, WindowSize);
                int halfWindow = safeWindowSize / 2;
                int firstPage = Math.Max(1, CurrentPage - halfWindow);
                int lastPage = Math.Min(TotalPages, firstPage + safeWindowSize - 1);
                return Math.Max(1, lastPage - safeWindowSize + 1);
            }
        }
        public int LastPage => Math.Min(TotalPages, FirstPage + Math.Max(1, WindowSize) - 1);
        public int PreviousPage => Math.Max(1, CurrentPage - 1);
        public int NextPage => Math.Min(TotalPages, CurrentPage + 1);
        public bool ShowPager => TotalPages > 1;

        public string BuildPageUrl(int pageNumber)
        {
            return PageUrlFactory?.Invoke(pageNumber);
        }
    }
}