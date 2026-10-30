namespace MinimalGallery.API.Models;

record NewUserRequest
{
    public required string Username {get;set;}
    public required string Password {get;set;}
}