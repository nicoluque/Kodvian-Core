import { Component, Inject } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';

import { EstadoCliente } from '../../models/clientes.models';

interface ClienteStatusDialogData {
  statusActual: EstadoCliente;
}

@Component({
  selector: 'app-cliente-status-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatSelectModule],
  templateUrl: './cliente-status-dialog.component.html',
  styleUrl: './cliente-status-dialog.component.scss'
})
export class ClienteStatusDialogComponent {
  readonly estados: EstadoCliente[] = ['Prospecto', 'Activo', 'Pausado', 'Finalizado'];
  readonly estadoControl: FormControl<EstadoCliente>;

  constructor(
    private readonly dialogRef: MatDialogRef<ClienteStatusDialogComponent>,
    @Inject(MAT_DIALOG_DATA) data: ClienteStatusDialogData
  ) {
    this.estadoControl = new FormControl<EstadoCliente>(data.statusActual, { nonNullable: true, validators: [Validators.required] });
  }

  confirmar(): void {
    if (this.estadoControl.invalid) {
      this.estadoControl.markAsTouched();
      return;
    }

    this.dialogRef.close(this.estadoControl.value);
  }
}
