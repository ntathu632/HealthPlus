import { Routes } from '@angular/router';

export const DOCTOR_ROUTES: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./doctor-dashboard/doctor-dashboard.component').then(m => m.DoctorDashboardComponent),
  },
  {
    path: 'patients',
    loadComponent: () =>
      import('./doctor-patients-list/doctor-patients-list.component').then(m => m.DoctorPatientsListComponent),
  },
  {
    path: 'patients/:id',
    loadComponent: () =>
      import('./patient-profile/patient-profile.component').then(m => m.PatientProfileComponent),
  },
  {
    path: 'appointments',
    loadComponent: () =>
      import('./doctor-appointments-list/doctor-appointments-list.component').then(m => m.DoctorAppointmentsListComponent),
  },
  {
    path: 'earnings',
    loadComponent: () =>
      import('./doctor-earnings/doctor-earnings.component').then(m => m.DoctorEarningsComponent),
  },
];
