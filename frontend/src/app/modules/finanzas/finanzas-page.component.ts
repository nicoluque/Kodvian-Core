import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';

import { CategoriaFormDialogComponent } from './components/categoria-form-dialog/categoria-form-dialog.component';
import { MovimientoFormDialogComponent } from './components/movimiento-form-dialog/movimiento-form-dialog.component';
import { CategoriaFinanciera, CategoriaFormulario, EstadoMovimiento, FinanzaFiltros, FinanzasLookups, MovimientoDetalle, MovimientoFormulario, MovimientoListado, ResumenMensual, TipoMovimiento } from './models/finanzas.models';
import { FinanzasService } from './services/finanzas.service';

@Component({
  selector: 'app-finanzas-page',
  standalone: true,
  imports: [ReactiveFormsModule, CurrencyPipe, MatCardModule, MatTableModule, MatPaginatorModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule, MatSnackBarModule],
  templateUrl: './finanzas-page.component.html',
  styleUrl: './finanzas-page.component.scss'
})
export class FinanzasPageComponent implements OnInit {
  private readonly finanzasService = inject(FinanzasService);
  private readonly fb = inject(FormBuilder);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  private tipoMovimientoInicial?: TipoMovimiento;

  readonly tipos: TipoMovimiento[] = ['Ingreso', 'Egreso'];
  readonly estados: EstadoMovimiento[] = ['Pendiente', 'Cobrado', 'Pagado', 'Vencido', 'Anulado'];
  readonly columnas = ['movementType', 'category', 'description', 'amount', 'movementDate', 'status', 'actions'];

  readonly filtrosForm = this.fb.group({
    dateFrom: [''],
    dateTo: [''],
    movementType: [''],
    categoryId: [''],
    clientId: [''],
    providerId: [''],
    status: ['']
  });

  movimientos: MovimientoListado[] = [];
  categorias: CategoriaFinanciera[] = [];
  lookups: FinanzasLookups = { categories: [], clients: [], projects: [], providers: [] };
  resumen: ResumenMensual = { monthlyIncome: 0, monthlyExpense: 0, monthlyResult: 0, pendingIncome: 0, pendingExpense: 0 };

  pageNumber = 1;
  pageSize = 10;
  total = 0;
  cargando = false;

  ngOnInit(): void {
    const accion = this.route.snapshot.queryParamMap.get('accion');
    const tipo = this.route.snapshot.queryParamMap.get('tipo')?.toLowerCase();

    if (accion === 'nuevo') {
      if (tipo === 'egreso') {
        this.tipoMovimientoInicial = 'Egreso';
      } else {
        this.tipoMovimientoInicial = 'Ingreso';
      }
    }

    this.cargarTodo();
  }

  cargarTodo(): void {
    this.cargarLookups();
    this.cargarCategorias();
    this.cargarResumen();
    this.cargarMovimientos();
  }

  cargarLookups(): void {
    this.finanzasService.obtenerLookups().subscribe({
      next: (data) => {
        this.lookups = data;
        this.ejecutarAccionInicial();
      },
      error: () => {
        this.snackBar.open('No se pudieron cargar los datos de referencia', 'Cerrar', { duration: 3500 });
        this.ejecutarAccionInicial();
      }
    });
  }

  private ejecutarAccionInicial(): void {
    if (!this.tipoMovimientoInicial) {
      return;
    }

    const tipo = this.tipoMovimientoInicial;
    this.tipoMovimientoInicial = undefined;
    this.router.navigate([], { relativeTo: this.route, queryParams: {}, replaceUrl: true });
    this.abrirDialogMovimiento(tipo);
  }

  cargarCategorias(): void {
    this.finanzasService.obtenerCategorias().subscribe({
      next: (data) => (this.categorias = data),
      error: () => this.snackBar.open('No se pudieron cargar las categorías', 'Cerrar', { duration: 3500 })
    });
  }

  cargarResumen(): void {
    this.finanzasService.obtenerResumenMensual().subscribe({
      next: (data) => (this.resumen = data),
      error: () => this.snackBar.open('No se pudo cargar el resumen mensual', 'Cerrar', { duration: 3500 })
    });
  }

  cargarMovimientos(): void {
    this.cargando = true;
    this.finanzasService.obtenerMovimientos(this.obtenerFiltros()).subscribe({
      next: (result) => {
        this.movimientos = result.items;
        this.total = result.totalCount;
        this.cargando = false;
      },
      error: () => {
        this.movimientos = [];
        this.total = 0;
        this.cargando = false;
        this.snackBar.open('Ocurrió un error al cargar los movimientos', 'Cerrar', { duration: 3500 });
      }
    });
  }

  aplicarFiltros(): void {
    if (!this.rangoFechasValido()) {
      this.snackBar.open('La fecha desde no puede ser mayor que la fecha hasta', 'Cerrar', { duration: 3500 });
      return;
    }

    this.pageNumber = 1;
    this.cargarMovimientos();
  }

  limpiarFiltros(): void {
    this.filtrosForm.reset({ dateFrom: '', dateTo: '', movementType: '', categoryId: '', clientId: '', providerId: '', status: '' });
    this.pageNumber = 1;
    this.cargarMovimientos();
  }

  cambiarPagina(event: PageEvent): void {
    this.pageNumber = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.cargarMovimientos();
  }

  nuevoIngreso(): void {
    this.abrirDialogMovimiento('Ingreso');
  }

  nuevoEgreso(): void {
    this.abrirDialogMovimiento('Egreso');
  }

  editarMovimiento(row: MovimientoListado): void {
    this.finanzasService.obtenerDetalle(row.id).subscribe({
      next: (detalle: MovimientoDetalle) => this.abrirDialogMovimiento(detalle.movementType, detalle),
      error: () => this.snackBar.open('No se pudo cargar el movimiento', 'Cerrar', { duration: 3500 })
    });
  }

  nuevaCategoria(): void {
    const dialogRef = this.dialog.open(CategoriaFormDialogComponent, { width: '520px' });
    dialogRef.afterClosed().subscribe((payload?: CategoriaFormulario) => {
      if (!payload) return;

      this.finanzasService.crearCategoria(payload).subscribe({
        next: () => {
          this.snackBar.open('La categoría se creó correctamente', 'Cerrar', { duration: 3000 });
          this.cargarCategorias();
          this.cargarLookups();
        },
        error: (error) => this.snackBar.open(error?.error?.message ?? 'Ocurrió un error al guardar categoría', 'Cerrar', { duration: 3500 })
      });
    });
  }

  editarCategoria(categoria: CategoriaFinanciera): void {
    const dialogRef = this.dialog.open(CategoriaFormDialogComponent, {
      width: '520px',
      data: { categoria }
    });

    dialogRef.afterClosed().subscribe((payload?: CategoriaFormulario) => {
      if (!payload) return;

      this.finanzasService.actualizarCategoria(categoria.id, payload).subscribe({
        next: () => {
          this.snackBar.open('La categoría se actualizó correctamente', 'Cerrar', { duration: 3000 });
          this.cargarCategorias();
          this.cargarLookups();
        },
        error: (error) => this.snackBar.open(error?.error?.message ?? 'Ocurrió un error al guardar categoría', 'Cerrar', { duration: 3500 })
      });
    });
  }

  private abrirDialogMovimiento(tipo: TipoMovimiento, movimiento?: MovimientoDetalle): void {
    const dialogRef = this.dialog.open(MovimientoFormDialogComponent, {
      width: '980px',
      data: {
        tipoInicial: tipo,
        movimiento,
        categorias: this.lookups.categories,
        clientes: this.lookups.clients,
        proyectos: this.lookups.projects,
        proveedores: this.lookups.providers
      }
    });

    dialogRef.afterClosed().subscribe((payload?: MovimientoFormulario) => {
      if (!payload) return;

      const request = movimiento
        ? this.finanzasService.actualizarMovimiento(movimiento.id, payload)
        : this.finanzasService.crearMovimiento(payload);

      request.subscribe({
        next: (savedMovement) => {
          if (payload.receiptFile) {
            this.finanzasService.subirComprobanteMovimiento(savedMovement.id, payload.receiptFile).subscribe({
              next: () => {
                this.snackBar.open('El movimiento y su comprobante se registraron correctamente', 'Cerrar', { duration: 3000 });
                this.cargarResumen();
                this.cargarMovimientos();
              },
              error: (error) => {
                this.snackBar.open(error?.error?.message ?? 'El movimiento se guardó, pero falló la carga del comprobante', 'Cerrar', { duration: 4000 });
                this.cargarResumen();
                this.cargarMovimientos();
              }
            });
            return;
          }

          this.snackBar.open('El movimiento se registró correctamente', 'Cerrar', { duration: 3000 });
          this.cargarResumen();
          this.cargarMovimientos();
        },
        error: (error) => this.snackBar.open(error?.error?.message ?? 'Ocurrió un error al guardar', 'Cerrar', { duration: 3500 })
      });
    });
  }

  private obtenerFiltros(): FinanzaFiltros {
    return {
      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
      dateFrom: this.filtrosForm.value.dateFrom || undefined,
      dateTo: this.filtrosForm.value.dateTo || undefined,
      movementType: (this.filtrosForm.value.movementType as TipoMovimiento | '') || undefined,
      categoryId: this.filtrosForm.value.categoryId || undefined,
      clientId: this.filtrosForm.value.clientId || undefined,
      providerId: this.filtrosForm.value.providerId || undefined,
      status: (this.filtrosForm.value.status as EstadoMovimiento | '') || undefined
    };
  }

  private rangoFechasValido(): boolean {
    const dateFrom = this.filtrosForm.value.dateFrom || '';
    const dateTo = this.filtrosForm.value.dateTo || '';

    if (!dateFrom || !dateTo) {
      return true;
    }

    return dateFrom <= dateTo;
  }
}
