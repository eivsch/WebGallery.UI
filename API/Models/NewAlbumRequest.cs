namespace MinimalGallery.API.Models;

record NewAlbumRequest
{
    public required string AlbumName { get; set; }
}