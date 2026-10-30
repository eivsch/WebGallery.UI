using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Infrastructure.Common;

public class UsernameResolver
{
    public UsernameResolver(IHttpContextAccessor httpContext)
    {
        Claim userId = httpContext.HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Sid);
        if (userId == null) return;
        if (!string.IsNullOrWhiteSpace(userId.Value))
            Username = userId.Value;
    }

    public string Username {get;}
}