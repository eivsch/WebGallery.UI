using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebGallery.UI.ViewModels;

namespace WebGallery.UI.Controllers
{
    [Route("[controller]")]
    public class PictureController : Controller
    {
        private readonly string _baseUrl = "http://192.168.1.251:5000/picture";

        [HttpGet("{id}")]
        public IActionResult Index(int id)
        {
            var vm = new PictureViewModel
            {
                Id = id,
                URL = _baseUrl + "/" + id,
                SubPics = new List<ThumbnailViewModel>
                {
                    new ThumbnailViewModel
                    {
                        Id = 5,
                        URL = "http://192.168.1.251:5000/picture/5",
                    },
                    new ThumbnailViewModel
                    {
                        Id = 6,
                        URL = "http://192.168.1.251:5000/picture/6",

                    },
                    new ThumbnailViewModel
                    {
                        Id = 7,
                        URL = "http://192.168.1.251:5000/picture/7",
                    },
                    new ThumbnailViewModel
                    {
                        Id = 8,
                        URL = "http://192.168.1.251:5000/picture/8",
                    },
                    new ThumbnailViewModel
                    {
                        Id = 9,
                        URL = "http://192.168.1.251:5000/picture/8",
                    },
                }
            };

            return View(vm);
        }
    }
}