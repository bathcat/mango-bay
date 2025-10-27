import { Component, input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { Review } from '@app/shared/schemas';
import { ReviewDisplayComponent } from './review-display.component';

@Component({
  selector: 'mbc-review-card',
  standalone: true,
  imports: [
    MatCardModule,
    ReviewDisplayComponent,
  ],
  template: `
    <mat-card>
      <mat-card-header>
        <mat-card-title>Customer Review</mat-card-title>
      </mat-card-header>

      <mat-card-content>
        <mbc-review-display [review]="review()" />
      </mat-card-content>
    </mat-card>
  `,
})
export class ReviewCardComponent {
  review = input.required<Review>();
}

