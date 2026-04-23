import { Injectable } from '@angular/core';

import { NavigationItem } from '../../shared/models/navigation-item.model';

@Injectable({
  providedIn: 'root'
})
export class NavigationService {
  private readonly items: NavigationItem[] = [
    { label: 'Inicio', icon: 'home', route: '/dashboard' },
    { label: 'Clientes', icon: 'groups', route: '/clientes' },
    { label: 'Desarrollador', icon: 'engineering', route: '/desarrolladores' },
    { label: 'Proyectos', icon: 'folder_open', route: '/proyectos' },
    { label: 'Tareas', icon: 'task', route: '/tareas' },
    { label: 'Finanzas', icon: 'payments', route: '/finanzas' },
    { label: 'Administración', icon: 'admin_panel_settings', route: '/administracion' }
  ];

  getItems(): NavigationItem[] {
    return this.items;
  }
}
