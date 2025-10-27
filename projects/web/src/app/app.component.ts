import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ErrorToastComponent } from '@app/core/errors/error-toast.component';
import { NavbarComponent } from '@app/shared/ui/navbar.component';

@Component({
  selector: 'mbc-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    ErrorToastComponent,
    NavbarComponent,
  ],
  template: `
    <mbc-navbar />
    <router-outlet />
    <mbc-error-toast />
  `,
  styles: ``,
})
export class AppComponent {}

