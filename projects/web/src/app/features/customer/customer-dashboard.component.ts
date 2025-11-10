import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { SearchCargoInputComponent } from './deliveries/search-cargo/search-cargo-input.component';
import { SearchPaymentsInputComponent } from './payments/search-payments-input.component';
import { NavigationService } from '@app/core/routing/navigation.service';
import { ClientRoutes } from '@app/core/routing/client-routes.const';

@Component({
  selector: 'mbc-customer-dashboard',
  standalone: true,
  imports: [
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    SearchCargoInputComponent,
    SearchPaymentsInputComponent,
  ],
  template: `
    <div class="dashboard-container">
      <h1>Customer Dashboard</h1>
      
      <div class="dashboard-grid">
        <mat-card class="dashboard-card">
          <mat-card-header>
            <mat-card-title>
              <mat-icon>local_shipping</mat-icon>
              My Deliveries
            </mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <p>View and track your cargo delivery history</p>
          </mat-card-content>
          <mat-card-actions>
            <button mat-raised-button color="primary" [routerLink]="routes.deliveries.list()">
              View Deliveries
            </button>
          </mat-card-actions>
        </mat-card>

        <mat-card class="dashboard-card">
          <mat-card-header>
            <mat-card-title>
              <mat-icon>search</mat-icon>
              Search by Cargo
            </mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <p>Find deliveries by searching cargo descriptions</p>
            <mbc-search-cargo-input (search)="onSearchCargo($event)" />
          </mat-card-content>
        </mat-card>

        <mat-card class="dashboard-card">
          <mat-card-header>
            <mat-card-title>
              <mat-icon>payment</mat-icon>
              Search Payments
            </mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <p>Find payments by searching cardholder names</p>
            <mbc-search-payments-input (search)="onSearchPayments($event)" />
          </mat-card-content>
        </mat-card>

        <mat-card class="dashboard-card">
          <mat-card-header>
            <mat-card-title>
              <mat-icon>add_circle</mat-icon>
              Book Now
            </mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <p>Schedule a new cargo delivery with our pilots</p>
          </mat-card-content>
          <mat-card-actions>
            <button mat-raised-button color="primary" [routerLink]="routes.booking.new()">
              Book Delivery
            </button>
          </mat-card-actions>
        </mat-card>
      </div>
    </div>
  `,
  styles: `
    .dashboard-container {
      padding: 2rem;
      max-width: 1200px;
      margin: 0 auto;
    }

    h1 {
      margin-bottom: 2rem;
      color: #1976d2;
      font-weight: 300;
    }

    .dashboard-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 1.5rem;
    }

    .dashboard-card {
      height: 100%;
    }

    .dashboard-card mat-card-header {
      margin-bottom: 1rem;
    }

    .dashboard-card mat-card-title {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 1.25rem;
      font-weight: 500;
    }

    .dashboard-card mat-icon {
      color: #1976d2;
    }

    .dashboard-card mat-card-content p {
      margin-bottom: 1rem;
    }

    .dashboard-card mat-card-actions {
      padding-top: 1rem;
    }

    .dashboard-card mbc-search-cargo-input {
      margin-top: 0.5rem;
    }
  `,
})
export class CustomerDashboardComponent {
  private navigationService = inject(NavigationService);
  
  protected readonly routes = ClientRoutes;

  onSearchCargo(searchTerm: string): void {
    this.navigationService.navigateToSearchCargo(searchTerm);
  }
  //TODO: Just call the state service directly.
  onSearchPayments(names: string[]): void {
    this.navigationService.navigateToSearchPayments(names);
  }
}

