namespace MinimalGallery.API.Models;

record NewMediaRequest
{
    public required string Name {get;set;}
    public long? Size {get;set;}
}