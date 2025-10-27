# Models

This folder contains Data Transfer Objects (DTOs), request/response models, and supporting model classes used for API communication and data transformation in the Mango Bay Cargo system.

## Purpose

Models represent non-persistent data structures that either serve as API contracts for external consumers or as nested objects owned by entities. They enable clean separation between the internal domain model and external API contracts, and provide ephemeral data structures for operations.

## Categories

-   **Entity-Owned Models**: Nested objects that belong to entities and are persisted as part of them (e.g., `CreditCardInfo`, `JobDetails`)
-   **Request Models**: Define the structure of data sent to API endpoints (e.g., `BookingRequest`)
-   **Response Models**: Define the structure of data returned from API endpoints (e.g., `PaymentResult`)
-   **Configuration Models**: Hold settings and options for various services (e.g., `JwtSettings`)
-   **Ephemeral Models**: Temporary utility classes for specific operations (e.g., `Page`, `CostEstimate`)

## Key Characteristics

-   **Non-Persistent**: Models are not stored in the database as independent entities
-   **API Contracts**: Define clear interfaces for external API consumers
-   **Validation Support**: Include data annotations for input validation
-   **Serialization Ready**: Designed for JSON serialization/deserialization
-   **Separation of Concerns**: Keep external contracts separate from internal domain entities

## Examples

-   `CreditCardInfo` - Entity-owned model nested within payment entities
-   `JobDetails` - Entity-owned model containing shipment specifications
-   `BookingRequest` - Request model for creating new cargo bookings
-   `AuthResult` - Response model for authentication operations
-   `JwtSettings` - Configuration model for JWT token handling
-   `Page<T>` - Ephemeral model for paginated results

## Usage

These models serve multiple purposes: entity-owned models are persisted as part of their parent entities, while request/response models and ephemeral models are used for API communication and temporary operations. They ensure type safety and clear contracts between different layers of the application.

## References

-   [Data Transfer Object](https://martinfowler.com/eaaCatalog/dataTransferObject.html) by Martin Fowler
