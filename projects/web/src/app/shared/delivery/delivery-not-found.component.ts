import { CommonModule } from '@angular/common';
import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'mbc-delivery-not-found',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
  ],
  template: `
    <mat-card class="not-found-card">
      <mat-card-header>
        <mat-card-title>{{ title() }}</mat-card-title>
      </mat-card-header>
      <mat-card-content>
        <div class="not-found-content">
          <mat-icon class="not-found-icon">error_outline</mat-icon>
          <p>{{ message() }}</p>
          <p class="not-found-hint">{{ hint() }}</p>
          <button mat-raised-button color="primary" [routerLink]="backButtonRoute()">
            {{ backButtonText() }}
          </button>
        </div>
      </mat-card-content>
    </mat-card>
  `
})
export class DeliveryNotFoundComponent {
  title = input.required<string>();
  message = input.required<string>();
  hint = input.required<string>();
  backButtonText = input.required<string>();
  backButtonRoute = input.required<string>();
}
