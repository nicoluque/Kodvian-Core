import { Component, Inject, inject } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

import { CategoriaFinanciera, EstadoMovimiento, LookupItem, MovimientoDetalle, MovimientoFormulario, TipoMovimiento } from '../../models/finanzas.models';

interface MovimientoFormData {
  tipoInicial: TipoMovimiento;
  movimiento?: MovimientoDetalle;
  categorias: CategoriaFinanciera[];
  clientes: LookupItem[];
  proyectos: LookupItem[];
  proveedores: LookupItem[];
}

@Component({
  selector: 'app-movimiento-form-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatSelectModule],
  templateUrl: './movimiento-form-dialog.component.html',
  styleUrl: './movimiento-form-dialog.component.scss'
})
export class MovimientoFormDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<MovimientoFormDialogComponent>);

  readonly estados: EstadoMovimiento[] = ['Pendiente', 'Cobrado', 'Pagado', 'Vencido', 'Anulado'];
  receiptFile: File | null = null;

  readonly form = this.fb.group({
    movementType: ['Ingreso' as TipoMovimiento, [Validators.required]],
    categoryId: ['', [Validators.required]],
    clientId: [''],
    providerId: [''],
    projectId: [''],
    description: ['', [Validators.required, Validators.maxLength(500)]],
    amount: [0, [Validators.required, Validators.min(0.01)]],
    movementDate: ['', [Validators.required]],
    dueDate: [''],
    status: ['Pendiente' as EstadoMovimiento, [Validators.required]],
    paymentMethod: ['', [Validators.maxLength(80)]],
    receiptNumber: ['', [Validators.maxLength(80)]],
    notes: ['', [Validators.maxLength(1000)]]
  }, { validators: [movementDateRangeValidator()] });

  constructor(@Inject(MAT_DIALOG_DATA) public readonly data: MovimientoFormData) {
    this.form.patchValue({
      movementType: data.tipoInicial
    });

    if (data.movimiento) {
      this.form.patchValue({
        movementType: data.movimiento.movementType,
        categoryId: data.movimiento.categoryId,
        clientId: data.movimiento.clientId ?? '',
        providerId: data.movimiento.providerId ?? '',
        projectId: data.movimiento.projectId ?? '',
        description: data.movimiento.description,
        amount: data.movimiento.amount,
        movementDate: data.movimiento.movementDate,
        dueDate: data.movimiento.dueDate ?? '',
        status: data.movimiento.status,
        paymentMethod: data.movimiento.paymentMethod ?? '',
        receiptNumber: data.movimiento.receiptNumber ?? '',
        notes: data.movimiento.notes ?? ''
      });
    }
  }

  guardar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    this.dialogRef.close({
      movementType: raw.movementType,
      categoryId: raw.categoryId,
      clientId: raw.clientId || null,
      providerId: raw.providerId || null,
      projectId: raw.projectId || null,
      description: raw.description,
      amount: Number(raw.amount),
      movementDate: raw.movementDate,
      dueDate: raw.dueDate || null,
      status: raw.status,
      paymentMethod: raw.paymentMethod || undefined,
      receiptNumber: raw.receiptNumber || undefined,
      notes: raw.notes || undefined,
      receiptFile: this.receiptFile
    } as MovimientoFormulario);
  }

  onReceiptSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;

    if (!file) {
      this.receiptFile = null;
      return;
    }

    if (file.type !== 'application/pdf') {
      this.form.setErrors({ invalidReceiptFile: true });
      this.receiptFile = null;
      input.value = '';
      return;
    }

    this.receiptFile = file;

    if (this.form.hasError('invalidReceiptFile')) {
      const errors = { ...(this.form.errors ?? {}) };
      delete errors['invalidReceiptFile'];
      this.form.setErrors(Object.keys(errors).length ? errors : null);
    }
  }

  categoriasFiltradas(): CategoriaFinanciera[] {
    const type = this.form.value.movementType;
    return this.data.categorias.filter((x) => x.isActive && x.movementType === type);
  }
}

function movementDateRangeValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const movementDate = control.get('movementDate')?.value as string | null;
    const dueDate = control.get('dueDate')?.value as string | null;

    if (movementDate && dueDate && dueDate < movementDate) {
      return { invalidDueDate: true };
    }

    return null;
  };
}
