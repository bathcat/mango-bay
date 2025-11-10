import { HttpRequest } from '@angular/common/http';

export function shouldAttemptTokenRefresh(
  errorStatus: number,
  requestUrl: string
): boolean {
  return errorStatus === 401 && !requestUrl.includes('/auth/');
}

export function createAuthHeader(token: string | null): { Authorization: string } | {} {
  if (!token) {
    return {};
  }
  return { Authorization: `Bearer ${token}` };
}

export function addAuthHeadersToRequest<T>(
  req: HttpRequest<T>,
  authHeaders: Record<string, string>
): HttpRequest<T> {
  if (Object.keys(authHeaders).length > 0) {
    return req.clone({ setHeaders: authHeaders });
  }
  return req;
}

export function applyAuthToken<T>(
  token: string | null,
  req: HttpRequest<T>
): HttpRequest<T> {
  const authHeaders = createAuthHeader(token);
  return addAuthHeadersToRequest(req, authHeaders);
}

