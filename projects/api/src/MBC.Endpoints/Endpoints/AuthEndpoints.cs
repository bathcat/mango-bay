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
using Microsoft.AspNetCore.Mvc;

namespace MBC.Endpoints.Endpoints;

//TODO: it seems like we should rename this to 'jwtendpoints' or some such
public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var authGroup = app.MapGroup(ApiRoutes.AuthJwt)
            .RequireRateLimiting(RateLimitPolicies.Auth)
            .WithTags("Authentication - JWT");

        authGroup.MapPost("/signup", SignUp)
            .WithName("JwtSignUp")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDescription("Create a new customer account with email and password (returns JWT tokens).");

        authGroup.MapPost("/signin", SignIn)
            .WithName("JwtSignIn")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithDescription("Sign in with email and password (returns JWT tokens).");

        authGroup.MapPost("/refresh", RefreshToken)
            .WithName("JwtRefreshToken")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithDescription("Exchange refresh token for new access token (JWT mechanism).");

        authGroup.MapPost("/signout", SignOut)
            .WithName("JwtSignOut")
            .Produces(StatusCodes.Status204NoContent)
            .WithDescription("Sign out and invalidate refresh token (JWT mechanism).");

    }

    public static async Task<Results<Ok<AuthResponse>, BadRequest<string>>> SignUp(
        IAuthService authService,
        IMapper<AuthResult, AuthResponse> authResponseMapper,
        SignUpRequest request)
    {
        var result = await authService.SignUp(request.Email, request.Password, request.Nickname);

        if (!result.Success)
        {
            return TypedResults.BadRequest(result.ErrorMessage ?? "Sign up failed");
        }

        var response = authResponseMapper.Map(result);
        return TypedResults.Ok(response);
    }

    public static async Task<Results<Ok<AuthResponse>, UnauthorizedHttpResult>> SignIn(
        IAuthService authService,
        IMapper<AuthResult, AuthResponse> authResponseMapper,
        SignInRequest request)
    {
        var result = await authService.SignIn(request.Email, request.Password);

        if (!result.Success)
        {
            return TypedResults.Unauthorized();
        }

        var response = authResponseMapper.Map(result);
        return TypedResults.Ok(response);
    }

    public static async Task<Results<Ok<AuthResponse>, UnauthorizedHttpResult>> RefreshToken(
        IAuthService authService,
        IMapper<AuthResult, AuthResponse> authResponseMapper,
        RefreshTokenRequest request)
    {
        var result = await authService.RefreshToken(request.RefreshToken);

        if (!result.Success)
        {
            return TypedResults.Unauthorized();
        }

        var response = authResponseMapper.Map(result);
        return TypedResults.Ok(response);
    }

    public static async Task<NoContent> SignOut(
        IAuthService authService,
        [FromBody] SignOutRequest request)
    {
        await authService.SignOut(request.RefreshToken);
        return TypedResults.NoContent();
    }




}

