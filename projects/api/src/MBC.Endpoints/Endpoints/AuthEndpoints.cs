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

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var authGroup = app.MapGroup(ApiRoutes.Auth)
            .RequireRateLimiting(RateLimitPolicies.Auth)
            .WithTags("Authentication");

        authGroup.MapPost("/signup", SignUp)
            .WithName("SignUp")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDescription("Create a new customer account with email and password.");

        authGroup.MapPost("/signin", SignIn)
            .WithName("SignIn")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithDescription("Sign in with email and password.");

        authGroup.MapPost("/refresh", RefreshToken)
            .WithName("RefreshToken")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithDescription("Exchange refresh token for new access token.");

        authGroup.MapPost("/signout", SignOut)
            .WithName("SignOut")
            .Produces(StatusCodes.Status204NoContent)
            .WithDescription("Sign out and invalidate refresh token.");

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

