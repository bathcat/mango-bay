# Authentication System - Setup & Testing Guide

## Overview

A complete JWT-based authentication system has been implemented with support for:

- Username/password sign up and sign in
- Google OAuth sign in
- JWT access tokens (15 min) + refresh tokens (30 days)
- Token storage in localStorage
- Automatic token refresh via HTTP interceptor

## Backend Setup

### 1. Configuration

The JWT secret and Google OAuth credentials are currently configured in `appsettings.json` for convenience:

- **JWT Secret:** Already configured with a development key
- **Google OAuth:** Placeholder values in `appsettings.json` - replace if you want to test Google sign-in

**Note:** These will be moved to User Secrets in a later exercise as a security best practice.

### 2. (Optional) Google OAuth Setup

If you want to enable Google sign-in, update the values in `appsettings.json`:

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing
3. Enable Google+ API
4. Create OAuth 2.0 credentials
5. Add authorized redirect URI: `http://localhost:5000/api/v1/auth/google/callback` (adjust port as needed)
6. Copy Client ID and Client Secret to `appsettings.json`

**Google OAuth is optional** - username/password authentication works without it.

### 3. Run the Backend

```bash
cd projects/api/src/MBC.Endpoints
dotnet run
```

The API will be available at `http://localhost:5000` (or configured port).

## Frontend Setup

### 1. Install Dependencies

```bash
cd projects/web
npm install
```

### 2. Run the Frontend

```bash
npm start
```

The web app will be available at `http://localhost:4200`.

## Testing Authentication

### Sign Up Flow

**Option 1: Email/Password**

1. Navigate to `http://localhost:4200`
2. Click "Sign Up" in the navigation bar
3. Fill in:
   - Email: `test@example.com`
   - Nickname: `TestUser`
   - Password: `password123`
4. Click "Sign Up"
5. You should be redirected to home page with user info in navbar

**Option 2: Google**

1. Click "Sign Up" in the navigation bar
2. Click "Sign up with Google"
3. Complete Google OAuth flow
4. Backend auto-creates account with email-based nickname
5. You'll be redirected back and automatically signed in

### Sign In Flow

1. Click "Sign Out" if currently signed in
2. Click "Sign In"
3. Enter email and password
4. Click "Sign In"
5. You should see your nickname/email in the navbar

### Google Sign In

1. Click "Sign In"
2. Click "Sign in with Google"
3. Complete Google OAuth flow
4. You'll be redirected back and automatically signed in

**Note:** Google OAuth requires proper configuration in Google Cloud Console.

### Token Management

- **Access tokens** are automatically attached to all API requests via the HTTP interceptor
- **Refresh tokens** are automatically used when access tokens expire (on 401 responses)
- Tokens are stored in `localStorage` (intentionally insecure for vulnerability demonstrations)

## API Endpoints

All authentication endpoints are under `/api/v1/auth`:

| Method | Endpoint                | Description                 |
| ------ | ----------------------- | --------------------------- |
| POST   | `/auth/signup`          | Create new customer account |
| POST   | `/auth/signin`          | Username/password login     |
| GET    | `/auth/google`          | Initiate Google OAuth flow  |
| GET    | `/auth/google/callback` | OAuth callback handler      |
| POST   | `/auth/refresh`         | Exchange refresh token      |
| POST   | `/auth/signout`         | Invalidate refresh token    |

## JWT Claims Structure

Access tokens include:

- `sub`: User ID (Guid)
- `email`: User email
- `role`: User role(s) - "Customer", "Pilot", or "Administrator"
- `customerId`: Customer ID (for Customer role only)
- `pilotId`: Pilot ID (for Pilot role only)
- `iss`, `aud`, `exp`, `iat`: Standard JWT claims

## Database Seeding

On startup, the following roles are automatically seeded:

- **Customer** - Can book shipments, create reviews
- **Pilot** - Can view assignments, upload proof of delivery
- **Administrator** - Full system access

New users signing up are automatically assigned the "Customer" role.

## Security Notes (For Demonstration)

Current implementation uses **intentionally insecure** patterns for security training:

1. **Secrets in appsettings.json** - JWT secret and OAuth credentials committed to source control
2. **localStorage for tokens** - Subject to XSS attacks (httpOnly cookies are better)
3. **No password complexity rules** - Any password is accepted
4. **Permissive CORS** - Allows credentials from localhost:4200
5. **No rate limiting on auth endpoints** - Vulnerable to brute force

These will be addressed in later phases when demonstrating secure patterns.

## Troubleshooting

### "No JWT secret configured"

Solution: Verify the JWT secret is present in `appsettings.json` under the `Jwt:Secret` key.

### "CORS error"

Solution: Verify:

- Frontend is running on `http://localhost:4200`
- Backend CORS policy matches in `Program.cs`

### "401 Unauthorized" on all requests

Solution: Check browser DevTools:

- Verify `Authorization: Bearer <token>` header is present
- Check if token is stored in localStorage
- Try signing out and signing in again

### Google OAuth fails

Solution:

- Verify Google OAuth credentials in `appsettings.json`
- Check redirect URI matches in Google Cloud Console: `http://localhost:5000/api/v1/auth/google/callback`
- Google OAuth is optional - username/password works without it

## Next Steps

Future enhancements (out of scope for initial implementation):

- **Move secrets to User Secrets** - JWT secret and OAuth credentials should be in user secrets, not committed to source control
- Auth guards for route protection
- Role-based endpoint authorization with `[Authorize]` attributes
- httpOnly cookies for tokens
- Password complexity requirements
- Email verification
- Password reset flow
- Multi-factor authentication
