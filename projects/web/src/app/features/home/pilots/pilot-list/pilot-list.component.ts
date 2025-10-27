import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { PilotListStateService } from './pilot-list-state.service';
import { PaginationComponent } from '@app/shared/pagination/pagination.component';
import { LoadableDirective } from '@app/shared/loadable';
import { ListCardComponent } from '@app/shared/ui/list-card.component';

@Component({
  selector: 'mbc-pilot-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatTableModule,
    PaginationComponent,
    LoadableDirective,
    ListCardComponent,
  ],
  template: `
    <mbc-list-card title="Pilots" subtitle="Available pilots for cargo delivery">
      <div *mbcLoadable="state.pilots$ as pilots; errorMessage: 'Failed to load pilots'">
        <table
          mat-table
          [dataSource]="pilots.items"
          class="pilots-table"
        >
          <ng-container matColumnDef="avatar">
            <th mat-header-cell *matHeaderCellDef>Avatar</th>
            <td mat-cell *matCellDef="let pilot">
              @if (pilot.avatarUrl) {
                <img
                  [src]="pilot.avatarUrl"
                  [alt]="pilot.fullName"
                  class="pilot-avatar"
                />
              }
            </td>
          </ng-container>

          <ng-container matColumnDef="shortName">
            <th mat-header-cell *matHeaderCellDef>Short Name</th>
            <td mat-cell *matCellDef="let pilot">
              <a [routerLink]="['/pilots', pilot.id]" class="pilot-link">
                {{ pilot.shortName }}
              </a>
            </td>
          </ng-container>

          <ng-container matColumnDef="fullName">
            <th mat-header-cell *matHeaderCellDef>Full Name</th>
            <td mat-cell *matCellDef="let pilot">{{ pilot.fullName }}</td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
        </table>

        <mbc-pagination
          [start]="pilots.offset + 1"
          [end]="pilots.offset + pilots.items.length"
          [total]="pilots.totalCount"
          [hasPrevious]="pilots.offset > 0"
          [hasNext]="pilots.hasMore"
          (previous)="state.previousPage()"
          (next)="state.nextPage()"
        />
      </div>
    </mbc-list-card>
  `,
})
export class PilotListComponent {
  public readonly state = inject(PilotListStateService);

  displayedColumns = ['avatar', 'shortName', 'fullName'];
}

