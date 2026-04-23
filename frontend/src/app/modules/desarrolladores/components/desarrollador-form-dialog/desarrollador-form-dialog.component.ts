import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

import { DesarrolladorExterno, DesarrolladorFormulario } from '../../models/desarrolladores.models';

interface DesarrolladorDialogData {
  desarrollador?: DesarrolladorExterno;
}

@Component({
  selector: 'app-desarrollador-form-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatSlideToggleModule],
  templateUrl: './desarrollador-form-dialog.component.html',
  styleUrl: './desarrollador-form-dialog.component.scss'
})
export class DesarrolladorFormDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<DesarrolladorFormDialogComponent>);
  readonly data = inject(MAT_DIALOG_DATA, { optional: true }) as DesarrolladorDialogData | null;

  readonly form = this.fb.group({
    fullName: ['', [Validators.required, Validators.maxLength(160)]],
    email: ['', [Validators.maxLength(120), Validators.email]],
    phone: ['', [Validators.maxLength(40)]],
    taxId: ['', [Validators.maxLength(20)]],
    notes: ['', [Validators.maxLength(1000)]],
    isActive: [true]
  });

  constructor() {
    if (!this.data?.desarrollador) {
      return;
    }

    this.form.patchValue({
      fullName: this.data.desarrollador.fullName,
      email: this.data.desarrollador.email ?? '',
      phone: this.data.desarrollador.phone ?? '',
      taxId: this.data.desarrollador.taxId ?? '',
      notes: this.data.desarrollador.notes ?? '',
      isActive: this.data.desarrollador.isActive
    });
  }

  guardar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    this.dialogRef.close({
      fullName: raw.fullName ?? '',
      email: raw.email || undefined,
      phone: raw.phone || undefined,
      taxId: raw.taxId || undefined,
      notes: raw.notes || undefined,
      isActive: raw.isActive ?? true
    } as DesarrolladorFormulario);
  }
}
