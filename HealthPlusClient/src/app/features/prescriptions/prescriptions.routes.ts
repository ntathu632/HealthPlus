import { Routes } from '@angular/router';

export const PRESCRIPTION_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./prescriptions-list/prescriptions-list.component').then(
        m => m.PrescriptionsListComponent
      ),
  },
];
