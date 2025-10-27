import { Observable, of, startWith, switchMap, filter, map, catchError, forkJoin } from 'rxjs';

export type LoadableStatus = 'loading' | 'loaded' | 'error';

export type Loading = { status: 'loading' };
export type Loaded<T> = { status: 'loaded'; value: T };
export type Error = { status: 'error'; error: any };

export type Loadable<T> = 
  | Loading
  | Loaded<T>
  | Error;

export const Loadable = {
  loading: { status: 'loading' } as Loading,
  loaded: <T>(value: T) => ({ status: 'loaded', value }) as Loaded<T>,
  error: (error: any) => ({ status: 'error', error }) as Error,
};

export function toLoadable<T>(source$: Observable<T>): Observable<Loadable<T>> {
  return source$.pipe(
    map(value => Loadable.loaded(value)),
    catchError(error => of(Loadable.error(error))),
    startWith(Loadable.loading)
  );
}

function createLoadableStream<T, P extends any[] = []>(
  params$: Observable<P | null> | null,
  apiCall: (...params: P) => Observable<T>
): Observable<Loadable<T>> {
  const stream$ = params$ 
    ? params$.pipe(filter(params => params !== null))
    : of([] as unknown as P);
    
  return stream$.pipe(
    switchMap(params => 
      apiCall(...params).pipe(
        map(data => Loadable.loaded(data)),
        catchError(error => of(Loadable.error(error))),
        startWith(Loadable.loading)
      )
    )
  );
}

export function loadable<T>(
  apiCall: () => Observable<T>
): Observable<Loadable<T>> {
  return createLoadableStream(null, apiCall);
}

export function loadableWithId<T>(
  id$: Observable<string | null>,
  apiCall: (id: string) => Observable<T>
): Observable<Loadable<T>> {
  return createLoadableStream(
    id$.pipe(map(id => id ? [id] as [string] : null)),
    apiCall
  );
}

export function loadableWithParams<T, P extends any[]>(
  params$: Observable<P | null>,
  apiCall: (...params: P) => Observable<T>
): Observable<Loadable<T>> {
  return createLoadableStream(params$, apiCall);
}

export interface DeliveryDetails {
  delivery: any;
  origin: any;
  destination: any;
  pilot: any;
  payment: any;
  review: any;
  proofImageDataUrl: string | null;
}

export function loadDeliveryDetails(
  client: any,
  deliveryId: string,
  includeProofImage: boolean = false
): Observable<DeliveryDetails> {
  return client.getDeliveryById(deliveryId).pipe(
    switchMap((delivery: any) => {
      const baseRequests = {
        delivery: of(delivery),
        origin: client.getSiteById(delivery.originId),
        destination: client.getSiteById(delivery.destinationId),
        pilot: client.getPilotById(delivery.pilotId),
        payment: client.getPaymentByDeliveryId(delivery.id),
        review: client.getReviewByDeliveryId(delivery.id),
      };

      if (includeProofImage && delivery.status === 'Delivered') {
        return forkJoin({
          ...baseRequests,
          proofImageDataUrl: client.getProofImageDataUrl(delivery.id).pipe(
            catchError(() => of(null))
          )
        });
      } else {
        return forkJoin({
          ...baseRequests,
          proofImageDataUrl: of(null)
        });
      }
    })
  );
}

