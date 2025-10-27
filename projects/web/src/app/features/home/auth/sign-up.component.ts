import { Component, inject } from '@angular/core';

import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'mbc-sign-up',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule
],
  template: `
    <mat-card class="sign-up-card">
      <mat-card-header>
        <mat-card-title>Create Account</mat-card-title>
      </mat-card-header>
      <mat-card-content>
        @if (errorMessage) {
          <div class="error-message">{{ errorMessage }}</div>
        }

        <form [formGroup]="signUpForm" (ngSubmit)="onSubmit()">
          <mat-form-field appearance="outline">
            <mat-label>Email</mat-label>
            <input matInput type="email" formControlName="email" />
            @if (signUpForm.controls.email.hasError('required')) {
              <mat-error>Email is required</mat-error>
            }
            @if (signUpForm.controls.email.hasError('email')) {
              <mat-error>Invalid email format</mat-error>
            }
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Nickname</mat-label>
            <input matInput type="text" formControlName="nickname" />
            @if (signUpForm.controls.nickname.hasError('required')) {
              <mat-error>Nickname is required</mat-error>
            }
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Password</mat-label>
            <input matInput type="password" formControlName="password" />
            @if (signUpForm.controls.password.hasError('required')) {
              <mat-error>Password is required</mat-error>
            }
            @if (signUpForm.controls.password.hasError('minlength')) {
              <mat-error>Password must be at least 6 characters</mat-error>
            }
          </mat-form-field>

          <button
            mat-raised-button
            color="primary"
            type="submit"
            [disabled]="signUpForm.invalid || isLoading"
          >
            {{ isLoading ? 'Creating account...' : 'Sign Up' }}
          </button>
        </form>

        <div class="divider">
          <span>OR</span>
        </div>



        <div class="sign-in-link">
          Already have an account? <a routerLink="/signin">Sign in</a>
        </div>
      </mat-card-content>
    </mat-card>
  `,
})
export class SignUpComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  errorMessage = '';
  isLoading = false;

  signUpForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    nickname: ['', [Validators.required]],
  });

  onSubmit(): void {
    if (this.signUpForm.invalid) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.signUp(this.signUpForm.getRawValue()).subscribe({
      next: () => {
        this.router.navigate(['/']);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage =
          err.error?.message || 'Sign up failed. Please try again.';
        console.error('Sign up error:', err);
      },
    });
  }

}

