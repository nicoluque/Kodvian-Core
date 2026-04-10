import { Component, Inject } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';

import { EstadoTarea } from '../../models/tareas.models';

interface TareaStatusDialogData {
  statusActual: EstadoTarea;
}

@Component({
  selector: 'app-tarea-status-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatSelectModule],
  templateUrl: './tarea-status-dialog.component.html'
})
export class TareaStatusDialogComponent {
  readonly estados: { value: EstadoTarea; label: string }[] = [
    { value: 'Pendiente', label: 'Pendiente' },
    { value: 'EnCurso', label: 'En curso' },
    { value: 'Bloqueada', label: 'Bloqueada' },
    { value: 'Finalizada', label: 'Finalizada' },
    { value: 'Cancelada', label: 'Cancelada' }
  ];

  readonly estadoControl: FormControl<EstadoTarea>;

  constructor(private readonly dialogRef: MatDialogRef<TareaStatusDialogComponent>, @Inject(MAT_DIALOG_DATA) data: TareaStatusDialogData) {
    this.estadoControl = new FormControl<EstadoTarea>(data.statusActual, { nonNullable: true, validators: [Validators.required] });
  }

  confirmar(): void {
    if (this.estadoControl.invalid) {
      this.estadoControl.markAsTouched();
      return;
    }

    this.dialogRef.close(this.estadoControl.value);
  }
}
