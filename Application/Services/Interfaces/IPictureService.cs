using Application.Pictures;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    public interface IPictureService
    {
        Task<PictureResponse> Get(string id);
        Task<PictureResponse> Get(int index);
        Task<PictureResponse> Get(string galleryId, int index);
    }
}
