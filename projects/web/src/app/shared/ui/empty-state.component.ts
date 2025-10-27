import { Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'mbc-empty-state',
  standalone: true,
  imports: [MatIconModule],
  template: `
    <mat-icon class="empty-icon">inventory_2</mat-icon>
    <ng-content></ng-content>
  `,
})
export class EmptyStateComponent {}

