using Microsoft.AspNetCore.Http;
using System.Net.Http;

namespace Infrastructure.Common
{
    public class WebGalleryFileServerClient : WebGalleryHttpClientBase
    {
        public WebGalleryFileServerClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
            : base(client, httpContextAccessor)
        { }
    }
}
