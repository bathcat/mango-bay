import { Injectable, inject, OnDestroy } from '@angular/core';
import { BehaviorSubject, Observable, catchError, map, of, Subscription } from 'rxjs';
import { Loadable } from '@app/shared/loadable';
import { ApiClient } from '@app/core/client';
import { DeliveryProof } from '@app/shared/schemas';

@Injectable()
export class CompleteDeliveryStateService implements OnDestroy {
  private client = inject(ApiClient);
  private readonly uploadStateSubject = new BehaviorSubject<Loadable<DeliveryProof> | null>(null);
  private readonly subscriptions: Subscription[] = [];

  readonly uploadState$: Observable<Loadable<DeliveryProof> | null> = this.uploadStateSubject.asObservable();

  get currentUploadState(): Loadable<DeliveryProof> | null {
    return this.uploadStateSubject.value;
  }

  uploadProofOfDelivery(deliveryId: string, file: File): void {
    this.uploadStateSubject.next(Loadable.loading);

    this.subscriptions.push(
      this.client.uploadProofOfDelivery(deliveryId, file).pipe(
        map(proof => Loadable.loaded(proof)),
        catchError(error => of(Loadable.error(error))),
      ).subscribe(this.uploadStateSubject)
    );
  }

  reset(): void {
    this.uploadStateSubject.next(null);
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    this.uploadStateSubject.complete();
  }
}
