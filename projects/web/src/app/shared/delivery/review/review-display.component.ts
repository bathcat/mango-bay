import { Component, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { Review } from '@app/shared/schemas';
import { RatingComponent } from './rating.component';
import { Rating } from './rating';
import { RichTextDisplayComponent } from '@app/shared/ui/rich-text-display.component';

@Component({
  selector: 'mbc-review-display',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    RatingComponent,
    RichTextDisplayComponent,
  ],
  template: `
    @if (review()) {
      <div class="review-display">
        <div class="review-header">
          <mbc-rating [rating]="getRating()" mode="readonly"></mbc-rating>
          <div class="review-date">
            <mat-icon>schedule</mat-icon>
            <span>{{ review()!.createdAt | date:'medium' }}</span>
          </div>
        </div>

        @if (review()!.notes) {
          <div class="review-notes">
            <mbc-rich-text-display [content]="review()!.notes"></mbc-rich-text-display>
          </div>
        }
      </div>
    }
  `,
})
export class ReviewDisplayComponent {
  review = input<Review | null>(null);

  getRating(): Rating | null {
    const review = this.review();
    return review ? Rating(review.rating) : null;
  }
}
