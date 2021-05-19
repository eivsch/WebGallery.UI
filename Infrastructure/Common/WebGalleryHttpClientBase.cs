using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;

namespace Infrastructure.Common
{
    public abstract class WebGalleryHttpClientBase
    {
        public WebGalleryHttpClientBase(HttpClient client, IHttpContextAccessor httpContext)
        {
            SetUserIdHeader();
            Client = client;

            void SetUserIdHeader()
            {
                var user = httpContext.HttpContext.User;
                var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid).Value;
                if (!string.IsNullOrWhiteSpace(userId))
                    client.DefaultRequestHeaders.Add("Gallery-User", userId);
            }
        }

        public HttpClient Client { get; }
    }
}
