import { Component, inject, OnInit, viewChild } from '@angular/core';

import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatChipsModule } from '@angular/material/chips';
import { AuthService } from './services/auth.service';
import { SampleDataButtonComponent } from '../../../shared/ui/sample-data-button.component';
import { TestAccountsMenuComponent } from './test-accounts-menu.component';

const password = 'One2three';

@Component({
  selector: 'mbc-sign-in',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatChipsModule,
    SampleDataButtonComponent,
    TestAccountsMenuComponent
  ],
  template: `
  <mat-card class="sign-in-card">
    <mat-card-header>
      <div class="header-content">
        <mat-card-title>Sign In</mat-card-title>
        @if (testMenuRef()?.menu()) {
          <mbc-sample-data-button 
            tooltip="Test logins"
            [menu]="testMenuRef()?.menu()">
          </mbc-sample-data-button>
        }
      </div>
    </mat-card-header>
    <mat-card-content>
      @if (errorMessage) {
        <div class="error-message">{{ errorMessage }}</div>
      }

      <form [formGroup]="signInForm" (ngSubmit)="onSubmit()">
        <mat-form-field appearance="outline">
          <mat-label>Email</mat-label>
          <input matInput type="email" formControlName="email" autocomplete="email" />
          @if (signInForm.controls.email.hasError('required')) {
            <mat-error>Email is required</mat-error>
          }
          @if (signInForm.controls.email.hasError('email')) {
            <mat-error>Invalid email format</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Password</mat-label>
          <input matInput type="password" formControlName="password" autocomplete="current-password" />
          @if (signInForm.controls.password.hasError('required')) {
            <mat-error>Password is required</mat-error>
          }
        </mat-form-field>

        <button
          mat-raised-button
          color="primary"
          type="submit"
          [disabled]="signInForm.invalid || isLoading">
          {{ isLoading ? 'Signing in...' : 'Sign In' }}
        </button>
      </form>

      <div class="divider">
        <span>OR</span>
      </div>


      <div class="sign-up-link">
        Don't have an account? <a routerLink="/signup">Sign up</a>
      </div>
    </mat-card-content>
  </mat-card>

  <mbc-test-accounts-menu
    #testMenu
    (fill)="fillCredentials($event.email, $event.password)">
  </mbc-test-accounts-menu>
  `,
})
export class SignInComponent  {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  testMenuRef = viewChild<TestAccountsMenuComponent>('testMenu');

  errorMessage = '';
  isLoading = false;

  

  signInForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });


  onSubmit(): void {
    if (this.signInForm.invalid) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.signIn(this.signInForm.getRawValue()).subscribe({
      next: () => {
        this.router.navigate(['/']);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = 'Invalid email or password';
        console.error('Sign in error:', err);
      },
    });
  }


  fillCredentials(email: string, password: string): void {
    this.signInForm.patchValue({ email, password });
  }
}

