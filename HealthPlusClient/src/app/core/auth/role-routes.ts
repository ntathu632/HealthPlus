export function homeRouteForRoles(roles: string[] | undefined | null): string {
  if (roles?.includes('Admin')) return '/admin';
  if (roles?.includes('Doctor')) return '/doctor';
  return '/dashboard';
}
