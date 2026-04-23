import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

import { DesarrolladorFormulario } from '../../models/proyectos.models';

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

  readonly form = this.fb.group({
    fullName: ['', [Validators.required, Validators.maxLength(160)]],
    email: ['', [Validators.maxLength(120), Validators.email]],
    phone: ['', [Validators.maxLength(40)]],
    taxId: ['', [Validators.maxLength(20)]],
    notes: ['', [Validators.maxLength(1000)]],
    isActive: [true]
  });

  guardar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    this.dialogRef.close({
      fullName: raw.fullName,
      email: raw.email || undefined,
      phone: raw.phone || undefined,
      taxId: raw.taxId || undefined,
      notes: raw.notes || undefined,
      isActive: raw.isActive ?? true
    } as DesarrolladorFormulario);
  }
}
