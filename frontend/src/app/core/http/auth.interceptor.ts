import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

import { AuthSessionService } from '../auth/auth-session.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const session = inject(AuthSessionService);

  const request = req.clone({ withCredentials: true });

  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      const isAuthEndpoint = request.url.includes('/api/auth/login');
      if (error.status === 401 && !isAuthEndpoint) {
        session.clearSession();
        void router.navigate(['/login']);
      }

      return throwError(() => error);
    })
  );
};
