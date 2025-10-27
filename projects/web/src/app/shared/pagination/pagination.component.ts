import { Component, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'mbc-pagination',
  standalone: true,
  imports: [MatButtonModule],
  template: `
    <button
      mat-raised-button
      (click)="previous.emit()"
      [disabled]="!hasPrevious()"
    >
      Previous
    </button>
    <span class="pagination-info">
      Showing {{ start() }} - {{ end() }} of {{ total() }}
    </span>
    <button
      mat-raised-button
      (click)="next.emit()"
      [disabled]="!hasNext()"
    >
      Next
    </button>
  `,
})
export class PaginationComponent {
  start = input.required<number>();
  end = input.required<number>();
  total = input.required<number>();
  hasPrevious = input<boolean>(false);
  hasNext = input<boolean>(false);
  
  previous = output<void>();
  next = output<void>();
}

