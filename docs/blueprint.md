# Mango Bay Cargo - Technical Blueprint

## Overview

Mango Bay Cargo is a cargo shipment booking and tracking platform for a fictional airline inspired by TaleSpin. This application serves as a teaching tool for a seminar on building secure applications with Angular and .NET, designed to demonstrate both secure development practices and common vulnerabilities.

## Current Status

**Implementation Phase:** Core features complete, ready for security demonstrations

**What's Built:**

- ✅ Complete authentication system (JWT + Google OAuth)
- ✅ Full CRUD operations for deliveries, pilots, sites, reviews, customers
- ✅ Payment processing with fake payment processor
- ✅ Image upload (proof of delivery, site images)
- ✅ Rich text reviews with Quill editor
- ✅ Role-based authorization (Customer, Pilot, Administrator)
- ✅ Angular frontend with extensive features (booking, assignments, reviews, admin panel)
- ✅ Unit tests across API layers (~15-25% coverage)
- ✅ Seed data system with pilots and sites

**Next Steps:**

- Add security vulnerabilities for demonstration purposes
- Move secrets from appsettings.json to User Secrets
- Document security exercises and attack scenarios

## Application Purpose

The application starts as a "normal" secure application with proper security controls. Insecure components will be added later for demonstration purposes to illustrate vulnerabilities, attack vectors, and appropriate mitigations.

## Terminology Guide

This application uses specific terminology. For AI assistants and developers:

- **Delivery** = The primary entity representing a cargo shipment booking
- **Assignment** = A delivery from the pilot's perspective (same entity, different view)
- **Site** = Cargo location (origin or destination) - a fully managed entity with CRUD
- **Review** = Customer rating/review of a pilot after delivery completion
- **Location** = Simplified 2D grid coordinates (X, Y as bytes 0-255, not lat/long)

## Business Domain

**Industry:** Cargo Air Transport  
**Primary Function:** Online booking and tracking of cargo shipments

### Key User Stories

1. **Ship Cargo (Customer)** ✅ Implemented

   - User creates an account
   - Selects origin and destination sites
   - Chooses shipping date
   - Selects a pilot
   - Enters cargo details (weight, dimensions)
   - Completes payment with credit card
   - Views booking confirmation

2. **View Assignments (Pilot)** ✅ Implemented

   - Pilot logs in
   - Views assigned deliveries (assignments)
   - Sees delivery details and route
   - Updates delivery status
   - Uploads proof of delivery image

3. **Create Review (Customer)** ✅ Implemented

   - Customer logs in
   - Locates completed delivery
   - Creates review with:
     - Star rating (1-5 stars via Rating value object)
     - Rich text description using Quill editor
   - Edits or deletes review

4. **Cancel Shipment (Administrator)** ✅ Implemented

   - Admin logs in
   - Locates existing delivery
   - Cancels the delivery
   - Issues refund via payment processor

5. **Manage Sites (Administrator)** ✅ Implemented
   - Admin creates/edits cargo sites
   - Sets site status (Upcoming, Current, Inactive)
   - Uploads site images
   - Manages site locations on 2D grid

## User Roles

- **Customer:** Books deliveries, pays, creates reviews, views delivery history
- **Pilot:** Views assignments, updates delivery status, uploads proof of delivery
- **Administrator:** Manages deliveries, sites, issues refunds, manages users

## Technology Stack

### Backend API

- **.NET 9** with C# 13
- **Minimal APIs** serving JSON responses
- **ASP.NET Core Identity** for user management and password hashing
- **Entity Framework Core** for data access with navigation properties
- **SQL Server** as the database (with InMemory provider option for development)
- **Vogen** for strongly-typed value objects (Rating)
- **Scalar** for API documentation and exploration (replaces Swagger UI)
- **Testing:** xUnit, Moq (unit tests, ~15-25% coverage achieved)

### Frontend Web Application

- **Angular 20** with standalone components (no NgModules)
- **Angular Material** for UI components
- **RxJS** for reactive state management in services
- **Zod** for runtime type validation and schema definitions
- **Quill** via `ngx-quill` for rich text editor (reviews)
- **TypeScript** with strict mode enabled
- **npm** for package management

### Third-Party Services

- **Mock Payment Processor:** In-process library (`MBC.Payments.Client`) with fake credit card processing
  - Accepts cards starting with "12" for successful payments
  - Cards starting with "13" = insufficient funds
  - Cards starting with "14" = invalid card
  - CVC "999" = security code mismatch
- **Mock Email Service:** ⏳ Planned - will log email content to console via ILogger

## Architecture Decisions

### API Architecture

**Style:** RESTful JSON APIs using Minimal Endpoints  
**Versioning:** URL path versioning (`/api/v1/...`) - hardcoded to v1  
**Error Handling:** RFC 7807 Problem Details for all error responses  
**Logging:** Built-in ILogger (to console/file) for demonstrating log security issues

### Authentication & Authorization

**Authentication Methods:**

1. **Username/Password:** ASP.NET Core Identity with secure password hashing
2. **OAuth 2.0 (Google):** Backend-initiated Authorization Code flow with PKCE
   - Uses `Microsoft.AspNetCore.Authentication.Google`
   - Backend handles OAuth redirect flow
   - Keeps client secrets server-side

**Token Strategy:**

- **JWT tokens** for API authentication
- **Access tokens** (short-lived) + **Refresh tokens** (long-lived)
- Tokens stored client-side (enables token hijacking demonstrations)
- Tokens sent via Authorization header: `Bearer <token>`

**Authorization Model:**

- Simple role-based authorization using `[Authorize(Roles = "...")]`
- Three roles: Customer, Pilot, Administrator
- Resource ownership validation where appropriate

### Data Layer

**Database:** SQL Server (supports both local installations and Docker containers)  
**ORM:** Entity Framework Core  
**Migrations:** EF Core Migrations with seed data via `migrationBuilder.InsertData`  
**Connection Strings:** Configurable per environment (supports LocalDB, Docker SQL Server, remote instances)

**Data Access Strategy:**

The application uses Entity Framework Core for all data access:

- **ASP.NET Core Identity** for user management (users, roles, tokens, claims)
- **EF Core DbContext** for all business entities (deliveries, pilots, customers, reviews, payments)
- **Navigation properties** to represent relationships between entities
- **Repository pattern** via `IStore<TId, TEntity>` interfaces for data access abstraction
- **Unit of Work pattern** via DbContext for transaction management

**Justification:**

This conventional approach is ideal for a security seminar:

- Single, well-known ORM that participants are likely familiar with
- Navigation properties make entity relationships explicit and easier to understand
- Reduces cognitive overhead - participants can focus on security concerns, not data access patterns
- Still demonstrates all relevant vulnerabilities (SQL injection, IDOR, mass assignment, etc.)
- Change tracking and Unit of Work are useful for demonstrating transaction vulnerabilities
- Less boilerplate code compared to hand-written SQL

### Security Architecture

**CORS:**

- Start permissive (allow broad origins)
- Lock down progressively (demonstrates security hardening)
- Supports credentials for JWT cookies/headers

**Rate Limiting:**

- Built-in .NET rate limiting middleware
- Applied to sensitive endpoints (login, registration, password reset)
- Demonstrates brute force attack mitigation

**Input Validation:**

- Data Annotations on DTOs and models
- Demonstrates common validation failures

**Content Security:**

- Rich text reviews stored as HTML (from Quill editor)
- Provides opportunities to demonstrate XSS vulnerabilities and sanitization

**File Upload Security:**

- Proof of delivery images stored on local filesystem
- Demonstrates path traversal, unrestricted upload, file type validation vulnerabilities

### Configuration & Secrets Management

**Current State:** 🔓 Intentionally insecure for demonstration

**Backend (.NET):**

- `appsettings.json` contains ALL configuration (including secrets) - **currently for convenience**
- ⏳ **Planned:** Move to User Secrets for development as a security exercise
- Environment variables for production deployment
- Demonstrates progression from insecure to secure practices

**Frontend (Angular):**

- `environment.ts` for API base URL configuration
- No sensitive data stored in frontend configuration

**Current Configuration in appsettings.json:**

- Database connection string (defaults to InMemory provider)
- JWT secret key (development key visible in source)
- Google OAuth client ID/secret (placeholder values)
- Image upload settings (max size, allowed types, upload directory)
- Cost calculation parameters (base rates, rush fees)

**⏳ Planned Migration Path:**

1. Move JWT secret to User Secrets
2. Move Google OAuth credentials to User Secrets
3. Add environment variable support for production
4. Document secrets management as security best practice

### File Storage

**Strategy:** Local filesystem storage at `assets/uploads/`  
**Implementation Status:** ✅ Implemented

**Use Cases:**

- Proof of delivery images (uploaded by pilots) ✅
- Site images (uploaded by administrators) ✅
- Future: pilot profile pictures, cargo manifests

**Current Configuration:**

- Max file size: 1MB (configurable)
- Allowed extensions: `.jpg`, `.jpeg`, `.png`, `.webp`
- Allowed MIME types: `image/jpeg`, `image/png`, `image/webp`
- Static file serving via ASP.NET Core middleware at `/uploads` path

**Security Considerations (for future demonstrations):**

- Path traversal vulnerabilities
- Unrestricted file upload demonstrations
- File type validation bypass
- Direct file access vulnerabilities
- Missing authorization checks on file access

### Email & Notifications

**Implementation:** Mock email service  
**Behavior:** Logs complete email content and URLs to console via ILogger  
**Use Cases:**

- Password reset links
- Booking confirmations
- Shipment status updates
- Email verification on registration

**Security Demonstrations:**

- Email enumeration attacks
- Insecure password reset tokens
- Copy/paste URLs from logs to simulate clicking email links

## Project Structure

```
$root$
├── .gitignore
├── docs/
│   ├── blueprint.md               # This document
│   ├── AUTHENTICATION_SETUP.md    # Auth setup guide
│   └── backlog.md                 # Project backlog
├── scripts/
│   └── run-watch.ps1              # Run both API and web in watch mode
└── projects/
    ├── api/                       # .NET backend
    │   ├── MBC.sln                # Solution file at api root
    │   ├── src/
    │   │   ├── Directory.Build.props
    │   │   ├── MBC.Core/          # Domain entities, interfaces, models
    │   │   │   ├── Entities/      # Customer, Delivery, Pilot, Site, etc.
    │   │   │   ├── Models/        # DTOs, requests, responses
    │   │   │   ├── Services/      # Service interfaces
    │   │   │   ├── Persistence/   # Repository interfaces (IStore<T>)
    │   │   │   ├── ValueObjects/  # Rating (Vogen value object)
    │   │   │   └── Authorization/ # ReviewOperations for authorization
    │   │   ├── MBC.Endpoints/     # Minimal API endpoints, DTOs, mappers
    │   │   │   ├── Endpoints/     # Auth, Booking, Pilot, Review, Site, etc.
    │   │   │   ├── Dtos/          # API data transfer objects
    │   │   │   ├── Mapping/       # Entity ↔ DTO mappers
    │   │   │   ├── Middleware/    # Global exception handler
    │   │   │   └── assets/uploads/# Uploaded images
    │   │   ├── MBC.Persistence/   # EF Core implementation
    │   │   │   ├── MBCDbContext.cs
    │   │   │   ├── Stores/        # Repository implementations
    │   │   │   ├── Migrations/    # EF Core migrations
    │   │   │   └── Services/      # DbMigrationService
    │   │   ├── MBC.Services/      # Business logic implementations
    │   │   │   ├── BookingService.cs
    │   │   │   ├── ReviewService.cs
    │   │   │   ├── SiteService.cs
    │   │   │   ├── AuthService.cs
    │   │   │   ├── SeedData/      # Pilot and site seed data
    │   │   │   └── Authorization/ # Authorization handlers
    │   │   └── MBC.Payments.Client/ # Mock payment processor library
    │   └── tests/
    │       ├── MBC.Core.Tests/
    │       ├── MBC.Endpoints.Tests/
    │       ├── MBC.Persistence.Tests/
    │       └── MBC.Services.Tests/
    └── web/                       # Angular 19 frontend
        ├── package.json
        ├── angular.json
        └── src/
            ├── app/
            │   ├── core/          # API client, auth services, interceptors
            │   ├── features/      # Feature modules (auth, booking, pilots, etc.)
            │   │   ├── auth/      # Sign in, sign up
            │   │   ├── booking/   # New booking flow
            │   │   ├── deliveries/ # Customer delivery list/detail
            │   │   ├── assignments/ # Pilot assignment list/detail
            │   │   ├── pilots/    # Pilot list, detail, picker
            │   │   ├── sites/     # Site list, detail, editor, image upload
            │   │   ├── review/    # Review creation, editing, display
            │   │   └── admin/     # Admin panel
            │   └── shared/        # Shared components, schemas (Zod)
            ├── environments/      # Environment configuration
            └── styles.scss        # Global Material theme
```

## Development Environment

**Supported Options:**

1. **Local development** (Windows/Mac/Linux with .NET 9, Node.js, SQL Server or LocalDB)
2. **Docker via VS Code** (using `.devcontainer`)
3. **GitHub Codespaces** (full cloud-based development)

**Protocol:** HTTP in development (simplifies traffic inspection with Fiddler/Burp Suite)

**Prerequisites:**

- .NET 9 SDK
- Node.js 18+ and npm
- SQL Server (LocalDB on Windows, Docker container, or remote instance)
  - **Note:** Currently configured to use InMemory provider by default
- Docker (optional, for containerized development)

### Quick Start

**Run both API and web app (recommended):**

```powershell
.\scripts\run-watch.ps1
```

This PowerShell script starts both services in watch mode:

- API runs at `http://localhost:5000` (or as configured)
- Web runs at `http://localhost:4200`
- Both auto-reload on file changes

**API only:**

```powershell
cd projects/api/src/MBC.Endpoints
dotnet run
```

Access Scalar API documentation at `http://localhost:5000/scalar/v1`

**Web only:**

```bash
cd projects/web
npm start
```

### Database Setup

**Current configuration:** InMemory provider (no database required)

To switch to SQL Server:

1. Update `appsettings.json`:

   ```json
   "Database": {
     "Provider": "SqlServer",
     "ConnectionString": "Server=(localdb)\\mssqllocaldb;Database=MangoBayCargo;..."
   }
   ```

2. Run migrations:
   ```powershell
   cd projects/api
   dotnet ef database update --project src/MBC.Persistence
   ```

See `projects/api/src/MBC.Persistence/README.md` for detailed migration instructions.

## API Structure

### Endpoint Organization

All endpoints under `/api/v1/` prefix. Explore full API documentation at `http://localhost:5000/scalar/v1`

**Implemented Endpoint Groups:**

- **`/api/v1/auth`** - Authentication & authorization ✅

  - `POST /signup` - Create customer account
  - `POST /signin` - Email/password login
  - `POST /refresh` - Refresh access token
  - `POST /signout` - Invalidate refresh token
  - `GET /google` - Initiate Google OAuth
  - `GET /google/callback` - OAuth callback handler

- **`/api/v1/bookings`** - Delivery management ✅

  - `POST /` - Book new delivery
  - `GET /{id}` - Get delivery by ID
  - `GET /my-deliveries` - Get deliveries for current customer (paginated)
  - `GET /my-assignments` - Get assignments for current pilot (paginated)
  - `GET /pilot/{pilotId}` - Get deliveries by pilot (paginated)
  - `DELETE /{id}` - Cancel delivery and refund
  - `PUT /{id}/status` - Update delivery status
  - `POST /{deliveryId}/proof-of-delivery` - Upload proof image
  - `POST /calculate-cost` - Calculate cost estimate

- **`/api/v1/pilots`** - Pilot information ✅

  - `GET /` - Get all pilots (paginated)
  - `GET /{id}` - Get pilot by ID
  - `GET /{pilotId}/reviews` - Get reviews for pilot (paginated)

- **`/api/v1/sites`** - Site management ✅

  - `GET /` - Get all sites (paginated, filterable by status)
  - `GET /{id}` - Get site by ID
  - `POST /` - Create site (admin only)
  - `PUT /{id}` - Update site (admin only)
  - `DELETE /{id}` - Delete site (admin only)
  - `POST /{siteId}/image` - Upload site image (admin only)

- **`/api/v1/reviews`** - Review management ✅

  - `POST /` - Create review
  - `GET /{id}` - Get review by ID
  - `GET /delivery/{deliveryId}` - Get review for delivery
  - `PUT /{reviewId}` - Update review
  - `DELETE /{reviewId}` - Delete review

- **`/api/v1/customers`** - Customer management ✅

  - `POST /` - Create customer profile
  - `GET /me` - Get current customer profile
  - `GET /{id}` - Get customer by ID

- **`/api/v1/payments`** - Payment operations ✅
  - `POST /refund` - Process refund (admin only)

### Response Formats

**Success Responses:**

- Plain JSON objects/arrays
- Appropriate HTTP status codes (200, 201, 204)

**Error Responses:**

- RFC 7807 Problem Details format
- Structured error information
- Appropriate HTTP status codes (400, 401, 403, 404, 500)

## Frontend Architecture

### Component Structure

- **Standalone components** (no NgModules)
- Feature-based organization
- Shared components for reusability

### State Management

- Services with RxJS observables
- No global state management library (NgRx, etc.)
- Clear separation between stateless and stateful services

#### Service Architecture Pattern

**Service Types:**

1. **Data Services** (stateless): Pure data access with added value

   - Only created when they provide value beyond `ApiClient`
   - Examples: caching, data transformation, business logic, complex API interactions
   - Naming: `*ApiService` (e.g., `PilotsApiService`)

2. **State Services** (stateful): Manage component state
   - Handle loading, loaded, and error states using `Loadable<T>` pattern
   - Call `ApiClient` directly (no unnecessary indirection)
   - Expose observables for components to bind to
   - Expose void methods for components to call
   - Naming: `*StateService` (e.g., `PilotDetailStateService`)

**Component Pattern:**

- Components are purely presentational (no state)
- Components bind to observables from their state service
- Components call void methods on their state service
- Components reference maximum 1 stateful service
- All business logic lives in services, not components

**Loadable State Pattern:**

```typescript
type LoadableStatus = "loading" | "loaded" | "error";
type Loadable<T> =
  | { status: "loading" }
  | { status: "loaded"; value: T }
  | { status: "error"; error: any };
```

**Reusable Stream Creation:**

```typescript
function loadable<T>(apiCall: () => Observable<T>): Observable<Loadable<T>>;

function loadableWithId<T>(
  id$: Observable<string | null>,
  apiCall: (id: string) => Observable<T>
): Observable<Loadable<T>>;

function loadableWithParams<T, P extends any[]>(
  params$: Observable<P | null>,
  apiCall: (...params: P) => Observable<T>
): Observable<Loadable<T>>;
```

Three clear functions handle different parameter patterns:

- **No parameters**: `loadable()` - executes immediately
- **Single ID parameter**: `loadableWithId()` - handles string ID observables
- **Multiple parameters**: `loadableWithParams()` - handles array parameter observables

**Usage Examples:**

```typescript
// No parameters - executes immediately
loadable(() => client.getAllPilots());

// Single ID parameter
loadableWithId(this.pilotIdSubject.asObservable(), (id) =>
  client.getPilotById(id)
);

// Multiple parameters
loadableWithParams(this.pageParamsSubject.asObservable(), (skip, take) =>
  client.getPilots(skip, take)
);
```

This function encapsulates the common pattern of:

1. Filtering out null values (where applicable)
2. Switching to API call
3. Mapping success to `Loadable.loaded`
4. Catching errors and mapping to `Loadable.error`
5. Starting with `Loadable.loading`

This pattern provides consistent loading states across all components and enables clean error handling while eliminating repetitive code.

### Routing & Navigation

- Angular Router with route guards
- Lazy loading for feature areas
- Authentication-based route protection

### HTTP & API Communication

- Angular HttpClient
- Interceptors for:
  - Adding JWT tokens to requests
  - Global error handling
  - Logging (for demonstration)

## Data Model

### Core Entities

All entities are in the `MBC.Core.Entities` namespace.

**MBCUser** (ASP.NET Core Identity)

- Identity-managed fields (email, username, password hash)
- Role (Customer, Pilot, Administrator) via Identity roles
- Extended by Customer and Pilot entities

**Customer**

- `Id` (Guid)
- `Nickname` (string) - Display name
- `UserId` (Guid) - Reference to Identity user

**Pilot**

- `Id` (Guid)
- `FullName` (string)
- `ShortName` (string) - Display name
- `AvatarUrl` (string?) - URL to avatar image
- `Bio` (string) - Biography/description
- `UserId` (Guid) - Reference to Identity user

**Delivery** (the primary entity, also called "booking" in API routes)

- `Id` (Guid)
- `CustomerId` (Guid) - Foreign key to Customer
- `PilotId` (Guid) - Foreign key to Pilot
- `PaymentId` (Guid) - Foreign key to Payment
- `Details` (JobDetails) - Owned entity with origin/destination site IDs, cargo weight, scheduled date
- `CompletedOn` (DateTime?) - Completion timestamp
- `Status` (DeliveryStatus enum) - Pending, InTransit, Delivered, Cancelled
- `ProofOfDeliveryPath` (string?) - File path to uploaded image
- `CreatedAt` (DateTime)
- `UpdatedAt` (DateTime)
- Navigation properties: Customer, Pilot, Origin (Site), Destination (Site), Payment, Review

**DeliveryReview**

- `Id` (Guid)
- `PilotId` (Guid) - Foreign key to Pilot being reviewed
- `CustomerId` (Guid) - Foreign key to Customer author
- `DeliveryId` (Guid) - Foreign key to associated Delivery
- `Rating` (Rating) - Value object (1-5 stars, Vogen type)
- `Notes` (string) - Rich text HTML from Quill editor
- `CreatedAt` (DateTime)
- Navigation properties: Pilot, Customer, Delivery

**Site** (origin/destination locations)

- `Id` (Guid)
- `Name` (string) - Site name
- `Notes` (string) - Description
- `Island` (string) - Island name
- `Address` (string) - Street address
- `Location` (Location) - Owned entity with X/Y grid coordinates
- `Status` (SiteStatus enum) - Upcoming, Current, Inactive
- `ImageUrl` (string?) - URL to site image

**Location** (owned by Site)

- `X` (byte) - Grid X coordinate (0-255)
- `Y` (byte) - Grid Y coordinate (0-255)

**Payment**

- `Id` (Guid)
- `Amount` (decimal)
- `Status` (PaymentStatus enum) - Pending, Completed, Refunded, Failed
- `TransactionId` (string) - From payment processor
- `CreditCard` (CreditCardInfo) - Owned entity with card details
- `CreatedAt` (DateTime)

**RefreshToken**

- `Id` (Guid)
- `UserId` (Guid) - Reference to Identity user
- `Token` (string) - Token value
- `ExpiresAt` (DateTime)
- `CreatedAt` (DateTime)

### Value Objects

**Rating** (using Vogen)

- Strongly-typed value object for 1-5 star ratings
- Enforces validation at compile time
- Prevents invalid ratings

### Enums

- `DeliveryStatus`: Pending, InTransit, Delivered, Cancelled
- `PaymentStatus`: Pending, Completed, Refunded, Failed
- `SiteStatus`: Upcoming, Current, Inactive

## Working with This Codebase

### Key Patterns & Conventions

This section provides context for AI assistants and developers to understand common patterns in the codebase.

**Backend Patterns:**

1. **Entity ↔ DTO Mapping**

   - Entities live in `MBC.Core.Entities`
   - DTOs live in `MBC.Endpoints.Dtos`
   - Mappers implement `IMapper<TSource, TDestination>` in `MBC.Endpoints.Mapping`
   - Example: `DeliveryMapper` converts `Delivery` → `DeliveryDto`

2. **Repository Pattern**

   - Interfaces in `MBC.Core.Persistence` (e.g., `IDeliveryStore`)
   - Base interface: `IStore<TId, TEntity>` provides `GetById`, `GetPage`
   - Implementations in `MBC.Persistence.Stores` using EF Core
   - Example: `DeliveryStore` implements `IDeliveryStore`

3. **Service Layer**

   - Interfaces in `MBC.Core.Services` (e.g., `IBookingService`)
   - Implementations in `MBC.Services` (e.g., `BookingService`)
   - Services coordinate between repositories and contain business logic
   - Services handle authorization checks

4. **Authorization**

   - Role-based: Customer, Pilot, Administrator (see `UserRoles.cs`)
   - Resource-based: Authorization handlers in `MBC.Services.Authorization`
   - Example: `ReviewAuthorizationHandler` checks if user owns a review
   - Operations defined in `MBC.Core.Authorization` (e.g., `ReviewOperations`)

5. **Seed Data**
   - Seed data in `MBC.Services.SeedData` (Pilots.cs, Sites.cs)
   - Loaded via `SeedService` at startup
   - Uses deterministic GUIDs for consistency

**Frontend Patterns:**

1. **Loadable State Pattern**

   - All async data uses `Loadable<T>` type
   - States: `loading`, `loaded`, `error`
   - Helper functions: `loadable()`, `loadableWithId()`, `loadableWithParams()`
   - Located in `@app/core/loadable.ts`

2. **State Services**

   - Named `*StateService` (e.g., `PilotDetailStateService`)
   - Manage component state with RxJS
   - Expose observables for components to bind
   - Expose void methods for user actions
   - Components reference maximum 1 state service

3. **API Client**

   - Centralized in `ApiClientService` in `@app/core/client`
   - All HTTP calls go through this service
   - State services call `ApiClient` directly (no intermediate services unless adding value)

4. **Zod Schemas**

   - All API types defined as Zod schemas in `@app/shared/schemas.ts`
   - Runtime validation of API responses
   - TypeScript types inferred from schemas
   - Example: `DeliverySchema`, `PilotSchema`, `SiteSchema`

5. **Routing**
   - Lazy-loaded routes in `app.routes.ts`
   - No route guards yet (planned for auth)
   - Feature-based organization

**Common File Locations:**

- Entity definitions: `projects/api/src/MBC.Core/Entities/`
- API endpoints: `projects/api/src/MBC.Endpoints/Endpoints/`
- Business logic: `projects/api/src/MBC.Services/`
- Repository implementations: `projects/api/src/MBC.Persistence/Stores/`
- Angular features: `projects/web/src/app/features/`
- Shared Angular code: `projects/web/src/app/shared/`
- Zod schemas: `projects/web/src/app/shared/schemas.ts`

**README Files:**

Almost every folder has a README.md explaining its purpose, patterns, and usage. Check these for detailed context:

- `MBC.Core/README.md` - Core domain architecture
- `MBC.Core/Entities/README.md` - Entity patterns
- `MBC.Core/Models/README.md` - Model categories
- `MBC.Core/Persistence/README.md` - Repository pattern
- `MBC.Core/Services/README.md` - Service layer contracts
- `MBC.Persistence/README.md` - Database migration guide

## Testing Strategy

**Scope:** Unit tests only (no integration tests)  
**Coverage Goal:** 15-25%  
**Framework:** xUnit with Moq for mocking

**Current Test Coverage:**

- ✅ `MBC.Core.Tests` - Model tests (e.g., Page<T>)
- ✅ `MBC.Endpoints.Tests` - Endpoint tests with mocked services
- ✅ `MBC.Persistence.Tests` - Store tests with in-memory DB
- ✅ `MBC.Services.Tests` - Service layer business logic tests

**Test Focus Areas:**

- Service layer business logic (BookingService, ReviewService)
- Authentication/authorization logic
- Validation logic
- Sample tests demonstrating patterns
- Global exception handler

## Security Features & Vulnerabilities

### Initial "Secure" Implementation

The application will initially demonstrate security best practices:

- Proper authentication and authorization
- Input validation and sanitization
- Parameterized queries (via EF Core)
- Secure password storage (Identity)
- HTTPS redirects (in production)
- Rate limiting on sensitive endpoints
- JWT with secure signing

### Planned Vulnerabilities (for demonstration)

To be added in later phases for security training:

- **XSS:** Unsanitized rich text reviews
- **SQL Injection:** Raw SQL queries in specific endpoints
- **IDOR:** Missing authorization checks on resource access
- **Token Hijacking:** Insecure token storage demonstrations
- **Path Traversal:** File upload/download vulnerabilities
- **CSRF:** Missing anti-forgery tokens
- **Information Disclosure:** Verbose error messages, stack traces in responses
- **Broken Authentication:** Weak password policies, missing MFA
- **Mass Assignment:** Over-posting vulnerabilities
- **Log Injection:** Unsanitized input in log statements
- **Email Enumeration:** User existence leakage
- **OAuth Vulnerabilities:** Missing/weak state parameter, redirect URI validation issues

## Deployment Considerations

**Target Environments:**

- Local development machines
- Docker containers (via docker-compose)
- GitHub Codespaces
- (Future) Azure App Service or similar cloud platform

**Configuration per Environment:**

- Database connection strings
- JWT signing keys
- OAuth client credentials
- CORS origins
- Rate limiting policies
- Logging levels

## Future Enhancements & Planned Features

### Security Demonstrations (High Priority)

- Implement intentional vulnerabilities for security training:
  - XSS via unsanitized review content
  - SQL Injection demonstrations
  - IDOR (Insecure Direct Object Reference) examples
  - Path traversal in file uploads
  - CSRF vulnerabilities
  - Information disclosure (verbose errors, stack traces)
  - Mass assignment over-posting
  - Log injection
  - Email enumeration
  - OAuth vulnerabilities

### Infrastructure & DevOps

- Move secrets to User Secrets and environment variables
- Add rate limiting middleware on auth endpoints
- Implement mock email service (ILogger-based)
- Docker Compose for full local environment
- CI/CD pipeline examples

### Testing & Quality

- Increase test coverage toward 25%
- Add integration tests (if beneficial for demos)
- Performance testing scenarios
- Security testing examples

### Additional Features (Lower Priority)

- Real-time delivery tracking (SignalR)
- Advanced authorization (resource-based policies)
- WebSocket security demonstrations
- Cloud storage for file uploads (Azure Blob, AWS S3)
- Pilot profile pictures
- Cargo manifests and documents
- Review moderation workflow
- Advanced email functionality (real SMTP)
- Mobile application (for mobile security topics)
- Admin dashboard enhancements
- Reporting and analytics

## Success Criteria

The application successfully serves its purpose as a security seminar platform when it:

1. Provides working implementations of common web application features
2. Demonstrates both secure and vulnerable code patterns
3. Is easy for participants to set up and run locally or in containers
4. Offers clear attack surfaces for security exercises
5. Shows realistic mitigation strategies for each vulnerability class
6. Maintains clean, readable code that's easy to understand and modify

---

**Document Version:** 2.0  
**Last Updated:** October 12, 2025  
**Status:** Active - Core features implemented, ready for security demonstrations
