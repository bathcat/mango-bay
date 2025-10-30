using Microsoft.AspNetCore.Http;

namespace MBC.Endpoints.Infrastructure;

public static class Cookies
{
    public const string AccessCookieName = "mbc_access";
    public const string RefreshCookieName = "mbc_refresh";

    public static string? GetAccessToken(HttpContext context)
    {
        context.Request.Cookies.TryGetValue(AccessCookieName, out var token);
        return token;
    }

    public static string? GetRefreshToken(HttpContext context)
    {
        context.Request.Cookies.TryGetValue(RefreshCookieName, out var token);
        return token;
    }

    public static bool HasAccessToken(HttpContext context)
        => !string.IsNullOrWhiteSpace(GetAccessToken(context));

    public static bool HasRefreshToken(HttpContext context)
        => !string.IsNullOrWhiteSpace(GetRefreshToken(context));
}
