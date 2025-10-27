import { ActivatedRoute, Router } from '@angular/router';
import { map, Observable } from 'rxjs';
import { routeQueryParam } from '../route-observables';

export interface PageParams {
  skip: number;
  take: number;
}

const param = 'page';

const parsePageNumber = (page: string): number => parseInt(page || '1', 10);

export class PageNavigator {
  readonly pageParams$: Observable<PageParams>;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private pageSize: number
  ) {
    this.pageParams$ = routeQueryParam(this.route, param).pipe(
      map(parsePageNumber),
      map(page => ({ skip: (page - 1) * this.pageSize, take: this.pageSize }))
    );
  }

  nextPage(): void {
    const currentPage = this.getCurrentPage();
    this.navigate(currentPage + 1);
  }

  previousPage(): void {
    const currentPage = this.getCurrentPage();
    if (currentPage > 1) {
      this.navigate(currentPage - 1);
    }
  }

  private navigate(page: number): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { page },
      queryParamsHandling: 'merge',
    });
  }

  private getCurrentPage(): number {
    return parsePageNumber(this.route.snapshot.queryParams[param]);
  }
}

