# Securing the Whole Stack: .NET + Angular Security Workshop

A 2-day hands-on workshop where you'll exploit vulnerabilities in a real application, then fix them using modern security practices.

## What You'll Be Able To Do

1. Configure .NET authentication and authorization with JWT and OAuth
2. Prevent code injection attacks (XSS, SQL injection) in full-stack applications
3. Implement secure file uploads and defend against path traversal attacks
4. Scan and remediate vulnerable dependencies in .NET and Angular projects

## The Application

You'll work with **Mango Bay Cargo** - a complete cargo booking and tracking platform built with .NET 9 and Angular 20. It includes authentication, payments, file uploads, reviews, and role-based authorization. The application starts secure, then you'll add vulnerabilities to understand attacks, and finally implement proper mitigations.

## Topics Covered

| Attack Vector              | What You'll Learn                                   | Technologies                        |
| -------------------------- | --------------------------------------------------- | ----------------------------------- |
| **Broken Authentication**  | JWT tokens, refresh strategies, OAuth 2.0 flows     | ASP.NET Identity, Google OAuth      |
| **Broken Authorization**   | RBAC, resource-based auth, IDOR vulnerabilities     | .NET authorization policies         |
| **Injection Attacks**      | XSS prevention, SQL injection, input validation     | Content Security Policy, EF Core    |
| **Insecure File Handling** | Path traversal, upload validation, MIME type checks | ASP.NET middleware                  |
| **Mass Assignment**        | Over-posting vulnerabilities, DTO patterns          | Data binding controls               |
| **Information Disclosure** | Log injection, error handling, email enumeration    | Structured logging, Problem Details |
| **Denial of Service**      | Rate limiting, brute force protection               | .NET rate limiting middleware       |
| **Supply Chain**           | Vulnerable dependencies, poisoned packages          | NuGet/NPM security scanning         |

## Prerequisites

- .NET 9 SDK
- Node.js 18+
- Git
- SQL Server (optional - only needed for SQL injection exercises)
- Editor: VS Code or Visual Studio 2022 recommended

## Setup

Choose your preferred environment:

- **GitHub Codespaces** - Zero installation, runs in browser
- **Docker + VS Code** - Consistent environment, local artifacts
- **Local Installation** - Fastest, most control

See [SETUP.md](SETUP.md) for detailed instructions.

## Quick Start

Once set up, run both API and web app:

```powershell
.\scripts\run-watch.ps1
```

- API: http://localhost:5110
- Web: http://localhost:4200
- API Docs: http://localhost:5110/scalar/v1
