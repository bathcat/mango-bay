import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { TokenStoreService } from '@app/features/home/auth/services/token-store.service';
import { AuthService } from '@app/features/home/auth/services/auth.service';
import { Router } from '@angular/router';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenStore = inject(TokenStoreService);
  const authService = inject(AuthService);
  const router = inject(Router);

  const accessToken = tokenStore.getAccessToken();

  if (accessToken) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${accessToken}`,
      },
    });
  }

  //TODO: Extract a method or a pure function from this mess. 
  return next(req).pipe(
    catchError((error) => {
      console.log(`error status: ${error.status}, message: ${error.message}`);
      if (error.status === 401 && !req.url.includes('/auth/')) {
        console.log('401.... the token is expired, refreshing...');
        return authService.refreshToken().pipe(
          switchMap(() => {
            const newAccessToken = tokenStore.getAccessToken();
            if (newAccessToken) {
              console.log('newAccessToken', newAccessToken);
              req = req.clone({
                setHeaders: {
                  Authorization: `Bearer ${newAccessToken}`,
                },
              });
            }
            return next(req);
          }),
          catchError((refreshError) => {
            authService.signOut();
            router.navigate(['/signin']);
            return throwError(() => refreshError);
          })
        );
      }
      return throwError(() => error);
    })
  );
};

