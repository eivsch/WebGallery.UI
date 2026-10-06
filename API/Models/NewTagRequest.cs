namespace MinimalGallery.API.Models;

record NewTagRequest
{
    public required string TagName {get;set;}
}