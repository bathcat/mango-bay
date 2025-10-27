import { Injectable, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { switchMap } from 'rxjs';
import { toLoadable } from '@app/shared/loadable';
import { PageNavigator } from '@app/shared/pagination/page-navigator';
import { ApiClient } from '@app/core/client';

@Injectable({
  providedIn: 'root',
})
export class PilotListStateService {
  private client = inject(ApiClient);
  private pagination = new PageNavigator(inject(ActivatedRoute), inject(Router), 4);

  pilots$ = this.pagination.pageParams$.pipe(
    switchMap(({ skip, take }) => toLoadable(this.client.getPilots(skip, take)))
  );

  nextPage = () => this.pagination.nextPage();
  previousPage = () => this.pagination.previousPage();
}
