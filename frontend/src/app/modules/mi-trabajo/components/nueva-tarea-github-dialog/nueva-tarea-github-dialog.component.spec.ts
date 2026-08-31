import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { NuevaTareaGitHubDialogComponent } from './nueva-tarea-github-dialog.component';

describe('NuevaTareaGitHubDialogComponent', () => {
  let fixture: ComponentFixture<NuevaTareaGitHubDialogComponent>;
  let dialogRef: jasmine.SpyObj<MatDialogRef<NuevaTareaGitHubDialogComponent>>;

  beforeEach(async () => {
    dialogRef = jasmine.createSpyObj('MatDialogRef', ['close']);

    await TestBed.configureTestingModule({
      imports: [NuevaTareaGitHubDialogComponent, NoopAnimationsModule],
      providers: [
        { provide: MatDialogRef, useValue: dialogRef },
        {
          provide: MAT_DIALOG_DATA,
          useValue: {
            repositories: [
              {
                projectId: 'project-1',
                projectName: 'Proyecto Demo',
                clientName: 'Cliente',
                projectStatus: 'EnCurso',
                gitHubOwner: 'kodvian',
                gitHubRepoName: 'alpha',
                fullName: 'kodvian/alpha',
                openIssuesCount: 0
              }
            ]
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(NuevaTareaGitHubDialogComponent);
    fixture.detectChanges();
  });

  it('should preselect repository when only one is available', () => {
    expect(fixture.componentInstance.form.controls.projectId.value).toBe('project-1');
  });

  it('should close dialog with request payload when form is valid', () => {
    fixture.componentInstance.form.patchValue({
      title: 'Nueva tarea',
      description: 'Detalle',
      priority: 'Alta'
    });
    fixture.componentInstance.guardar();

    expect(dialogRef.close).toHaveBeenCalledWith({
      projectId: 'project-1',
      title: 'Nueva tarea',
      description: 'Detalle',
      priority: 'Alta'
    });
  });
});
