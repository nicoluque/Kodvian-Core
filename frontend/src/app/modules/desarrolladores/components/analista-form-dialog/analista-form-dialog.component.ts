import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

import { TeamUser, TeamUserFormulario } from '../../models/desarrolladores.models';

interface AnalistaDialogData {
  analista?: TeamUser;
}

@Component({
  selector: 'app-analista-form-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatSlideToggleModule],
  templateUrl: './analista-form-dialog.component.html',
  styleUrl: './analista-form-dialog.component.scss'
})
export class AnalistaFormDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<AnalistaFormDialogComponent>);
  readonly data = inject(MAT_DIALOG_DATA, { optional: true }) as AnalistaDialogData | null;

  readonly form = this.fb.group({
    fullName: ['', [Validators.required, Validators.maxLength(160)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(120)]],
    password: ['', [Validators.minLength(8)]],
    isActive: [true]
  });

  constructor() {
    if (!this.data?.analista) {
      return;
    }

    this.form.patchValue({
      fullName: this.data.analista.fullName,
      email: this.data.analista.email,
      password: '',
      isActive: this.data.analista.isActive
    });
  }

  guardar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const password = raw.password?.trim() || undefined;
    if (!this.data?.analista && !password) {
      this.form.controls.password.setErrors({ required: true });
      this.form.controls.password.markAsTouched();
      return;
    }

    this.dialogRef.close({
      fullName: raw.fullName ?? '',
      email: raw.email ?? '',
      password,
      isActive: raw.isActive ?? true
    } as TeamUserFormulario);
  }
}
