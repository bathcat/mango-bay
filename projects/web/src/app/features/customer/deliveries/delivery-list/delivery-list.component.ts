import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { DeliveryListStateService } from './delivery-list-state.service';
import { EmptyStateComponent } from '@app/shared/ui/empty-state.component';
import { LoadableDirective } from '@app/shared/loadable';
import { DeliveryTableComponent } from '../delivery-table.component';
import { ListCardComponent } from '@app/shared/ui/list-card.component';

@Component({
  selector: 'mbc-delivery-list',
  standalone: true,
  providers: [DeliveryListStateService],
  imports: [
    CommonModule,
    RouterLink,
    MatButtonModule,
    EmptyStateComponent,
    LoadableDirective,
    DeliveryTableComponent,
    ListCardComponent,
  ],
  template: `
    <mbc-list-card title="My Deliveries" subtitle="Your cargo delivery history">
      <div *mbcLoadable="state.deliveries$ as deliveries; errorMessage: 'Failed to load deliveries'">
        @if (deliveries.totalCount === 0) {
          <mbc-empty-state>
            <p>No deliveries yet.</p>
            <a mat-raised-button color="primary" routerLink="/new-booking">Book your first delivery!</a>
          </mbc-empty-state>
        }

        @if (deliveries.totalCount > 0) {
          <mbc-delivery-table
            [deliveries]="deliveries.items"
            [totalCount]="deliveries.totalCount"
            [paginationStart]="deliveries.offset + 1"
            [paginationEnd]="deliveries.offset + deliveries.items.length"
            [hasPrevious]="deliveries.offset > 0"
            [hasNext]="deliveries.hasMore"
            (previous)="state.previousPage()"
            (next)="state.nextPage()"
          />
        }
      </div>
    </mbc-list-card>
  `,
})
export class DeliveryListComponent {
  public readonly state = inject(DeliveryListStateService);
}
