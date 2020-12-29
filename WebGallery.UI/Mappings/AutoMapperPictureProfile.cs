using AutoMapper;

namespace WebGallery.UI.Mappings
{
    public class AutoMapperPictureProfile : Profile
    {
        public AutoMapperPictureProfile()
        {
            // Domain -> Application
            CreateMap<DomainModel.Aggregates.Picture.Picture, Application.Pictures.PictureResponse>();

            // Application -> UI
            CreateMap<Application.Pictures.PictureResponse, UI.ViewModels.Bio.BioPictureViewModel>();
        }
    }
}
