import { Routes } from '@angular/router';

export const REMINDER_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./reminders-list/reminders-list.component').then(
        m => m.RemindersListComponent
      ),
  },
];
