using Microsoft.AspNetCore.Http;
using System.Net.Http;

namespace Infrastructure.Common
{
    /// <summary>
    /// A 'named' client
    /// </summary>
    public class WebGalleryApiClient : WebGalleryHttpClientBase
    {
        public WebGalleryApiClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
            : base (client, httpContextAccessor)
        { }
    }
}
