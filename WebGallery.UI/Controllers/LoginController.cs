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
using System.Security.Cryptography;
using System.Text;
using Infrastructure.MinimalApi;

namespace WebGallery.UI.Controllers
{
    public class LoginController : Controller
    {
        private LoginManager _loginManager;
        private MinimalApiProxy _apiProxy;

        public LoginController(IHttpContextAccessor httpContextAccessor, MinimalApiProxy apiProxy)
        {
            _loginManager = new LoginManager(httpContextAccessor);
            _apiProxy = apiProxy;
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
                CredentialsDTO creds = await _apiProxy.GetCredentials(vm.Username);
                string hashedPw = GetHash(vm.Password);
                if (vm.Username == creds.Username 
                    && hashedPw == creds.Password)
                {
                    await _loginManager.LoginAsync(vm.Username);
            
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("invalidCredentials", "Invalid credentials.");
                }
            }

            return View("Index", vm);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("/register")]
        public IActionResult Register()
        {
            return View("Register");
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/register")]
        public async Task<IActionResult> Register(RegisterUserViewModel vm)
        {
            if (ModelState.IsValid)
            {
                string hashedPw = GetHash(vm.Password);
                await _apiProxy.PostUser(vm.Username, hashedPw);

                return View("Register_Success", vm);
            }
            else
            {
                ModelState.AddModelError("invalidCredentials", "Invalid registration info.");
                
                return View("Register", vm);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            await _loginManager.LogoutAsync();

            return RedirectToAction("Index");
        }

        // TODO: add salt
        static string GetHash(string input)
        {
            byte[] data = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                string hexStr = data[i].ToString("x2");
                sBuilder.Append(hexStr);
            }

            return sBuilder.ToString();
        }
    }
}
