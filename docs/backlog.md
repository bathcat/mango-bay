# Mango Bay Cargo - Project Backlog

## Overview

This backlog tracks tasks, features, and improvements for the Mango Bay Cargo security seminar platform. Items are organized by priority and category to help manage development phases and ensure nothing important is forgotten.

## Backlog Categories

- **Documentation** - README files, architectural docs, setup guides
- **Core Features** - Essential functionality for the seminar platform
- **Security Demonstrations** - Vulnerabilities and mitigations for training
- **Infrastructure** - Development environment, deployment, tooling
- **Testing** - Unit tests, test coverage, quality assurance
- **Polish** - UI/UX improvements, performance optimizations

---

### Documentation

- [ ] **Create README.md for (almost) every folder**

  - Explain what's going on in this folder
  - Document patterns
  - Include examples
  - Include links to Microsoft documentation / Martin Fowler stuff
  - **Status:** Pending (waiting for project to stabilize)

---

### Security Demonstrations

- [ ] **Implement Client-Side Google Identity Services (OIDC)**

  - Replace server-side OAuth redirect flow with client-side Google Identity Services
  - Frontend: Integrate Google Identity Services JS library
  - Backend: Create POST endpoint to verify Google ID Token and issue native JWT tokens
  - Demonstrates secure token handling (tokens in POST body vs URL parameters)
  - Avoids token exposure in browser history, logs, and Referer headers
  - Better UX with client-controlled authentication flow
  - **Reference:** OpenID Connect with Google Sign-In
  - **Status:** Pending (current server-side OAuth will be removed first)
