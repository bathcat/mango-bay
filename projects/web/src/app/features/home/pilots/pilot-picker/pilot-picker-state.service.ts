import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs';
import { toLoadable } from '@app/shared/loadable';
import { ApiClient } from '@app/core/client';

@Injectable({
  providedIn: 'root',
})
export class PilotPickerStateService {
  private readonly client = inject(ApiClient);

  public readonly pilots$ = toLoadable(
    this.client.getPilots(0, 999).pipe(map(page => page.items))
  );
}
