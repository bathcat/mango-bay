# Services

This folder contains service interfaces that define the business logic contracts for the Mango Bay Cargo system. These interfaces establish the operations available for core business workflows and domain logic.

## Purpose

Service interfaces define the business operations and workflows that can be performed on domain entities. They encapsulate complex business rules, coordinate between multiple repositories, and provide a clean API for use cases like booking deliveries, processing payments, and managing users.

## Categories

-   **Core Business Services**: Handle primary domain workflows (e.g., `IDeliveryService`)
-   **Supporting Services**: Provide specialized functionality (e.g., `ITokenService`, `IPaymentProcessor`)
-   **Infrastructure Services**: Handle cross-cutting concerns (e.g., `IDbMigrationService`)

## Key Interfaces

-   `IDeliveryService` - Core service for cargo shipment booking, cancellation, and tracking
-   `ICustomerService` - Customer profile management and operations
-   `IReviewService` - Pilot review and rating functionality
-   `IAuthService` - Authentication and user management
-   `ITokenService` - JWT token generation and validation

## Key Characteristics

-   **Business Logic Focus**: Encapsulate complex business rules and workflows
-   **Transaction Management**: Coordinate multiple repository operations atomically
-   **Error Handling**: Define clear error responses and exception handling patterns
-   **Async Operations**: All operations are asynchronous for scalability
-   **Dependency Injection**: Designed for clean dependency injection and testing
-   **Security Considerations**: Business logic layer where authorization and validation rules are enforced

## Usage

API endpoints and other consumers depend on these interfaces to perform business operations, ensuring separation between the API layer and business logic implementation. Services coordinate between multiple repositories and handle complex business rules that cannot be expressed in simple CRUD operations.

## Security Notes

-   Authorization checks should be performed at the service layer before executing business operations
-   Input validation and sanitization should be coordinated with the Models layer
-   Business rules should enforce security constraints (e.g., users can only access their own data)

## References

-   [Service Layer](https://martinfowler.com/eaaCatalog/serviceLayer.html) by Martin Fowler
-   [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html) by Martin Fowler
