import { Injectable, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { combineLatest, switchMap, map } from 'rxjs';
import { toLoadable } from '@app/shared/loadable';
import { PageNavigator } from '@app/shared/pagination/page-navigator';
import { ApiClient } from '@app/core/client';

@Injectable()
export class AssignmentListStateService {
  private client = inject(ApiClient);
  private pagination = new PageNavigator(inject(ActivatedRoute), inject(Router), 4);

  assignments$ = this.pagination.pageParams$.pipe(
    switchMap(({ skip, take }) => {
      const assignments$ = this.client.getMyAssignments(skip, take);
      const sites$ = this.client.getSites(0, 999).pipe(map(page => page.items));
      const pilots$ = this.client.getPilots(0, 999).pipe(map(page => page.items));

      return toLoadable(
        combineLatest([assignments$, sites$, pilots$]).pipe(
          map(([assignments, sites, pilots]) => ({
            ...assignments,
            items: assignments.items.map(assignment => ({
              ...assignment,
              originName: sites.find(s => s.id === assignment.originId)?.name || 'Unknown',
              destinationName: sites.find(s => s.id === assignment.destinationId)?.name || 'Unknown',
              pilotName: pilots.find(p => p.id === assignment.pilotId)?.fullName || 'Unknown',
            }))
          }))
        )
      );
    })
  );

  nextPage = () => this.pagination.nextPage();
  previousPage = () => this.pagination.previousPage();
}
