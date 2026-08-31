import { Component, Inject, OnInit, inject } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { AuthSessionService } from '../../../../core/auth/auth-session.service';
import { compareDates, formatDateToIso, parseIsoDate } from '../../../../core/date.utils';
import { EstadoProyecto, GitHubRepoLinkRequest, LookupItem, PrioridadProyecto, ProyectoDetalle, ProyectoFormulario } from '../../models/proyectos.models';
import { ProyectosService } from '../../services/proyectos.service';

interface ProyectoFormData {
  proyecto?: ProyectoDetalle;
  clientes: LookupItem[];
  responsables: LookupItem[];
}

@Component({
  selector: 'app-proyecto-form-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatDatepickerModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatIconModule,
    MatSnackBarModule
  ],
  templateUrl: './proyecto-form-dialog.component.html',
  styleUrl: './proyecto-form-dialog.component.scss'
})
export class ProyectoFormDialogComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<ProyectoFormDialogComponent>);
  private readonly authSession = inject(AuthSessionService);
  private readonly proyectosService = inject(ProyectosService);
  private readonly snackBar = inject(MatSnackBar);

  esAdministrador = false;
  githubBusy = false;
  linkedRepoUrl: string | null = null;
  linkedRepoLabel: string | null = null;

  readonly estados: { value: EstadoProyecto; label: string }[] = [
    { value: 'Planificacion', label: 'Planificación' },
    { value: 'Presupuestado', label: 'Presupuestado' },
    { value: 'EnCurso', label: 'En curso' },
    { value: 'Pausado', label: 'Pausado' },
    { value: 'Finalizado', label: 'Finalizado' },
    { value: 'Cancelado', label: 'Cancelado' }
  ];

  readonly prioridades: PrioridadProyecto[] = ['Baja', 'Media', 'Alta', 'Urgente'];

  readonly form = this.fb.group({
    clientId: ['', [Validators.required]],
    name: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.maxLength(2000)]],
    responsibleId: [''],
    status: ['Planificacion' as EstadoProyecto, [Validators.required]],
    priority: ['Media' as PrioridadProyecto, [Validators.required]],
    startDate: [null as Date | null],
    estimatedDeliveryDate: [null as Date | null],
    closingDate: [null as Date | null],
    budget: [null as number | null, [Validators.min(0)]],
    progressPercentage: [0, [Validators.min(0), Validators.max(100)]],
    isActive: [true]
  }, { validators: [projectDateRangeValidator()] });

  readonly githubForm = this.fb.group({
    owner: ['', [Validators.required, Validators.maxLength(100)]],
    repo: ['', [Validators.required, Validators.maxLength(200)]]
  });

  constructor(@Inject(MAT_DIALOG_DATA) public readonly data: ProyectoFormData) {
    if (data.proyecto) {
      this.form.patchValue({
        clientId: data.proyecto.clientId,
        name: data.proyecto.name,
        description: data.proyecto.description ?? '',
        responsibleId: data.proyecto.responsibleId ?? '',
        status: data.proyecto.status,
        priority: data.proyecto.priority,
        startDate: parseIsoDate(data.proyecto.startDate),
        estimatedDeliveryDate: parseIsoDate(data.proyecto.estimatedDeliveryDate),
        closingDate: parseIsoDate(data.proyecto.closingDate),
        budget: data.proyecto.budget ?? null,
        progressPercentage: data.proyecto.progressPercentage,
        isActive: data.proyecto.isActive
      });
      this.applyLinkedRepo(data.proyecto);
    }
  }

  ngOnInit(): void {
    this.authSession.ensureSessionLoaded().subscribe((user) => {
      this.esAdministrador = user?.role === 'Administrador';
    });
  }

  get puedeGestionarGitHub(): boolean {
    return this.esAdministrador && !!this.data.proyecto?.id;
  }

  guardar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();

    this.dialogRef.close({
      clientId: raw.clientId,
      name: raw.name,
      description: raw.description || undefined,
      responsibleId: raw.responsibleId || null,
      status: raw.status,
      priority: raw.priority,
      startDate: formatDateToIso(raw.startDate),
      estimatedDeliveryDate: formatDateToIso(raw.estimatedDeliveryDate),
      closingDate: formatDateToIso(raw.closingDate),
      budget: raw.budget,
      progressPercentage: raw.progressPercentage ?? 0,
      isActive: !!raw.isActive
    } as ProyectoFormulario);
  }

  validarRepo(): void {
    if (!this.data.proyecto || this.githubForm.invalid) {
      this.githubForm.markAllAsTouched();
      return;
    }

    this.githubBusy = true;
    this.proyectosService.validarRepoGitHub(this.data.proyecto.id, this.getGitHubRepoPayload()).subscribe({
      next: (result) => {
        this.githubBusy = false;
        this.snackBar.open(result.message, 'Cerrar', { duration: 4000 });
      },
      error: (error) => {
        this.githubBusy = false;
        this.snackBar.open(error?.error?.message ?? 'No se pudo validar el repositorio', 'Cerrar', { duration: 3500 });
      }
    });
  }

  vincularRepo(): void {
    if (!this.data.proyecto || this.githubForm.invalid) {
      this.githubForm.markAllAsTouched();
      return;
    }

    this.githubBusy = true;
    this.proyectosService.vincularRepoGitHub(this.data.proyecto.id, this.getGitHubRepoPayload()).subscribe({
      next: (detalle) => {
        this.githubBusy = false;
        this.applyLinkedRepo(detalle);
        this.snackBar.open('Repositorio vinculado correctamente', 'Cerrar', { duration: 3000 });
      },
      error: (error) => {
        this.githubBusy = false;
        this.snackBar.open(error?.error?.message ?? 'No se pudo vincular el repositorio', 'Cerrar', { duration: 3500 });
      }
    });
  }

  desvincularRepo(): void {
    if (!this.data.proyecto) {
      return;
    }

    this.githubBusy = true;
    this.proyectosService.desvincularRepoGitHub(this.data.proyecto.id).subscribe({
      next: (detalle) => {
        this.githubBusy = false;
        this.applyLinkedRepo(detalle);
        this.githubForm.reset({ owner: '', repo: '' });
        this.snackBar.open('Repositorio desvinculado correctamente', 'Cerrar', { duration: 3000 });
      },
      error: (error) => {
        this.githubBusy = false;
        this.snackBar.open(error?.error?.message ?? 'No se pudo desvincular el repositorio', 'Cerrar', { duration: 3500 });
      }
    });
  }

  private getGitHubRepoPayload(): GitHubRepoLinkRequest {
    const raw = this.githubForm.getRawValue();
    return {
      owner: raw.owner!.trim(),
      repo: raw.repo!.trim()
    };
  }

  private applyLinkedRepo(proyecto: ProyectoDetalle): void {
    this.data.proyecto = { ...this.data.proyecto!, ...proyecto };
    if (proyecto.gitHubOwner && proyecto.gitHubRepoName) {
      this.linkedRepoLabel = `${proyecto.gitHubOwner}/${proyecto.gitHubRepoName}`;
      this.linkedRepoUrl = proyecto.gitHubRepoUrl ?? null;
      this.githubForm.patchValue({
        owner: proyecto.gitHubOwner,
        repo: proyecto.gitHubRepoName
      });
    } else {
      this.linkedRepoLabel = null;
      this.linkedRepoUrl = null;
    }
  }
}

function projectDateRangeValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const startDate = control.get('startDate')?.value as Date | null;
    const estimatedDeliveryDate = control.get('estimatedDeliveryDate')?.value as Date | null;
    const closingDate = control.get('closingDate')?.value as Date | null;

    if (startDate && estimatedDeliveryDate && compareDates(estimatedDeliveryDate, startDate) < 0) {
      return { invalidEstimatedDeliveryDate: true };
    }

    if (startDate && closingDate && compareDates(closingDate, startDate) < 0) {
      return { invalidClosingDate: true };
    }

    return null;
  };
}
