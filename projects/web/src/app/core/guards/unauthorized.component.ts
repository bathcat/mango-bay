import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'mbc-unauthorized',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
  ],
  template: `
    <div class="unauthorized-container">
      <mat-card class="unauthorized-card">
        <mat-card-header>
          <mat-card-title>Access Denied</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <p>You don't have permission to access this page.</p>
        </mat-card-content>
        <mat-card-actions>
          <button mat-raised-button color="primary" routerLink="/">
            Go Home
          </button>
        </mat-card-actions>
      </mat-card>
    </div>
  `,
  styles: `
    .unauthorized-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 50vh;
      padding: 2rem;
    }

    .unauthorized-card {
      max-width: 400px;
      text-align: center;
    }

    mat-card-title {
      color: #d32f2f;
      font-size: 1.5rem;
    }

    mat-card-content p {
      font-size: 1.1rem;
      margin: 1rem 0;
    }
  `,
})
export class UnauthorizedComponent {}
