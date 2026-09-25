import { Routes } from '@angular/router';
import { authGuard, guestGuard, roleGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/landing/landing.component').then(m => m.LandingComponent),
  },
  {
    path: 'auth',
    canActivate: [guestGuard],
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES),
  },
  {
    path: 'gioi-thieu',
    loadComponent: () => import('./features/info/about/about.component').then(m => m.AboutComponent),
  },
  {
    path: 'tinh-nang',
    loadComponent: () => import('./features/info/features-page/features-page.component').then(m => m.FeaturesPageComponent),
  },
  {
    path: 'cau-hoi-thuong-gap',
    loadComponent: () => import('./features/info/faq/faq.component').then(m => m.FaqComponent),
  },
  {
    path: 'lien-he',
    loadComponent: () => import('./features/info/contact/contact.component').then(m => m.ContactComponent),
  },
  {
    path: 'dieu-khoan-su-dung',
    loadComponent: () => import('./features/info/terms/terms.component').then(m => m.TermsComponent),
  },
  {
    path: 'chinh-sach-bao-mat',
    loadComponent: () => import('./features/info/privacy/privacy.component').then(m => m.PrivacyComponent),
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/main-layout/main-layout.component').then(m => m.MainLayoutComponent),
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
      },
      {
        path: 'health-records',
        loadChildren: () => import('./features/health-records/health-records.routes').then(m => m.HEALTH_RECORD_ROUTES),
      },
      {
        path: 'medical-history',
        loadChildren: () => import('./features/medical-history/medical-history.routes').then(m => m.MEDICAL_HISTORY_ROUTES),
      },
      {
        path: 'vaccines',
        loadChildren: () => import('./features/vaccines/vaccines.routes').then(m => m.VACCINE_ROUTES),
      },
      {
        path: 'prescriptions',
        loadChildren: () => import('./features/prescriptions/prescriptions.routes').then(m => m.PRESCRIPTION_ROUTES),
      },
      {
        path: 'reminders',
        loadChildren: () => import('./features/reminders/reminders.routes').then(m => m.REMINDER_ROUTES),
      },
      {
        path: 'appointments',
        loadChildren: () => import('./features/appointments/appointments.routes').then(m => m.APPOINTMENT_ROUTES),
      },
      {
        path: 'payments',
        loadComponent: () => import('./features/payments/payments-list/payments-list.component').then(m => m.PaymentsListComponent),
      },
      {
        path: 'ai-chat',
        loadComponent: () => import('./features/ai-chat/ai-chat.component').then(m => m.AiChatComponent),
      },
      {
        path: 'profile',
        loadComponent: () => import('./features/profile/profile.component').then(m => m.ProfileComponent),
      },
      {
        path: 'admin',
        canActivate: [roleGuard(['Admin'])],
        loadChildren: () => import('./features/admin/admin.routes').then(m => m.ADMIN_ROUTES),
      },
      {
        path: 'doctor',
        canActivate: [roleGuard(['Doctor'])],
        loadChildren: () => import('./features/doctor/doctor.routes').then(m => m.DOCTOR_ROUTES),
      },
    ],
  },
  { path: '**', redirectTo: 'dashboard' },
];
