import { ActivatedRoute } from '@angular/router';
import { Observable } from 'rxjs';
import { map, filter, distinctUntilChanged } from 'rxjs/operators';

export function routeParam(route: ActivatedRoute, key: string): Observable<string> {
  return route.params.pipe(
    map(params => params[key]),
    filter(value => value != null),
    distinctUntilChanged()
  );
}

export function routeQueryParam(route: ActivatedRoute, key: string): Observable<string> {
  return route.queryParams.pipe(
    map(params => params[key] ?? ''),
    distinctUntilChanged()
  );
}

