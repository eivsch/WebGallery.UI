using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace WebGallery.UI.Authentication
{
    public class LoginManager
    {
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public LoginManager(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected void SetAuthenticationToken()
        {
            Guid authToken = Guid.NewGuid();
            _httpContextAccessor.HttpContext.Session.Set("AuthenticationToken", Encoding.ASCII.GetBytes(Convert.ToString(authToken)));

            CookieOptions option = new CookieOptions
            {
                Expires = DateTime.Now.AddHours(24),
                IsEssential = true,
                HttpOnly = true
            };

            _httpContextAccessor.HttpContext.Response.Cookies.Append("AuthenticationToken", authToken.ToString(), option);
        }

        public async Task LoginAsync(string userId)
        {
            var claims = SetIdentityClaims(userId);

            ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        }

        public async Task LogoutAsync()
        {
            await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            //_httpContextAccessor.HttpContext.Session.Clear();

            foreach (var cookie in _httpContextAccessor.HttpContext.Request.Cookies)
            {
                _httpContextAccessor.HttpContext.Response.Cookies.Delete(cookie.Key);
            }
        }

        private List<Claim> SetIdentityClaims(string userId)
        {
            return new List<Claim>
            {
                new Claim(ClaimTypes.Sid, userId),
                new Claim(ClaimTypes.Name, "Name"),
                new Claim(ClaimTypes.Surname, "Surname"),
            };
        }
    }
}
