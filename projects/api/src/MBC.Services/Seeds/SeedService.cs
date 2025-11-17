using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MBC.Core;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Core.Persistence;
using MBC.Core.Services;
using MBC.Core.ValueObjects;
using MBC.Services.Authentication;
using MBC.Services.Core;
using MBC.Services.Seeds.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MBC.Services.Seeds;

public class SeedService : IHostedService
{
    private const string SeedPassword = "One2three";

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SeedService> _logger;

    public SeedService(IServiceProvider serviceProvider, ILogger<SeedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting database seeding");

        using (var scope = _serviceProvider.CreateScope())
        {
            var migrationService = scope.ServiceProvider.GetRequiredService<IDbMigrationService>();
            await migrationService.Migrate(cancellationToken);
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var siteStore = scope.ServiceProvider.GetRequiredService<ISiteStore>();
            var existingSites = await siteStore.GetPage(0, 1);
            if (existingSites.Items.Any())
            {
                _logger.LogInformation("Database already seeded, skipping");
                return;
            }
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            await SeedRoles(roleManager);
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var siteStore = scope.ServiceProvider.GetRequiredService<ISiteStore>();
            await SeedSites(siteStore);
        }

        Guid customerId;
        using (var scope = _serviceProvider.CreateScope())
        {
            var authService = CreateSeedAuthService(scope.ServiceProvider);
            customerId = await SeedCustomer(authService);
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var authService = CreateSeedAuthService(scope.ServiceProvider);
            await SeedPilots(authService);
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var authService = CreateSeedAuthService(scope.ServiceProvider);
            await SeedAdmin(authService);
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var mockCurrentUser = new MockCurrentUser(customerId, UserRoles.Customer, customerId);
            var mockAuthorizationService = new MockMbcAuthorizationService();
            var mockDeliveryService = new DeliveryService(
                scope.ServiceProvider.GetRequiredService<IDeliveryStore>(),
                scope.ServiceProvider.GetRequiredService<IPaymentStore>(),
                scope.ServiceProvider.GetRequiredService<IPaymentProcessor>(),
                scope.ServiceProvider.GetRequiredService<IPilotStore>(),
                scope.ServiceProvider.GetRequiredService<ISiteStore>(),
                mockAuthorizationService,
                mockCurrentUser,
                scope.ServiceProvider.GetRequiredService<ILogger<DeliveryService>>(),
                scope.ServiceProvider.GetRequiredService<IOptions<CostCalculationOptions>>());

            var pilotStore = scope.ServiceProvider.GetRequiredService<IPilotStore>();
            var siteStore = scope.ServiceProvider.GetRequiredService<ISiteStore>();
            await SeedDeliveries(mockDeliveryService, pilotStore, siteStore, customerId);
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var mockAuthorizationService = new MockMbcAuthorizationService();
            var mockReviewService = new ReviewService(
                scope.ServiceProvider.GetRequiredService<IDeliveryReviewStore>(),
                scope.ServiceProvider.GetRequiredService<IDeliveryStore>(),
                mockAuthorizationService,
                scope.ServiceProvider.GetRequiredService<ILogger<ReviewService>>(),
                scope.ServiceProvider.GetRequiredService<IHtmlSanitizer>());

            var deliveryStore = scope.ServiceProvider.GetRequiredService<IDeliveryStore>();
            await SeedReviews(mockReviewService, deliveryStore, customerId);
        }

        _logger.LogInformation("Database seeding completed");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task SeedRoles(RoleManager<IdentityRole<Guid>> roleManager)
    {
        _logger.LogInformation("Seeding roles");

        string[] roles = [UserRoles.Customer, UserRoles.Pilot, UserRoles.Administrator];

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>
                {
                    Id = Guid.NewGuid(),
                    Name = roleName,
                    NormalizedName = roleName.ToUpper()
                });
            }
        }
    }

    private async Task SeedSites(ISiteStore siteStore)
    {
        _logger.LogInformation("Seeding sites");

        foreach (var site in Sites.Seeds)
        {
            await siteStore.Create(site);
        }
    }

    private async Task<Guid> SeedCustomer(IAuthService authService)
    {
        _logger.LogInformation("Seeding customer");

        var result = await authService.SignUp(
            "jbloggs@goomail.com",
            SeedPassword,
            "jbloggs");

        if (!result.Success)
        {
            _logger.LogError("Failed to seed customer: {Error}", result.ErrorMessage);
            throw new InvalidOperationException($"Failed to seed customer: {result.ErrorMessage}");
        }

        return result.CustomerId!.Value;
    }

    private async Task SeedPilots(IAuthService authService)
    {
        _logger.LogInformation("Seeding pilots");

        foreach (var pilotSeed in Pilots.Seeds)
        {
            var email = $"{pilotSeed.ShortName}@mangobaycargo.com";

            var result = await authService.CreatePilot(
                email,
                SeedPassword,
                pilotSeed.ShortName,
                pilotSeed.FullName,
                pilotSeed.Bio,
                pilotSeed.AvatarUrl);

            if (!result.Success)
            {
                _logger.LogError("Failed to seed pilot {PilotName}: {Error}", pilotSeed.FullName, result.ErrorMessage);
                throw new InvalidOperationException($"Failed to seed pilot {pilotSeed.FullName}: {result.ErrorMessage}");
            }
        }
    }

    private async Task SeedAdmin(IAuthService authService)
    {
        _logger.LogInformation("Seeding admin");

        var result = await authService.CreateAdmin(
            "admin@mangobaycargo.com",
            SeedPassword);

        if (!result.Success)
        {
            _logger.LogError("Failed to seed admin: {Error}", result.ErrorMessage);
            throw new InvalidOperationException($"Failed to seed admin: {result.ErrorMessage}");
        }
    }

    private const int SampleDeliveryCount = 15;

    private static readonly (string CargoDescription, decimal Weight)[] SampleDeliveries =
    [
        ("Fresh mangoes from the bay", 15.5m),
        ("Rare tropical spices", 8.2m),
        ("Emergency medical supplies", 12.0m),
        ("Vintage rum collection", 25.8m),
        ("Antique furniture", 100.0m),
        ("Frozen tuna", 50.0m),
        ("Rocks", 90.0m),
        ("Exotic bird specimens", 3.1m),
        ("Cape Suzette's finest honey", 22.3m),
        ("Pirate treasure chest", 75.0m),
        ("Live penguin for zoo", 15.0m),
        ("Weather balloon equipment", 35.7m),
        ("Ancient map collection", 8.9m),
        ("Crystal clear glacier ice", 40.2m),
        ("Mechanic's toolbox", 28.5m),
        ("Radio transmission equipment", 18.6m),
        ("Emergency parachute supplies", 12.4m),
        ("Tropical fruit basket", 19.8m),
        ("Aviator's leather jacket", 2.1m),
        ("Mysterious cargo - handle with care", 33.3m)
    ];

    private static readonly (int Rating, string Notes)[] SampleReviews =
    [
        (5, "<p>Excellent pilot! Very professional and on-time delivery.</p>"),
        (5, "<p>Outstanding service, would definitely book again!</p>"),
        (4, "<p>Good flight, just a bit bumpy near the destination.</p>"),
        (5, "<p>Cargo arrived in perfect condition. Highly recommend!</p>"),
        (4, "<p>Smooth delivery, though arrived 30 minutes late.</p>")
    ];

    private async Task SeedDeliveries(IDeliveryService deliveryService, IPilotStore pilotStore, ISiteStore siteStore, Guid customerId)
    {
        _logger.LogInformation("Seeding deliveries");

        var sitesPage = await siteStore.GetPage(0, int.MaxValue);
        var pilotsPage = await pilotStore.GetPage(0, int.MaxValue);
        var sites = sitesPage.Items.ToList();
        var pilots = pilotsPage.Items.ToList();

        var today = DateOnly.FromDateTime(DateTime.Today);


        for (int i = 0; i < SampleDeliveryCount; i++)
        {
            var origin = sites.PickRandom();
            var destination = sites.PickRandom(s => s.Id != origin.Id);
            var pilot = pilots.PickRandom();
            var sample = SampleDeliveries[i];

            var bookingRequest = new DeliveryRequest
            {
                CustomerId = customerId,
                PilotId = pilot.Id,
                Details = new JobDetails
                {
                    OriginId = origin.Id,
                    DestinationId = destination.Id,
                    CargoDescription = sample.CargoDescription,
                    CargoWeightKg = sample.Weight,
                    ScheduledFor = today.AddDays(Random.Shared.Next(1, 30))
                },
                CreditCard = new CreditCardInfo
                {
                    CardNumber = "1211111111111111",
                    Expiration = DateOnly.FromDateTime(DateTime.Today.AddYears(2)),
                    Cvc = "123",
                    CardholderName = "Joe Bloggs"
                }
            };

            try
            {
                await deliveryService.Book(bookingRequest);
                _logger.LogDebug("Created delivery {Index} for pilot {PilotName}", i + 1, pilot.FullName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create delivery {Index}", i + 1);
                throw;
            }
        }
    }

    private async Task SeedReviews(IReviewService reviewService, IDeliveryStore deliveryStore, Guid customerId)
    {
        _logger.LogInformation("Seeding reviews");

        var deliveriesPage = await deliveryStore.GetByCustomerId(customerId, 0, int.MaxValue);
        var deliveries = deliveriesPage.Items.ToList();

        var deliveriesToReview = deliveries.Take(5).ToList();

        for (int i = 0; i < deliveriesToReview.Count; i++)
        {
            var delivery = deliveriesToReview[i];
            var sample = SampleReviews[i];

            delivery.Status = DeliveryStatus.Delivered;
            delivery.CompletedOn = DateTime.UtcNow.AddDays(-Random.Shared.Next(5, 20));
            await deliveryStore.Update(delivery);

            try
            {
                await reviewService.CreateReview(
                    customerId,
                    delivery.Id,
                    Rating.From(sample.Rating),
                    sample.Notes);

                _logger.LogDebug("Created review {Index} for delivery {DeliveryId}", i + 1, delivery.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create review {Index}", i + 1);
                throw;
            }
        }
    }

    private IAuthService CreateSeedAuthService(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<MBCUser>>();
        var signInManager = serviceProvider.GetRequiredService<SignInManager<MBCUser>>();
        var tokenService = serviceProvider.GetRequiredService<ITokenService>();
        var refreshTokenStore = serviceProvider.GetRequiredService<IRefreshTokenStore>();
        var customerStore = serviceProvider.GetRequiredService<ICustomerStore>();
        var pilotStore = serviceProvider.GetRequiredService<IPilotStore>();
        var fingerprintService = new SeedFingerprintService();
        var jwtSettings = serviceProvider.GetRequiredService<IOptions<JwtSettings>>();
        var logger = serviceProvider.GetRequiredService<ILogger<AuthService>>();

        return new AuthService(
            userManager,
            signInManager,
            tokenService,
            refreshTokenStore,
            customerStore,
            pilotStore,
            fingerprintService,
            jwtSettings,
            logger
        );
    }
}
