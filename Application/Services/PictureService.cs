using Application.Pictures;
using Application.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PictureService : IPictureService
    {
        public Task<IEnumerable<PictureResponse>> GetPictures(string galleryId)
        {
            throw new NotImplementedException();
        }
    }
}
