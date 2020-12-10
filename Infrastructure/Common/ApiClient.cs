using System.Net.Http;
using System.Text;
using System.Text.Json;

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

    public class JsonContent : StringContent
    {
        public JsonContent(object obj, JsonSerializerOptions opts = null) :
            base(JsonSerializer.Serialize(obj, options: opts), Encoding.UTF8, "application/json")
        { }
    }
}
