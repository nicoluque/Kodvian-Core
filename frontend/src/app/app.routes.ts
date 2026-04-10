import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./login-page.component').then((m) => m.LoginPageComponent)
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/main-layout.component').then((m) => m.MainLayoutComponent),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'dashboard', loadChildren: () => import('./modules/dashboard/dashboard.routes').then((m) => m.DASHBOARD_ROUTES) },
      { path: 'clientes', loadChildren: () => import('./modules/clientes/clientes.routes').then((m) => m.CLIENTES_ROUTES) },
      { path: 'proyectos', loadChildren: () => import('./modules/proyectos/proyectos.routes').then((m) => m.PROYECTOS_ROUTES) },
      { path: 'tareas', loadChildren: () => import('./modules/tareas/tareas.routes').then((m) => m.TAREAS_ROUTES) },
      { path: 'finanzas', loadChildren: () => import('./modules/finanzas/finanzas.routes').then((m) => m.FINANZAS_ROUTES) },
      { path: 'administracion', loadChildren: () => import('./modules/administracion/administracion.routes').then((m) => m.ADMINISTRACION_ROUTES) }
    ]
  },
  { path: '**', redirectTo: 'login' }
];
