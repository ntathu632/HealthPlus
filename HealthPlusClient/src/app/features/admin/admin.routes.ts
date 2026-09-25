import { Routes } from '@angular/router';

export const ADMIN_ROUTES: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'dashboard',
    loadComponent: () => import('./admin-dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent),
  },
  {
    path: 'users',
    loadComponent: () => import('./admin-users-list/admin-users-list.component').then(m => m.AdminUsersListComponent),
  },
  {
    path: 'assignments',
    loadComponent: () => import('./assignments-list/assignments-list.component').then(m => m.AssignmentsListComponent),
  },
  {
    path: 'appointments',
    loadComponent: () => import('./admin-appointments-list/admin-appointments-list.component').then(m => m.AdminAppointmentsListComponent),
  },
  {
    path: 'roles',
    loadComponent: () => import('./roles-permissions/roles-permissions.component').then(m => m.RolesPermissionsComponent),
  },
  {
    path: 'audit-logs',
    loadComponent: () => import('./audit-logs-list/audit-logs-list.component').then(m => m.AuditLogsListComponent),
  },
  {
    path: 'settings',
    loadComponent: () => import('./system-settings-list/system-settings-list.component').then(m => m.SystemSettingsListComponent),
  },
];
