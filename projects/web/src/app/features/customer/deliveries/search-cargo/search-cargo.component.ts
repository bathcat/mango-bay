import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { SearchCargoStateService } from './search-cargo-state.service';
import { SearchCargoInputComponent } from './search-cargo-input.component';
import { DeliveryTableComponent } from '../delivery-table.component';
import { EmptyStateComponent } from '@app/shared/ui/empty-state.component';
import { LoadableDirective } from '@app/shared/loadable';
import { ListCardComponent } from '@app/shared/ui/list-card.component';

@Component({
  selector: 'mbc-search-cargo',
  standalone: true,
  providers: [SearchCargoStateService],
  imports: [
    CommonModule,
    SearchCargoInputComponent,
    DeliveryTableComponent,
    EmptyStateComponent,
    LoadableDirective,
    ListCardComponent,
  ],
  template: `
    <div class="search-toolbar">
      <mbc-search-cargo-input (search)="onSearch($event)" />
    </div>

    <mbc-list-card title="Search Cargo" subtitle="Find your deliveries by cargo description">
      @if ((state.searchTerm$ | async) === '') {
        <mbc-empty-state>
          <p>Enter a search term to find your deliveries.</p>
        </mbc-empty-state>
      }

      @if ((state.searchTerm$ | async); as searchTerm) {
        <div class="search-info">
          <p class="search-terms">Search results for: "{{ searchTerm }}"</p>
        </div>
      }

      @if ((state.searchTerm$ | async)) {
        <div *mbcLoadable="state.deliveries$ as deliveries; errorMessage: 'Failed to search deliveries'">
          @if ($any(deliveries).totalCount === 0) {
            <mbc-empty-state>
              <p>No deliveries found matching your search.</p>
              <p>Try a different search term or check your delivery history.</p>
            </mbc-empty-state>
          }

          @if ($any(deliveries).totalCount > 0) {
            <mbc-delivery-table
              [deliveries]="$any(deliveries).items"
              [totalCount]="$any(deliveries).totalCount"
              [paginationStart]="$any(deliveries).offset + 1"
              [paginationEnd]="$any(deliveries).offset + $any(deliveries).items.length"
              [hasPrevious]="$any(deliveries).offset > 0"
              [hasNext]="$any(deliveries).hasMore"
              (previous)="state.previousPage()"
              (next)="state.nextPage()"
            />
          }
        </div>
      }
    </mbc-list-card>
  `,
})
export class SearchCargoComponent {
  public readonly state = inject(SearchCargoStateService);

  onSearch(searchTerm: string): void {
    this.state.performSearch(searchTerm);
  }
}

