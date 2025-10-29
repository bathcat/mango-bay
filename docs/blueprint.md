# Mango Bay Cargo - Technical Blueprint

## Quick Overview

**Purpose:** Security seminar platform demonstrating secure web app development with Angular + .NET

**Domain:** Cargo shipment booking and tracking (inspired by TaleSpin airline)

**Current State:** Core features complete, ready for security demonstrations

**Key Entities:** Customer, Pilot, Delivery, Site, Review, Payment, DeliveryProof

**User Roles:** Customer (books/pays/reviews), Pilot (assignments/proof-of-delivery), Admin (management)

## Technology Stack

- **Backend:** .NET 9, C# 13, Minimal APIs, EF Core, SQL Server/Sqlite
- **Frontend:** Angular 20, TypeScript, Material UI, RxJS, Zod validation
- **Security:** JWT, Role-based auth, Rate limiting
- **Tools:** Scalar (API docs), Vogen (value objects), xUnit/Moq (testing)

## Current Implementation Status

### ✅ Core Features Implemented

**Authentication & Users:**

- JWT access/refresh tokens
- Role-based auth (Customer, Pilot, Administrator)
- ASP.NET Core Identity with secure password hashing

**Business Logic:**

- Delivery booking with cost calculation
- Pilot assignments and status updates
- Customer reviews with rich text (Quill editor)
- Site management with image uploads
- Payment processing (mock processor)
- Proof of delivery image uploads

**Frontend Features:**

- Responsive Material UI
- Lazy-loaded feature modules
- Reactive state management with RxJS
- Route guards and auth interceptors
- Zod runtime validation
- Error handling and loading states

### 🔄 API Endpoints (`/api/v1/`)

| Endpoint Group | Key Operations                                     |
| -------------- | -------------------------------------------------- |
| `/auth`        | signup, signin, refresh, signout                   |
| `/deliveries`  | CRUD deliveries, status updates, cost calc, search |
| `/pilots`      | List pilots, get by ID, get reviews                |
| `/sites`       | CRUD sites, image uploads                          |
| `/reviews`     | CRUD reviews                                       |
| `/customers`   | Get/update customer profile                        |
| `/proofs`      | Upload/retrieve proof-of-delivery images           |
| `/payments`    | Process refunds                                    |

### 📱 Frontend Routes

| Route            | Access   | Purpose                              |
| ---------------- | -------- | ------------------------------------ |
| `/`              | Public   | Home, pilot/site listings            |
| `/auth/*`        | Public   | Sign in/up                           |
| `/booking`       | Customer | New delivery booking                 |
| `/deliveries/*`  | Customer | Delivery list/detail, reviews        |
| `/assignments/*` | Pilot    | Assignment list/detail, proof upload |
| `/admin`         | Admin    | Site/delivery management             |
| `/search`        | Public   | Mock search functionality            |

## Architecture Patterns

### Backend (.NET)

**Entity ↔ DTO Mapping:** Clean separation with dedicated mapper classes
**Repository Pattern:** `IStore<TId, TEntity>` interfaces with EF Core implementations
**Service Layer:** Business logic with authorization checks
**Authorization:** Advanced stakeholder-based permissions with `AuthorizedFor` flags

**Key Interfaces:**

- `IStore<TId, TEntity>` - CRUD operations
- `IService` - Business logic contracts
- `IMapper<TSource, TDestination>` - Entity/DTO conversion

### Frontend (Angular)

**Loadable Pattern:** Consistent async state management (`loading` | `loaded` | `error`)
**State Services:** RxJS-based state management (one per component)
**API Client:** Centralized HTTP communication with interceptors
**Zod Schemas:** Runtime type validation and TypeScript inference

**Key Patterns:**

- `loadable<T>()` - Execute API call with loading states
- `loadableWithId<T>()` - Handle ID-based API calls
- `*StateService` - Component state management
- `*ApiService` - Data transformation (rare, prefer direct client calls)

## Business Domain

**Industry:** Cargo Air Transport (inspired by TaleSpin)
**Primary Function:** Online booking and tracking of cargo shipments

### Key User Stories

1. **Ship Cargo (Customer)** - Book deliveries, pay, create reviews
2. **View Assignments (Pilot)** - Manage assigned deliveries, upload proof-of-delivery
3. **Manage Sites (Admin)** - CRUD sites, upload images, manage locations

### Terminology

- **Delivery** = Primary entity (cargo shipment booking)
- **Assignment** = Delivery from pilot's perspective
- **Site** = Cargo location (origin/destination) with CRUD
- **Review** = Customer rating/review of pilot (rich text)
- **Location** = 2D grid coordinates (X, Y as bytes 0-255)
- **DeliveryProof** = Image proof uploaded by pilots after delivery

## Quick Reference for AI Assistants

### Common Queries & Locations

**"How does authentication work?"**

- Backend: `MBC.Endpoints/Endpoints/AuthEndpoints.cs`, `MBC.Services/Authentication/`
- Frontend: `core/auth.interceptor.ts`, `features/home/auth/`
- JWT settings: `MBC.Core/Models/JwtSettings.cs`

**"Where is the data model/entity definitions?"**

- Entities: `MBC.Core/Entities/` (Customer, Delivery, Pilot, Site, Review, Payment, DeliveryProof)
- DTOs: `MBC.Endpoints/Dtos/`
- Mappers: `MBC.Endpoints/Mapping/`
- DbContext: `MBC.Persistence/MBCDbContext.cs`

**"How are API endpoints organized?"**

- Endpoints: `MBC.Endpoints/Endpoints/` (Auth, Delivery, Pilot, Site, Review, etc.)
- Routes: `MBC.Endpoints/Endpoints/Infrastructure/ApiRoutes.cs`
- Base URL: `/api/v1/` (e.g., `/api/v1/deliveries`)

**"Where is business logic implemented?"**

- Services: `MBC.Services/` (BookingService, ReviewService, SiteService, etc.)
- Interfaces: `MBC.Core/Services/`
- Authorization: `MBC.Services/Authorization/`

**"How does frontend state management work?"**

- State Services: `*StateService` (e.g., `PilotDetailStateService`)
- Loadable Pattern: `@app/shared/loadable/`
- API Client: `@app/core/client/`
- Schemas: `@app/shared/schemas.ts`

**"Where are UI components/features?"**

- Features: `@app/features/` (auth, booking, deliveries, assignments, admin, etc.)
- Shared UI: `@app/shared/ui/`
- Routing: `app.routes.ts`

### Development Setup

**Run both API + Web:**

```powershell
.\scripts\run-watch.ps1
```

- API: `http://localhost:5000` (Scalar docs: `/scalar/v1`)
- Web: `http://localhost:4200`

**Database:**

- Default: InMemory (no setup required)
- SQL Server: Update `appsettings.json`, run `dotnet ef database update`

**Key Files:**

- API Program: `MBC.Endpoints/Program.cs`
- Web App Config: `app.config.ts`
- Environment Config: `environments/`
- Package management: `package.json`, `*.csproj` files

### Security Testing Context

**Current Security State:** Secure implementation (vulnerabilities to be added for demos)

**Mock Payment Rules:**

- ✅ "12" prefix = success
- ❌ "13" prefix = insufficient funds
- ❌ "14" prefix = invalid card
- ❌ CVC "999" = security code mismatch

**Planned Vulnerabilities:** XSS, SQL injection, IDOR, CSRF, mass assignment, etc.

## Data Model Summary

### Core Entities

- **MBCUser** (Identity): Email, password hash, roles
- **Customer/Pilot**: Extend MBCUser with profile data
- **Delivery**: Primary entity (booking/assignment)
- **Site**: Origin/destination locations with grid coordinates
- **Review**: Customer ratings of pilots (rich text)
- **Payment**: Transaction records
- **DeliveryProof**: Image uploads by pilots

### Key Relationships

- Delivery → Customer, Pilot, Origin Site, Destination Site, Payment, Review
- Customer/Pilot → MBCUser (1:1 extension)
- Review → Customer, Pilot, Delivery

## Project Structure

```
projects/
├── api/src/                    # .NET backend
│   ├── MBC.Core/              # Domain models, interfaces
│   ├── MBC.Endpoints/         # Minimal API endpoints
│   ├── MBC.Services/          # Business logic
│   ├── MBC.Persistence/       # EF Core, migrations
│   └── MBC.Payments.Client/   # Mock payment processor
└── web/src/app/               # Angular frontend
    ├── core/                  # Auth, API client, interceptors
    ├── features/              # Feature modules
    └── shared/                # Reusable components, schemas
```

---

**Document Version:** 3.0 (Concise Edition)
**Last Updated:** October 29, 2025
**Status:** Active - Core features implemented, ready for security demonstrations
