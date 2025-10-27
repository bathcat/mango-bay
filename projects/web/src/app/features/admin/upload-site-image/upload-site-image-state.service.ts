import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, catchError, map, of } from 'rxjs';
import { Loadable } from '@app/shared/loadable';
import { ApiClient } from '@app/core/client';

@Injectable({
  providedIn: 'root',
})
export class UploadSiteImageStateService {
  private readonly uploadStateSubject = new BehaviorSubject<Loadable<string> | null>(null);

  public readonly uploadState$: Observable<Loadable<string> | null> = this.uploadStateSubject.asObservable();

  public get currentUploadState(): Loadable<string> | null {
    return this.uploadStateSubject.value;
  }

  constructor(private client: ApiClient) {}

  public uploadSiteImage(siteId: string, file: File): void {
    this.uploadStateSubject.next(Loadable.loading);

    this.client.uploadSiteImage(siteId, file).pipe(
      map(path => Loadable.loaded(path)),
      catchError(error => of(Loadable.error(error))),
    ).subscribe(this.uploadStateSubject);
  }

  public reset(): void {
    this.uploadStateSubject.next(null);
  }
}

