import { CommonModule } from '@angular/common';
import { Component, input } from '@angular/core';
import { Payment } from '@app/shared/schemas';

@Component({
  selector: 'mbc-payment-info',
  standalone: true,
  imports: [CommonModule],
  template: `
    <h3>Payment Information</h3>
    <div class="mbc-detail-grid">
      <div class="mbc-detail-item">
        <span class="mbc-detail-label">Amount:</span>
        <span class="mbc-detail-value">\${{ payment().amount.toFixed(2) }} USD</span>
      </div>
      <div class="mbc-detail-item">
        <span class="mbc-detail-label">Payment Status:</span>
        <span class="mbc-detail-value mbc-payment-status" [ngClass]="'payment-' + payment().status.toLowerCase()">{{ payment().status }}</span>
      </div>
      <div class="mbc-detail-item">
        <span class="mbc-detail-label">Transaction ID:</span>
        <span class="mbc-detail-value mbc-transaction-id">{{ payment().transactionId }}</span>
      </div>
    </div>
  `
})
export class PaymentInfoComponent {
  payment = input.required<Payment>();
}
