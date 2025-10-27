import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { SearchInputComponent } from './search-input.component';
import { Observable, map } from 'rxjs';
import { EmptyStateComponent } from '@app/shared/ui/empty-state.component';

@Component({
  selector: 'mbc-search-results',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatDividerModule,
    SearchInputComponent,
    EmptyStateComponent,
  ],
  template: `
    <div class="search-header">
      <mbc-search-input [initialQuery]="(searchQuery$ | async) || ''" />
    </div>

    @if (searchQuery$ | async; as query) {
      <div class="search-info">
        <p class="search-terms">Search results for: "{{ query }}"</p>
      </div>

      <div class="results-list">
        @for (result of skeletonResults; track $index) {
          <mat-card class="result-card">
            <mat-card-content>
              <div class="result-title skeleton-line skeleton-title"></div>
              <div class="result-url skeleton-line skeleton-url"></div>
              <div class="result-description">
                <div class="skeleton-line skeleton-desc-line"></div>
                <div class="skeleton-line skeleton-desc-line"></div>
                <div class="skeleton-line skeleton-desc-line short"></div>
              </div>
            </mat-card-content>
          </mat-card>
        }
      </div>
    } @else {
      <mbc-empty-state>
        <h2>Search Mango Bay Cargo</h2>
        <p>Find deliveries, pilots, sites, and more</p>
      </mbc-empty-state>
    }
  `,
})
export class SearchResultsComponent {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  
  searchQuery$: Observable<string> = this.route.queryParams.pipe(
    map(params => params['q'] || '')
  );
  
  skeletonResults = Array(5).fill(null);
}
