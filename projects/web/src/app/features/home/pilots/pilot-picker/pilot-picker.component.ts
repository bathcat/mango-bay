import { Component, input, inject, model } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatSelectModule } from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { Pilot } from '@app/shared/schemas';
import { PilotPickerStateService } from './pilot-picker-state.service';

@Component({
  selector: 'mbc-pilot-picker',
  standalone: true,
  imports: [
    CommonModule,
    MatSelectModule,
    MatOptionModule,
    MatFormFieldModule,
    MatIconModule,
  ],
  template: `
    <mat-form-field appearance="outline" class="pilot-picker-field">
      <mat-select
        [value]="value()"
        (selectionChange)="onSelectionChange($event.value)"
        [placeholder]="placeholder()"
        [disabled]="(state.pilots$ | async)?.status === 'loading'"
        aria-label="Select a pilot"
      >
      <mat-select-trigger>
        @if (value()) {
          {{ value()!.fullName }}
        }
      </mat-select-trigger>

        @if (state.pilots$ | async; as pilotsState) {
          @switch (pilotsState.status) {
            @case ('loading') {
              <mat-option disabled>
                <mat-icon>hourglass_empty</mat-icon>
                Loading pilots...
              </mat-option>
            }
            @case ('error') {
              <mat-option disabled>
                <mat-icon>error</mat-icon>
                {{ pilotsState.error }}
              </mat-option>
            }
            @case ('loaded') {
              @for (pilot of pilotsState.value; track pilot.id) {
                <mat-option [value]="pilot">
                  <div class="pilot-option">
                    @if (pilot.avatarUrl) {
                      <img 
                        [src]="pilot.avatarUrl" 
                        [alt]="pilot.fullName"
                        class="pilot-avatar"
                      />
                    }
                    <div class="pilot-info">
                      <div class="pilot-name">{{ pilot.fullName }}</div>
                      <div class="pilot-short-name">{{ pilot.shortName }}</div>
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
export class PilotPickerComponent {
  placeholder = input<string>('');
  value = model<Pilot | null>(null);

  public readonly state = inject(PilotPickerStateService);

  onSelectionChange(pilot: Pilot | null): void {
    this.value.set(pilot);
  }
}
