import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, catchError, map, of, tap } from 'rxjs';

import { AuthApiService } from './auth-api.service';
import { CurrentUser } from './auth.models';

@Injectable({ providedIn: 'root' })
export class AuthSessionService {
  private readonly authApi = inject(AuthApiService);

  private readonly userSubject = new BehaviorSubject<CurrentUser | null>(null);
  private loaded = false;

  readonly user$ = this.userSubject.asObservable();

  get user(): CurrentUser | null {
    return this.userSubject.value;
  }

  login(email: string, password: string): Observable<CurrentUser> {
    return this.authApi.login(email, password).pipe(
      map((response) => response.user),
      tap((user) => {
        this.userSubject.next(user);
        this.loaded = true;
      })
    );
  }

  logout(): Observable<void> {
    return this.authApi.logout().pipe(
      catchError(() => of(undefined)),
      tap(() => this.clearSession())
    );
  }

  ensureSessionLoaded(): Observable<CurrentUser | null> {
    if (this.loaded) {
      return of(this.userSubject.value);
    }

    return this.refreshCurrentUser();
  }

  refreshCurrentUser(): Observable<CurrentUser | null> {
    return this.authApi.me().pipe(
      tap((user) => {
        this.userSubject.next(user);
        this.loaded = true;
      }),
      map((user) => user as CurrentUser | null),
      catchError(() => {
        this.clearSession();
        this.loaded = true;
        return of(null);
      })
    );
  }

  clearSession(): void {
    this.userSubject.next(null);
    this.loaded = true;
  }

  hasPermission(permission: string): Observable<boolean> {
    return this.ensureSessionLoaded().pipe(
      map((user) => user?.permissions.includes(permission) ?? false)
    );
  }
}
