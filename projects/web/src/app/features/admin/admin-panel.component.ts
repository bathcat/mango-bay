import { Component } from '@angular/core';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';

@Component({
  selector: 'mbc-admin-panel',
  standalone: true,
  imports: [
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTabsModule
],
  template: `
    <div class="admin-container">
      <h1>Administrator Panel</h1>
      
      <mat-tab-group class="admin-tabs">
        <mat-tab label="Users">
          <div class="tab-content">
            <mat-card>
              <mat-card-header>
                <mat-card-title>
                  <mat-icon>people</mat-icon>
                  User Management
                </mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <p>Manage customer and pilot accounts</p>
                <p class="placeholder-text">This feature will be implemented in a future update.</p>
              </mat-card-content>
              <mat-card-actions>
                <button mat-raised-button color="primary" disabled>
                  Manage Users
                </button>
              </mat-card-actions>
            </mat-card>
          </div>
        </mat-tab>

        <mat-tab label="Bookings">
          <div class="tab-content">
            <mat-card>
              <mat-card-header>
                <mat-card-title>
                  <mat-icon>local_shipping</mat-icon>
                  Booking Management
                </mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <p>View and manage all cargo bookings</p>
                <p class="placeholder-text">This feature will be implemented in a future update.</p>
              </mat-card-content>
              <mat-card-actions>
                <button mat-raised-button color="primary" disabled>
                  View Bookings
                </button>
                <button mat-raised-button color="warn" disabled>
                  Cancel Booking
                </button>
              </mat-card-actions>
            </mat-card>
          </div>
        </mat-tab>

        <mat-tab label="Payments">
          <div class="tab-content">
            <mat-card>
              <mat-card-header>
                <mat-card-title>
                  <mat-icon>payment</mat-icon>
                  Payment Management
                </mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <p>Process refunds and manage payment issues</p>
                <p class="placeholder-text">This feature will be implemented in a future update.</p>
              </mat-card-content>
              <mat-card-actions>
                <button mat-raised-button color="primary" disabled>
                  View Payments
                </button>
                <button mat-raised-button color="accent" disabled>
                  Process Refund
                </button>
              </mat-card-actions>
            </mat-card>
          </div>
        </mat-tab>

        <mat-tab label="Reports">
          <div class="tab-content">
            <mat-card>
              <mat-card-header>
                <mat-card-title>
                  <mat-icon>analytics</mat-icon>
                  System Reports
                </mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <p>Generate reports on system usage and performance</p>
                <p class="placeholder-text">This feature will be implemented in a future update.</p>
              </mat-card-content>
              <mat-card-actions>
                <button mat-raised-button color="primary" disabled>
                  Generate Report
                </button>
              </mat-card-actions>
            </mat-card>
          </div>
        </mat-tab>
      </mat-tab-group>
    </div>
  `,
  styles: `
    .admin-container {
      padding: 2rem;
      max-width: 1200px;
      margin: 0 auto;
    }

    h1 {
      margin-bottom: 2rem;
      color: #d32f2f;
      font-weight: 300;
    }

    .admin-tabs {
      margin-top: 1rem;
    }

    .tab-content {
      padding: 1.5rem 0;
    }

    .tab-content mat-card {
      max-width: 600px;
    }

    .tab-content mat-card-header {
      margin-bottom: 1rem;
    }

    .tab-content mat-card-title {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 1.25rem;
      font-weight: 500;
    }

    .tab-content mat-icon {
      color: #d32f2f;
    }

    .placeholder-text {
      font-style: italic;
      color: #666;
      margin-top: 0.5rem;
    }

    .tab-content mat-card-content p:first-child {
      margin-bottom: 0.5rem;
    }

    .tab-content mat-card-actions {
      padding-top: 1rem;
      display: flex;
      gap: 0.5rem;
    }
  `,
})
export class AdminPanelComponent {}
