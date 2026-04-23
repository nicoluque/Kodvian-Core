import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';

import { DesarrolladorFormDialogComponent } from './components/desarrollador-form-dialog/desarrollador-form-dialog.component';
import { DesarrolladorExterno, DesarrolladorFormulario, ResumenContratoDesarrollador } from './models/desarrolladores.models';
import { DesarrolladoresService } from './services/desarrolladores.service';

@Component({
  selector: 'app-desarrolladores-page',
  standalone: true,
  imports: [ReactiveFormsModule, MatCardModule, MatTableModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, MatSnackBarModule, CurrencyPipe, DatePipe],
  templateUrl: './desarrolladores-page.component.html',
  styleUrl: './desarrolladores-page.component.scss'
})
export class DesarrolladoresPageComponent implements OnInit {
  private readonly desarrolladoresService = inject(DesarrolladoresService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly fb = inject(FormBuilder);

  readonly columnas = ['fullName', 'email', 'phone', 'taxId', 'status', 'actions'];
  readonly filtrosForm = this.fb.group({ search: [''] });

  desarrolladores: DesarrolladorExterno[] = [];
  filtrados: DesarrolladorExterno[] = [];
  resumenContratos: ResumenContratoDesarrollador[] = [];
  selectedDeveloper: DesarrolladorExterno | null = null;
  summaryYear = new Date().getFullYear();
  cargando = false;
  cargandoResumen = false;

  ngOnInit(): void {
    this.cargarDesarrolladores();
    this.filtrosForm.valueChanges.subscribe(() => this.aplicarFiltroLocal());
  }

  cargarDesarrolladores(): void {
    this.cargando = true;
    this.desarrolladoresService.obtenerListado().subscribe({
      next: (data) => {
        this.desarrolladores = data;
        this.aplicarFiltroLocal();
        this.cargando = false;
      },
      error: () => {
        this.desarrolladores = [];
        this.filtrados = [];
        this.cargando = false;
        this.snackBar.open('No se pudieron cargar los desarrolladores', 'Cerrar', { duration: 3500 });
      }
    });
  }

  nuevoDesarrollador(): void {
    const ref = this.dialog.open(DesarrolladorFormDialogComponent, {
      width: '980px',
      maxWidth: 'calc(100vw - 32px)',
      maxHeight: 'calc(100vh - 32px)',
      autoFocus: false
    });

    ref.afterClosed().subscribe((payload?: DesarrolladorFormulario) => {
      if (!payload) return;

      this.desarrolladoresService.crear(payload).subscribe({
        next: () => {
          this.snackBar.open('Desarrollador creado correctamente', 'Cerrar', { duration: 3000 });
          this.cargarDesarrolladores();
        },
        error: (error) => this.snackBar.open(error?.error?.message ?? 'No se pudo crear el desarrollador', 'Cerrar', { duration: 3500 })
      });
    });
  }

  editarDesarrollador(row: DesarrolladorExterno): void {
    const ref = this.dialog.open(DesarrolladorFormDialogComponent, {
      width: '980px',
      maxWidth: 'calc(100vw - 32px)',
      maxHeight: 'calc(100vh - 32px)',
      autoFocus: false,
      data: { desarrollador: row }
    });

    ref.afterClosed().subscribe((payload?: DesarrolladorFormulario) => {
      if (!payload) return;

      this.desarrolladoresService.actualizar(row.id, payload).subscribe({
        next: () => {
          this.snackBar.open('Desarrollador actualizado correctamente', 'Cerrar', { duration: 3000 });
          this.cargarDesarrolladores();
        },
        error: (error) => this.snackBar.open(error?.error?.message ?? 'No se pudo actualizar el desarrollador', 'Cerrar', { duration: 3500 })
      });
    });
  }

  toggleResumen(row: DesarrolladorExterno, event?: Event): void {
    event?.stopPropagation();

    if (this.selectedDeveloper?.id === row.id) {
      this.selectedDeveloper = null;
      this.resumenContratos = [];
      return;
    }

    this.selectedDeveloper = row;
    this.cargarResumen(row.id);
  }

  private cargarResumen(developerId: string): void {
    this.cargandoResumen = true;
    this.resumenContratos = [];

    this.desarrolladoresService.obtenerResumenContratos(developerId, this.summaryYear).subscribe({
      next: (data) => {
        this.resumenContratos = data;
        this.cargandoResumen = false;
      },
      error: (error) => {
        this.cargandoResumen = false;
        this.snackBar.open(error?.error?.message ?? 'No se pudo cargar el resumen del desarrollador', 'Cerrar', { duration: 3500 });
      }
    });
  }

  obtenerModalidad(value: ResumenContratoDesarrollador): string {
    return value.paymentMode === 'Percentage' ? 'Porcentaje' : 'Monto fijo';
  }

  estadoPago(value: ResumenContratoDesarrollador): string {
    return value.isUpToDate ? 'Al dia' : 'Pendiente';
  }

  private aplicarFiltroLocal(): void {
    const search = (this.filtrosForm.value.search ?? '').trim().toLowerCase();
    if (!search) {
      this.filtrados = [...this.desarrolladores];
      return;
    }

    this.filtrados = this.desarrolladores.filter((x) =>
      x.fullName.toLowerCase().includes(search)
      || (x.email ?? '').toLowerCase().includes(search)
      || (x.phone ?? '').toLowerCase().includes(search)
      || (x.taxId ?? '').toLowerCase().includes(search));
  }
}
