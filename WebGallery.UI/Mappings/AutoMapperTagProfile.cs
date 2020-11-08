using AutoMapper;

namespace WebGallery.UI.Mappings
{
    public class AutoMapperTagProfile : Profile
    {
        public AutoMapperTagProfile()
        {
            CreateMap<DomainModel.Aggregates.Tags.Tag, Application.Tags.TagResponse>();
        }
    }
}
