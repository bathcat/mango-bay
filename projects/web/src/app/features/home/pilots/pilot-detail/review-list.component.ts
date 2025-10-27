import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDividerModule } from '@angular/material/divider';
import { ReviewListStateService } from './review-list-state.service';
import { LoadableDirective } from '@app/shared/loadable';
import { PaginationComponent } from '@app/shared/pagination/pagination.component';
import { ReviewDisplayComponent } from '@app/shared/delivery/review/review-display.component';
import { EmptyStateComponent } from '@app/shared/ui/empty-state.component';
import { ListCardComponent } from '@app/shared/ui/list-card.component';

@Component({
  selector: 'mbc-review-list',
  standalone: true,
  imports: [
    CommonModule,
    MatDividerModule,
    LoadableDirective,
    PaginationComponent,
    ReviewDisplayComponent,
    EmptyStateComponent,
    ListCardComponent,
  ],
  providers: [ReviewListStateService],
  template: `
    <div *mbcLoadable="state.reviews$ as reviews; errorMessage: 'Failed to load reviews'">
      <mbc-list-card 
        title="Reviews" 
        [subtitle]="'Customer reviews for this pilot (' + reviews.totalCount + ')'">
        @if (reviews.items.length > 0) {
          <div class="reviews-container">
            @for (review of reviews.items; track review.id; let last = $last) {
              <mbc-review-display [review]="review" />
              @if (!last) {
                <mat-divider></mat-divider>
              }
            }
          </div>

          <mbc-pagination
            [start]="reviews.offset + 1"
            [end]="reviews.offset + reviews.items.length"
            [total]="reviews.totalCount"
            [hasPrevious]="reviews.offset > 0"
            [hasNext]="reviews.hasMore"
            (previous)="state.previousPage()"
            (next)="state.nextPage()"
          />
        } @else {
          <mbc-empty-state>
            <p>No reviews yet for this pilot.</p>
          </mbc-empty-state>
        }
      </mbc-list-card>
    </div>
  `,
})
export class ReviewListComponent {
  readonly state = inject(ReviewListStateService);
}

