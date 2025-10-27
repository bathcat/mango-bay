import { Injectable, inject, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { BehaviorSubject, combineLatest, Observable, Subject, switchMap, startWith, tap, Subscription } from 'rxjs';
import { Loadable, toLoadable } from '@app/shared/loadable';
import { PageNavigator } from '@app/shared/pagination/page-navigator';
import { CreateOrUpdateSiteRequest, Site } from '@app/shared/schemas';
import { ApiClient } from '@app/core/client';

@Injectable({
  providedIn: 'root',
})
export class SiteListStateService implements OnDestroy {
  private client = inject(ApiClient);
  private pagination = new PageNavigator(inject(ActivatedRoute), inject(Router), 4);
  
  private readonly refresh$ = new BehaviorSubject<void>(undefined);
  private readonly createAction$ = new Subject<CreateOrUpdateSiteRequest>();
  private readonly updateAction$ = new Subject<{ id: string, request: CreateOrUpdateSiteRequest }>();
  private readonly deleteAction$ = new Subject<string>();
  private readonly subscriptions: Subscription[] = [];

  sites$ = combineLatest([
    this.pagination.pageParams$,
    this.refresh$
  ]).pipe(
    switchMap(([{ skip, take }, _]) => toLoadable(this.client.getSites(skip, take)))
  );

  createState$ = this.createAction$.pipe(
    switchMap(request => toLoadable(
      this.client.createSite(request).pipe(
        tap(() => this.refreshSites())
      )
    )),
    startWith(null as Loadable<Site> | null)
  );

  updateState$ = this.updateAction$.pipe(
    switchMap(({ id, request }) => toLoadable(
      this.client.updateSite(id, request).pipe(
        tap(() => this.refreshSites())
      )
    )),
    startWith(null as Loadable<Site> | null)
  );

  deleteState$ = this.deleteAction$.pipe(
    switchMap(id => toLoadable(
      this.client.deleteSite(id).pipe(
        tap(() => this.refreshSites())
      )
    )),
    startWith(null as Loadable<unknown> | null)
  );

  nextPage = () => this.pagination.nextPage();
  previousPage = () => this.pagination.previousPage();

  createSite(request: CreateOrUpdateSiteRequest): void {
    this.createAction$.next(request);
  }

  updateSite(id: string, request: CreateOrUpdateSiteRequest): void {
    this.updateAction$.next({ id, request });
  }

  deleteSite(id: string): void {
    this.deleteAction$.next(id);
  }

  refreshSites(): void {
    this.refresh$.next();
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    this.refresh$.complete();
    this.createAction$.complete();
    this.updateAction$.complete();
    this.deleteAction$.complete();
  }
}
