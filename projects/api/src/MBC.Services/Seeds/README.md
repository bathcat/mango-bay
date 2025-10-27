# Seeds

Development-only database seeding that populates the application with sample data on startup. Creates users, pilots, sites, and deliveries for demonstration and testing purposes.

## What Belongs Here

-   `SeedService` - the hosted service that runs seeding on application startup
-   Seed data definitions in `/Data` subdirectory (pilots, sites)
-   Mock implementations for bypassing authorization during seeding
-   Helper utilities for seed data generation

## What Doesn't Belong Here

-   Production data initialization (seeds are for development only)
-   Database migrations (use `MBC.Persistence/Migrations`)
-   Business logic that belongs in the service layer
-   Test data (use test fixtures in test projects)

## Key Concepts

**Demo/Development Only**: This seeding approach has no analog in production systems. Real applications would have data migration scripts for initial releases or import tools for real data. Seeds exist solely to provide a working demo environment with realistic sample data.

**Service-Based Seeding**: Seeds work through the service layer (`IAuthService`, `IDeliveryService`, etc.) rather than direct SQL `INSERT` statements. This allows seeding to work with both SQL Server databases and the in-memory database provider.

**Idempotent Execution**: The `SeedService` checks for existing data in the Sites table before seeding. If any sites exist, seeding is skipped entirely.

**Fresh Start**: To reseed the database, delete your database via SQL Server Management Studio (SSMS). On next application startup, EF Core migrations will recreate the schema and `SeedService` will populate it with fresh data.

## What Gets Seeded

1. **Roles**: Customer, Pilot, Administrator
2. **Sites**: Cargo locations with coordinates and descriptions (from `Data/Sites.cs`)
3. **Pilots**: Demo pilot accounts with bios and avatars (from `Data/Pilots.cs`)
4. **Test Customer**: Single customer account (`jbloggs@goomail.com`)
5. **Admin Account**: Administrator user (`admin@mangobaycargo.com`)
6. **Sample Deliveries**: 15 deliveries with varied cargo descriptions and statuses

All seeded accounts use the password `One2three` for easy testing.

## Seeding Mechanics

`SeedService` implements `IHostedService` and runs during application startup:

1. Runs EF Core migrations to ensure schema exists
2. Checks if Sites table has any rows - if yes, skip seeding
3. Creates Identity roles if they don't exist
4. Seeds sites, pilots, customer, and admin accounts via service layer
5. Creates sample deliveries using a mock `ICurrentUser` to bypass authorization

## Further Reading

-   [IHostedService](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services) - Background tasks in ASP.NET Core
-   [EF Core Data Seeding](https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding) - Alternative migration-based seeding approach
