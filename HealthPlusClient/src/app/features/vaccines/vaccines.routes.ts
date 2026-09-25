import { Routes } from '@angular/router';

export const VACCINE_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./vaccines-list/vaccines-list.component').then(
        m => m.VaccinesListComponent
      ),
  },
];
