import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { ClientRoutes } from './client-routes.const';

@Injectable({ providedIn: 'root' })
export class NavigationService {
  private router = inject(Router);

  navigateToHome(): Promise<boolean> {
    return this.router.navigate([ClientRoutes.home()]);
  }

  navigateToCustomerDashboard(): Promise<boolean> {
    return this.router.navigate([ClientRoutes.customerDashboard()]);
  }

  navigateToPilotDashboard(): Promise<boolean> {
    return this.router.navigate([ClientRoutes.pilotDashboard()]);
  }

  navigateToAdmin(): Promise<boolean> {
    return this.router.navigate([ClientRoutes.admin()]);
  }

  navigateToUnauthorized(): Promise<boolean> {
    return this.router.navigate([ClientRoutes.unauthorized()]);
  }

  navigateToSignIn(): Promise<boolean> {
    return this.router.navigate([ClientRoutes.auth.signIn()]);
  }

  navigateToSignUp(): Promise<boolean> {
    return this.router.navigate([ClientRoutes.auth.signUp()]);
  }

  navigateToPilotList(): Promise<boolean> {
    return this.router.navigate([ClientRoutes.pilots.list()]);
  }

  navigateToPilot(pilotId: string): Promise<boolean> {
    return this.router.navigate([ClientRoutes.pilots.detail(pilotId)]);
  }

  navigateToSiteList(): Promise<boolean> {
    return this.router.navigate([ClientRoutes.sites.list()]);
  }

  navigateToSite(siteId: string): Promise<boolean> {
    return this.router.navigate([ClientRoutes.sites.detail(siteId)]);
  }

  navigateToNewBooking(): Promise<boolean> {
    return this.router.navigate([ClientRoutes.booking.new()]);
  }

  navigateToDeliveryList(): Promise<boolean> {
    return this.router.navigate([ClientRoutes.deliveries.list()]);
  }

  navigateToDelivery(deliveryId: string): Promise<boolean> {
    return this.router.navigate([ClientRoutes.deliveries.detail(deliveryId)]);
  }

  navigateToSearchCargo(query: string): Promise<boolean> {
    return this.router.navigate([ClientRoutes.deliveries.searchCargo()], { 
      queryParams: { q: query } 
    });
  }

  navigateToSearchPayments(names: string[]): Promise<boolean> {
    return this.router.navigate([ClientRoutes.payments.search()], { 
      queryParams: { names: names.join(',') } 
    });
  }

  navigateToAssignmentList(): Promise<boolean> {
    return this.router.navigate([ClientRoutes.assignments.list()]);
  }

  navigateToAssignment(assignmentId: string): Promise<boolean> {
    return this.router.navigate([ClientRoutes.assignments.detail(assignmentId)]);
  }

  navigateToSearch(query: string): Promise<boolean> {
    return this.router.navigate([ClientRoutes.search()], { 
      queryParams: { q: query } 
    });
  }
}

