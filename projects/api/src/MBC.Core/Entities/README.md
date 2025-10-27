# Entities

This folder contains domain entity classes that represent the core business objects in the Mango Bay Cargo system. These entities encapsulate the fundamental data structures and business rules of the cargo shipment domain.

## Purpose

Domain entities represent the **persistent** business objects that are stored in the database and serve as the foundation of the application's domain model. Each entity has an Id field for unique identification and represents a business concept that needs to be tracked and persisted.

## Key Characteristics

-   **Persistent with Identity**: Entities that are stored in the database and identified by unique Id fields
-   **Domain-Driven Design**: Follow [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html) principles with clear business meaning
-   **Simple Structure**: Focus on core data properties without complex business logic
-   **ASP.NET Core Identity Integration**: Include references to Identity users where applicable
-   **Entity Framework Core Ready**: Designed for EF Core with navigation properties and proper relationships

## Examples

-   `Customer` - Represents customers who book cargo shipments
-   `Pilot` - Represents pilots who deliver cargo
-   `Delivery` - Represents cargo shipment bookings
-   `DeliveryReview` - Represents customer reviews of pilot services

## Usage

These entities serve as the persistent data model for the application. They are manipulated by business services and persisted through repository interfaces, forming the core data structures that represent the state of the cargo shipment domain.

## References

-   [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html) by Martin Fowler
-   [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) by Microsoft
