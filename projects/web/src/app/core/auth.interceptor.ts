import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '@app/features/home/auth/services/auth.service';
import { NavigationService } from '@app/core/routing/navigation.service';
import { shouldAttemptTokenRefresh } from './auth.interceptor.utils';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const navigationService = inject(NavigationService);

  return next(req).pipe(
    catchError((error) => {
      if (!shouldAttemptTokenRefresh(error.status, req.url)) {
        return throwError(() => error);
      }

      return authService.refreshToken().pipe(
        switchMap(() => next(req)),
        catchError((refreshError) => {
          authService.signOut();
          navigationService.navigateToSignIn();
          return throwError(() => refreshError);
        })
      );
    })
  );
};

