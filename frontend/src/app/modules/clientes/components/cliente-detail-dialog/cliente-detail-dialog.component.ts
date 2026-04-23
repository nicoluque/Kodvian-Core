import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

import { ClienteDetalle, EstadoCliente, getEstadoClienteLabel } from '../../models/clientes.models';

@Component({
  selector: 'app-cliente-detail-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule],
  templateUrl: './cliente-detail-dialog.component.html',
  styleUrl: './cliente-detail-dialog.component.scss'
})
export class ClienteDetailDialogComponent {
  constructor(@Inject(MAT_DIALOG_DATA) public readonly cliente: ClienteDetalle) {}

  valor(texto?: string | number | null): string {
    return texto === null || texto === undefined || texto === '' ? '-' : String(texto);
  }

  mostrarEstado(estado: EstadoCliente): string {
    return getEstadoClienteLabel(estado);
  }
}
