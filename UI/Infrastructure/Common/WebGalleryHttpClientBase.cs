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
                Claim userId = httpContext.HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Sid);
                if (userId == null) return;
                if (!string.IsNullOrWhiteSpace(userId.Value))
                    client.DefaultRequestHeaders.Add("Gallery-User", userId.Value);
            }
        }

        public HttpClient Client { get; }
    }
}
