import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { Subject, takeUntil } from 'rxjs';
import { UploadSiteImageStateService } from './upload-site-image-state.service';
import { EditConfirmDialogComponent } from '@app/shared/ui/edit-confirm-dialog.component';

@Component({
  selector: 'mbc-upload-site-image',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatIconModule,
    EditConfirmDialogComponent,
  ],
  template: `
    <mbc-edit-confirm-dialog
      title="Upload Site Image"
      subtitle="Upload an image for {{ data.siteName }}"
      editStepLabel="Select Image"
      previewStepLabel="Preview & Confirm"
      submitButtonText="Confirm & Upload"
      loadingText="Uploading image..."
      [canProceed]="canProceed"
      [isLoading]="isLoading"
      [error]="error"
      (submit)="onConfirm()"
      (cancel)="onCancel()">
      
      <div edit-step>
        <div class="upload-step">
          <div class="upload-area" (click)="triggerFileInput()">
            <mat-icon class="upload-icon">cloud_upload</mat-icon>
            <p>Click to select an image file</p>
            <p class="upload-hint">JPEG, PNG, or WebP (max 1MB)</p>
          </div>
          
          <input
            #fileInput
            type="file"
            accept="image/*"
            (change)="onFileSelected($event)"
            style="display: none;"
          />

          @if (selectedFile) {
            <div class="selected-file">
              <mat-icon>check_circle</mat-icon>
              <span>{{ selectedFile.name }}</span>
            </div>
          }
        </div>
      </div>
      
      <div preview-step>
        @if (previewUrl) {
          <div class="image-preview">
            <img [src]="previewUrl" alt="Preview" />
          </div>
        }
      </div>
    </mbc-edit-confirm-dialog>
  `,
})
export class UploadSiteImageComponent implements OnInit, OnDestroy {
  public readonly state = inject(UploadSiteImageStateService);
  private readonly dialogRef = inject(MatDialogRef<UploadSiteImageComponent>);
  public readonly data = inject<{ siteId: string; siteName: string }>(MAT_DIALOG_DATA);
  private readonly destroy$ = new Subject<void>();

  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  public selectedFile: File | null = null;
  public previewUrl: string | null = null;

  ngOnInit(): void {
    this.state.reset();
    
    this.state.uploadState$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(uploadState => {
      if (uploadState && uploadState.status === 'loaded') {
        this.dialogRef.close(true);
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      
      if (!this.validateFile(file)) {
        return;
      }

      this.selectedFile = file;
      this.createPreviewUrl(file);
    }
  }

  onConfirm(): void {
    if (this.selectedFile && this.data.siteId) {
      this.state.uploadSiteImage(this.data.siteId, this.selectedFile);
    }
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }

  triggerFileInput(): void {
    this.fileInput.nativeElement.click();
  }

  private validateFile(file: File): boolean {
    const maxSize = 1048576;
    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];
    const allowedExtensions = ['.jpg', '.jpeg', '.png', '.webp'];

    if (file.size > maxSize) {
      alert(`File size must be less than ${maxSize / 1024 / 1024}MB`);
      return false;
    }

    if (!allowedTypes.includes(file.type)) {
      alert('File must be a JPEG, PNG, or WebP image');
      return false;
    }

    const extension = file.name.toLowerCase().substring(file.name.lastIndexOf('.'));
    if (!allowedExtensions.includes(extension)) {
      alert('File must have a .jpg, .jpeg, .png, or .webp extension');
      return false;
    }

    return true;
  }

  private createPreviewUrl(file: File): void {
    if (this.previewUrl) {
      URL.revokeObjectURL(this.previewUrl);
    }
    this.previewUrl = URL.createObjectURL(file);
  }

  getErrorMessage(uploadState: any): string {
    return uploadState?.error || 'Unknown error';
  }

  get canProceed(): boolean {
    return !!this.selectedFile;
  }

  get isLoading(): boolean {
    return this.state.currentUploadState?.status === 'loading';
  }

  get error(): string | null {
    const uploadState = this.state.currentUploadState;
    return uploadState?.status === 'error' ? this.getErrorMessage(uploadState) : null;
  }
}

