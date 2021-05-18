﻿using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Common
{
    /// <summary>
    /// A 'named' client
    /// </summary>
    public class ApiClient
    {
        public ApiClient(HttpClient client, IHttpContextAccessor httpContext)
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

    public class JsonContent : StringContent
    {
        public JsonContent(object obj, JsonSerializerOptions opts = null) :
            base(JsonSerializer.Serialize(obj, options: opts), Encoding.UTF8, "application/json")
        { }
    }
}
