import { DatePipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';

import { TareaDetailDialogComponent } from '../tareas/components/tarea-detail-dialog/tarea-detail-dialog.component';
import { EstadoTarea } from '../tareas/models/tareas.models';
import { ProyectoListado, TareaDetalle, TareaListado } from './models/mi-trabajo.models';
import { MiTrabajoService } from './services/mi-trabajo.service';

@Component({
  selector: 'app-mi-trabajo-page',
  standalone: true,
  imports: [MatCardModule, MatTableModule, MatSelectModule, MatButtonModule, MatIconModule, MatSnackBarModule, DatePipe],
  templateUrl: './mi-trabajo-page.component.html',
  styleUrl: './mi-trabajo-page.component.scss'
})
export class MiTrabajoPageComponent implements OnInit {
  private readonly miTrabajoService = inject(MiTrabajoService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);

  readonly columnasProyectos = ['name', 'status', 'priority', 'progress', 'delivery'];
  readonly columnasTareas = ['title', 'project', 'status', 'priority', 'dueDate', 'actions'];
  readonly estados: { value: EstadoTarea; label: string }[] = [
    { value: 'Pendiente', label: 'Pendiente' },
    { value: 'EnCurso', label: 'En curso' },
    { value: 'Bloqueada', label: 'Bloqueada' },
    { value: 'Finalizada', label: 'Finalizada' },
    { value: 'Cancelada', label: 'Cancelada' }
  ];

  proyectos: ProyectoListado[] = [];
  tareas: TareaListado[] = [];
  cargando = false;
  actualizandoId: string | null = null;

  ngOnInit(): void {
    this.cargarDatos();
  }

  get tareasPendientes(): number {
    return this.tareas.filter((tarea) => tarea.status !== 'Finalizada' && tarea.status !== 'Cancelada').length;
  }

  cargarDatos(): void {
    this.cargando = true;
    this.miTrabajoService.obtenerOverview().subscribe({
      next: (data) => {
        this.proyectos = data.projects;
        this.tareas = data.tasks;
        this.cargando = false;
      },
      error: () => {
        this.proyectos = [];
        this.tareas = [];
        this.cargando = false;
        this.snackBar.open('No se pudo cargar tu trabajo asignado', 'Cerrar', { duration: 3500 });
      }
    });
  }

  cambiarEstado(tarea: TareaListado, estado: EstadoTarea): void {
    this.actualizandoId = tarea.id;
    this.miTrabajoService.actualizarEstadoTarea(tarea.id, estado, tarea.kanbanOrder).subscribe({
      next: () => {
        this.actualizandoId = null;
        this.snackBar.open('El estado de la tarea se actualizó correctamente', 'Cerrar', { duration: 3000 });
        this.cargarDatos();
      },
      error: (error) => {
        this.actualizandoId = null;
        this.snackBar.open(error?.error?.message ?? 'No se pudo actualizar el estado', 'Cerrar', { duration: 3500 });
      }
    });
  }

  verDetalle(tarea: TareaListado): void {
    this.miTrabajoService.obtenerDetalleTarea(tarea.id).subscribe({
      next: (detalle: TareaDetalle) => this.dialog.open(TareaDetailDialogComponent, { width: '820px', data: detalle }),
      error: () => this.snackBar.open('No se pudo obtener el detalle de la tarea', 'Cerrar', { duration: 3500 })
    });
  }

  mostrarEstado(value: string): string {
    return value === 'EnCurso' ? 'En curso' : value;
  }
}
