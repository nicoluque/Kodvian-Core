import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';

import { AuthSessionService } from '../auth/auth-session.service';

export const permissionGuard: CanActivateFn = (route) => {
  const session = inject(AuthSessionService);
  const router = inject(Router);
  const permission = route.data?.['permission'] as string | undefined;

  if (!permission) {
    return true;
  }

  return session.ensureSessionLoaded().pipe(
    map((user) => {
      if (!user) {
        return router.createUrlTree(['/login']);
      }

      return user.permissions.includes(permission) ? true : router.createUrlTree([getFallbackRoute(user)]);
    })
  );
};

function getFallbackRoute(user: { developerId?: string; permissions: string[] }): string {
  if (user.developerId && user.permissions.includes('developer.work.read')) return '/mi-trabajo';
  if (user.permissions.includes('projects.read')) return '/proyectos';
  if (user.permissions.includes('dashboard.read')) return '/dashboard';
  if (user.permissions.includes('clients.read')) return '/clientes';
  if (user.permissions.includes('team.read')) return '/equipo';
  return '/login';
}
