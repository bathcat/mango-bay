# Mango Bay Cargo - Web Application

Angular frontend for the Mango Bay Cargo security seminar application.

## Quick Start

### Run both API and Web (recommended)

From the project root:

```powershell
.\scripts\run-watch.ps1
```

This will start both the .NET API and Angular web app in watch mode.

### Run Web only

From this directory:

```bash
npm start
```

The app will be available at `http://localhost:4200`

## Architecture

- **Angular 19** with standalone components
- **Angular Material** for UI components
- **RxJS** for reactive state management
- **Zod** for runtime type validation
- **TypeScript** with strict mode

## Project Structure

```
src/
├── app/
│   ├── core/
│   │   ├── config/          # API configuration
│   │   └── services/        # Core services (pilot service)
│   ├── features/
│   │   └── pilots/          # Pilot feature components
│   └── shared/
│       └── models/          # Shared types and Zod schemas
├── environments/            # Environment configuration
└── styles.scss             # Global Material theme
```

## Features

- Paginated pilot list with Material table
- Loading states and error handling
- Responsive design
- Type-safe API responses with Zod validation
