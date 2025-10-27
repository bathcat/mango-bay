# Endpoints

HTTP endpoint definitions using .NET Minimal APIs, organized into resource-focused static classes similar to traditional controllers. Provides AOT-friendly API routes with clear separation between HTTP concerns and business logic.

Each static class defines routes for a single resource (deliveries, sites, pilots, etc.) and delegates business logic to service layer implementations.

## What Belongs Here

-   Route mapping and HTTP verb definitions
-   Request parameter binding and validation
-   Response type specifications and status codes
-   Endpoint-level authorization (when appropriate)
-   DTO ↔ Core Model conversions via mappers
-   Endpoint metadata (names, tags, descriptions for OpenAPI)

## What Doesn't Belong Here

-   Business logic or domain rules (use service layer in `MBC.Services`)
-   Data access or database queries (use stores in `MBC.Persistence`)
-   DTO definitions (use `../Dtos`)
-   Entity definitions (use `MBC.Core/Entities`)
-   Mapper implementations (use `../Mapping`)

## Key Concepts

**Minimal APIs with AOT Support**: Uses .NET Minimal APIs instead of traditional controllers for native AOT compatibility. Endpoints are defined as static methods with dependency injection via method parameters rather than constructor injection.

**Controller-Style Organization**: Each resource gets its own static class (`SiteEndpoints`, `DeliveryEndpoints`, etc.) with a `MapXxxEndpoints(this WebApplication app)` extension method that registers all routes for that resource. This provides controller-like organization without the overhead of the MVC framework.

**Authorization Placement**: This codebase demonstrates both endpoint-level authorization (`SiteEndpoints` using `.RequireAuthorization()`) and service-level authorization (`DeliveryEndpoints` delegating to services). In production applications, choose one approach consistently to avoid security gaps. Service-level authorization is preferred when multiple APIs share the same services, while endpoint-level authorization works for simple role checks in single-API scenarios.

**Typed Results**: Uses `TypedResults` and typed result unions (`Results<Ok<T>, NotFound>`) for compile-time safety and better OpenAPI documentation generation.

**Dependency Injection**: Method parameters are automatically resolved from DI container. Services, mappers, stores, and configuration are injected per-request.

## Usage

**Create a new endpoint group:**

```csharp
public static class ShipmentEndpoints
{
    public static void MapShipmentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup(ApiRoutes.Shipments)
            .RequireAuthorization()
            .WithTags("Shipments");

        group.MapGet("/", GetShipments)
            .WithName("GetShipments")
            .Produces<Page<ShipmentDto>>(StatusCodes.Status200OK);
    }

    public static async Task<Ok<Page<ShipmentDto>>> GetShipments(
        IShipmentStore store,
        IMapper<Shipment, ShipmentDto> mapper,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10)
    {
        var shipments = await store.GetPage(skip, take);
        var page = PageMapper.Map(shipments, mapper);
        return TypedResults.Ok(page);
    }
}
```

**Register in `Infrastructure/WebApplicationExtensions.cs`:**

```csharp
public static WebApplication MapEndpoints(this WebApplication app)
{
    app.MapAuthEndpoints();
    app.MapDeliveryEndpoints();
    app.MapShipmentEndpoints();  // Add your new endpoint here
    // ... other endpoints
    return app;
}
```

Then `Program.cs` stays lean with a single call: `app.MapEndpoints();`

## Endpoint Organization Pattern

Each endpoint file follows this structure:

1. Single static class named `{Resource}Endpoints`
2. Public `Map{Resource}Endpoints()` extension method
3. Route group creation with common middleware
4. Individual route mappings with metadata
5. Static handler methods with injected dependencies

**Example structure:**

```csharp
public static class ResourceEndpoints
{
    public static void MapResourceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/resource");
        group.MapGet("/", GetAll);
        group.MapPost("/", Create).RequireAuthorization();
    }

    public static async Task<Ok<List<T>>> GetAll(IStore store) { ... }
    public static async Task<Created<T>> Create(IService service) { ... }
}
```

## Infrastructure Subdirectory

The `Infrastructure/` subdirectory contains shared endpoint infrastructure like `ApiRoutes.cs` for centralized route path constants with `/api/v1` prefix. Keep endpoint-specific logic in the main endpoint files; only shared infrastructure belongs in `Infrastructure/`.

## Further Reading

-   [Minimal APIs Overview](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis) - Microsoft's guide to Minimal APIs
-   [Native AOT Deployment](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/native-aot) - Why Minimal APIs enable AOT compilation
-   [Route Groups](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/route-handlers#route-groups) - Organizing routes with common configuration
-   [Typed Results](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses#typed-results) - Type-safe response handling
