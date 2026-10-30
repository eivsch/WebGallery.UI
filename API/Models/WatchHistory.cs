namespace MinimalGallery.API.Models;

record WatchHistoryItem
{
    public required string Album { get; set; }
    public required string MediaName { get; set; }
    public string? MediaId { get; set; }
    public required DateTime LastPlayedUtc { get; set; }
    public int PlayCount { get; set; }
    public double PositionSeconds { get; set; }
    public double? DurationSeconds { get; set; }
    public bool Watched { get; set; }
}

record SavePositionRequest
{
    public required string Album { get; set; }
    public required string MediaName { get; set; }
    public required double PositionSeconds { get; set; }
    public double? DurationSeconds { get; set; }
}

record RecordPlayRequest
{
    public required string Album { get; set; }
    public required string MediaName { get; set; }
    public string? MediaId { get; set; }
}
