using System;
using MBC.Core.Persistence;
using MBC.Core.Services;
using MBC.Persistence.Services;
using MBC.Persistence.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MBC.Persistence;

public static class BuilderExtensions
{

    private static class Providers
    {
        public const string Sqlite = "Sqlite";
        public const string SqlServer = "SqlServer";
    }

    public static IHostApplicationBuilder AddPersistence(this IHostApplicationBuilder builder)
    {
        var dbProvider = builder.Configuration.GetValue<string>("Database:Provider", Providers.Sqlite);

        builder.Services.AddDbContext<MBCDbContext>(options =>
        {
            if (dbProvider.Equals(Providers.SqlServer, StringComparison.OrdinalIgnoreCase))
            {
                var connectionString = builder.Configuration.GetValue<string>("Database:RuntimeConnectionString");
                options.UseSqlServer(connectionString);
            }
            else
            {
                var connectionString = builder.Configuration.GetValue<string>("Database:SqliteConnectionString");
                options.UseSqlite(connectionString);
            }

            options.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        builder.Services.AddScoped<ICustomerStore, CustomerStore>();
        builder.Services.AddScoped<IDeliveryStore, DeliveryStore>();
        builder.Services.AddScoped<IPaymentStore, PaymentStore>();
        builder.Services.AddScoped<IDeliveryReviewStore, DeliveryReviewStore>();
        builder.Services.AddScoped<IDeliveryProofStore, DeliveryProofStore>();
        builder.Services.AddScoped<IPilotStore, PilotStore>();
        builder.Services.AddScoped<ISiteStore, SiteStore>();
        builder.Services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();

        builder.Services.AddScoped<IDbMigrationService, DbMigrationService>();

        return builder;
    }
}
