using AutoMapper;

namespace WebGallery.UI.Mappings
{
    public class AutoMapperGalleryProfile : Profile
    {
        public AutoMapperGalleryProfile()
        {
            CreateMap<DomainModel.Aggregates.Gallery.Gallery, Application.Galleries.GalleryResponse>();
            CreateMap<DomainModel.Aggregates.Gallery.GalleryItem, Application.Galleries.GalleryItem>();            
            CreateMap<DomainModel.Common.Enums.MediaType, Application.Enums.MediaType>();
        }
    }
}
