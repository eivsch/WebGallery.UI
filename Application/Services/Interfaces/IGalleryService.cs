using Application.Galleries;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    public interface IGalleryService
    {
        Task<IEnumerable<GalleryResponse>> GetAll();
        Task<GalleryResponse> GetRandom(int numberOfPics);
    }
}
