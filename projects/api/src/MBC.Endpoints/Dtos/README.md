# Dtos

Data Transfer Objects for API request and response payloads. These types define the public API contract, separate from internal domain entities.

## What Belongs Here

-   Request DTOs (user input from POST/PUT endpoints)
-   Response DTOs (data returned to clients)
-   Simple record types with required properties
-   Only properties intended for public API consumption

## What Doesn't Belong Here

-   Domain entities (use `MBC.Core/Entities`)
-   Business logic or validation rules (use service layer)
-   Mapping logic (use `../Mapping`)
-   Internal-only fields not meant for API exposure

## Key Concepts

**Security Through Selective Exposure**: DTOs control what data leaves your system. Internal entities may contain sensitive fields (user IDs, internal timestamps, system flags) that should never be exposed in API responses. For example, `DeliveryReview` entity includes `CustomerId` for authorization checks, but `DeliveryReviewDto` omits it to prevent information disclosure.

**Serialization Control**: DTOs define the exact JSON structure clients receive, independent of internal data models. This allows entities to evolve without breaking API contracts, and prevents over-posting vulnerabilities where clients could set unintended properties.

**Record Types**: All DTOs use C# record types with `required` properties and `init` accessors, providing immutability and clear serialization contracts. Records give us concise syntax with value-based equality semantics.

## Naming Conventions

-   Response DTOs: `{Entity}Dto` (e.g., `DeliveryDto`, `PilotDto`)
-   Request DTOs: `{Action}{Entity}Request` (e.g., `CreateReviewRequest`, `UpdateDeliveryStatusRequest`)
-   Auth-related: Specific names like `SignInRequest`, `AuthResponse`

## Usage Example

```csharp
public sealed record DeliveryReviewDto
{
    public required Guid Id { get; init; }
    public required Guid PilotId { get; init; }
    public required Rating Rating { get; init; }
    public required string Notes { get; init; }
    public required DateTime CreatedAt { get; init; }
}
```

Notice `CustomerId` from the entity is intentionally excluded - the API client doesn't need to know which customer wrote the review if they're already authenticated.

## Further Reading

-   [DTO Pattern](https://martinfowler.com/eaaCatalog/dataTransferObject.html) - Martin Fowler's catalog entry
-   [Records in C#](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record) - Immutable reference types
-   [OWASP Mass Assignment](https://cheatsheetseries.owasp.org/cheatsheets/Mass_Assignment_Cheat_Sheet.html) - Why DTOs help prevent over-posting
