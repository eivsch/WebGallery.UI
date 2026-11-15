using System.ComponentModel;

namespace MinimalGallery.API.Models;

record SearchRequest
{
    public SearchRequest()
    {
        MaxResults = 128;
        AllTagsMustMatch = true;
        HitsToSkip = 0;
    }

    public string? Albums { get; set; }
    public string? Tags { get; set; }
    public string? FileExtensions { get; set; }
    public string? MediaNameContains { get; set; }
    public long? MinFileSize { get; set; }
    public long? MaxFileSize { get; set; }

    /// <summary>Exclude media with a recorded play, regardless of completion or tags.</summary>
    public bool? UnwatchedOnly { get; set; }

    /// <summary>Sample up to MaxResults uniformly across all matches after HitsToSkip.</summary>
    public bool? Random { get; set; }
    
    [DefaultValue(128)]
    public int MaxResults { get; set; }
    
    [DefaultValue(true)]
    public bool AllTagsMustMatch { get; set; }
    
    [DefaultValue(0)]
    public int? HitsToSkip { get; set; }
    public string? CreatedAfterDate { get; set; }
    public string? CreatedBeforeDate { get; set; }
}