import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, Inject, OnInit, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';

import {
  ComprobanteArchivo,
  ContratoDesarrollador,
  ContratoDesarrolladorFormulario,
  DesarrolladorExterno,
  DesarrolladorFormulario,
  LedgerContrato,
  PagoDesarrollador,
  PagoDesarrolladorFormulario,
  ProyectoListado
} from '../../models/proyectos.models';
import { ProyectosService } from '../../services/proyectos.service';
import { ContratoDesarrolladorFormDialogComponent } from '../contrato-desarrollador-form-dialog/contrato-desarrollador-form-dialog.component';
import { ContratoLedgerDialogComponent } from '../contrato-ledger-dialog/contrato-ledger-dialog.component';
import { DesarrolladorFormDialogComponent } from '../desarrollador-form-dialog/desarrollador-form-dialog.component';
import { PagoDesarrolladorFormDialogComponent } from '../pago-desarrollador-form-dialog/pago-desarrollador-form-dialog.component';

interface DevelopersDialogData {
  project: ProyectoListado;
}

@Component({
  selector: 'app-proyecto-developers-dialog',
  standalone: true,
  imports: [MatDialogModule, MatTableModule, MatButtonModule, CurrencyPipe, DatePipe],
  templateUrl: './proyecto-developers-dialog.component.html',
  styleUrl: './proyecto-developers-dialog.component.scss'
})
export class ProyectoDevelopersDialogComponent implements OnInit {
  private readonly proyectosService = inject(ProyectosService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly contractColumns = ['developer', 'mode', 'amount', 'startDate', 'actions'];
  readonly paymentColumns = ['date', 'amount', 'period', 'reference', 'receipts'];

  developers: DesarrolladorExterno[] = [];
  contracts: ContratoDesarrollador[] = [];
  payments: PagoDesarrollador[] = [];
  selectedContract?: ContratoDesarrollador;

  constructor(@Inject(MAT_DIALOG_DATA) public readonly data: DevelopersDialogData) {}

  ngOnInit(): void {
    this.loadDevelopers();
    this.loadContracts();
  }

  loadDevelopers(): void {
    this.proyectosService.obtenerDesarrolladores().subscribe({
      next: (data) => this.developers = data,
      error: () => this.snackBar.open('No se pudieron cargar los desarrolladores', 'Cerrar', { duration: 3500 })
    });
  }

  loadContracts(): void {
    this.proyectosService.obtenerContratosDesarrollador(this.data.project.id).subscribe({
      next: (data) => {
        this.contracts = data;
        if (this.selectedContract) {
          const current = data.find((x) => x.id === this.selectedContract?.id);
          this.selectedContract = current;
          if (current) {
            this.loadPayments(current.id);
          }
        }
      },
      error: () => this.snackBar.open('No se pudieron cargar los contratos', 'Cerrar', { duration: 3500 })
    });
  }

  loadPayments(contractId: string): void {
    this.proyectosService.obtenerPagosContrato(contractId).subscribe({
      next: (data) => this.payments = data,
      error: () => this.snackBar.open('No se pudieron cargar los pagos', 'Cerrar', { duration: 3500 })
    });
  }

  crearDesarrollador(): void {
    const ref = this.dialog.open(DesarrolladorFormDialogComponent, { width: '760px', maxWidth: 'calc(100vw - 32px)', maxHeight: 'calc(100vh - 32px)', autoFocus: false });
    ref.afterClosed().subscribe((payload?: DesarrolladorFormulario) => {
      if (!payload) return;

      this.proyectosService.crearDesarrollador(payload).subscribe({
        next: () => {
          this.snackBar.open('Desarrollador creado correctamente', 'Cerrar', { duration: 3000 });
          this.loadDevelopers();
        },
        error: (error) => this.snackBar.open(error?.error?.message ?? 'No se pudo crear el desarrollador', 'Cerrar', { duration: 3500 })
      });
    });
  }

  crearContrato(): void {
    const ref = this.dialog.open(ContratoDesarrolladorFormDialogComponent, {
      width: '760px',
      maxWidth: 'calc(100vw - 32px)',
      maxHeight: 'calc(100vh - 32px)',
      autoFocus: false,
      data: { developers: this.developers }
    });

    ref.afterClosed().subscribe((payload?: ContratoDesarrolladorFormulario) => {
      if (!payload) return;

      this.proyectosService.crearContratoDesarrollador(this.data.project.id, payload).subscribe({
        next: () => {
          this.snackBar.open('Contrato creado correctamente', 'Cerrar', { duration: 3000 });
          this.loadContracts();
        },
        error: (error) => this.snackBar.open(error?.error?.message ?? 'No se pudo crear el contrato', 'Cerrar', { duration: 3500 })
      });
    });
  }

  editarContrato(contract: ContratoDesarrollador): void {
    const ref = this.dialog.open(ContratoDesarrolladorFormDialogComponent, {
      width: '760px',
      maxWidth: 'calc(100vw - 32px)',
      maxHeight: 'calc(100vh - 32px)',
      autoFocus: false,
      data: { developers: this.developers, contract }
    });

    ref.afterClosed().subscribe((payload?: ContratoDesarrolladorFormulario) => {
      if (!payload) return;

      this.proyectosService.actualizarContratoDesarrollador(contract.id, payload).subscribe({
        next: () => {
          this.snackBar.open('Contrato actualizado correctamente', 'Cerrar', { duration: 3000 });
          this.loadContracts();
        },
        error: (error) => this.snackBar.open(error?.error?.message ?? 'No se pudo actualizar el contrato', 'Cerrar', { duration: 3500 })
      });
    });
  }

  seleccionarContrato(contract: ContratoDesarrollador): void {
    this.selectedContract = contract;
    this.loadPayments(contract.id);
  }

  registrarPago(contract: ContratoDesarrollador): void {
    const ref = this.dialog.open(PagoDesarrolladorFormDialogComponent, { width: '760px', maxWidth: 'calc(100vw - 32px)', maxHeight: 'calc(100vh - 32px)', autoFocus: false, data: { contractId: contract.id } });
    ref.afterClosed().subscribe((result?: { payload: PagoDesarrolladorFormulario; receiptFile: File | null }) => {
      if (!result) return;

      this.proyectosService.registrarPagoContrato(contract.id, result.payload).subscribe({
        next: (payment) => {
          if (result.receiptFile) {
            this.proyectosService.subirComprobantePago(payment.id, result.receiptFile).subscribe({
              next: () => {
                this.snackBar.open('Pago y comprobante registrados', 'Cerrar', { duration: 3000 });
                this.loadPayments(contract.id);
              },
              error: () => {
                this.snackBar.open('Pago registrado, pero falló la carga del comprobante', 'Cerrar', { duration: 3500 });
                this.loadPayments(contract.id);
              }
            });
            return;
          }

          this.snackBar.open('Pago registrado correctamente', 'Cerrar', { duration: 3000 });
          this.loadPayments(contract.id);
        },
        error: (error) => this.snackBar.open(error?.error?.message ?? 'No se pudo registrar el pago', 'Cerrar', { duration: 3500 })
      });
    });
  }

  verLedger(contract: ContratoDesarrollador): void {
    const year = new Date().getFullYear();
    this.proyectosService.obtenerLedgerContrato(contract.id, year).subscribe({
      next: (ledger: LedgerContrato) => {
        this.dialog.open(ContratoLedgerDialogComponent, {
          width: '980px',
          maxWidth: 'calc(100vw - 32px)',
          maxHeight: 'calc(100vh - 32px)',
          autoFocus: false,
          data: ledger
        });
      },
      error: (error) => this.snackBar.open(error?.error?.message ?? 'No se pudo obtener el ledger', 'Cerrar', { duration: 3500 })
    });
  }

  descargarComprobante(paymentId: string, receipt: ComprobanteArchivo): void {
    const url = `/api/developer-payments/${paymentId}/receipts/${receipt.id}`;
    window.open(url, '_blank', 'noopener');
  }

  eliminarComprobante(paymentId: string, receipt: ComprobanteArchivo): void {
    const confirmed = window.confirm(`Se eliminará el comprobante ${receipt.fileName}. ¿Continuar?`);
    if (!confirmed) {
      return;
    }

    this.proyectosService.eliminarComprobantePago(paymentId, receipt.id).subscribe({
      next: () => {
        this.snackBar.open('Comprobante eliminado correctamente', 'Cerrar', { duration: 3000 });
        if (this.selectedContract) {
          this.loadPayments(this.selectedContract.id);
        }
      },
      error: (error) => this.snackBar.open(error?.error?.message ?? 'No se pudo eliminar el comprobante', 'Cerrar', { duration: 3500 })
    });
  }
}
