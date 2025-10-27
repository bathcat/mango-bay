import { Component, input, inject, model } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatSelectModule } from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { Site } from '@app/shared/schemas';
import { SitePickerStateService } from './site-picker-state.service';

@Component({
  selector: 'mbc-site-picker',
  standalone: true,
  imports: [
    CommonModule,
    MatSelectModule,
    MatOptionModule,
    MatFormFieldModule,
    MatIconModule,
  ],
  template: `
    <mat-form-field appearance="outline" class="site-picker-field">
      <mat-select
        [value]="value()"
        (selectionChange)="onSelectionChange($event.value)"
        [placeholder]="placeholder()"
        [disabled]="(state.sites$ | async)?.status === 'loading'"
        aria-label="Select a site"
      >
      <mat-select-trigger>
        @if (value()) {
          {{ value()!.name }}
        }
      </mat-select-trigger>

        @if (state.sites$ | async; as sitesState) {
          @switch (sitesState.status) {
            @case ('loading') {
              <mat-option disabled>
                <mat-icon>hourglass_empty</mat-icon>
                Loading sites...
              </mat-option>
            }
            @case ('error') {
              <mat-option disabled>
                <mat-icon>error</mat-icon>
                {{ sitesState.error }}
              </mat-option>
            }
            @case ('loaded') {
              @for (site of sitesState.value; track site.id) {
                <mat-option [value]="site">
                  <div class="site-option">
                    @if (site.imageUrl) {
                      <img 
                        [src]="site.imageUrl" 
                        [alt]="site.name"
                        class="site-thumbnail"
                      />
                    }
                    <div class="site-info">
                      <div class="site-name">{{ site.name }}</div>
                      <div class="site-island">{{ site.island }}</div>
                    </div>
                  </div>
                </mat-option>
              }
            }
          }
        }
      </mat-select>
      
    </mat-form-field>
  `,
})
export class SitePickerComponent {
  placeholder = input<string>('');
  value = model<Site | null>(null);

  public readonly state = inject(SitePickerStateService);

  onSelectionChange(site: Site | null): void {
    this.value.set(site);
  }
}

