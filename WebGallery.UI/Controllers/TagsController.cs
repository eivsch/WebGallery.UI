using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Infrastructure.MinimalApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebGallery.UI.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class TagsController : Controller
    {
        private readonly MinimalApiProxy _minimalApiProxy;
        readonly string _username;

        public TagsController(MinimalApiProxy minimalApiProxy, IHttpContextAccessor httpContext)
        {
            _minimalApiProxy = minimalApiProxy;
            Claim claim = httpContext.HttpContext.User.Claims.FirstOrDefault(f => f.Type == ClaimTypes.Sid);
            _username = claim.Value;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Current = "Tags";

            return View();
        }

        [HttpGet("{tag}")]
        public async Task<IActionResult> Get(string tag)
        {
            return Redirect($"/Single/Custom?nbr=48&tags={tag}&tagFilterMode=custominclusive&mediaFilterMode=include");
        }
    }
}
