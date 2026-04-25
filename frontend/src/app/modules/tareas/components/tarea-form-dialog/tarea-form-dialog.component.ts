import { Component, Inject, inject } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

import { compareDates, formatDateToIso, parseIsoDate } from '../../../../core/date.utils';
import { EstadoTarea, LookupItem, PrioridadTarea, TareaDetalle, TareaFormulario } from '../../models/tareas.models';

interface TareaFormData {
  tarea?: TareaDetalle;
  projects: LookupItem[];
  developers: LookupItem[];
}

@Component({
  selector: 'app-tarea-form-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatDatepickerModule, MatInputModule, MatSelectModule, MatSlideToggleModule],
  templateUrl: './tarea-form-dialog.component.html',
  styleUrl: './tarea-form-dialog.component.scss'
})
export class TareaFormDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<TareaFormDialogComponent>);

  readonly estados: { value: EstadoTarea; label: string }[] = [
    { value: 'Pendiente', label: 'Pendiente' },
    { value: 'EnCurso', label: 'En curso' },
    { value: 'Bloqueada', label: 'Bloqueada' },
    { value: 'Finalizada', label: 'Finalizada' },
    { value: 'Cancelada', label: 'Cancelada' }
  ];

  readonly prioridades: PrioridadTarea[] = ['Baja', 'Media', 'Alta', 'Urgente'];

  readonly form = this.fb.group({
    projectId: ['', [Validators.required]],
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.maxLength(2000)]],
    developerId: [''],
    status: ['Pendiente' as EstadoTarea, [Validators.required]],
    priority: ['Media' as PrioridadTarea, [Validators.required]],
    startDate: [null as Date | null],
    dueDate: [null as Date | null],
    finishedDate: [null as Date | null],
    estimatedHours: [null as number | null, [Validators.min(0)]],
    realHours: [null as number | null, [Validators.min(0)]],
    kanbanOrder: [0],
    isActive: [true]
  }, { validators: [taskDateRangeValidator()] });

  constructor(@Inject(MAT_DIALOG_DATA) public readonly data: TareaFormData) {
    if (data.tarea) {
      this.form.patchValue({
        projectId: data.tarea.projectId,
        title: data.tarea.title,
        description: data.tarea.description ?? '',
        developerId: data.tarea.developerId ?? '',
        status: data.tarea.status,
        priority: data.tarea.priority,
        startDate: parseIsoDate(data.tarea.startDate),
        dueDate: parseIsoDate(data.tarea.dueDate),
        finishedDate: parseIsoDate(data.tarea.finishedDate),
        estimatedHours: data.tarea.estimatedHours ?? null,
        realHours: data.tarea.realHours ?? null,
        kanbanOrder: data.tarea.kanbanOrder,
        isActive: data.tarea.isActive
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
      projectId: raw.projectId,
      title: raw.title,
      description: raw.description || undefined,
      developerId: raw.developerId || null,
      responsibleId: this.data.tarea?.responsibleId ?? null,
      status: raw.status,
      priority: raw.priority,
      startDate: formatDateToIso(raw.startDate),
      dueDate: formatDateToIso(raw.dueDate),
      finishedDate: formatDateToIso(raw.finishedDate),
      estimatedHours: raw.estimatedHours,
      realHours: raw.realHours,
      kanbanOrder: raw.kanbanOrder ?? 0,
      isActive: !!raw.isActive
    } as TareaFormulario);
  }
}

function taskDateRangeValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const startDate = control.get('startDate')?.value as Date | null;
    const dueDate = control.get('dueDate')?.value as Date | null;
    const finishedDate = control.get('finishedDate')?.value as Date | null;

    if (startDate && dueDate && compareDates(dueDate, startDate) < 0) {
      return { invalidDueDate: true };
    }

    if (startDate && finishedDate && compareDates(finishedDate, startDate) < 0) {
      return { invalidFinishedDate: true };
    }

    return null;
  };
}
