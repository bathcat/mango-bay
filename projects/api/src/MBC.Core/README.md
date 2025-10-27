# MBC.Core

The core domain layer of the Mango Bay Cargo system, containing the fundamental business logic, entities, and contracts that define the cargo shipment domain.

## Purpose

This project implements the domain layer following [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html) principles, providing a clean separation between business logic and infrastructure concerns. It serves as the foundation for a security seminar application that demonstrates both secure development practices and common vulnerabilities.

## Architecture

The project is organized into five key folders that work together to define the domain:

-   **[Entities](Entities/)** - Core business objects with identity and persistence
-   **[ValueObjects](ValueObjects/)** - Immutable domain concepts that encapsulate business rules
-   **[Models](Models/)** - Data transfer objects and API contracts
-   **[Persistence](Persistence/)** - Repository interfaces for data access abstraction
-   **[Services](Services/)** - Business logic interfaces and workflows

## Design Principles

-   **Domain-Driven Design**: Business logic is expressed through rich domain models
-   **Dependency Inversion**: Core layer depends on abstractions, not concrete implementations
-   **Security-First**: Designed to demonstrate both secure patterns and common vulnerabilities
-   **Testability**: All interfaces support easy mocking and unit testing

## References

-   [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html) by Martin Fowler
-   [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) by Robert C. Martin
-   [OWASP Secure Coding Practices](https://owasp.org/www-project-secure-coding-practices-quick-reference-guide/)
