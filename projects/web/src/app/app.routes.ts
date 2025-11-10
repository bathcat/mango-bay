import { Routes } from '@angular/router';
import { roleGuard, adminGuard, homeGuard } from './core/guards/route-guards';
import { ClientRoutes, toRoutePath } from './core/routing/client-routes.const';

import { HomeComponent } from './features/home/home.component';
import { CustomerDashboardComponent } from './features/customer/customer-dashboard.component';
import { SiteListComponent } from './features/sites/site-list/site-list.component';
import { SiteDetailComponent } from './features/sites/site-detail/site-detail.component';
import { PilotDashboardComponent } from './features/pilot/pilot-dashboard.component';
import { AdminPanelComponent } from './features/admin/admin-panel.component';
import { UnauthorizedComponent } from './core/guards/unauthorized.component';

export const routes: Routes = [
  {
    path: toRoutePath(ClientRoutes.home()),
    canActivate: [homeGuard],
    component: HomeComponent,
  },
  {
    path: toRoutePath(ClientRoutes.customerDashboard()),
    canActivate: [roleGuard(['Customer'])],
    component: CustomerDashboardComponent,
  },
  {
    path: toRoutePath(ClientRoutes.auth.signIn()),
    loadComponent: () =>
      import('./features/home/auth/sign-in.component').then(
        m => m.SignInComponent
      ),
  },
  {
    path: toRoutePath(ClientRoutes.auth.signUp()),
    loadComponent: () =>
      import('./features/home/auth/sign-up.component').then(
        m => m.SignUpComponent
      ),
  },
  {
    path: toRoutePath(ClientRoutes.pilots.list()),
    loadComponent: () =>
      import('./features/home/pilots/pilot-list/pilot-list.component').then(
        m => m.PilotListComponent
      ),
  },
  {
    path: 'pilots/:id',
    loadComponent: () =>
      import('./features/home/pilots/pilot-detail/pilot-detail.component').then(
        m => m.PilotDetailComponent
      ),
  },
  {
    path: toRoutePath(ClientRoutes.sites.list()),
    component: SiteListComponent,
  },
  {
    path: 'sites/:id',
    component: SiteDetailComponent,
  },
  {
    path: toRoutePath(ClientRoutes.booking.new()),
    canActivate: [roleGuard(['Customer'])],
    loadComponent: () =>
      import('./features/customer/booking/booking-page/booking-page.component').then(
        m => m.BookingPageComponent
      ),
  },
  {
    path: toRoutePath(ClientRoutes.deliveries.list()),
    canActivate: [roleGuard(['Customer'])],
    loadComponent: () =>
      import('./features/customer/deliveries/delivery-list/delivery-list.component').then(
        m => m.DeliveryListComponent
      ),
  },
  {
    path: toRoutePath(ClientRoutes.deliveries.searchCargo()),
    canActivate: [roleGuard(['Customer'])],
    loadComponent: () =>
      import('./features/customer/deliveries/search-cargo/search-cargo.component').then(
        m => m.SearchCargoComponent
      ),
  },
  {
    path: toRoutePath(ClientRoutes.payments.search()),
    canActivate: [roleGuard(['Customer'])],
    loadComponent: () =>
      import('./features/customer/payments/search-payments.component').then(
        m => m.SearchPaymentsComponent
      ),
  },
  {
    path: 'deliveries/:id',
    canActivate: [roleGuard(['Customer'])],
    loadComponent: () =>
      import('./features/customer/deliveries/delivery-detail/delivery-detail.component').then(
        m => m.DeliveryDetailComponent
      ),
  },
  {
    path: toRoutePath(ClientRoutes.assignments.list()),
    canActivate: [roleGuard(['Pilot'])],
    loadComponent: () =>
      import('./features/pilot/assignments/assignment-list/assignment-list.component').then(
        m => m.AssignmentListComponent
      ),
  },
  {
    path: 'assignments/:id',
    canActivate: [roleGuard(['Pilot'])],
    loadComponent: () =>
      import('./features/pilot/assignments/assignment-detail/assignment-detail.component').then(
        m => m.AssignmentDetailComponent
      ),
  },
  {
    path: toRoutePath(ClientRoutes.pilotDashboard()),
    canActivate: [roleGuard(['Pilot'])],
    component: PilotDashboardComponent,
  },
  {
    path: toRoutePath(ClientRoutes.admin()),
    canActivate: [adminGuard],
    component: AdminPanelComponent,
  },
  {
    path: toRoutePath(ClientRoutes.search()),
    loadComponent: () =>
      import('./features/home/search/search-results.component').then(
        m => m.SearchResultsComponent
      ),
  },
  {
    path: toRoutePath(ClientRoutes.unauthorized()),
    component: UnauthorizedComponent,
  },
];

