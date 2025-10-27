import { Injectable, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { BehaviorSubject, combineLatest, switchMap, map, catchError, shareReplay } from 'rxjs';
import { toLoadable, loadDeliveryDetails } from '@app/shared/loadable';
import { routeParam } from '@app/shared/route-observables';
import { ApiClient } from '@app/core/client';

@Injectable()
export class DeliveryDetailsStateService {
  private route = inject(ActivatedRoute);
  private client = inject(ApiClient);

  private readonly refresh$ = new BehaviorSubject<void>(undefined);

  details$ = combineLatest([
    routeParam(this.route, 'id'),
    this.refresh$
  ]).pipe(
    switchMap(([id, _]) =>
      toLoadable(
        loadDeliveryDetails(this.client, id, true).pipe(
          catchError(err => {
            if (err.status === 404) {
              throw new Error('Not found');
            }
            throw err;
          })
        )
      )
    ),
    shareReplay(1)
  );

  hasProofImage$ = this.details$.pipe(
    map(state =>
      state.status === 'loaded' &&
      state.value.delivery.status === 'Delivered' &&
      state.value.proofImageDataUrl !== null
    )
  );

  isDelivered$ = this.details$.pipe(
    map(state =>
      state.status === 'loaded' &&
      state.value.delivery.status === 'Delivered'
    )
  );

  refresh(): void {
    this.refresh$.next();
  }
}
