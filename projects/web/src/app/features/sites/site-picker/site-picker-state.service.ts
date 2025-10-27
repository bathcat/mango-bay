import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs';
import { toLoadable } from '@app/shared/loadable';
import { ApiClient } from '@app/core/client';

@Injectable({
  providedIn: 'root',
})
export class SitePickerStateService {
  private readonly client = inject(ApiClient);

  public readonly sites$ = toLoadable(
    this.client.getSites(0, 999).pipe(map(page => page.items))
  );
}
