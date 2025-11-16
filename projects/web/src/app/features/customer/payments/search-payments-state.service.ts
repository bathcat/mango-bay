import { Injectable, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { map, switchMap, of } from 'rxjs';
import { toLoadable } from '@app/shared/loadable';
import { ApiClient } from '@app/core/client';

@Injectable()
export class SearchPaymentsStateService {
  private client = inject(ApiClient);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  searchNames$ = this.route.queryParamMap.pipe(
    map(params => {
      const namesParam = params.get('names');
      return namesParam ? namesParam.split(',') : [];
    })
  );

  payments$ = this.searchNames$.pipe(
    switchMap(names => {
      if (names.length === 0) {
        return toLoadable(of([]));
      }

      return toLoadable(this.client.searchPaymentsByCardholderNames(names));
    })
  );

  performSearch(names: string[]): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { names: names.join(',') },
      queryParamsHandling: 'merge',
    });
  }
}

