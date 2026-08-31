import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { MiTrabajoService } from './mi-trabajo.service';

describe('MiTrabajoService', () => {
  let service: MiTrabajoService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(MiTrabajoService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('crearIssue should post to issues endpoint', () => {
    let result: unknown;
    service
      .crearIssue({
        projectId: 'project-1',
        title: 'Nueva tarea',
        description: 'Detalle',
        priority: 'Alta'
      })
      .subscribe((data) => (result = data));

    const req = httpMock.expectOne('/api/my-work/issues');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      projectId: 'project-1',
      title: 'Nueva tarea',
      description: 'Detalle',
      priority: 'Alta'
    });
    req.flush({
      success: true,
      message: 'ok',
      data: {
        id: 'issue-1',
        projectId: 'project-1',
        projectName: 'Proyecto Demo',
        title: 'Nueva tarea',
        repositoryFullName: 'kodvian/alpha',
        gitHubIssueNumber: 101,
        status: 'Open',
        priority: 'Alta',
        createdAt: '2026-01-01T00:00:00Z'
      }
    });

    expect(result).toEqual({
      id: 'issue-1',
      projectId: 'project-1',
      projectName: 'Proyecto Demo',
      title: 'Nueva tarea',
      repositoryFullName: 'kodvian/alpha',
      gitHubIssueNumber: 101,
      status: 'Open',
      priority: 'Alta',
      createdAt: '2026-01-01T00:00:00Z'
    });
  });

  it('actualizarEstadoIssue should patch issue status', () => {
    let result: unknown;
    service.actualizarEstadoIssue('issue-1', 'Closed').subscribe((data) => (result = data));

    const req = httpMock.expectOne('/api/my-work/issues/issue-1/status');
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual({ status: 'Closed' });
    req.flush({
      success: true,
      message: 'ok',
      data: {
        id: 'issue-1',
        projectId: 'project-1',
        projectName: 'Proyecto Demo',
        title: 'Nueva tarea',
        repositoryFullName: 'kodvian/alpha',
        gitHubIssueNumber: 101,
        status: 'Closed',
        createdAt: '2026-01-01T00:00:00Z'
      }
    });

    expect(result).toEqual({
      id: 'issue-1',
      projectId: 'project-1',
      projectName: 'Proyecto Demo',
      title: 'Nueva tarea',
      repositoryFullName: 'kodvian/alpha',
      gitHubIssueNumber: 101,
      status: 'Closed',
      createdAt: '2026-01-01T00:00:00Z'
    });
  });
});
