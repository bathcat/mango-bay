import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { PilotDetailStateService } from './pilot-detail-state.service';
import { LoadableDirective } from '@app/shared/loadable';
import { ReviewListComponent } from './review-list.component';

@Component({
  selector: 'mbc-pilot-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    LoadableDirective,
    ReviewListComponent,
  ],
  providers: [PilotDetailStateService],
  template: `
    <div class="back-button-container">
      <button mat-button routerLink="/pilots">
        <mat-icon>arrow_back</mat-icon>
        Back to Pilots
      </button>
    </div>

    <div class="pilot-detail-container">
      <mat-card *mbcLoadable="state.pilot$ as pilot; errorMessage: 'Failed to load pilot'">
        <mat-card-header>
          @if (pilot.avatarUrl) {
            <img
              mat-card-avatar
              [src]="pilot.avatarUrl"
              [alt]="pilot.fullName"
              class="pilot-avatar-large"
            />
          }
          <mat-card-title>{{ pilot.fullName }}</mat-card-title>
          <mat-card-subtitle>"{{ pilot.shortName }}"</mat-card-subtitle>
        </mat-card-header>

        <mat-card-content>
          <div class="pilot-bio">
            <h3>About</h3>
            <p>{{ pilot.bio }}</p>
          </div>
        </mat-card-content>
      </mat-card>
    </div>

    <mbc-review-list />
  `,
  styles: [`
    .pilot-detail-container {
      display: block;
      padding: 2rem;
      max-width: var(--mbc-list-max-width);
      margin: 0 auto;
    }
  `],
})
export class PilotDetailComponent {
  readonly state = inject(PilotDetailStateService);
}

