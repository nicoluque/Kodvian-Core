import { Component, Inject, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { formatDateToIso } from '../../../../core/date.utils';
import { PagoDesarrolladorFormulario } from '../../models/proyectos.models';

interface PagoDialogData {
  contractId: string;
}

interface PagoDialogResult {
  payload: PagoDesarrolladorFormulario;
  receiptFile: File | null;
}

@Component({
  selector: 'app-pago-desarrollador-form-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatDatepickerModule, MatInputModule],
  templateUrl: './pago-desarrollador-form-dialog.component.html',
  styleUrl: './pago-desarrollador-form-dialog.component.scss'
})
export class PagoDesarrolladorFormDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<PagoDesarrolladorFormDialogComponent>);

  receiptFile: File | null = null;

  readonly form = this.fb.group({
    paymentDate: [new Date(), [Validators.required]],
    amount: [0, [Validators.required, Validators.min(0.01)]],
    periodYear: [new Date().getFullYear(), [Validators.required, Validators.min(2000), Validators.max(2100)]],
    periodMonth: [new Date().getMonth() + 1, [Validators.required, Validators.min(1), Validators.max(12)]],
    reference: ['', [Validators.maxLength(120)]],
    notes: ['', [Validators.maxLength(1000)]]
  });

  constructor(@Inject(MAT_DIALOG_DATA) public readonly data: PagoDialogData) {}

  guardar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    this.dialogRef.close({
      payload: {
        paymentDate: formatDateToIso(raw.paymentDate) ?? '',
        amount: Number(raw.amount),
        periodYear: Number(raw.periodYear),
        periodMonth: Number(raw.periodMonth),
        reference: raw.reference || undefined,
        notes: raw.notes || undefined
      },
      receiptFile: this.receiptFile
    } as PagoDialogResult);
  }

  onReceiptSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;

    if (!file) {
      this.receiptFile = null;
      return;
    }

    this.receiptFile = file.type === 'application/pdf' ? file : null;
    if (!this.receiptFile) {
      input.value = '';
    }
  }
}
