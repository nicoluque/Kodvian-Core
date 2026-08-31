import { Component, Inject, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

import { MiTrabajoCreateIssueRequest, MiTrabajoRepositorio } from '../../models/mi-trabajo.models';

export interface NuevaTareaGitHubDialogData {
  repositories: MiTrabajoRepositorio[];
}

@Component({
  selector: 'app-nueva-tarea-github-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatSelectModule],
  templateUrl: './nueva-tarea-github-dialog.component.html',
  styleUrl: './nueva-tarea-github-dialog.component.scss'
})
export class NuevaTareaGitHubDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<NuevaTareaGitHubDialogComponent>);

  readonly prioridades = ['Baja', 'Media', 'Alta', 'Urgente'] as const;

  readonly form = this.fb.group({
    projectId: ['', [Validators.required]],
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.maxLength(2000)]],
    priority: ['' as '' | (typeof this.prioridades)[number]]
  });

  constructor(@Inject(MAT_DIALOG_DATA) public readonly data: NuevaTareaGitHubDialogData) {
    if (data.repositories.length === 1) {
      this.form.patchValue({ projectId: data.repositories[0].projectId });
    }
  }

  repoLabel(repo: MiTrabajoRepositorio): string {
    return repo.fullName || `${repo.gitHubOwner}/${repo.gitHubRepoName}`;
  }

  guardar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const request: MiTrabajoCreateIssueRequest = {
      projectId: value.projectId!,
      title: value.title!.trim(),
      description: value.description?.trim() || undefined,
      priority: value.priority || undefined
    };
    this.dialogRef.close(request);
  }

  cancelar(): void {
    this.dialogRef.close();
  }
}
