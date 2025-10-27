import { CommonModule } from '@angular/common';
import { Component, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { DeliveryStatusComponent } from './delivery-status.component';
import { PaginationComponent } from '@app/shared/pagination/pagination.component';
import { DeliveryInfo } from './delivery-info';

@Component({
  selector: 'mbc-delivery-table',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatTableModule,
    DeliveryStatusComponent,
    PaginationComponent,
  ],
  template: `
    <table
      mat-table
      [dataSource]="deliveries()"
      class="deliveries-table"
    >
      <ng-container matColumnDef="id">
        <th mat-header-cell *matHeaderCellDef>ID</th>
        <td mat-cell *matCellDef="let delivery">
          <a [routerLink]="['/deliveries', delivery.id]" class="delivery-link">
            {{ truncateId(delivery.id) }}
          </a>
        </td>
      </ng-container>

      <ng-container matColumnDef="route">
        <th mat-header-cell *matHeaderCellDef>Route</th>
        <td mat-cell *matCellDef="let delivery">
          <span class="route-text">
            {{ delivery.originName }} → {{ delivery.destinationName }}
          </span>
        </td>
      </ng-container>

      <ng-container matColumnDef="cargo">
        <th mat-header-cell *matHeaderCellDef>Cargo</th>
        <td mat-cell *matCellDef="let delivery">
          <div class="cargo-info">
            <div class="cargo-description">{{ delivery.cargoDescription }}</div>
            <div class="cargo-weight">{{ delivery.cargoWeightKg }} kg</div>
          </div>
        </td>
      </ng-container>

      <ng-container matColumnDef="scheduledFor">
        <th mat-header-cell *matHeaderCellDef>Scheduled</th>
        <td mat-cell *matCellDef="let delivery">
          {{ delivery.scheduledFor | date:'shortDate' }}
        </td>
      </ng-container>

      <ng-container matColumnDef="status">
        <th mat-header-cell *matHeaderCellDef>Status</th>
        <td mat-cell *matCellDef="let delivery">
          <mbc-delivery-status [status]="delivery.status"></mbc-delivery-status>
        </td>
      </ng-container>

      <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
      <tr
        mat-row
        *matRowDef="let row; columns: displayedColumns"
        class="delivery-row"
      ></tr>
    </table>

    <mbc-pagination
      [start]="paginationStart()"
      [end]="paginationEnd()"
      [total]="totalCount()"
      [hasPrevious]="hasPrevious()"
      [hasNext]="hasNext()"
      (previous)="previous.emit()"
      (next)="next.emit()"
    />
  `,
})
export class DeliveryTableComponent {
  deliveries = input.required<DeliveryInfo[]>();
  totalCount = input.required<number>();
  paginationStart = input.required<number>();
  paginationEnd = input.required<number>();
  hasPrevious = input.required<boolean>();
  hasNext = input.required<boolean>();

  previous = output<void>();
  next = output<void>();

  displayedColumns = ['id', 'route', 'cargo', 'scheduledFor', 'status'];

  truncateId(id: string): string {
    return id.substring(0, 8);
  }
}

