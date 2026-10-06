using MinimalGallery.API.Models;

record GetAlbumsResponse
{
    public required List<UserAlbumMeta> Albums { get; set; }
    public required int TotalCount { get; set; }
    public required int From { get; set; }
    public required int CurrentSize { get; set; }
}