using System.Net.Http;

namespace Infrastructure.Common
{
    /// <summary>
    /// A 'named' client
    /// </summary>
    public class ApiClient
    {
        public HttpClient Client { get; }

        public ApiClient(HttpClient client)
        {
            Client = client;
        }
    }
}
