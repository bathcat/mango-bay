import { Component, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatStepperModule } from '@angular/material/stepper';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'mbc-edit-confirm-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatStepperModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatIconModule,
  ],
  template: `
    <mat-card>
      <mat-card-header>
        <mat-card-title>{{ title() }}</mat-card-title>
        <mat-card-subtitle>{{ subtitle() }}</mat-card-subtitle>
        <button 
          mat-icon-button 
          class="close-button"
          (click)="onCancel()"
          aria-label="Close dialog">
          <mat-icon>close</mat-icon>
        </button>
      </mat-card-header>

      <mat-card-content>
        <mat-stepper [selectedIndex]="currentStep()" linear>
          <mat-step [label]="editStepLabel()" [completed]="canProceed()">
            <ng-content select="[edit-step]"></ng-content>
            
            <div class="step-actions">
              <button mat-button (click)="onCancel()">Cancel</button>
              <button 
                mat-raised-button 
                color="primary" 
                (click)="onNext()"
                [disabled]="!canProceed()">
                Next
              </button>
            </div>
          </mat-step>

          <mat-step [label]="previewStepLabel()">
            <ng-content select="[preview-step]"></ng-content>
            
            @if (isLoading()) {
              <div class="submit-progress">
                <mat-spinner diameter="24"></mat-spinner>
                <span>{{ loadingText() }}</span>
              </div>
            }
            @if (error()) {
              <div class="submit-error">
                <mat-icon color="warn">error</mat-icon>
                <span>{{ error() }}</span>
              </div>
            }

            <div class="step-actions">
              <button mat-button (click)="onBack()">Go Back</button>
              <button 
                mat-raised-button 
                color="primary" 
                (click)="onConfirm()"
                [disabled]="isLoading()">
                {{ submitButtonText() }}
              </button>
            </div>
          </mat-step>
        </mat-stepper>
      </mat-card-content>
    </mat-card>
  `,
})
export class EditConfirmDialogComponent {
  title = input.required<string>();
  subtitle = input.required<string>();
  editStepLabel = input<string>('Edit');
  previewStepLabel = input<string>('Preview & Confirm');
  submitButtonText = input<string>('Submit');
  loadingText = input<string>('Submitting...');
  
  canProceed = input<boolean>(false);
  isLoading = input<boolean>(false);
  error = input<string | null>(null);
  
  submit = output<void>();
  cancel = output<void>();
  
  currentStep = signal(0);
  
  onNext(): void {
    this.currentStep.set(1);
  }
  
  onBack(): void {
    this.currentStep.set(0);
  }
  
  onConfirm(): void {
    this.submit.emit();
  }
  
  onCancel(): void {
    this.cancel.emit();
  }
}
