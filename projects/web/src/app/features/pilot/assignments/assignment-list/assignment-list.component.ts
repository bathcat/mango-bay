import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { DeliveryStatusComponent } from '../../../customer/deliveries/delivery-status.component';
import { AssignmentListStateService } from './assignment-list-state.service';
import { PaginationComponent } from '@app/shared/pagination/pagination.component';
import { EmptyStateComponent } from '@app/shared/ui/empty-state.component';
import { LoadableDirective } from '@app/shared/loadable';
import { ListCardComponent } from '@app/shared/ui/list-card.component';

@Component({
  selector: 'mbc-assignment-list',
  standalone: true,
  providers: [AssignmentListStateService],
  imports: [
    CommonModule,
    RouterLink,
    MatTableModule,
    DeliveryStatusComponent,
    PaginationComponent,
    EmptyStateComponent,
    LoadableDirective,
    ListCardComponent,
  ],
  template: `
    <mbc-list-card title="My Assignments" subtitle="Your cargo delivery assignments">
      <div *mbcLoadable="state.assignments$ as assignments; errorMessage: 'Failed to load assignments'; errorSubtitle: 'Unable to retrieve your delivery assignments. Please try again.'">
        @if (assignments.totalCount === 0) {
          <mbc-empty-state>
            <p>No assignments yet.</p>
            <p>Check back later for new delivery assignments.</p>
          </mbc-empty-state>
        }

        @if (assignments.totalCount > 0) {
          <table
            mat-table
            [dataSource]="assignments.items"
            class="assignments-table"
          >
            <ng-container matColumnDef="id">
              <th mat-header-cell *matHeaderCellDef>ID</th>
              <td mat-cell *matCellDef="let assignment">
                <a [routerLink]="['/assignments', assignment.id]" class="assignment-link">
                  {{ truncateId(assignment.id) }}
                </a>
              </td>
            </ng-container>

            <ng-container matColumnDef="route">
              <th mat-header-cell *matHeaderCellDef>Route</th>
              <td mat-cell *matCellDef="let assignment">
                <span class="route-text">
                  {{ assignment.originName }} → {{ assignment.destinationName }}
                </span>
              </td>
            </ng-container>

            <ng-container matColumnDef="cargo">
              <th mat-header-cell *matHeaderCellDef>Cargo</th>
              <td mat-cell *matCellDef="let assignment">
                <div class="cargo-info">
                  <div class="cargo-description">{{ assignment.cargoDescription }}</div>
                  <div class="cargo-weight">{{ assignment.cargoWeightKg }} kg</div>
                </div>
              </td>
            </ng-container>

            <ng-container matColumnDef="scheduledFor">
              <th mat-header-cell *matHeaderCellDef>Scheduled</th>
              <td mat-cell *matCellDef="let assignment">
                {{ assignment.scheduledFor | date:'shortDate' }}
              </td>
            </ng-container>

            <ng-container matColumnDef="status">
              <th mat-header-cell *matHeaderCellDef>Status</th>
              <td mat-cell *matCellDef="let assignment">
                <mbc-delivery-status [status]="assignment.status"></mbc-delivery-status>
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr
              mat-row
              *matRowDef="let row; columns: displayedColumns"
              class="assignment-row"
            ></tr>
          </table>

          <mbc-pagination
            [start]="assignments.offset + 1"
            [end]="assignments.offset + assignments.items.length"
            [total]="assignments.totalCount"
            [hasPrevious]="assignments.offset > 0"
            [hasNext]="assignments.hasMore"
            (previous)="state.previousPage()"
            (next)="state.nextPage()"
          />
        }
      </div>
    </mbc-list-card>
  `,
})
export class AssignmentListComponent {
  public readonly state = inject(AssignmentListStateService);

  displayedColumns = ['id', 'route', 'cargo', 'scheduledFor', 'status'];

  truncateId(id: string): string {
    return id.substring(0, 8);
  }
}
