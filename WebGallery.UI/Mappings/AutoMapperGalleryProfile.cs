using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebGallery.UI.Mappings
{
    public class AutoMapperGalleryProfile : Profile
    {
        public AutoMapperGalleryProfile()
        {
            CreateMap<DomainModel.Aggregates.Gallery.Gallery, Application.Galleries.GalleryResponse>();
        }
    }
}
