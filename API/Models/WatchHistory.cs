namespace MinimalGallery.API.Models;

record WatchHistoryItem
{
    public required string Album { get; set; }
    public required string MediaName { get; set; }
    public string? MediaId { get; set; }
    public required DateTime LastPlayedUtc { get; set; }
}

record RecordPlayRequest
{
    public required string Album { get; set; }
    public required string MediaName { get; set; }
    public string? MediaId { get; set; }
}
