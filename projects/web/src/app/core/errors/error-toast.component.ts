import { Component, OnDestroy, OnInit } from '@angular/core';
import { MatSnackBar, MatSnackBarRef } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { Subject, takeUntil } from 'rxjs';
import { ErrorService } from './error.service';
import { ErrorInfo } from './error.models';
import { ERROR_CONSTANTS, ERROR_TYPE_LABELS, ERROR_TYPE_COLORS } from './error.constants';
import { ErrorDetailsDialogComponent } from './error-details-dialog.component';

@Component({
  selector: 'mbc-error-toast',
  template: '',
  standalone: true,
})
export class ErrorToastComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();
  private currentSnackBarRef: MatSnackBarRef<any> | null = null;

  constructor(
    private errorService: ErrorService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.errorService.error$
      .pipe(takeUntil(this.destroy$))
      .subscribe(error => this.showErrorToast(error));
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private showErrorToast(error: ErrorInfo): void {
    if (this.currentSnackBarRef) {
      this.currentSnackBarRef.dismiss();
    }

    this.currentSnackBarRef = this.snackBar.open(
      error.userMessage,
      'Learn More',
      {
        duration: ERROR_CONSTANTS.TOAST_AUTO_DISMISS_MS,
        horizontalPosition: 'center',
        verticalPosition: 'bottom',
        panelClass: ['error-toast-panel'],
      }
    );

    this.currentSnackBarRef.onAction().subscribe(() => {
      this.openErrorDetailsDialog(error);
    });
  }

  private openErrorDetailsDialog(error: ErrorInfo): void {
    this.dialog.open(ErrorDetailsDialogComponent, {
      data: error,
      width: '500px',
      maxWidth: '90vw',
    });
  }
}

