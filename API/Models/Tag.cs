namespace MinimalGallery.API.Models;

record Tag
{
    public required string TagName {get;set;}
    public required DateTimeOffset Created {get;set;}
}