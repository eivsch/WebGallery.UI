namespace MinimalGallery.API.Models;

record Media
{
    public required string Id {get;set;}
    public required string Name {get;set;}
    public required DateTimeOffset Created {get;set;}
    public long? Size {get;set;}
    public List<Tag> Tags {get;set;} = [];
    public int Likes {get;set;}
    public int? Index {get;set;}
}