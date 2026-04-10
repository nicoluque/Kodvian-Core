import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { map } from 'rxjs';

import { AuthSessionService } from '../auth/auth-session.service';

const ADMINISTRATION_READ = 'administration.read';

export const administrationGuard: CanActivateFn = () => {
  const session = inject(AuthSessionService);
  const router = inject(Router);

  return session.hasPermission(ADMINISTRATION_READ).pipe(
    map((hasPermission) => hasPermission ? true : router.createUrlTree(['/dashboard']))
  );
};
