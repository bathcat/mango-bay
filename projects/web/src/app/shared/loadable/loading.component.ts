import { Component } from '@angular/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'mbc-loading',
  standalone: true,
  imports: [MatProgressSpinnerModule],
  template: `<mat-spinner></mat-spinner>`,
})
export class LoadingComponent {}

