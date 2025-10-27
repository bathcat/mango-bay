import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { SiteListStateService } from '../sites/site-list/site-list-state.service';
import { SiteEditorComponent, SiteEditorData } from './site-editor/site-editor.component';
import { CreateOrUpdateSiteRequest } from '@app/shared/schemas';

@Component({
  selector: 'mbc-create-site',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    SiteEditorComponent,
  ],
  template: `
    <mbc-site-editor
      [data]="editorData"
      (submit)="onEditorSubmit($event)"
      (cancel)="onEditorCancel()">
    </mbc-site-editor>
  `
})
export class CreateSiteComponent implements OnInit, OnDestroy {
  private readonly dialogRef = inject(MatDialogRef<CreateSiteComponent>);
  public readonly siteState = inject(SiteListStateService);
  private readonly router = inject(Router);
  private readonly destroy$ = new Subject<void>();

  public editorData: SiteEditorData = {
    mode: 'create',
    isLoading: false,
  };

  ngOnInit(): void {
    this.siteState.createState$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(state => {
      if (state?.status === 'loading') {
        this.editorData = {
          ...this.editorData,
          isLoading: true,
          error: undefined,
        };
      } else if (state?.status === 'error') {
        this.editorData = {
          ...this.editorData,
          isLoading: false,
          error: state.error?.message || 'An error occurred while creating the site',
        };
      } else if (state?.status === 'loaded') {
        this.dialogRef.close(true);
        this.router.navigate(['/sites', state.value.id]);
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onEditorSubmit(request: CreateOrUpdateSiteRequest): void {
    this.siteState.createSite(request);
  }

  onEditorCancel(): void {
    this.dialogRef.close(false);
  }
}
