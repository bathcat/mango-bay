using System;
using System.Text;
using MBC.Core;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Endpoints.Security;
using MBC.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace MBC.Endpoints.Security;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentity(this IServiceCollection services)
    {
        services.AddIdentity<MBCUser, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 0;
            options.Password.RequiredUniqueChars = 0;
        })
        .AddEntityFrameworkStores<MBCDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }

    public static IHostApplicationBuilder AddAuthentication(this IHostApplicationBuilder builder)
    {
        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

        var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
        if (jwtSettings?.Secret == null)
        {
            throw new Exception("JwtSettings.Secret is not set");
        }
        var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });


        builder.Services.AddScoped<ICurrentUser, CurrentUser>();

        return builder;
    }

    public static IHostApplicationBuilder AddCorsPolicy(this IHostApplicationBuilder builder)
    {
        var frontendSettings = builder.Configuration
            .GetSection(FrontendSettings.SectionName)
            .Get<FrontendSettings>()
            ?? throw new Exception("FrontendSettings not found");

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(frontendSettings.BaseUrl)
                      .AllowCredentials()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        return builder;
    }

}
