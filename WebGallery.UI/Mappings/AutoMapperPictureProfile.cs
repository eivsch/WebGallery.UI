using AutoMapper;

namespace WebGallery.UI.Mappings
{
    public class AutoMapperPictureProfile : Profile
    {
        public AutoMapperPictureProfile()
        {
            CreateMap<DomainModel.Aggregates.Picture.Picture, Application.Pictures.PictureResponse>();
        }
    }
}
