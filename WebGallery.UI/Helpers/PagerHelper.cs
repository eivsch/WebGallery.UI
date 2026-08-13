using System;

using WebGallery.UI.ViewModels.Paging;

namespace WebGallery.UI.Helpers
{
    public static class PagerHelper
    {
        public static PagerViewModel Create(int totalItems, int currentOffset, int pageSize, int windowSize, Func<int, string> pageUrlFactory, string ariaLabel)
        {
            return new PagerViewModel
            {
                TotalItems = totalItems,
                CurrentOffset = Math.Max(0, currentOffset),
                PageSize = Math.Max(1, pageSize),
                WindowSize = Math.Max(1, windowSize),
                PageUrlFactory = pageUrlFactory,
                AriaLabel = ariaLabel,
            };
        }
    }
}