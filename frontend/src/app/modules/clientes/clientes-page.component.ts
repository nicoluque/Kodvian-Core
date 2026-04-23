import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';

import { ClienteDetailDialogComponent } from './components/cliente-detail-dialog/cliente-detail-dialog.component';
import { ClienteFormDialogComponent } from './components/cliente-form-dialog/cliente-form-dialog.component';
import { ClienteStatusDialogComponent } from './components/cliente-status-dialog/cliente-status-dialog.component';
import { ClienteDetalle, ClienteFormulario, ClienteListado, ESTADO_CLIENTE_OPTIONS, EstadoCliente, PagedResult, getEstadoClienteLabel } from './models/clientes.models';
import { ClientesService } from './services/clientes.service';

@Component({
  selector: 'app-clientes-page',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    CurrencyPipe,
    MatCardModule,
    MatTableModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule
  ],
  templateUrl: './clientes-page.component.html',
  styleUrl: './clientes-page.component.scss'
})
export class ClientesPageComponent implements OnInit {
  private readonly clientesService = inject(ClientesService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly estados = ESTADO_CLIENTE_OPTIONS;
  readonly columnas = ['commercialName', 'contact', 'status', 'monthlyAmount', 'isActive', 'actions'];

  readonly filtrosForm = this.fb.group({
    search: [''],
    status: ['']
  });

  clientes: ClienteListado[] = [];
  cargando = false;
  total = 0;
  pageNumber = 1;
  pageSize = 10;

  ngOnInit(): void {
    this.cargarClientes();
    this.ejecutarAccionInicial();
  }

  private ejecutarAccionInicial(): void {
    const accion = this.route.snapshot.queryParamMap.get('accion');

    if (accion === 'nuevo') {
      this.router.navigate([], { relativeTo: this.route, queryParams: {}, replaceUrl: true });
      this.abrirNuevoCliente();
    }
  }

  cargarClientes(): void {
    this.cargando = true;

    this.clientesService
      .obtenerListado({
        pageNumber: this.pageNumber,
        pageSize: this.pageSize,
        search: this.filtrosForm.value.search?.trim() || undefined,
        status: (this.filtrosForm.value.status as EstadoCliente | '') || undefined
      })
      .subscribe({
        next: (result: PagedResult<ClienteListado>) => {
          this.clientes = result.items;
          this.total = result.totalCount;
          this.cargando = false;
        },
        error: () => {
          this.clientes = [];
          this.total = 0;
          this.cargando = false;
          this.snackBar.open('Ocurrió un error al cargar los clientes', 'Cerrar', { duration: 3500 });
        }
      });
  }

  aplicarFiltros(): void {
    this.pageNumber = 1;
    this.cargarClientes();
  }

  limpiarFiltros(): void {
    this.filtrosForm.reset({ search: '', status: '' });
    this.pageNumber = 1;
    this.cargarClientes();
  }

  cambiarPagina(event: PageEvent): void {
    this.pageNumber = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.cargarClientes();
  }

  abrirNuevoCliente(): void {
    const dialogRef = this.dialog.open(ClienteFormDialogComponent, {
      width: '980px',
      maxWidth: 'calc(100vw - 32px)',
      maxHeight: 'calc(100vh - 32px)',
      autoFocus: false
    });

    dialogRef.afterClosed().subscribe((formValue?: ClienteFormulario) => {
      if (!formValue) {
        return;
      }

      this.clientesService.crear(formValue).subscribe({
        next: () => {
          this.snackBar.open('El cliente se creó correctamente', 'Cerrar', { duration: 3000 });
          this.cargarClientes();
        },
        error: (error) => {
          this.snackBar.open(error?.error?.message ?? 'Ocurrió un error al guardar', 'Cerrar', { duration: 3500 });
        }
      });
    });
  }

  verDetalle(row: ClienteListado): void {
    this.clientesService.obtenerDetalle(row.id).subscribe({
      next: (detalle: ClienteDetalle) => {
        this.dialog.open(ClienteDetailDialogComponent, {
          width: '980px',
          maxWidth: 'calc(100vw - 32px)',
          maxHeight: 'calc(100vh - 32px)',
          autoFocus: false,
          data: detalle
        });
      },
      error: () => {
        this.snackBar.open('No se pudo obtener el detalle del cliente', 'Cerrar', { duration: 3500 });
      }
    });
  }

  editar(row: ClienteListado): void {
    this.clientesService.obtenerDetalle(row.id).subscribe({
      next: (detalle: ClienteDetalle) => {
        const dialogRef = this.dialog.open(ClienteFormDialogComponent, {
          width: '980px',
          maxWidth: 'calc(100vw - 32px)',
          maxHeight: 'calc(100vh - 32px)',
          autoFocus: false,
          data: { cliente: detalle }
        });

        dialogRef.afterClosed().subscribe((formValue?: ClienteFormulario) => {
          if (!formValue) {
            return;
          }

          this.clientesService.actualizar(row.id, formValue).subscribe({
            next: () => {
              this.snackBar.open('El cliente se actualizó correctamente', 'Cerrar', { duration: 3000 });
              this.cargarClientes();
            },
            error: (error) => {
              this.snackBar.open(error?.error?.message ?? 'Ocurrió un error al guardar', 'Cerrar', { duration: 3500 });
            }
          });
        });
      },
      error: () => {
        this.snackBar.open('No se pudo cargar la información para editar', 'Cerrar', { duration: 3500 });
      }
    });
  }

  cambiarEstado(row: ClienteListado): void {
    const dialogRef = this.dialog.open(ClienteStatusDialogComponent, {
      width: '520px',
      maxWidth: 'calc(100vw - 32px)',
      maxHeight: 'calc(100vh - 32px)',
      autoFocus: false,
      data: { statusActual: row.status }
    });

    dialogRef.afterClosed().subscribe((estado?: EstadoCliente) => {
      if (!estado) {
        return;
      }

      this.clientesService.cambiarEstado(row.id, estado).subscribe({
        next: () => {
          this.snackBar.open('El estado del cliente se actualizó correctamente', 'Cerrar', { duration: 3000 });
          this.cargarClientes();
        },
        error: (error) => {
          this.snackBar.open(error?.error?.message ?? 'Ocurrió un error al actualizar el estado', 'Cerrar', { duration: 3500 });
        }
      });
    });
  }

  mostrarEstado(estado: EstadoCliente): string {
    return getEstadoClienteLabel(estado);
  }
}
