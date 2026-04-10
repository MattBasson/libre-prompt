using System.Security.Claims;

namespace PromptPortal.Api.Middleware;

public class DevAuthMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var claims = new[]
        {
            new Claim("sub", "dev-user@local"),
            new Claim("tenant_id", "local-dev"),
            new Claim("name", "Local Developer"),
            new Claim("role", "PromptReader"),
            new Claim("role", "PromptEditor"),
            new Claim("role", "PromptAdmin")
        };

        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "DevAuth"));

        await next(context);
    }
}
