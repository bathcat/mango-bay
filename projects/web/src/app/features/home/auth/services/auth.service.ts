import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, finalize, map, Observable, shareReplay } from 'rxjs';
import {
  AuthResponse,
  SignInRequest,
  SignUpRequest,
  User,
} from '@app/shared/schemas';
import { TokenStoreService } from './token-store.service';
import { ApiClient } from '@core/client';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly apiClient = inject(ApiClient);
  private readonly tokenStore = inject(TokenStoreService);
  private readonly router = inject(Router);

  private readonly currentUserSubject = new BehaviorSubject<User | null>(null);
  public readonly currentUser$ = this.currentUserSubject.asObservable();
  public readonly isAuthenticated$ = new BehaviorSubject<boolean>(false);
  private refreshInProgress$: Observable<AuthResponse> | null = null;

  constructor() {
    this.initializeAuthState();
  }

  private initializeAuthState(): void {
    if (this.tokenStore.hasTokens()) {
      const user = this.tokenStore.getUser();
      if (user) {
        this.currentUserSubject.next(user);
      }
      this.isAuthenticated$.next(true);
    }
  }

  signUp = (request: SignUpRequest): Observable<AuthResponse> =>
    this.apiClient.signUp(request).pipe(
      map((authResponse) => {
        this.handleAuthResponse(authResponse);
        return authResponse;
      })
    );

  signIn = (request: SignInRequest): Observable<AuthResponse> =>
    this.apiClient.signIn(request).pipe(
      map((authResponse) => {
        this.handleAuthResponse(authResponse);
        return authResponse;
      })
    );

  signOut(): void {
    const refreshToken = this.tokenStore.getRefreshToken();

    if (refreshToken) {
      this.apiClient.signOut(refreshToken).subscribe({
        error: (err) => console.error('Sign out error:', err),
      });
    }

    this.clearAuthState();
    this.router.navigate(['/']);
  }

  refreshToken = (): Observable<AuthResponse> => {
    if (this.refreshInProgress$) {
      return this.refreshInProgress$;
    }

    const refreshToken = this.tokenStore.getRefreshToken();
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    this.refreshInProgress$ = this.apiClient.refreshToken({ refreshToken }).pipe(
      map((authResponse) => {
        this.handleAuthResponse(authResponse);
        return authResponse;
      }),
      finalize(() => {
        this.refreshInProgress$ = null;
      }),
      shareReplay(1)
    );

    return this.refreshInProgress$;
  };

  handleOAuthCallback(accessToken: string, refreshToken: string): void {
    this.tokenStore.setAccessToken(accessToken);
    this.tokenStore.setRefreshToken(refreshToken);
    this.isAuthenticated$.next(true);
  }

  private handleAuthResponse(authResponse: AuthResponse): void {
    this.tokenStore.setAccessToken(authResponse.accessToken);
    this.tokenStore.setRefreshToken(authResponse.refreshToken);
    this.tokenStore.setUser(authResponse.user);
    this.currentUserSubject.next(authResponse.user);
    this.isAuthenticated$.next(true);
  }

  private clearAuthState(): void {
    this.tokenStore.clearTokens();
    this.currentUserSubject.next(null);
    this.isAuthenticated$.next(false);
  }

  getCurrentUser = (): User | null => this.currentUserSubject.value;
}

