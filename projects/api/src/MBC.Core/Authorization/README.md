# Authorization in Mango Bay Cargo

## Authentication vs Authorization

**Authentication** answers "Who are you?" (identity verification). **Authorization** answers "What are you allowed to do?" (permission checks).

See [Microsoft's Authentication vs Authorization](https://learn.microsoft.com/en-us/azure/active-directory/develop/authentication-vs-authorization) for more details.

## Three Authorization Approaches

This application demonstrates three distinct authorization approaches, each with different tradeoffs.

### 1. Simple Role-Based Access Control (RBAC) at Endpoint

**Pattern:** Declarative authorization using `.RequireAuthorization(policy => policy.RequireRole(...))`

**Example:** Site management endpoints (create, update, delete) restricted to Administrators only.

**Pros:**

-   Extremely simple and explicit
-   Authorization visible in endpoint registration
-   No code to write beyond declaration
-   Easy to reason about
-   Fast to implement

**Cons:**

-   Blunt instrument - all-or-nothing by role
-   Cannot express resource ownership or relationships
-   No access to resource being protected
-   Testing requires integration tests (harder than unit tests)
-   Repetitive if authorization logic becomes complex

**When to use:** Pure role-based operations where everyone in a role has identical permissions (e.g., admin-only endpoints).

### 2. RBAC with Custom Logic at Endpoint

**Pattern:** Manual authorization checks in endpoint handlers using `ICurrentUser` and role checks.

**Example:** Customer endpoints where pilots/admins can view any customer, but customers can only view themselves.

**Pros:**

-   Still relatively simple
-   Can express resource ownership
-   Authorization logic visible in endpoint
-   Easy to understand by reading the handler
-   Flexible - write any logic you need

**Cons:**

-   Programmatic checks are easy to forget or mess up
-   Hard to unit test (requires mocking HTTP context/claims)
-   Not automatically enforced at service layer (no protection for CLI, background jobs, etc.)
-   Authorization logic scattered across endpoints
-   No centralized policy to audit

**When to use:** Straightforward resource ownership scenarios where authorization logic is simple and varies between endpoints.

### 3. Resource-Based Authorization with ASP.NET Core Infrastructure

**Pattern:** `IAuthorizationService` with custom handlers implementing `AuthorizationHandler<TRequirement, TResource>`.

**Example:** Delivery, Review, and DeliveryProof services using stakeholder-based authorization strategies.

**Implementation details:**

-   **Operations:** Define requirements in `*Operations` classes (e.g., `ReviewOperations.Create`, `DeliveryOperations.Read`)
-   **Handlers:** Implement `AuthorizationHandler<OperationAuthorizationRequirement, TResource>` to check permissions
-   **Strategies:** Use `StakeholderAuthorizationStrategy` to define permissions for different user roles (Admin, Customer, Pilot, Anonymous)
-   **Wrapper:** `IMbcAuthorizationService` simplifies calls by automatically passing current user (and is **synchronous** to prevent forgetting to await)

**Pros:**

-   Authorization enforced at service layer (works for HTTP, CLI, background jobs, WebSockets, etc.)
-   Highly testable - easy to unit test service methods and handlers independently
-   Centralized authorization policies
-   Expressive - can check complex relationships (stakeholders, ownership, delegation)
-   Reusable across multiple endpoints
-   Policy-based - easy to audit and modify

**Cons:**

-   More infrastructure required (handlers, strategies, operations)
-   Steeper learning curve
-   More files to maintain
-   Authorization not visible at endpoint registration (hidden in service)
-   Can be overkill for simple scenarios

**When to use:** Complex authorization scenarios involving resource relationships, stakeholder permissions, or when you need authorization enforced beyond HTTP endpoints.

#### The IMbcAuthorizationService Wrapper

We created a thin wrapper around `IAuthorizationService` to:

1. Automatically inject the current user (no need to pass `ClaimsPrincipal` every time)
2. Provide a simpler API for common operations
3. Make methods **synchronous** to prevent accidentally forgetting to `await` (which would cause exceptions to be swallowed)

**Hybrid approach:** `IMbcAuthorizationService` also exposes `AuthorizeOrThrow(IEnumerable<string> roles)` for role-based checks at the service layer (e.g., `DeliveryService.Book`). This combines service-level enforcement with role-based simplicity.

## Decision Guidelines

**Choose your approach based on:**

-   **Application complexity:** Simple apps can use endpoint RBAC; complex apps benefit from resource-based authorization
-   **Team sophistication:** Junior teams may prefer explicit endpoint logic; experienced teams can leverage `IAuthorizationService`
-   **Worst-case scenario:** High-stakes applications (financial, healthcare, security) should use resource-based authorization for defense in depth
-   **Non-HTTP APIs:** If you have CLI tools, background jobs, or WebSocket APIs, service-layer authorization is essential

**Rule of thumb:**

-   Pure role checks → Endpoint RBAC (Approach #1)
-   Simple "can only access own resources" → Endpoint with custom logic (Approach #2)
-   Complex stakeholder relationships or multi-channel access → Resource-based authorization (Approach #3)

## Further Reading

-   [Microsoft: Resource-based authorization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased)
-   [Microsoft: Policy-based authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies)
-   [Martin Fowler: Role-Based Access Control](https://martinfowler.com/bliki/RoleBasedAccessControl.html)
-   [OWASP: Authorization Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html)
