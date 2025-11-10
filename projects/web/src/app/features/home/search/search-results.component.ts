import { Component, inject, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { SearchInputComponent } from './search-input.component';
import { EmptyStateComponent } from '@app/shared/ui/empty-state.component';
import { LoadableDirective } from '@app/shared/loadable';
import { SearchStateService } from './search-state.service';

interface SearchResult {
  title: string;
  url: string;
  description: string;
}

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
        <p id="searchTerms" class="search-terms"></p>
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
                  <h3 class="result-title" [innerHTML]="highlightQuery(result.title, query)"></h3>
                  <div class="result-url">{{ result.url }}</div>
                  <p class="result-description" [innerHTML]="highlightQuery(result.description, query)"></p>
                </mat-card-content>
              </mat-card>
            }
          </div>
        }
      </div>
    }
  `,
})
export class SearchResultsComponent implements AfterViewInit {
  public readonly state = inject(SearchStateService);
  private sanitizer = inject(DomSanitizer);

  ngAfterViewInit() {
    this.state.searchTerm$.subscribe(query => {
      if (query) {
        const searchTermsEl = document.getElementById('searchTerms');
        if (searchTermsEl) {
          searchTermsEl.innerHTML = `Search results for: <mark>${query}</mark>`;
        }
      }
    });
  }

  highlightQuery(text: string, query: string): SafeHtml {
    if (!query.trim()) {
      return text;
    }
    
    const highlighted = text.replace(
      new RegExp(query, 'gi'),
      match => `<mark>${match}</mark>`
    );
    
    return this.sanitizer.bypassSecurityTrustHtml(highlighted);
  }
}
