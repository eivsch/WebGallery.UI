using AutoMapper;
using WebGallery.UI.ViewModels.Uploads;

namespace WebGallery.UI.Mappings
{
    public class AutoMapperUploadProfile : Profile
    {
        public AutoMapperUploadProfile()
        {
            CreateMap<DomainModel.Aggregates.Upload.Upload, Application.Uploads.UploadResponse>();
            CreateMap<DomainModel.Aggregates.Upload.UploadFile, Application.Uploads.UploadFile>();

            CreateMap<Application.Uploads.UploadResponse, UploadResultViewModel>();
        }
    }
}
