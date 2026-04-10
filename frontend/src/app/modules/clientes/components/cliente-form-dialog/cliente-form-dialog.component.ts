import { Component, Inject, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

import { ClienteDetalle, ClienteFormulario, EstadoCliente } from '../../models/clientes.models';

interface ClienteFormDialogData {
  cliente?: ClienteDetalle;
}

@Component({
  selector: 'app-cliente-form-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule
  ],
  templateUrl: './cliente-form-dialog.component.html',
  styleUrl: './cliente-form-dialog.component.scss'
})
export class ClienteFormDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<ClienteFormDialogComponent>);

  readonly estados: EstadoCliente[] = ['Prospecto', 'Activo', 'Pausado', 'Finalizado'];
  readonly data: ClienteFormDialogData;

  readonly form = this.fb.group({
    commercialName: ['', [Validators.required, Validators.maxLength(200)]],
    legalName: ['', [Validators.maxLength(220)]],
    taxId: ['', [Validators.maxLength(20)]],
    contactName: ['', [Validators.maxLength(120)]],
    contactEmail: ['', [Validators.email, Validators.maxLength(120)]],
    contactPhone: ['', [Validators.maxLength(40)]],
    address: ['', [Validators.maxLength(180)]],
    city: ['', [Validators.maxLength(120)]],
    province: ['', [Validators.maxLength(120)]],
    country: ['', [Validators.maxLength(120)]],
    status: ['Prospecto' as EstadoCliente, [Validators.required]],
    serviceType: ['', [Validators.maxLength(120)]],
    monthlyAmount: [null as number | null, [Validators.min(0)]],
    billingDay: [null as number | null, [Validators.min(1), Validators.max(31)]],
    notes: ['', [Validators.maxLength(1000)]],
    isActive: [true]
  });

  constructor(@Inject(MAT_DIALOG_DATA) data: ClienteFormDialogData | null) {
    this.data = data ?? {};

    if (this.data.cliente) {
      this.form.patchValue({
        commercialName: this.data.cliente.commercialName,
        legalName: this.data.cliente.legalName ?? '',
        taxId: this.data.cliente.taxId ?? '',
        contactName: this.data.cliente.contactName ?? '',
        contactEmail: this.data.cliente.contactEmail ?? '',
        contactPhone: this.data.cliente.contactPhone ?? '',
        address: this.data.cliente.address ?? '',
        city: this.data.cliente.city ?? '',
        province: this.data.cliente.province ?? '',
        country: this.data.cliente.country ?? '',
        status: this.data.cliente.status,
        serviceType: this.data.cliente.serviceType ?? '',
        monthlyAmount: this.data.cliente.monthlyAmount ?? null,
        billingDay: this.data.cliente.billingDay ?? null,
        notes: this.data.cliente.notes ?? '',
        isActive: this.data.cliente.isActive
      });
    }
  }

  guardar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.dialogRef.close(this.form.getRawValue() as ClienteFormulario);
  }
}
