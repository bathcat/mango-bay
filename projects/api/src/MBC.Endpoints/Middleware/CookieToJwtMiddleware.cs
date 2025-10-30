using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MBC.Endpoints.Infrastructure;
using System.Threading.Tasks;

namespace MBC.Endpoints.Middleware;

public class CookieToJwtMiddleware
{
    private const string AuthorizationHeader = "Authorization";
    private readonly RequestDelegate _next;
    private readonly ILogger<CookieToJwtMiddleware> _logger;

    public CookieToJwtMiddleware(RequestDelegate next, ILogger<CookieToJwtMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.ContainsKey(AuthorizationHeader)
            && Cookies.HasAccessToken(context))
        {
            var accessToken = Cookies.GetAccessToken(context);
            context.Request.Headers.Append(AuthorizationHeader, $"Bearer {accessToken}");
            _logger.LogDebug("Transformed cookie to Bearer token for request: {Path}", context.Request.Path);
        }

        await _next(context);
    }
}

//TODO Bust this into a new file.
public static class CookieToJwtMiddlewareExtensions
{
    public static IApplicationBuilder UseCookieToJwtMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CookieToJwtMiddleware>();
    }
}


