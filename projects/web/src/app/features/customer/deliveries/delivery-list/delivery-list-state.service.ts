import { Injectable, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { combineLatest, switchMap, map } from 'rxjs';
import { toLoadable } from '@app/shared/loadable';
import { PageNavigator } from '@app/shared/pagination/page-navigator';
import { ApiClient } from '@app/core/client';

@Injectable()
export class DeliveryListStateService {
  private client = inject(ApiClient);
  private pagination = new PageNavigator(inject(ActivatedRoute), inject(Router), 4);

  deliveries$ = this.pagination.pageParams$.pipe(
    switchMap(({ skip, take }) => {
      const deliveries$ = this.client.getMyDeliveries(skip, take);
      const sites$ = this.client.getSites(0, 999).pipe(map(page => page.items));
      const pilots$ = this.client.getPilots(0, 999).pipe(map(page => page.items));

      return toLoadable(
        combineLatest([deliveries$, sites$, pilots$]).pipe(
          map(([deliveries, sites, pilots]) => ({
            ...deliveries,
            items: deliveries.items.map(delivery => ({
              ...delivery,
              originName: sites.find(s => s.id === delivery.originId)?.name || 'Unknown',
              destinationName: sites.find(s => s.id === delivery.destinationId)?.name || 'Unknown',
              pilotName: pilots.find(p => p.id === delivery.pilotId)?.fullName || 'Unknown',
            }))
          }))
        )
      );
    })
  );

  nextPage = () => this.pagination.nextPage();
  previousPage = () => this.pagination.previousPage();
}
