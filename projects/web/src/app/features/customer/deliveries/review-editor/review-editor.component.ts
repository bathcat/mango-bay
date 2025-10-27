import { Component, input, output, ElementRef, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { Subject } from 'rxjs';
import { Review, Rating } from '@app/shared/schemas';
import { EditConfirmDialogComponent } from '@app/shared/ui/edit-confirm-dialog.component';
import { RatingComponent } from '@app/shared/delivery/review/rating.component';
import { RichTextAreaComponent } from '@app/shared/delivery/review/rich-text-area.component';

export interface ReviewEditorData {
  mode: 'create' | 'edit';
  deliveryId?: string;
  initialData?: Review;
  isLoading?: boolean;
  error?: string;
}

const REVIEW_CONSTANTS = {
  MAX_NOTES_LENGTH: 2000,
  EDITOR_HEIGHT_PX: 200,
  EDITOR_LINE_HEIGHT_PX: 20,
  EDITOR_LINES_DEFAULT: 10,
} as const;


@Component({
  selector: 'mbc-review-editor',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatIconModule,
    RatingComponent,
    EditConfirmDialogComponent,
    RichTextAreaComponent,
  ],
  templateUrl: './review-editor.component.html',
  styleUrl: './review-editor.component.scss'
})
export class ReviewEditorComponent implements OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly elementRef = inject(ElementRef);
  private readonly destroy$ = new Subject<void>();

  data = input.required<ReviewEditorData>();
  submit = output<{ rating: Rating; notes: string }>();
  cancel = output<void>();

  public readonly reviewForm: FormGroup;
  public readonly constants = REVIEW_CONSTANTS;
  public notesValue = '';


  constructor() {
    this.reviewForm = this.fb.group({
      rating: [null, [Validators.required]],
      notes: ['', [Validators.maxLength(this.constants.MAX_NOTES_LENGTH)]],
    });
  }

  ngOnInit(): void {
    this.initializeForm();
  }


  private initializeForm(): void {
    const data = this.data();
    if (data.mode === 'edit' && data.initialData) {
      this.reviewForm.patchValue({
        rating: data.initialData.rating,
        notes: data.initialData.notes,
      });
      this.notesValue = data.initialData.notes || '';
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onConfirm(): void {
    if (this.reviewForm.valid) {
      const { rating } = this.reviewForm.value;
      this.submit.emit({ rating, notes: this.notesValue || '' });
    }
  }

  onNotesChange(value: string): void {
    this.notesValue = value;
    this.reviewForm.get('notes')?.setValue(value);
  }

  getTitle(): string {
    return this.data().mode === 'create' ? 'Write a Review' : 'Edit Review';
  }

  getSubtitle(): string {
    return this.data().mode === 'create' 
      ? 'Share your experience with this pilot' 
      : 'Update your review for this pilot';
  }

  getSubmitButtonText(): string {
    return this.data().mode === 'create' ? 'Submit Review' : 'Update Review';
  }

  getLoadingText(): string {
    return this.data().mode === 'create' ? 'Submitting review...' : 'Updating review...';
  }

  get canProceed(): boolean {
    return this.reviewForm.get('rating')?.valid ?? false;
  }

  get isLoading(): boolean {
    return this.data().isLoading ?? false;
  }

  get error(): string | null {
    return this.data().error ?? null;
  }
}
