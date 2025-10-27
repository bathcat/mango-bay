import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Subject, takeUntil } from 'rxjs';
import { ReviewStateService } from './review-state.service';
import { ReviewEditorComponent, ReviewEditorData } from './review-editor/review-editor.component';
import { Rating } from '@app/shared/schemas';

interface CreateReviewDialogData {
  deliveryId: string;
}

@Component({
  selector: 'mbc-create-review',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    ReviewEditorComponent,
  ],
  template: `
    <mbc-review-editor
      [data]="getEditorData()"
      (submit)="onEditorSubmit($event)"
      (cancel)="onEditorCancel()">
    </mbc-review-editor>
  `,
})
export class CreateReviewComponent implements OnInit, OnDestroy {
  private readonly dialogRef = inject(MatDialogRef<CreateReviewComponent>);
  private readonly dialogData = inject<CreateReviewDialogData>(MAT_DIALOG_DATA);
  public readonly reviewState = inject(ReviewStateService);
  private readonly destroy$ = new Subject<void>();

  public readonly deliveryId = this.dialogData.deliveryId;

  ngOnInit(): void {
    this.reviewState.createState$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(state => {
      if (state?.status === 'loaded') {
        this.dialogRef.close(true);
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.reviewState.resetCreate();
  }

  onEditorSubmit(data: { rating: Rating; notes: string }): void {
    this.reviewState.createReview(this.deliveryId, data.rating, data.notes);
  }

  onEditorCancel(): void {
    this.dialogRef.close(false);
  }

  getEditorData(): ReviewEditorData {
    const createState = this.reviewState.createStateSubject.value;
    return {
      mode: 'create',
      deliveryId: this.deliveryId,
      isLoading: createState?.status === 'loading',
      error: createState?.status === 'error' ? (createState.error?.message || 'An error occurred while creating the review') : undefined,
    };
  }
}
