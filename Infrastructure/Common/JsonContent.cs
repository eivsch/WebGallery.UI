using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Common
{
    public class JsonContent : StringContent
    {
        public JsonContent(object obj, JsonSerializerOptions opts = null) :
            base(JsonSerializer.Serialize(obj, options: opts), Encoding.UTF8, "application/json")
        { }
    }
}
