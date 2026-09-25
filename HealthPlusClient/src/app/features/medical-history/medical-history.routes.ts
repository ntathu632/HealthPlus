import { Routes } from '@angular/router';

export const MEDICAL_HISTORY_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./medical-history-list/medical-history-list.component').then(
        m => m.MedicalHistoryListComponent
      ),
  },
];
