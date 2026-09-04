import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { guestGuard } from './guards/guest.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'projects' },
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./components/auth/login/login').then((m) => m.Login)
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./components/auth/register/register').then((m) => m.Register)
  },
  {
    path: 'projects',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./components/projects/project-list/project.list').then((m) => m.ProjectList)
  },
  {
    path: 'projects/new',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./components/projects/project-form/project.form').then((m) => m.ProjectForm)
  },
  {
    path: 'projects/:id/edit',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./components/projects/project-form/project.form').then((m) => m.ProjectForm)
  },
  { path: '**', redirectTo: 'projects' }
];