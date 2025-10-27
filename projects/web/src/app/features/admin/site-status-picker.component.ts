import { Component, model } from '@angular/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { SiteStatus, SiteStatusSchema } from '../../shared/schemas';

@Component({
  selector: 'mbc-site-status-picker',
  standalone: true,
  imports: [MatFormFieldModule, MatSelectModule],
  template: `
    <mat-form-field appearance="outline" class="status-field">
      <mat-label>Site Status</mat-label>
      <mat-select [value]="value()" (valueChange)="value.set($event)">
        <mat-option [value]="null">None</mat-option>
        @for (status of statusOptions; track status) {
          <mat-option [value]="status">{{ status }}</mat-option>
        }
      </mat-select>
    </mat-form-field>
  `,
})
export class SiteStatusPickerComponent {
  value = model<SiteStatus | null>(null);

  readonly statusOptions = SiteStatusSchema.options;
}

