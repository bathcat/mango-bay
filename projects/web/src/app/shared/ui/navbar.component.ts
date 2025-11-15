import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '@app/features/home/auth/services/auth.service';
import { SearchInputComponent } from '@app/features/home/search/search-input.component';

@Component({
  selector: 'mbc-navbar',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    RouterLinkActive,
    MatToolbarModule,
    MatButtonModule,
    SearchInputComponent,
  ],
  template: `
    <mat-toolbar color="primary">
      <div class="navbar-container">
        <a mat-button routerLink="/" class="brand-link">MBC</a>
        <nav class="nav-links">
          <a
            mat-raised-button
            color="accent"
            routerLink="/pilots"
            routerLinkActive="active"
          >
            Pilots
          </a>
          <a
            mat-raised-button
            color="accent"
            routerLink="/sites"
            routerLinkActive="active"
          >
            Sites
          </a>
          @if (authService.isAuthenticated$ | async) {
            @switch ((authService.currentUser$ | async)?.role) {
              @case ('Customer') {
                <a
                  mat-raised-button
                  color="primary"
                  routerLink="/customer-dashboard"
                  routerLinkActive="active"
                >
                  Dashboard
                </a>
              }
              @case ('Pilot') {
                <a
                  mat-raised-button
                  color="accent"
                  routerLink="/pilot-dashboard"
                  routerLinkActive="active"
                >
                  My Jobs
                </a>
              }
              @case ('Administrator') {
                <a
                  mat-raised-button
                  color="warn"
                  routerLink="/admin"
                  routerLinkActive="active"
                >
                  Admin Panel
                </a>
              }
              @default {
                <!-- No navigation items for this role -->
              }
            }
          }
        </nav>
        <div class="search-section">
          <mbc-search-input />
        </div>
        <nav class="auth-links">
          @if (authService.isAuthenticated$ | async) {
            <span class="user-info">{{ (authService.currentUser$ | async)?.email }}</span>
            <button mat-raised-button color="warn" (click)="authService.signOut()">
              Sign Out
            </button>
          } @else {
            <a mat-raised-button routerLink="/signin">Sign In</a>
            <a mat-raised-button color="accent" routerLink="/signup">Sign Up</a>
          }
        </nav>
      </div>
    </mat-toolbar>
  `,
  styles: `
    mat-toolbar {
      position: sticky;
      top: 0;
      z-index: 1000;
    }

    .navbar-container {
      width: 100%;
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 1rem;
    }

    .brand-link {
      font-size: 1.75rem;
      font-weight: 700;
      letter-spacing: 0.5px;
    }

    .nav-links {
      display: flex;
      gap: 0.5rem;
    }

    .search-section {
      margin-left: auto;
      margin-right: 1rem;
    }

    .auth-links {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .user-info {
      font-size: 0.875rem;
      font-weight: 500;
      padding: 0 0.5rem;
    }
  `,
})
export class NavbarComponent {
  readonly authService = inject(AuthService);
}
