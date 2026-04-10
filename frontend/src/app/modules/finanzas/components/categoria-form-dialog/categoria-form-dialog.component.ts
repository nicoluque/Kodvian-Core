import { Component, Inject, inject } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

import { CategoriaFinanciera, CategoriaFormulario, TipoMovimiento } from '../../models/finanzas.models';

interface CategoriaDialogData {
  categoria?: CategoriaFinanciera;
}

@Component({
  selector: 'app-categoria-form-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatSlideToggleModule],
  templateUrl: './categoria-form-dialog.component.html',
  styleUrl: './categoria-form-dialog.component.scss'
})
export class CategoriaFormDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<CategoriaFormDialogComponent>);

  readonly data: CategoriaDialogData;

  readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(120)]],
    movementType: ['Ingreso' as TipoMovimiento, [Validators.required]],
    isActive: [true]
  });

  constructor(@Inject(MAT_DIALOG_DATA) data: CategoriaDialogData | null) {
    this.data = data ?? {};

    if (this.data.categoria) {
      this.form.patchValue({
        name: this.data.categoria.name,
        movementType: this.data.categoria.movementType,
        isActive: this.data.categoria.isActive
      });
    }
  }

  guardar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.dialogRef.close(this.form.getRawValue() as CategoriaFormulario);
  }
}
