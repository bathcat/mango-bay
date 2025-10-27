import { inject } from '@angular/core';
import { Router, type CanActivateFn } from '@angular/router';
import { AuthService } from '@app/features/home/auth/services/auth.service';
import { UserRole } from '@app/shared/schemas';
import { ClientRoutes } from '../routing/client-routes.const';

export const roleGuard = (allowedRoles: UserRole[]): CanActivateFn => {
  return (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);
    
    const user = authService.getCurrentUser();
    
    if (!user) {
      return router.parseUrl(ClientRoutes.unauthorized());
    }
    
    if (allowedRoles.includes(user.role)) {
      return true;
    }
    
    return router.parseUrl(ClientRoutes.unauthorized());
  };
};

export const adminGuard: CanActivateFn = roleGuard(['Administrator']);

export const homeGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  
  const user = authService.getCurrentUser();
  
  if (user?.role === 'Customer') {
    return router.parseUrl(ClientRoutes.customerDashboard());
  }
  
  return true;
};