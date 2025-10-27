import { Injectable, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { switchMap } from 'rxjs';
import { toLoadable } from '@app/shared/loadable';
import { routeParam } from '@app/shared/route-observables';
import { ApiClient } from '@app/core/client';

@Injectable()
export class PilotDetailStateService {
  private route = inject(ActivatedRoute);
  private client = inject(ApiClient);

  pilot$ = routeParam(this.route, 'id').pipe(
    switchMap(id => toLoadable(this.client.getPilotById(id)))
  );
}
