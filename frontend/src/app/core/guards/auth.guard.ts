import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { map } from 'rxjs';

import { AuthSessionService } from '../auth/auth-session.service';

export const authGuard: CanActivateFn = () => {
  const session = inject(AuthSessionService);
  const router = inject(Router);

  return session.ensureSessionLoaded().pipe(
    map((user) => user ? true : router.createUrlTree(['/login']))
  );
};
