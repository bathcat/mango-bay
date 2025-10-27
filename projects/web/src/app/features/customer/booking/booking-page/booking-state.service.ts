import { Injectable, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { BehaviorSubject, Subject, Observable, map, switchMap, shareReplay } from 'rxjs';
import { ApiClient } from '@app/core/client';
import { JobDetails, CostEstimate, BookingRequest, Delivery, CreditCardInfo, Site, Pilot } from '@app/shared/schemas';
import { validateDifferentLocations, validateFutureDate, convertExpirationToIsoDate } from './booking-helpers';
import { toLoadable, Loadable } from '@app/shared/loadable';

@Injectable()
export class BookingStateService {
  private readonly fb = inject(FormBuilder);
  private readonly client = inject(ApiClient);

  readonly bookingForm: FormGroup;

  private readonly creditCardInfo$ = new BehaviorSubject<CreditCardInfo | null>(null);
  private readonly calculateCostTrigger$ = new Subject<void>();
  private readonly bookDeliveryTrigger$ = new Subject<void>();

  readonly creditCardInfo = this.creditCardInfo$.asObservable();

  readonly costEstimate$ = this.calculateCostTrigger$.pipe(
    map(() => this.buildJobDetails()),
    switchMap(jobDetails => toLoadable(this.client.calculateCost(jobDetails))),
    shareReplay(1)
  );

  readonly isCalculating$ = this.costEstimate$.pipe(
    map(state => state.status === 'loading')
  );

  readonly completedDelivery$ = this.bookDeliveryTrigger$.pipe(
    map(() => this.buildBookingRequest()),
    switchMap(request => toLoadable(this.client.bookDelivery(request))),
    shareReplay(1)
  );

  readonly isBooking$ = this.completedDelivery$.pipe(
    map(state => state.status === 'loading')
  );

  constructor() {
    this.bookingForm = this.fb.group({
      originId: ['', Validators.required],
      destinationId: ['', Validators.required],
      pilotId: ['', Validators.required],
      cargoDescription: ['', [Validators.required, Validators.minLength(3)]],
      cargoWeightKg: [0, [Validators.required, Validators.min(0.1), Validators.max(2000)]],
      scheduledFor: ['', Validators.required],
    }, { validators: [validateDifferentLocations, validateFutureDate] });
  }

  updateOrigin(site: Site | null): void {
    this.bookingForm.patchValue({ originId: site?.id || '' });
  }

  updateDestination(site: Site | null): void {
    this.bookingForm.patchValue({ destinationId: site?.id || '' });
  }

  updatePilot(pilot: Pilot | null): void {
    this.bookingForm.patchValue({ pilotId: pilot?.id || '' });
  }

  updateCreditCardInfo(creditCard: CreditCardInfo): void {
    this.creditCardInfo$.next(creditCard);
  }

  calculateCost(): void {
    this.calculateCostTrigger$.next();
  }

  bookDelivery(): void {
    this.bookDeliveryTrigger$.next();
  }

  private buildJobDetails(): JobDetails {
    return {
      originId: this.bookingForm.value.originId,
      destinationId: this.bookingForm.value.destinationId,
      cargoDescription: this.bookingForm.value.cargoDescription,
      cargoWeightKg: this.bookingForm.value.cargoWeightKg,
      scheduledFor: this.bookingForm.value.scheduledFor.toISOString().split('T')[0],
    };
  }

  private buildBookingRequest(): BookingRequest {
    const creditCard = this.creditCardInfo$.value;
    if (!creditCard) {
      throw new Error('Credit card info is required');
    }

    return {
      pilotId: this.bookingForm.value.pilotId,
      details: this.buildJobDetails(),
      creditCard: {
        ...creditCard,
        expiration: convertExpirationToIsoDate(creditCard.expiration),
      },
    };
  }
}

