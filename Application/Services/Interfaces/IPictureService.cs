﻿using Application.Pictures;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    public interface IPictureService
    {
        Task<IEnumerable<PictureResponse>> GetPictures(string galleryId);
    }
}