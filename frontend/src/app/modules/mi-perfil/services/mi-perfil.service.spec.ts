import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { MiPerfilService } from './mi-perfil.service';

describe('MiPerfilService', () => {
  let service: MiPerfilService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(MiPerfilService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('obtenerPerfil should map api response data', () => {
    let result: unknown;
    service.obtenerPerfil().subscribe((data) => (result = data));

    const req = httpMock.expectOne('/api/profile');
    expect(req.request.method).toBe('GET');
    req.flush({
      success: true,
      message: 'ok',
      data: {
        id: '1',
        fullName: 'Dev',
        email: 'dev@kodvian.local',
        role: 'Desarrollador',
        gitHubConnected: false
      }
    });

    expect(result).toEqual({
      id: '1',
      fullName: 'Dev',
      email: 'dev@kodvian.local',
      role: 'Desarrollador',
      gitHubConnected: false
    });
  });

  it('desconectarGitHub should call delete endpoint', () => {
    let completed = false;
    service.desconectarGitHub().subscribe(() => (completed = true));

    const req = httpMock.expectOne('/api/profile/github/disconnect');
    expect(req.request.method).toBe('DELETE');
    req.flush({ success: true, message: 'ok', data: {} });

    expect(completed).toBeTrue();
  });

  it('getConnectUrl should return relative connect path', () => {
    expect(service.getConnectUrl()).toBe('/api/profile/github/connect');
  });
});
