import { Component, input, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { Rating } from './rating';

@Component({
  selector: 'mbc-rating',
  standalone: true,
  imports: [MatIconModule],
  template: `
    @for (star of stars; track star) {
      <button 
        type="button"
        class="star-button"
        [class.editable]="mode() === 'editable'"
        [class.filled]="star <= (rating() || 0)"
        [attr.aria-label]="'Rate ' + star + ' star' + (star === 1 ? '' : 's')"
        [attr.aria-checked]="star <= (rating() || 0)"
        [disabled]="mode() === 'readonly'"
        (click)="onStarClick(star)">
        
        <mat-icon>{{ star <= (rating() || 0) ? 'star' : 'star_border' }}</mat-icon>
      </button>
    }
  `,
  host: {
    '[attr.aria-label]': "mode() === 'readonly' ? 'Rating: ' + (rating() || 0) + ' stars' : 'Rate this item'",
    'role': 'radiogroup',
    '[attr.aria-readonly]': "mode() === 'readonly'"
  },
})
export class RatingComponent {
  rating = input<Rating | null>(null);
  mode = input<'readonly' | 'editable'>('readonly');
  
  ratingChange = output<Rating>();
  
  stars = [5, 4, 3, 2, 1];
  
  onStarClick(starValue: number) {
    if (this.mode() === 'readonly') return;
    
    this.ratingChange.emit(starValue as Rating);
  }
}
