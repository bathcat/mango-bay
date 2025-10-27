# Persistence

This folder contains repository interfaces that define data access contracts for the Mango Bay Cargo system. These interfaces establish the abstraction layer between business logic and data storage mechanisms.

## Purpose

Repository interfaces define the data access operations available for each entity type, enabling loose coupling between the business logic layer and the actual data persistence implementation. They provide a consistent API for CRUD operations and specialized queries.

## Architecture

-   **Repository Pattern**: Each entity has a corresponding store interface following the [Repository pattern](https://martinfowler.com/eaaCatalog/repository.html)
-   **Generic Base Interface**: `IStore<TId, TEntity>` provides common read operations
-   **Specialized Operations**: Entity-specific interfaces extend the base with create, update, delete, and custom query methods
-   **Dependency Injection Ready**: Designed for easy mocking and testing
-   **Entity Framework Core Implementation**: Concrete implementations use EF Core for data access

## Key Interfaces

-   `IStore<TId, TEntity>` - Base interface providing `GetById` and `GetPage` operations
-   `ICustomerStore` - Customer-specific operations including CRUD and user lookup
-   `IDeliveryStore` - Delivery-specific operations with status and filtering capabilities
-   `IPilotStore` - Pilot-specific operations including availability and rating queries

## Key Characteristics

-   **Read-Focused Base**: Common operations are reading entities by ID or in pages
-   **Specialized Extensions**: Each store adds entity-specific create, update, delete, and query methods
-   **Consistent Patterns**: Similar method signatures and return types across all stores
-   **Async Support**: All operations are asynchronous for scalability

## Usage

Business services depend on these interfaces rather than concrete implementations, allowing for easy testing with mocks and flexibility in choosing the actual data access technology (Entity Framework Core, Dapper, etc.).

## Design Limitations

While this repository pattern provides clean separation and testability, it has important limitations for production applications:

-   **No Unit of Work Pattern**: Each repository operation is independent, making it difficult to coordinate multiple operations atomically across different entities
-   **Limited Transaction Support**: The interfaces don't provide explicit transaction management, leaving it to the concrete implementation to handle rollbacks and consistency
-   **Potential for Inconsistent State**: Without proper transaction boundaries, partial updates could leave the system in an inconsistent state if operations fail midway through a business process

In a production system, you would probably want to support transactions.

## References

-   [Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html) by Martin Fowler
-   [Unit of Work Pattern](https://martinfowler.com/eaaCatalog/unitOfWork.html) by Martin Fowler
-   [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) by Microsoft
