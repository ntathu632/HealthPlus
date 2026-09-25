import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { TokenService } from './token.service';
import { homeRouteForRoles } from './role-routes';

export const authGuard: CanActivateFn = () => {
  const tokenService = inject(TokenService);
  const router = inject(Router);

  if (tokenService.isLoggedIn()) return true;

  router.navigate(['/auth/login']);
  return false;
};

export const guestGuard: CanActivateFn = () => {
  const tokenService = inject(TokenService);
  const router = inject(Router);

  if (!tokenService.isLoggedIn()) return true;

  router.navigate([homeRouteForRoles(tokenService.getUser()?.roles)]);
  return false;
};

export function roleGuard(allowedRoles: string[]): CanActivateFn {
  return () => {
    const tokenService = inject(TokenService);
    const router = inject(Router);

    if (!tokenService.isLoggedIn()) {
      router.navigate(['/auth/login']);
      return false;
    }

    const roles = tokenService.getUser()?.roles ?? [];
    if (allowedRoles.some(r => roles.includes(r))) return true;

    router.navigate([homeRouteForRoles(roles)]);
    return false;
  };
}
