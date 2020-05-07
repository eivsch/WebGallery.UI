using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using WebGallery.UI.ViewModels.Single;

namespace WebGallery.UI.Controllers
{
    [Route("[controller]")]
    public class SingleController : Controller
    {
        private readonly IPictureService _pictureService;

        private readonly string _baseUrl = "http://localhost:5000/pictures";

        public SingleController(IPictureService pictureService)
        {
            _pictureService = pictureService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Index(string id, int offset)
        {
            var pics = await _pictureService.GetPictures(id, offset);

            var list = new List<SingleGalleryImageViewModel>();
            foreach(var pic in pics.OrderBy(i => i.FolderSortOrder))
            {
                list.Add(new SingleGalleryImageViewModel 
                { 
                    Id = pic.Id, 
                    Url = $"{_baseUrl}/{id}/{pic.FolderSortOrder}"
                });
            }

            var vm = new SingleGalleryViewModel
            {
                Id = id,
                Images = list,
            };

            return View(vm);
        }
    }
}