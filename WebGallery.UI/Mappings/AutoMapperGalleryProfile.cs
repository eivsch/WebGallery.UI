using AutoMapper;

namespace WebGallery.UI.Mappings
{
    public class AutoMapperGalleryProfile : Profile
    {
        public AutoMapperGalleryProfile()
        {
            CreateMap<DomainModel.Aggregates.Gallery.Gallery, Application.Galleries.GalleryResponse>();
            CreateMap<DomainModel.Aggregates.Gallery.GalleryItem, Application.Galleries.GalleryItem>();            
            //CreateMap<DomainModel.Common.Enums.MediaType, Application.Enums.MediaType>();

            CreateMap<Application.Pictures.PictureResponse, Application.Galleries.GalleryItem>()
                .ForMember(dest => dest.IndexGlobal, opt => opt.MapFrom(src => src.GlobalSortOrder));
        }
    }
}
