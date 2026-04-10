import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { ApiResponse, CurrentUser, LoginResponse } from './auth.models';

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  private readonly http = inject(HttpClient);

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http
      .post<ApiResponse<LoginResponse>>('/api/auth/login', { email, password }, { withCredentials: true })
      .pipe(map((response) => response.data));
  }

  logout(): Observable<void> {
    return this.http
      .post<ApiResponse<object>>('/api/auth/logout', {}, { withCredentials: true })
      .pipe(map(() => undefined));
  }

  me(): Observable<CurrentUser> {
    return this.http
      .get<ApiResponse<CurrentUser>>('/api/auth/me', { withCredentials: true })
      .pipe(map((response) => response.data));
  }
}
