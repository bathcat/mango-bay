import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { MatStepperModule, MatStepper } from '@angular/material/stepper';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { RouterLink } from '@angular/router';
import { Subject, takeUntil, combineLatest, map, filter } from 'rxjs';

import { SitePickerComponent } from '../../../sites/site-picker/site-picker.component';
import { PilotPickerComponent } from '../../../home/pilots/pilot-picker/pilot-picker.component';
import { CreditCardInfoComponent } from '../credit-card-info/credit-card-info.component';
import { BookingStateService } from './booking-state.service';
import { Site, Pilot, CreditCardInfo } from '@app/shared/schemas';
import * as helpers from './booking-helpers';

@Component({
  selector: 'mbc-booking-page',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatStepperModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatProgressSpinnerModule,
    MatCardModule,
    MatDividerModule,
    SitePickerComponent,
    PilotPickerComponent,
    CreditCardInfoComponent,
  ],
  providers: [BookingStateService],
  templateUrl: './booking-page.component.html',
  styleUrl: './booking-page.component.scss',
})
export class BookingPageComponent implements OnInit, OnDestroy {
  readonly state = inject(BookingStateService);

  minDate = helpers.getMinDate();
  private hasAttemptedCalculation = false;
  private touchedFields = new Set<string>();

  private destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.minDate = helpers.getMinDate();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onOriginChange(site: Site | null): void {
    this.state.updateOrigin(site);
    this.touchedFields.add('originId');
  }

  onDestinationChange(site: Site | null): void {
    this.state.updateDestination(site);
    this.touchedFields.add('destinationId');
  }

  onPilotChange(pilot: Pilot | null): void {
    this.state.updatePilot(pilot);
    this.touchedFields.add('pilotId');
  }

  onCreditCardChange(creditCard: CreditCardInfo): void {
    this.state.updateCreditCardInfo(creditCard);
  }

  onFieldTouched(fieldName: string): void {
    this.touchedFields.add(fieldName);
  }

  canProceedToEstimate(): boolean {
    return helpers.canProceedToEstimate(this.state.bookingForm);
  }

  canProceedToBooking$ = combineLatest([
    this.state.creditCardInfo,
    this.state.costEstimate$
  ]).pipe(
    map(([creditCard, costEstimate]) => 
      helpers.canProceedToBooking(creditCard, costEstimate.status === 'loaded' ? costEstimate.value : null)
    )
  );

  calculateCost(stepper: MatStepper): void {
    this.hasAttemptedCalculation = true;
    if (!this.state.bookingForm.valid) return;

    this.state.calculateCost();

    this.state.costEstimate$
      .pipe(
        takeUntil(this.destroy$),
        filter(state => state.status === 'loaded')
      )
      .subscribe({
        next: () => {
          stepper.next();
        }
      });
  }

  goBack(stepper: MatStepper): void {
    stepper.previous();
  }

  bookDelivery(stepper: MatStepper): void {
    if (!this.state.bookingForm.valid) return;

    this.state.bookDelivery();

    this.state.completedDelivery$
      .pipe(
        takeUntil(this.destroy$),
        filter(state => state.status === 'loaded')
      )
      .subscribe({
        next: () => {
          stepper.next();
        }
      });
  }

  bookAnother(): void {
    window.location.reload();
  }

  getFieldError(fieldName: string): string {
    const control = this.state.bookingForm.get(fieldName);
    const shouldShowError = this.hasAttemptedCalculation || this.touchedFields.has(fieldName) || control?.touched;
    return shouldShowError ? helpers.getFieldError(control, fieldName) : '';
  }

  hasSameOriginDestination(): boolean {
    return this.hasAttemptedCalculation && helpers.hasSameOriginDestination(this.state.bookingForm);
  }

  hasInvalidDate(): boolean {
    return this.hasAttemptedCalculation && helpers.hasInvalidDate(this.state.bookingForm);
  }
}
