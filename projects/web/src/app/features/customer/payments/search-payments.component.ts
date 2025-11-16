import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { SearchPaymentsStateService } from './search-payments-state.service';
import { SearchPaymentsInputComponent } from './search-payments-input.component';
import { PaymentTableComponent } from './payment-table.component';
import { EmptyStateComponent } from '@app/shared/ui/empty-state.component';
import { LoadableDirective } from '@app/shared/loadable';
import { ListCardComponent } from '@app/shared/ui/list-card.component';

@Component({
  selector: 'mbc-search-payments',
  standalone: true,
  providers: [SearchPaymentsStateService],
  imports: [
    CommonModule,
    SearchPaymentsInputComponent,
    PaymentTableComponent,
    EmptyStateComponent,
    LoadableDirective,
    ListCardComponent,
  ],
  template: `
    <div class="search-toolbar">
      <mbc-search-payments-input (search)="onSearch($event)" />
    </div>

    <mbc-list-card title="Search Payments" subtitle="Find payments by cardholder names">
      @if ((state.searchNames$ | async)?.length === 0) {
        <mbc-empty-state>
          <p>Enter cardholder names to search for payments.</p>
        </mbc-empty-state>
      }

      @if ((state.searchNames$ | async); as searchNames) {
        @if (searchNames.length > 0) {
          <div class="search-info">
            <p class="search-terms">Searching for: {{ searchNames.join(', ') }}</p>
          </div>
        }
      }

      @if ((state.searchNames$ | async)?.length ?? 0 > 0) {
        <div *mbcLoadable="state.payments$ as payments; errorMessage: 'Failed to search payments'">
          @if ($any(payments).length === 0) {
            <mbc-empty-state>
              <p>No payments found matching your search.</p>
              <p>Try different cardholder names.</p>
            </mbc-empty-state>
          }

          @if ($any(payments).length > 0) {
            <mbc-payment-table [payments]="$any(payments)" />
          }
        </div>
      }
    </mbc-list-card>
  `,
  styles: `
    .search-toolbar {
      margin-bottom: 1.5rem;
    }

    .search-info {
      padding: 1rem;
      background-color: #f5f5f5;
      border-radius: 4px;
      margin-bottom: 1rem;
    }

    .search-terms {
      margin: 0;
      font-size: 0.875rem;
      color: #666;
    }
  `,
})
export class SearchPaymentsComponent {
  public readonly state = inject(SearchPaymentsStateService);

  onSearch(names: string[]): void {
    this.state.performSearch(names);
  }
}

