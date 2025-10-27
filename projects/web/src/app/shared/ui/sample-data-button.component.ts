import { Component, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';

@Component({
  selector: 'mbc-sample-data-button',
  standalone: true,
  imports: [MatIconModule, MatTooltipModule, MatMenuModule],
  template: `
    <button
      type="button"
      class="trigger-button"
      [matMenuTriggerFor]="menu()"
      [matTooltip]="tooltip()"
      [attr.aria-label]="tooltip()"
    >
      <mat-icon>construction</mat-icon>
    </button>
  `,
})
export class SampleDataButtonComponent {
  tooltip = input<string>('Sample data');
  menu = input.required<any>();
}

