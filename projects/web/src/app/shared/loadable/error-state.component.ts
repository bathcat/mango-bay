import { Component, Input } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'mbc-error-state',
  template: `
    <mat-card class="error-state-card">
      <mat-card-content class="error-content">
        <div class="error-icon">
          <mat-icon>error_outline</mat-icon>
        </div>
        
        <div class="error-message">
          <h3>{{ message || 'Something went wrong' }}</h3>
          <p>{{ subtitle || 'An unexpected error occurred. Please try again.' }}</p>
        </div>
        
        <div class="error-actions">
          <button 
            mat-raised-button 
            color="primary" 
            (click)="tryAgain()"
            class="try-again-button"
          >
            <mat-icon>refresh</mat-icon>
            Try Again
          </button>
        </div>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .error-state-card {
      text-align: center;
      max-width: 400px;
      margin: 32px auto;
    }

    .error-content {
      padding: 32px 24px;
    }

    .error-icon {
      margin-bottom: 16px;
    }

    .error-icon mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      color: #f44336;
    }

    .error-message h3 {
      margin: 0 0 8px 0;
      color: #333;
      font-weight: 500;
    }

    .error-message p {
      margin: 0 0 24px 0;
      color: #666;
      line-height: 1.5;
    }

    .try-again-button {
      min-width: 140px;
    }

    .try-again-button mat-icon {
      margin-right: 8px;
    }
  `],
  standalone: true,
  imports: [
    MatCardModule,
    MatButtonModule,
    MatIconModule,
  ],
})
export class ErrorStateComponent {
  @Input() message?: string;
  @Input() subtitle?: string;

  tryAgain(): void {
    window.location.reload();
  }
}

