# MBC.Persistence - Database Migrations Guide

This document explains how to work with Entity Framework Core migrations in the Mango Bay Cargo project.

## Prerequisites

Install the EF Core command-line tools globally:

```powershell
dotnet tool install --global dotnet-ef
```

To update to the latest version:

```powershell
dotnet tool update --global dotnet-ef
```

Verify installation:

```powershell
dotnet ef
```

## Connection String Configuration

The `DesignTimeDbContextFactory` reads the connection string from:

1. **Environment Variable** (highest priority): `MANGO_BAY_CONNECTION_STRING`
2. **Default Fallback**: LocalDB connection string

### LocalDB (Windows Default)

The default connection string targets SQL Server LocalDB:

```
Server=(localdb)\mssqllocaldb;Database=MangoBayCargo;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

No configuration needed on Windows with LocalDB installed.

### Docker SQL Server

To use Docker SQL Server, set the environment variable before running migration commands:

**PowerShell:**

```powershell
$env:MANGO_BAY_CONNECTION_STRING = "Server=localhost,1433;Database=MangoBayCargo;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

**Bash:**

```bash
export MANGO_BAY_CONNECTION_STRING="Server=localhost,1433;Database=MangoBayCargo;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

**Docker SQL Server Setup:**

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```

## Working Directory

All `dotnet ef` commands should be run from the **`projects/api`** directory:

```powershell
cd projects/api
```

## Creating a New Migration

### Basic Command

```powershell
dotnet ef migrations add <MigrationName> --project src/MBC.Persistence
```

**Example:**

```powershell
dotnet ef migrations add AddCustomerPhoneNumber --project src/MBC.Persistence
```

### Migration Naming Conventions

Use descriptive PascalCase names that indicate what changed:

-   `AddCustomerPhoneNumber`
-   `CreateDeliveryNotesTable`
-   `UpdatePilotRatingPrecision`
-   `RemoveDeprecatedFields`

### Adding Seed Data

Seed data is added directly in migration files using `migrationBuilder.InsertData()`. This keeps the DbContext focused on entity configuration and treats seed data as a one-time data migration.

**Steps to add seed data:**

1. Create a new migration (or edit an existing one before applying it)
2. Add `migrationBuilder.InsertData()` calls in the `Up()` method
3. Add corresponding `migrationBuilder.DeleteData()` calls in the `Down()` method for rollback

**Example: Inserting seed data**

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // ... table creation code ...

    migrationBuilder.InsertData(
        table: "Pilots",
        columns: new[] { "Id", "Nickname", "UserId" },
        values: new object[,]
        {
            { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Baloo", new Guid("a0000000-0000-0000-0000-000000000001") },
            { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Kit Cloudkicker", new Guid("a0000000-0000-0000-0000-000000000002") }
        });
}
```

**Example: Deleting seed data on rollback**

```csharp
protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DeleteData(
        table: "Pilots",
        keyColumn: "Id",
        keyValues: new object[]
        {
            new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")
        });

    // ... table dropping code ...
}
```

**Important Notes:**

-   Always use deterministic GUIDs (`Guid.Parse()`) for seed data, never `Guid.NewGuid()`
-   Add InsertData calls after table creation but before CreateIndex calls
-   Add DeleteData calls at the beginning of the Down method before dropping tables
-   For enum properties, use the integer value (e.g., `Status = 1` for `SiteStatus.Current`)
-   For owned entities, include the owned properties as columns (e.g., `Location_X`, `Location_Y`)

## Applying Migrations

### Update Database to Latest Migration

```powershell
dotnet ef database update --project src/MBC.Persistence
```

This creates the database if it doesn't exist and applies all pending migrations.

### Update to a Specific Migration

```powershell
dotnet ef database update <MigrationName> --project src/MBC.Persistence
```

### Rollback All Migrations (Drop Database)

```powershell
dotnet ef database update 0 --project src/MBC.Persistence
```

This reverts all migrations, effectively returning to an empty database.

## Removing Migrations

### Remove the Last Migration (Not Yet Applied)

```powershell
dotnet ef migrations remove --project src/MBC.Persistence
```

This deletes the migration files and reverts the model snapshot. Only works if the migration hasn't been applied to any database.

### Remove Applied Migrations

1. First, roll back the database to before the migration:

    ```powershell
    dotnet ef database update <PreviousMigrationName> --project src/MBC.Persistence
    ```

2. Then remove the migration:
    ```powershell
    dotnet ef migrations remove --project src/MBC.Persistence
    ```

## Viewing Migration SQL

To see the SQL that will be executed without applying it:

```powershell
dotnet ef migrations script --project src/MBC.Persistence
```

**Generate SQL for a specific migration:**

```powershell
dotnet ef migrations script <FromMigration> <ToMigration> --project src/MBC.Persistence
```

**Generate SQL for all migrations:**

```powershell
dotnet ef migrations script --project src/MBC.Persistence --output migration.sql
```

## Listing Migrations

### View All Migrations

```powershell
dotnet ef migrations list --project src/MBC.Persistence
```

## Common Troubleshooting

### "Unable to create a DbContext"

Ensure you're in the `projects/api` directory and the project builds successfully:

```powershell
cd projects/api
dotnet build
```

### "The name 'Environment' does not exist"

Make sure `using System;` is present in `DesignTimeDbContextFactory.cs`.

### SQL Server Authentication Mode Issues

If you see login failures with SQL Server Authentication (User ID/Password in connection string), your SQL Server may be configured for Windows Authentication only.

**Option 1: Enable Mixed Mode Authentication**

1. Open SQL Server Management Studio (SSMS)
2. Right-click your server instance → **Properties**
3. Go to **Security** page
4. Under "Server authentication", select **"SQL Server and Windows Authentication mode"**
5. Click **OK**
6. **Restart SQL Server** (right-click server instance → Restart, or restart via Windows Services)

**Option 2: Use Windows Authentication Instead**

Change your connection strings to use Windows Authentication:

```
Server=.;Database=MangoBayCargo;Integrated Security=true;TrustServerCertificate=True;MultipleActiveResultSets=true
```

Replace `User ID=...;Password=...` with `Integrated Security=true;Trusted_Connection=True`

### Connection Failures

Verify your connection string:

-   LocalDB: Ensure SQL Server LocalDB is installed
-   Docker: Ensure the container is running (`docker ps`)
-   Check firewall settings for remote SQL Server instances

### Migration Build Errors

If you see compilation errors in generated migrations:

-   Ensure all referenced entity types are properly imported
-   Check that enum values are represented as integers
-   Verify Guid syntax is correct: `new Guid("...")`

## Quick Reference

| Task                  | Command                                                         |
| --------------------- | --------------------------------------------------------------- |
| Create migration      | `dotnet ef migrations add <Name> --project src/MBC.Persistence` |
| Apply migrations      | `dotnet ef database update --project src/MBC.Persistence`       |
| Remove last migration | `dotnet ef migrations remove --project src/MBC.Persistence`     |
| View SQL script       | `dotnet ef migrations script --project src/MBC.Persistence`     |
| List migrations       | `dotnet ef migrations list --project src/MBC.Persistence`       |
| Drop database         | `dotnet ef database drop --project src/MBC.Persistence`         |

## Additional Resources

-   [EF Core Migrations Documentation](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
-   [Data Seeding Documentation](https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding)
-   [EF Core Tools Reference](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)
