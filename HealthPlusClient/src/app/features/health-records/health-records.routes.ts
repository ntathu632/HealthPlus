import { Routes } from '@angular/router';

export const HEALTH_RECORD_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./health-records-list/health-records-list.component').then(
        m => m.HealthRecordsListComponent
      ),
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./health-record-detail/health-record-detail.component').then(
        m => m.HealthRecordDetailComponent
      ),
  },
];
