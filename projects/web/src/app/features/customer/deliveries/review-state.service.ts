import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, catchError, map, of } from 'rxjs';
import { Loadable } from '@app/shared/loadable';
import { ApiClient } from '@app/core/client';
import { Review, Rating } from '@app/shared/schemas';

@Injectable({
  providedIn: 'root',
})
export class ReviewStateService {
  public readonly createStateSubject = new BehaviorSubject<Loadable<Review> | null>(null);
  public readonly updateStateSubject = new BehaviorSubject<Loadable<Review> | null>(null);

  public readonly createState$: Observable<Loadable<Review> | null> = this.createStateSubject.asObservable();
  public readonly updateState$: Observable<Loadable<Review> | null> = this.updateStateSubject.asObservable();

  constructor(private client: ApiClient) {}

  public createReview(deliveryId: string, rating: Rating, notes: string): void {
    this.createStateSubject.next(Loadable.loading);

    this.client.createReview(deliveryId, rating, notes).pipe(
      map(review => Loadable.loaded(review)),
      catchError(error => of(Loadable.error(error))),
    ).subscribe(this.createStateSubject);
  }

  public updateReview(reviewId: string, rating: Rating, notes: string): void {
    this.updateStateSubject.next(Loadable.loading);

    this.client.updateReview(reviewId, rating, notes).pipe(
      map(review => Loadable.loaded(review)),
      catchError(error => of(Loadable.error(error))),
    ).subscribe(this.updateStateSubject);
  }

  public resetCreate(): void {
    this.createStateSubject.next(null);
  }

  public resetUpdate(): void {
    this.updateStateSubject.next(null);
  }

  public reset(): void {
    this.createStateSubject.next(null);
    this.updateStateSubject.next(null);
  }
}
