import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { TokenStoreService } from '@app/features/home/auth/services/token-store.service';
import { AuthService } from '@app/features/home/auth/services/auth.service';
import { NavigationService } from '@app/core/routing/navigation.service';
import { shouldAttemptTokenRefresh, applyAuthToken } from './auth.interceptor.utils';
import { AuthResponse } from '@app/shared/schemas';

export const authInterceptor: HttpInterceptorFn = (originalRequest, next) => {
  const tokenStore = inject(TokenStoreService);
  const authService = inject(AuthService);
  const navigationService = inject(NavigationService);

  const authenticatedRequest = applyAuthToken(tokenStore.getAccessToken(), originalRequest);

  return next(authenticatedRequest).pipe(
    catchError(error => {
      
      if (!shouldAttemptTokenRefresh(error.status, originalRequest.url)) {
        return throwError(() => error);
      }
      return authService.refreshToken().pipe(
        switchMap((refreshResponse: AuthResponse) => {
          const retryRequest = applyAuthToken(refreshResponse.accessToken, originalRequest);
          return next(retryRequest);
        }),
        catchError(refreshError => {
          authService.signOut();
          navigationService.navigateToSignIn();
          return throwError(() => refreshError);
        })
      );
    })
  );
};

