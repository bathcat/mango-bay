import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Subject, takeUntil } from 'rxjs';
import { ReviewStateService } from './review-state.service';
import { ReviewEditorComponent, ReviewEditorData } from './review-editor/review-editor.component';
import { Review, Rating } from '@app/shared/schemas';

interface EditReviewDialogData {
  review: Review;
}

@Component({
  selector: 'mbc-edit-review',
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
export class EditReviewComponent implements OnInit, OnDestroy {
  private readonly dialogRef = inject(MatDialogRef<EditReviewComponent>);
  private readonly dialogData = inject<EditReviewDialogData>(MAT_DIALOG_DATA);
  public readonly reviewState = inject(ReviewStateService);
  private readonly destroy$ = new Subject<void>();

  public readonly review = this.dialogData.review;

  ngOnInit(): void {
    this.reviewState.updateState$.pipe(
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
    this.reviewState.resetUpdate();
  }

  onEditorSubmit(data: { rating: Rating; notes: string }): void {
    this.reviewState.updateReview(this.review.id, data.rating, data.notes);
  }

  onEditorCancel(): void {
    this.dialogRef.close(false);
  }

  getEditorData(): ReviewEditorData {
    const updateState = this.reviewState.updateStateSubject.value;
    return {
      mode: 'edit',
      initialData: this.review,
      isLoading: updateState?.status === 'loading',
      error: updateState?.status === 'error' ? (updateState.error?.message || 'An error occurred while updating the review') : undefined,
    };
  }
}
