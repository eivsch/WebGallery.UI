namespace MinimalGallery.API.Models;

record SearchHit
{
    public int MediaAlbumIndex {get;set;}
    public required string AlbumName {get;set;}
    public required Media MediaItem {get;set;}
}