import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, finalize, map, Observable, shareReplay } from 'rxjs';
import {
  AuthWebResponse,
  SignInRequest,
  SignUpRequest,
  User,
} from '@app/shared/schemas';
import { UserStoreService } from './user-store.service';
import { ApiClient } from '@core/client';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly apiClient = inject(ApiClient);
  private readonly userStore = inject(UserStoreService);
  private readonly router = inject(Router);

  private readonly currentUserSubject = new BehaviorSubject<User | null>(null);
  public readonly currentUser$ = this.currentUserSubject.asObservable();
  public readonly isAuthenticated$ = new BehaviorSubject<boolean>(false);
  private refreshInProgress$: Observable<AuthWebResponse> | null = null;

  constructor() {
    this.initializeAuthState();
  }

  private initializeAuthState(): void {
    const user = this.userStore.getUser();
    if (user) {
      this.currentUserSubject.next(user);
      this.isAuthenticated$.next(true);
    }
  }

  signUp = (request: SignUpRequest): Observable<AuthWebResponse> =>
    this.apiClient.signUp(request).pipe(
      map((authResponse) => {
        this.handleAuthResponse(authResponse);
        return authResponse;
      })
    );

  signIn = (request: SignInRequest): Observable<AuthWebResponse> =>
    this.apiClient.signIn(request).pipe(
      map((authResponse) => {
        this.handleAuthResponse(authResponse);
        return authResponse;
      })
    );

  signOut(): void {
    this.apiClient.signOut().subscribe({
      error: (err) => console.error('Sign out error:', err),
    });

    this.clearAuthState();
    this.router.navigate(['/']);
  }

  refreshToken = (): Observable<AuthWebResponse> => {
    if (this.refreshInProgress$) {
      return this.refreshInProgress$;
    }

    this.refreshInProgress$ = this.apiClient.refreshToken().pipe(
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

  private handleAuthResponse(authResponse: AuthWebResponse): void {
    this.userStore.setUser(authResponse.user);
    this.currentUserSubject.next(authResponse.user);
    this.isAuthenticated$.next(true);
  }

  private clearAuthState(): void {
    this.userStore.clearUser();
    this.currentUserSubject.next(null);
    this.isAuthenticated$.next(false);
  }

  getCurrentUser = (): User | null => this.currentUserSubject.value;
}

