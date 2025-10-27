import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ErrorInfo } from './error.models';
import { ERROR_TYPE_LABELS, ERROR_TYPE_COLORS } from './error.constants';

@Component({
  selector: 'mbc-error-details-dialog',
  template: `
    <div class="error-dialog">
      <div class="error-header">
        <div class="error-type-section">
          <span class="error-type-badge" [style.background-color]="getTypeColor()">
            {{ getTypeLabel() }}
          </span>
          <h2 mat-dialog-title>Error Details</h2>
        </div>
        <button 
          mat-icon-button 
          class="close-button" 
          (click)="close()"
          matTooltip="Close"
        >
          <mat-icon>close</mat-icon>
        </button>
      </div>

      <div mat-dialog-content class="error-content">
        <div class="user-message">
          <strong>Message:</strong>
          <p>{{ error.userMessage }}</p>
        </div>

        <div class="technical-details">
          <div class="technical-header">
            <strong>Technical Details:</strong>
            <button 
              mat-icon-button 
              class="copy-button" 
              (click)="copyToClipboard()"
              matTooltip="Copy to clipboard"
            >
              <mat-icon>content_copy</mat-icon>
            </button>
          </div>
          <div class="technical-text">
            {{ error.technicalDetails }}
          </div>
        </div>

        <div class="context-details" *ngIf="hasContextDetails()">
          <strong>Context:</strong>
          <ul>
            <li><strong>Operation:</strong> {{ error.context.operation }}</li>
            <li *ngIf="error.context.endpoint"><strong>Endpoint:</strong> {{ error.context.endpoint }}</li>
            <li *ngIf="error.context.requestId"><strong>Request ID:</strong> {{ error.context.requestId }}</li>
            <li><strong>Timestamp:</strong> {{ error.context.timestamp | date:'medium' }}</li>
          </ul>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .error-dialog {
      max-width: 100%;
      padding: 2rem;
    }

    .error-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 20px;
      padding-bottom: 16px;
      border-bottom: 1px solid #e0e0e0;
    }

    .error-type-section {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .error-type-badge {
      color: white;
      font-size: 12px;
      font-weight: 500;
      padding: 4px 8px;
      border-radius: 12px;
      text-transform: uppercase;
    }

    .close-button {
      color: #666;
    }

    .error-content {
      max-height: 60vh;
      overflow-y: auto;
    }

    .user-message {
      margin-bottom: 20px;
    }

    .user-message p {
      margin: 8px 0 0 0;
      color: #333;
    }

    .technical-details {
      margin-bottom: 20px;
    }

    .technical-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 8px;
    }

    .copy-button {
      width: 32px;
      height: 32px;
      color: #666;
    }

    .technical-text {
      font-family: 'Courier New', monospace;
      background: #f5f5f5;
      padding: 12px;
      border-radius: 4px;
      white-space: pre-wrap;
      word-break: break-word;
      font-size: 13px;
      line-height: 1.4;
    }

    .context-details {
      background: #f9f9f9;
      padding: 12px;
      border-radius: 4px;
      border-left: 3px solid #2196f3;
    }

    .context-details ul {
      margin: 8px 0 0 0;
      padding-left: 20px;
    }

    .context-details li {
      margin-bottom: 4px;
    }

    .dialog-actions {
      justify-content: flex-end;
      margin-top: 20px;
      padding-top: 16px;
      border-top: 1px solid #e0e0e0;
    }
  `],
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
  ],
})
export class ErrorDetailsDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<ErrorDetailsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public error: ErrorInfo
  ) {}

  close(): void {
    this.dialogRef.close();
  }

  getTypeLabel(): string {
    return ERROR_TYPE_LABELS[this.error.type];
  }

  getTypeColor(): string {
    return ERROR_TYPE_COLORS[this.error.type];
  }

  hasContextDetails(): boolean {
    return !!(this.error.context.operation || this.error.context.endpoint || this.error.context.requestId);
  }

  async copyToClipboard(): Promise<void> {
    try {
      await navigator.clipboard.writeText(this.error.technicalDetails);
    } catch (err) {
      console.error('Failed to copy to clipboard:', err);
    }
  }
}
