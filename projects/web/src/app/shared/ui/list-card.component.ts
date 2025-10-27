import { CommonModule } from '@angular/common';
import { Component, input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'mbc-list-card',
  standalone: true,
  imports: [MatCardModule, CommonModule],
  template: `
    <mat-card>
      <mat-card-header>
        <mat-card-title>{{ title() }}</mat-card-title>
        @if (subtitle()) {
          <mat-card-subtitle>{{ subtitle() }}</mat-card-subtitle>
        }
      </mat-card-header>
      <mat-card-content>
        <ng-content />
      </mat-card-content>
    </mat-card>
  `,
  styles: `
    :host {
      display: block;
      padding: 2rem;
      max-width: var(--mbc-list-max-width);
      margin: 0 auto;
    }
  `
})
export class ListCardComponent {
  title = input.required<string>();
  subtitle = input<string>();
}
