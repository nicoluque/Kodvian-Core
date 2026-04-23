import { Component, Inject, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

import { ContratoDesarrollador, ContratoDesarrolladorFormulario, DesarrolladorExterno, ModoPagoContrato } from '../../models/proyectos.models';

interface ContratoDialogData {
  developers: DesarrolladorExterno[];
  contract?: ContratoDesarrollador;
}

@Component({
  selector: 'app-contrato-desarrollador-form-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatSlideToggleModule],
  templateUrl: './contrato-desarrollador-form-dialog.component.html',
  styleUrl: './contrato-desarrollador-form-dialog.component.scss'
})
export class ContratoDesarrolladorFormDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<ContratoDesarrolladorFormDialogComponent>);

  readonly paymentModes: { value: ModoPagoContrato; label: string }[] = [
    { value: 'Percentage', label: 'Porcentaje mensual' },
    { value: 'FixedAmount', label: 'Monto fijo' }
  ];

  readonly form = this.fb.group({
    developerId: ['', [Validators.required]],
    paymentMode: ['Percentage' as ModoPagoContrato, [Validators.required]],
    percentage: [null as number | null],
    agreedAmount: [null as number | null],
    startDate: ['', [Validators.required]],
    endDate: [''],
    isActive: [true],
    notes: ['', [Validators.maxLength(1000)]]
  });

  constructor(@Inject(MAT_DIALOG_DATA) public readonly data: ContratoDialogData) {
    if (data.contract) {
      this.form.patchValue({
        developerId: data.contract.developerId,
        paymentMode: data.contract.paymentMode,
        percentage: data.contract.percentage ?? null,
        agreedAmount: data.contract.agreedAmount ?? null,
        startDate: data.contract.startDate,
        endDate: data.contract.endDate ?? '',
        isActive: data.contract.isActive,
        notes: data.contract.notes ?? ''
      });
    }
  }

  guardar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const payload: ContratoDesarrolladorFormulario = {
      developerId: raw.developerId ?? '',
      paymentMode: (raw.paymentMode ?? 'Percentage') as ModoPagoContrato,
      percentage: raw.paymentMode === 'Percentage' ? Number(raw.percentage) : null,
      agreedAmount: raw.paymentMode === 'FixedAmount' ? Number(raw.agreedAmount) : null,
      startDate: raw.startDate ?? '',
      endDate: raw.endDate || null,
      isActive: raw.isActive ?? true,
      notes: raw.notes || undefined
    };

    this.dialogRef.close(payload);
  }
}
