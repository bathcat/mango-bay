import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Subject, takeUntil } from 'rxjs';
import { SiteListStateService } from '../sites/site-list/site-list-state.service';
import { SiteEditorComponent, SiteEditorData } from './site-editor/site-editor.component';
import { CreateOrUpdateSiteRequest, Site } from '@app/shared/schemas';

interface EditSiteDialogData {
  site: Site;
}

@Component({
  selector: 'mbc-edit-site',
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
export class EditSiteComponent implements OnInit, OnDestroy {
  private readonly dialogRef = inject(MatDialogRef<EditSiteComponent>);
  private readonly dialogData = inject<EditSiteDialogData>(MAT_DIALOG_DATA);
  public readonly siteState = inject(SiteListStateService);
  private readonly destroy$ = new Subject<void>();

  public readonly site = this.dialogData.site;
  public editorData: SiteEditorData = {
    mode: 'edit',
    initialData: this.site,
    isLoading: false,
  };

  ngOnInit(): void {
    this.siteState.updateState$.pipe(
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
          error: state.error?.message || 'An error occurred while updating the site',
        };
      } else if (state?.status === 'loaded') {
        this.dialogRef.close(true);
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onEditorSubmit(request: CreateOrUpdateSiteRequest): void {
    this.siteState.updateSite(this.site.id, request);
  }

  onEditorCancel(): void {
    this.dialogRef.close(false);
  }
}
