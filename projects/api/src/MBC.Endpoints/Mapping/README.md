# Mapping

Manual object-to-object mappers for converting between domain entities and API DTOs. Implements a simple `IMapper<TSource, TDestination>` interface without reflection-based mapping libraries.

## What Belongs Here

-   Mapper classes implementing `IMapper<TSource, TDestination>`
-   Entity → DTO conversions (for responses)
-   DTO → Core Model conversions (for requests)
-   Mapper DI registration in `ServiceCollectionExtensions.cs`
-   Helper utilities like `PageMapper` for common patterns

## What Doesn't Belong Here

-   Business logic or validation (use service layer)
-   Data access code (use stores)
-   DTO definitions (use `../Dtos`)
-   Entity definitions (use `MBC.Core`)

## Key Concepts

**Manual Mapping for AOT**: All mapping is explicit code without reflection-based libraries like AutoMapper. This ensures native AOT compatibility and eliminates runtime reflection overhead. Each mapper is a simple class with a `Map()` method.

**Compiler-Enforced Synchronization**: When you add or remove a required property from an entity or DTO, the compiler immediately flags every mapper that needs updating. This prevents the common problem of forgetting to update mapping code when models change, catching synchronization issues at compile time rather than runtime.

**Simple Interface**: All mappers implement `IMapper<TSource, TDestination>` with a single `Map()` method. This provides consistency, testability, and easy dependency injection without framework magic.

## Usage Example

```csharp
public class DeliveryReviewMapper : IMapper<DeliveryReview, DeliveryReviewDto>
{
    public DeliveryReviewDto Map(DeliveryReview source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new DeliveryReviewDto
        {
            Id = source.Id,
            PilotId = source.PilotId,
            Rating = source.Rating,
            Notes = source.Notes,
            CreatedAt = source.CreatedAt
        };
    }
}
```

Notice `CustomerId` is explicitly excluded - the mapper is where you enforce which fields cross the API boundary.

**Register in `ServiceCollectionExtensions.cs`:**

```csharp
services.AddSingleton<IMapper<DeliveryReview, DeliveryReviewDto>, DeliveryReviewMapper>();
```

Most mappers are registered as singletons since they're stateless.

## Further Reading

-   [DTO Pattern](https://martinfowler.com/eaaCatalog/dataTransferObject.html) - Context for why mapping exists
-   [Native AOT Compatibility](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/) - Why manual mapping over reflection
