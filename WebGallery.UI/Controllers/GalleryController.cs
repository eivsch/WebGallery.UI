using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebGallery.UI.ViewModels;

namespace WebGallery.UI.Controllers
{
    public class GalleryController : Controller
    {
        private readonly string _baseUrl = "http://192.168.1.251:5000/picture";

        public IActionResult Index()
        {
            var vm = new GalleryViewModel
            {
                Columns = new List<GalleryColumnViewModel> 
                {
                    new GalleryColumnViewModel{ Pics = new List<string>() },
                    new GalleryColumnViewModel{ Pics = new List<string>() },
                    new GalleryColumnViewModel{ Pics = new List<string>() },
                    new GalleryColumnViewModel{ Pics = new List<string>() },
                }
            };

            int counter = 1;
            foreach (var column in vm.Columns)
            {
                for (int i = 0; i<4; i++)
                    column.Pics.Add(_baseUrl + "/" + counter++);
            }

            return View(vm);
        }
    }
}