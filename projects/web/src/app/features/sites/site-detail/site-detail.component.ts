import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { SiteDetailStateService } from './site-detail-state.service';
import { LoadableDirective } from '@app/shared/loadable';

@Component({
  selector: 'mbc-site-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    LoadableDirective,
  ],
  providers: [SiteDetailStateService],
  template: `
    <div class="back-button-container">
        <button mat-button routerLink="/sites">
          <mat-icon>arrow_back</mat-icon>
          Back to Sites
        </button>
      </div>

      <mat-card *mbcLoadable="state.site$ as site; errorMessage: 'Failed to load site'">
        <mat-card-header>
          <mat-card-title>{{ site.name }}</mat-card-title>
          <mat-card-subtitle>{{ site.island }}</mat-card-subtitle>
        </mat-card-header>

        @if (site.imageUrl) {
          <img
            mat-card-image
            [src]="site.imageUrl"
            [alt]="site.name"
            class="site-image-large"
          />
        }

        <mat-card-content>
          <div class="site-info">
            <div class="info-section">
              <h3>Status</h3>
              <p>{{ site.status }}</p>
            </div>

            <div class="info-section">
              <h3>Address</h3>
              <p>{{ site.address }}</p>
            </div>

            <div class="info-section">
              <h3>Location</h3>
              <p>X: {{ site.location.x }}, Y: {{ site.location.y }}</p>
            </div>

            <div class="info-section">
              <h3>Notes</h3>
              <p>{{ site.notes }}</p>
            </div>
          </div>
        </mat-card-content>
      </mat-card>
  `,
})
export class SiteDetailComponent {
  readonly state = inject(SiteDetailStateService);
}

