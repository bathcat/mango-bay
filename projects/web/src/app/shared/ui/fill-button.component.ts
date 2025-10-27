import { Component, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'mbc-fill-button',
  standalone: true,
  imports: [MatButtonModule, MatIconModule, MatTooltipModule],
  template: `
    <button
      mat-mini-fab
      color="primary"
      type="button"
      [matTooltip]="tooltip()"
      [attr.aria-label]="ariaLabel()"
      [disabled]="disabled()"
      (click)="click.emit()">
      <mat-icon>edit</mat-icon>
    </button>
  `
})
export class FillButtonComponent {
  ariaLabel = input.required<string>();
  tooltip = input.required<string>();
  disabled = input<boolean>(false);
  click = output<void>();
}
