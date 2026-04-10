import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { DashboardOverview } from './models/dashboard.models';
import { DashboardService } from './services/dashboard.service';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [CurrencyPipe, RouterLink, MatCardModule, MatButtonModule, MatSnackBarModule],
  templateUrl: './dashboard-page.component.html',
  styleUrl: './dashboard-page.component.scss'
})
export class DashboardPageComponent implements OnInit {
  private readonly dashboardService = inject(DashboardService);
  private readonly snackBar = inject(MatSnackBar);

  cargando = false;

  resumen: DashboardOverview = {
    kpis: {
      activeClients: 0,
      projectsInProgress: 0,
      overdueTasks: 0,
      tasksForToday: 0,
      monthlyIncome: 0,
      monthlyExpense: 0,
      monthlyResult: 0,
      pendingCollections: 0,
      pendingPayments: 0
    },
    priorityTasks: [],
    upcomingCollections: [],
    recentMovements: []
  };

  ngOnInit(): void {
    this.cargarResumen();
  }

  cargarResumen(): void {
    this.cargando = true;

    this.dashboardService.obtenerResumen().subscribe({
      next: (data) => {
        this.resumen = data;
        this.cargando = false;
      },
      error: () => {
        this.cargando = false;
        this.snackBar.open('Ocurrió un error al cargar el inicio', 'Cerrar', { duration: 3500 });
      }
    });
  }

  mostrarEstado(status: string): string {
    if (status === 'EnCurso') return 'En curso';
    return status;
  }
}
