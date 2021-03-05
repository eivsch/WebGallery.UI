using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WebGallery.UI.Authentication;
using WebGallery.UI.ViewModels.Login;

namespace WebGallery.UI.Controllers
{
    public class LoginController : Controller
    {
        private LoginManager _loginManager;
        private IConfiguration _configuration;

        public LoginController(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _loginManager = new LoginManager(httpContextAccessor);
            _configuration = configuration;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginFormViewModel vm)
        {
            if (ModelState.IsValid)
            {
                if (vm.Username == _configuration.GetValue("Auth:Username", "") 
                    && vm.Password == _configuration.GetValue("Auth:Password", ""))
                {
                    await _loginManager.LoginAsync("1");
            
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("invalidCredentials", "Invalid credentials.");
                }
            }

            return View("Index", vm);
        }

        [Authorize]
        [HttpGet]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            await _loginManager.LogoutAsync();

            return RedirectToAction("Index");
        }
    }
}
