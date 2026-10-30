namespace MinimalGallery.API.Models;

record UserCredentials
{
    public required string Username {get;set;}
    public required string Password {get;set;}
}