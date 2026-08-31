import { DatePipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';

import { AuthSessionService } from '../../core/auth/auth-session.service';
import {
  NuevaTareaGitHubDialogComponent,
  NuevaTareaGitHubDialogData
} from './components/nueva-tarea-github-dialog/nueva-tarea-github-dialog.component';
import { MiTrabajoCreateIssueRequest, MiTrabajoIssue, MiTrabajoIssueStatus, MiTrabajoRepositorio } from './models/mi-trabajo.models';
import { MiTrabajoService } from './services/mi-trabajo.service';

const DEVELOPER_ISSUES_WRITE = 'developer.issues.write';
const DEVELOPER_TASKS_STATUS_WRITE = 'developer.tasks.status.write';

@Component({
  selector: 'app-mi-trabajo-page',
  standalone: true,
  imports: [MatCardModule, MatTableModule, MatButtonModule, MatIconModule, MatSnackBarModule, MatDialogModule, MatSelectModule, DatePipe, RouterLink],
  templateUrl: './mi-trabajo-page.component.html',
  styleUrl: './mi-trabajo-page.component.scss'
})
export class MiTrabajoPageComponent implements OnInit {
  private readonly miTrabajoService = inject(MiTrabajoService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);
  private readonly authSession = inject(AuthSessionService);

  readonly columnasRepos = ['repo', 'client', 'status', 'openIssues', 'actions'];
  readonly columnasIssues = ['title', 'repo', 'status', 'priority', 'createdAt', 'actions'];

  repositorios: MiTrabajoRepositorio[] = [];
  issues: MiTrabajoIssue[] = [];
  totalRepositorios = 0;
  totalIssues = 0;
  issuesAbiertas = 0;
  githubNotConnected = false;
  cargando = false;
  sincronizando = false;
  creandoIssue = false;
  puedeCrearIssue = false;
  puedeCambiarEstadoIssue = false;
  actualizandoEstadoIssueId: string | null = null;

  readonly estadosIssue: { value: MiTrabajoIssueStatus; label: string }[] = [
    { value: 'Open', label: 'Abierta' },
    { value: 'Closed', label: 'Cerrada' }
  ];

  ngOnInit(): void {
    this.authSession.hasPermission(DEVELOPER_ISSUES_WRITE).subscribe((allowed: boolean) => {
      this.puedeCrearIssue = allowed;
    });
    this.authSession.hasPermission(DEVELOPER_TASKS_STATUS_WRITE).subscribe((allowed: boolean) => {
      this.puedeCambiarEstadoIssue = allowed;
    });
    this.cargarDatos();
  }

  cargarDatos(): void {
    this.cargando = true;
    forkJoin({
      overview: this.miTrabajoService.obtenerOverview(),
      repos: this.miTrabajoService.obtenerRepositorios(1, 50),
      issues: this.miTrabajoService.obtenerIssues({ pageNumber: 1, pageSize: 50 })
    }).subscribe({
      next: ({ overview, repos, issues }) => {
        this.totalRepositorios = overview.repositoryCount;
        this.totalIssues = overview.totalIssuesCount;
        this.issuesAbiertas = overview.openIssuesCount;
        this.githubNotConnected = overview.gitHubNotConnected;
        this.repositorios = repos.items;
        this.issues = issues.items;
        this.cargando = false;
      },
      error: () => {
        this.repositorios = [];
        this.issues = [];
        this.totalRepositorios = 0;
        this.totalIssues = 0;
        this.issuesAbiertas = 0;
        this.githubNotConnected = false;
        this.cargando = false;
        this.snackBar.open('No se pudo cargar tu trabajo asignado', 'Cerrar', { duration: 3500 });
      }
    });
  }

  mostrarEstadoProyecto(value: string): string {
    if (value === 'EnCurso') return 'En curso';
    if (value === 'Planificacion') return 'Planificación';
    return value;
  }

  mostrarEstadoIssue(status: string): string {
    return status === 'Open' ? 'Abierta' : status === 'Closed' ? 'Cerrada' : status;
  }

  repoLabel(row: MiTrabajoRepositorio): string {
    return row.fullName || `${row.gitHubOwner}/${row.gitHubRepoName}`;
  }

  sincronizar(): void {
    if (this.githubNotConnected) {
      this.snackBar.open('Conectá GitHub en Mi perfil para sincronizar issues', 'Cerrar', { duration: 3500 });
      return;
    }

    this.sincronizando = true;
    this.miTrabajoService.sincronizarIssues().subscribe({
      next: (result) => {
        this.sincronizando = false;
        const total = result.importedCount + result.updatedCount;
        const mensaje =
          total > 0
            ? `Sincronización completada: ${result.importedCount} nuevas, ${result.updatedCount} actualizadas.`
            : 'Sincronización completada. No hay issues nuevas para importar.';
        this.snackBar.open(mensaje, 'Cerrar', { duration: 4000 });
        this.cargarDatos();
      },
      error: (error) => {
        this.sincronizando = false;
        this.snackBar.open(error?.error?.message ?? 'No se pudo sincronizar con GitHub', 'Cerrar', { duration: 3500 });
      }
    });
  }

  abrirNuevaTarea(): void {
    if (this.githubNotConnected) {
      this.snackBar.open('Conectá GitHub en Mi perfil para crear issues', 'Cerrar', { duration: 3500 });
      return;
    }

    if (this.repositorios.length === 0) {
      this.snackBar.open('No tenés repositorios asignados para crear una issue', 'Cerrar', { duration: 3500 });
      return;
    }

    const dialogRef = this.dialog.open<NuevaTareaGitHubDialogComponent, NuevaTareaGitHubDialogData, MiTrabajoCreateIssueRequest>(
      NuevaTareaGitHubDialogComponent,
      {
        width: '560px',
        data: { repositories: this.repositorios }
      }
    );

    dialogRef.afterClosed().subscribe((request) => {
      if (!request) {
        return;
      }

      this.creandoIssue = true;
      this.miTrabajoService.crearIssue(request).subscribe({
        next: (issue: MiTrabajoIssue) => {
          this.creandoIssue = false;
          this.snackBar.open(`Issue #${issue.gitHubIssueNumber} creada en GitHub`, 'Cerrar', { duration: 4000 });
          this.cargarDatos();
        },
        error: (error) => {
          this.creandoIssue = false;
          this.snackBar.open(error?.error?.message ?? 'No se pudo crear la issue en GitHub', 'Cerrar', { duration: 3500 });
        }
      });
    });
  }

  cambiarEstadoIssue(issue: MiTrabajoIssue, nuevoEstado: MiTrabajoIssueStatus): void {
    if (nuevoEstado === issue.status || this.actualizandoEstadoIssueId) {
      return;
    }

    if (this.githubNotConnected) {
      this.snackBar.open('Conectá GitHub en Mi perfil para actualizar issues', 'Cerrar', { duration: 3500 });
      this.issues = [...this.issues];
      return;
    }

    if (nuevoEstado === 'Closed') {
      const confirmed = window.confirm(`¿Cerrar la issue #${issue.gitHubIssueNumber} en GitHub?`);
      if (!confirmed) {
        this.issues = [...this.issues];
        return;
      }
    }

    this.actualizandoEstadoIssueId = issue.id;
    this.miTrabajoService.actualizarEstadoIssue(issue.id, nuevoEstado).subscribe({
      next: (actualizada) => {
        this.actualizandoEstadoIssueId = null;
        issue.status = actualizada.status;
        this.snackBar.open('Estado actualizado en GitHub', 'Cerrar', { duration: 3000 });
        this.cargarDatos();
      },
      error: (error) => {
        this.actualizandoEstadoIssueId = null;
        this.issues = [...this.issues];
        this.snackBar.open(error?.error?.message ?? 'No se pudo actualizar el estado', 'Cerrar', { duration: 3500 });
      }
    });
  }
}
