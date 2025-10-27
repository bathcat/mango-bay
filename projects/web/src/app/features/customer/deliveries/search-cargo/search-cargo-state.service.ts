import { Injectable, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { combineLatest, switchMap, map, of } from 'rxjs';
import { toLoadable } from '@app/shared/loadable';
import { PageNavigator } from '@app/shared/pagination/page-navigator';
import { ApiClient } from '@app/core/client';

@Injectable()
export class SearchCargoStateService {
  private client = inject(ApiClient);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private pagination = new PageNavigator(this.route, this.router, 10);

  searchTerm$ = this.route.queryParamMap.pipe(
    map(params => params.get('q') || '')
  );

  deliveries$ = combineLatest([this.searchTerm$, this.pagination.pageParams$]).pipe(
    switchMap(([searchTerm, { skip, take }]) => {
      if (!searchTerm.trim()) {
        return toLoadable(of({ items: [], totalCount: 0, offset: 0, countRequested: take, hasMore: false }));
      }

      const deliveries$ = this.client.searchDeliveries(searchTerm, skip, take);
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

  performSearch(searchTerm: string): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { q: searchTerm, skip: 0 },
      queryParamsHandling: 'merge',
    });
  }

  nextPage = () => this.pagination.nextPage();
  previousPage = () => this.pagination.previousPage();
}

