using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebGallery.UI.Controllers
{
    public class VideoClipsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}