import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { DeliveryStatus } from '@app/shared/schemas';

interface StatusConfig {
  icon: string;
  class: string;
  tooltip: string;
  label: string;
}

@Component({
  selector: 'mbc-delivery-status',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatTooltipModule],
  template: `
    <div class="status-badge" [ngClass]="statusConfig.class" [matTooltip]="statusConfig.tooltip">
      <mat-icon class="status-icon">{{ statusConfig.icon }}</mat-icon>
      <span class="status-label">{{ statusConfig.label }}</span>
    </div>
  `,
})
export class DeliveryStatusComponent {
  status = input.required<DeliveryStatus>();

  get statusConfig(): StatusConfig {
    const configs: Record<DeliveryStatus, StatusConfig> = {
      Pending: {
        icon: 'schedule',
        class: 'status-pending',
        tooltip: 'Payment is pending',
        label: 'Pending',
      },
      Confirmed: {
        icon: 'check_circle',
        class: 'status-confirmed',
        tooltip: 'Payment completed, delivery confirmed',
        label: 'Confirmed',
      },
      InTransit: {
        icon: 'flight',
        class: 'status-in-transit',
        tooltip: 'Cargo is currently in transit',
        label: 'In Transit',
      },
      Delivered: {
        icon: 'done_all',
        class: 'status-delivered',
        tooltip: 'Cargo has been delivered',
        label: 'Delivered',
      },
      Cancelled: {
        icon: 'cancel',
        class: 'status-cancelled',
        tooltip: 'Booking has been cancelled',
        label: 'Cancelled',
      },
    };
    return configs[this.status()];
  }
}
