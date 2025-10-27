import { Injectable, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { combineLatest, switchMap } from 'rxjs';
import { toLoadable } from '@app/shared/loadable';
import { PageNavigator } from '@app/shared/pagination/page-navigator';
import { routeParam } from '@app/shared/route-observables';
import { ApiClient } from '@app/core/client';

@Injectable()
export class ReviewListStateService {
  private client = inject(ApiClient);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private pagination = new PageNavigator(this.route, this.router, 5);

  reviews$ = combineLatest([
    routeParam(this.route, 'id'),
    this.pagination.pageParams$
  ]).pipe(
    switchMap(([pilotId, { skip, take }]) => 
      toLoadable(this.client.getReviewsByPilotId(pilotId, skip, take))
    )
  );

  nextPage = () => this.pagination.nextPage();
  previousPage = () => this.pagination.previousPage();
}

