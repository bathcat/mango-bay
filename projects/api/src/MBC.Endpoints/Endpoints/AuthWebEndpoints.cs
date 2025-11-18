using System;
using System.Threading.Tasks;
using MBC.Core.Services;
using MBC.Endpoints.Dtos;
using MBC.Endpoints.Endpoints.Infrastructure;
using MBC.Endpoints.Mapping;
using MBC.Endpoints.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using MBC.Core.Models;

namespace MBC.Endpoints.Endpoints;

public static class AuthWebEndpoints
{
    private const string AccessCookieName = "mbc_access";
    private const string RefreshCookieName = "mbc_refresh";

    public static void MapAuthWebEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var authWebGroup = app.MapGroup(ApiRoutes.AuthWeb)
            .RequireRateLimiting(RateLimitPolicies.Auth)
            .WithTags("Authentication - Web");

        authWebGroup.MapPost("/signup", SignUp)
            .WithName("WebSignUp")
            .Produces<AuthWebResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDescription("Create a new customer account with email and password (sets HTTP-only cookies).");

        authWebGroup.MapPost("/signin", SignIn)
            .WithName("WebSignIn")
            .Produces<AuthWebResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithDescription("Sign in with email and password (sets HTTP-only cookies).");

        authWebGroup.MapPost("/refresh", RefreshToken)
            .WithName("WebRefreshToken")
            .Produces<AuthWebResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithDescription("Exchange refresh token cookie for new access token cookie.");

        authWebGroup.MapPost("/signout", SignOut)
            .WithName("WebSignOut")
            .Produces(StatusCodes.Status204NoContent)
            .WithDescription("Sign out and clear HTTP-only cookies.");
    }

    public static async Task<Results<Ok<AuthWebResponse>, BadRequest<string>>> SignUp(
        HttpContext httpContext,
        IAuthService authService,
        IMapper<AuthResult, AuthResponse> authResponseMapper,
        IOptions<JwtSettings> jwtSettings,
        SignUpRequest request)
    {
        var result = await authService.SignUp(request.Email, request.Password, request.Nickname);

        if (!result.Success)
        {
            return TypedResults.BadRequest(result.ErrorMessage ?? "Sign up failed");
        }

        var authResponse = authResponseMapper.Map(result);
        SetAuthCookies(httpContext, authResponse.AccessToken, authResponse.RefreshToken, jwtSettings.Value);

        var webResponse = new AuthWebResponse
        {
            User = authResponse.User
        };

        return TypedResults.Ok(webResponse);
    }

    public static async Task<Results<Ok<AuthWebResponse>, UnauthorizedHttpResult>> SignIn(
        HttpContext httpContext,
        IAuthService authService,
        IMapper<AuthResult, AuthResponse> authResponseMapper,
        IOptions<JwtSettings> jwtSettings,
        SignInRequest request)
    {
        var result = await authService.SignIn(request.Email, request.Password);

        if (!result.Success)
        {
            return TypedResults.Unauthorized();
        }

        var authResponse = authResponseMapper.Map(result);
        SetAuthCookies(httpContext, authResponse.AccessToken, authResponse.RefreshToken, jwtSettings.Value);

        var webResponse = new AuthWebResponse
        {
            User = authResponse.User
        };

        return TypedResults.Ok(webResponse);
    }

    public static async Task<Results<Ok<AuthWebResponse>, UnauthorizedHttpResult>> RefreshToken(
        HttpContext httpContext,
        IAuthService authService,
        IMapper<AuthResult, AuthResponse> authResponseMapper,
        IOptions<JwtSettings> jwtSettings)
    {
        if (!httpContext.Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken)
            || string.IsNullOrWhiteSpace(refreshToken))
        {
            return TypedResults.Unauthorized();
        }

        var result = await authService.RefreshToken(refreshToken);

        if (!result.Success)
        {
            return TypedResults.Unauthorized();
        }

        var authResponse = authResponseMapper.Map(result);
        SetAuthCookies(httpContext, authResponse.AccessToken, authResponse.RefreshToken, jwtSettings.Value);

        var webResponse = new AuthWebResponse
        {
            User = authResponse.User
        };

        return TypedResults.Ok(webResponse);
    }

    public static async Task<NoContent> SignOut(
        HttpContext httpContext,
        IAuthService authService)
    {
        if (httpContext.Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken)
            && !string.IsNullOrWhiteSpace(refreshToken))
        {
            await authService.SignOut(refreshToken);
        }

        ClearAuthCookies(httpContext);
        return TypedResults.NoContent();
    }

    private static void SetAuthCookies(HttpContext httpContext, string accessToken, string refreshToken, JwtSettings jwtSettings)
    {
        var accessCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            MaxAge = TimeSpan.FromMinutes(jwtSettings.AccessTokenExpirationMinutes),
            Secure = false
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Path = $"{ApiRoutes.AuthWeb}/refresh",
            MaxAge = TimeSpan.FromDays(jwtSettings.RefreshTokenExpirationDays),
            Secure = false
        };

        httpContext.Response.Cookies.Append(AccessCookieName, accessToken, accessCookieOptions);
        httpContext.Response.Cookies.Append(RefreshCookieName, refreshToken, refreshCookieOptions);
    }

    private static void ClearAuthCookies(HttpContext httpContext)
    {
        var accessExpiredOptions = new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(-1),
            Secure = false
        };

        var refreshExpiredOptions = new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Path = $"{ApiRoutes.AuthWeb}/refresh",
            Expires = DateTimeOffset.UtcNow.AddDays(-1),
            Secure = false
        };

        httpContext.Response.Cookies.Append(AccessCookieName, string.Empty, accessExpiredOptions);
        httpContext.Response.Cookies.Append(RefreshCookieName, string.Empty, refreshExpiredOptions);
    }
}


