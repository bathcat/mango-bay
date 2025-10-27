import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { map, Observable } from 'rxjs';
import { SiteListStateService } from './site-list-state.service';
import { CreateSiteComponent } from '../../admin/create-site.component';
import { EditSiteComponent } from '../../admin/edit-site.component';
import { UploadSiteImageComponent } from '../../admin/upload-site-image/upload-site-image.component';
import { AuthService } from '@app/features/home/auth/services/auth.service';
import { Site } from '@app/shared/schemas';
import { PaginationComponent } from '@app/shared/pagination/pagination.component';
import { LoadableDirective } from '@app/shared/loadable';
import { ListCardComponent } from '@app/shared/ui/list-card.component';

@Component({
  selector: 'mbc-site-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    PaginationComponent,
    LoadableDirective,
    ListCardComponent,
  ],
  template: `
    <div class="admin-actions">
      @if (isAdmin$ | async) {
        <button mat-raised-button color="primary" (click)="openCreateDialog()">
          <mat-icon>add</mat-icon>
          New Site
        </button>
      }
    </div>

    <mbc-list-card title="Sites" subtitle="Places we serve across the islands">
      <div *mbcLoadable="state.sites$ as sites; errorMessage: 'Failed to load sites'">
        <table
          mat-table
          [dataSource]="sites.items"
          class="sites-table"
        >
          <ng-container matColumnDef="image">
            <th mat-header-cell *matHeaderCellDef>Image</th>
            <td mat-cell *matCellDef="let site">
              @if (site.imageUrl) {
                <img
                  [src]="site.imageUrl"
                  [alt]="site.name"
                  class="site-image"
                />
              }
            </td>
          </ng-container>

          <ng-container matColumnDef="name">
            <th mat-header-cell *matHeaderCellDef>Name</th>
            <td mat-cell *matCellDef="let site">
              <a [routerLink]="['/sites', site.id]" class="site-link">
                {{ site.name }}
              </a>
            </td>
          </ng-container>

          <ng-container matColumnDef="island">
            <th mat-header-cell *matHeaderCellDef>Island</th>
            <td mat-cell *matCellDef="let site">{{ site.island }}</td>
          </ng-container>

          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>Status</th>
            <td mat-cell *matCellDef="let site">{{ site.status }}</td>
          </ng-container>

          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef>Actions</th>
            <td mat-cell *matCellDef="let site">
              <button mat-icon-button color="primary" (click)="openEditDialog(site)" title="Edit site">
                <mat-icon>edit</mat-icon>
              </button>
              <button mat-icon-button color="accent" (click)="openUploadImageDialog(site)" title="Upload image">
                <mat-icon>photo_camera</mat-icon>
              </button>
              <button mat-icon-button color="warn" (click)="confirmDelete(site)" title="Delete site">
                <mat-icon>delete</mat-icon>
              </button>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="(isAdmin$ | async) ? displayedColumnsWithActions : displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: (isAdmin$ | async) ? displayedColumnsWithActions : displayedColumns"></tr>
        </table>

        <mbc-pagination
          [start]="sites.offset + 1"
          [end]="sites.offset + sites.items.length"
          [total]="sites.totalCount"
          [hasPrevious]="sites.offset > 0"
          [hasNext]="sites.hasMore"
          (previous)="state.previousPage()"
          (next)="state.nextPage()"
        />
      </div>
    </mbc-list-card>
  `,
})
export class SiteListComponent {
  public readonly state = inject(SiteListStateService);
  private readonly dialog = inject(MatDialog);
  private readonly authService = inject(AuthService);

  public readonly isAdmin$: Observable<boolean> = this.authService.currentUser$.pipe(
    map(user => user?.role === 'Administrator')
  );

  displayedColumns = ['image', 'name', 'island', 'status'];
  displayedColumnsWithActions = ['image', 'name', 'island', 'status', 'actions'];

  openCreateDialog(): void {
    this.dialog.open(CreateSiteComponent, {
      width: '600px',
      disableClose: true,
    });
  }

  openEditDialog(site: Site): void {
    this.dialog.open(EditSiteComponent, {
      width: '600px',
      disableClose: true,
      data: { site },
    });
  }

  openUploadImageDialog(site: Site): void {
    const dialogRef = this.dialog.open(UploadSiteImageComponent, {
      width: '600px',
      disableClose: true,
      data: { siteId: site.id, siteName: site.name },
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.state.refreshSites();
      }
    });
  }

  confirmDelete(site: Site): void {
    const confirmed = confirm(`Are you sure you want to delete "${site.name}"? This action cannot be undone.`);
    if (confirmed) {
      this.state.deleteSite(site.id);
    }
  }
}

