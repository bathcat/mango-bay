import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { Payment } from '@app/shared/schemas';

@Component({
  selector: 'mbc-payment-table',
  standalone: true,
  imports: [CommonModule, MatTableModule],
  template: `
    <table mat-table [dataSource]="payments" class="payment-table">
      <ng-container matColumnDef="amount">
        <th mat-header-cell *matHeaderCellDef>Amount</th>
        <td mat-cell *matCellDef="let payment">\${{ payment.amount.toFixed(2) }}</td>
      </ng-container>

      <ng-container matColumnDef="cardholderName">
        <th mat-header-cell *matHeaderCellDef>Cardholder Name</th>
        <td mat-cell *matCellDef="let payment">{{ payment.creditCard.cardholderName }}</td>
      </ng-container>

      <ng-container matColumnDef="cardNumber">
        <th mat-header-cell *matHeaderCellDef>Card Number</th>
        <td mat-cell *matCellDef="let payment">{{ payment.creditCard.cardNumber }}</td>
      </ng-container>

      <ng-container matColumnDef="status">
        <th mat-header-cell *matHeaderCellDef>Status</th>
        <td mat-cell *matCellDef="let payment">
          <span [class]="'status-badge status-' + payment.status.toLowerCase()">
            {{ payment.status }}
          </span>
        </td>
      </ng-container>

      <ng-container matColumnDef="transactionId">
        <th mat-header-cell *matHeaderCellDef>Transaction ID</th>
        <td mat-cell *matCellDef="let payment">{{ payment.transactionId }}</td>
      </ng-container>

      <ng-container matColumnDef="createdAt">
        <th mat-header-cell *matHeaderCellDef>Created</th>
        <td mat-cell *matCellDef="let payment">{{ payment.createdAt | date: 'short' }}</td>
      </ng-container>

      <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
      <tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
    </table>
  `,
  styles: `
    .payment-table {
      width: 100%;
    }

    .status-badge {
      padding: 0.25rem 0.5rem;
      border-radius: 4px;
      font-size: 0.875rem;
      font-weight: 500;
    }

    .status-pending {
      background-color: #fff3cd;
      color: #856404;
    }

    .status-completed {
      background-color: #d4edda;
      color: #155724;
    }

    .status-failed {
      background-color: #f8d7da;
      color: #721c24;
    }

    .status-refunded {
      background-color: #d1ecf1;
      color: #0c5460;
    }
  `,
})
export class PaymentTableComponent {
  @Input() payments: Payment[] = [];

  displayedColumns = ['amount', 'cardholderName', 'cardNumber', 'status', 'transactionId', 'createdAt'];
}

