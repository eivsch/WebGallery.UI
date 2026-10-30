namespace WebGallery.UI.Configuration
{
    public class DisplayOptions
    {
        public int PageSize { get; set; } = 48;
        public int PaginationWindowSize { get; set; } = 7;
        public int SingleSearchMinQueryLength { get; set; } = 5;
        public int SingleSearchMaxResultsCap { get; set; } = 2000;
        public int SingleSearchScanBatchSize { get; set; } = 200;
        public int SingleSearchMaxHitsToScan { get; set; } = 20000;
    }
}