import { Component } from '@angular/core';

import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'mbc-pilot-dashboard',
  standalone: true,
  imports: [
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule
],
  template: `
    <div class="dashboard-container">
      <h1>Pilot Dashboard</h1>
      
      <div class="dashboard-grid">
        <mat-card class="dashboard-card">
          <mat-card-header>
            <mat-card-title>
              <mat-icon>flight</mat-icon>
              My Jobs
            </mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <p>View and manage your assigned deliveries</p>
          </mat-card-content>
          <mat-card-actions>
            <button mat-raised-button color="primary" routerLink="/assignments">
              View Jobs
            </button>
          </mat-card-actions>
        </mat-card>

        <mat-card class="dashboard-card">
          <mat-card-header>
            <mat-card-title>
              <mat-icon>upload</mat-icon>
              Upload Proof
            </mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <p>Upload proof of delivery for completed jobs</p>
            <p class="placeholder-text">This feature will be implemented in a future update.</p>
          </mat-card-content>
          <mat-card-actions>
            <button mat-raised-button color="accent" disabled>
              Upload
            </button>
          </mat-card-actions>
        </mat-card>

        <mat-card class="dashboard-card">
          <mat-card-header>
            <mat-card-title>
              <mat-icon>schedule</mat-icon>
              Schedule
            </mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <p>View your upcoming deliveries and availability</p>
            <p class="placeholder-text">This feature will be implemented in a future update.</p>
          </mat-card-content>
          <mat-card-actions>
            <button mat-raised-button color="primary" disabled>
              View Schedule
            </button>
          </mat-card-actions>
        </mat-card>

        <mat-card class="dashboard-card">
          <mat-card-header>
            <mat-card-title>
              <mat-icon>star</mat-icon>
              Reviews
            </mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <p>View customer reviews and ratings</p>
            <p class="placeholder-text">This feature will be implemented in a future update.</p>
          </mat-card-content>
          <mat-card-actions>
            <button mat-raised-button color="accent" disabled>
              View Reviews
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

    .placeholder-text {
      font-style: italic;
      color: #666;
      margin-top: 0.5rem;
    }

    .dashboard-card mat-card-content p:first-child {
      margin-bottom: 0.5rem;
    }

    .dashboard-card mat-card-actions {
      padding-top: 1rem;
    }
  `,
})
export class PilotDashboardComponent {}
