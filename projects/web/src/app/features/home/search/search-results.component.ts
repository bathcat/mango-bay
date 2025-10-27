import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { SearchInputComponent } from './search-input.component';
import { EmptyStateComponent } from '@app/shared/ui/empty-state.component';
import { LoadableDirective } from '@app/shared/loadable';
import { SearchStateService } from './search-state.service';

@Component({
  selector: 'mbc-search-results',
  standalone: true,
  providers: [SearchStateService],
  imports: [
    CommonModule,
    MatCardModule,
    MatDividerModule,
    SearchInputComponent,
    EmptyStateComponent,
    LoadableDirective,
  ],
  template: `
    <div class="search-header">
      <mbc-search-input [initialQuery]="(state.searchTerm$ | async) || ''" />
    </div>

    @if ((state.searchTerm$ | async) === '') {
      <mbc-empty-state>
        <h2>Search Mango Bay Cargo</h2>
        <p>Find deliveries, pilots, sites, and more</p>
      </mbc-empty-state>
    }

    @if ((state.searchTerm$ | async); as query) {
      <div class="search-info">
        <p class="search-terms">Search results for: "{{ query }}"</p>
      </div>

      <div *mbcLoadable="state.results$ as results; errorMessage: 'Failed to load search results'">
        @if ($any(results).length === 0) {
          <mbc-empty-state>
            <p>No results found matching your search.</p>
            <p>Try a different search term.</p>
          </mbc-empty-state>
        }

        @if ($any(results).length > 0) {
          <div class="results-list">
            @for (result of $any(results); track result.url) {
              <mat-card class="result-card">
                <mat-card-content>
                  <h3 class="result-title">{{ result.title }}</h3>
                  <div class="result-url">{{ result.url }}</div>
                  <p class="result-description">{{ result.description }}</p>
                </mat-card-content>
              </mat-card>
            }
          </div>
        }
      </div>
    }
  `,
})
export class SearchResultsComponent {
  public readonly state = inject(SearchStateService);
}
