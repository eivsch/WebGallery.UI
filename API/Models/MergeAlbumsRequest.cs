namespace MinimalGallery.API.Models;

record MergeAlbumsRequest
{
    public required List<string> AlbumNamesSource { get; set; }
    public required string AlbumNameTarget { get; set; }
}