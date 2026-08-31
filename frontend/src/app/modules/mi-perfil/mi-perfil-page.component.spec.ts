import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { ActivatedRoute, Router, convertToParamMap } from '@angular/router';
import { of } from 'rxjs';

import { MiPerfilPageComponent } from './mi-perfil-page.component';
import { MiPerfilService } from './services/mi-perfil.service';

describe('MiPerfilPageComponent', () => {
  let fixture: ComponentFixture<MiPerfilPageComponent>;
  let component: MiPerfilPageComponent;
  let miPerfilService: jasmine.SpyObj<MiPerfilService>;
  let router: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    miPerfilService = jasmine.createSpyObj('MiPerfilService', ['obtenerPerfil', 'desconectarGitHub', 'getConnectUrl']);
    router = jasmine.createSpyObj('Router', ['navigate']);
    miPerfilService.obtenerPerfil.and.returnValue(
      of({
        id: '1',
        fullName: 'Dev User',
        email: 'dev@kodvian.local',
        role: 'Desarrollador',
        gitHubConnected: false
      })
    );
    miPerfilService.getConnectUrl.and.returnValue('/api/profile/github/connect');

    await TestBed.configureTestingModule({
      imports: [MiPerfilPageComponent, NoopAnimationsModule],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: MiPerfilService, useValue: miPerfilService },
        { provide: Router, useValue: router },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: { queryParamMap: convertToParamMap({ connected: 'true' }) }
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MiPerfilPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should load profile on init', () => {
    expect(miPerfilService.obtenerPerfil).toHaveBeenCalled();
    expect(component.perfil?.fullName).toBe('Dev User');
  });

  it('should clear oauth query params after success', () => {
    expect(router.navigate).toHaveBeenCalled();
  });

  it('should redirect to github connect url', () => {
    const locationRef = window.location as Location & { assign: (url: string) => void };
    const assignSpy = jasmine.createSpy('assign');
    Object.defineProperty(window, 'location', {
      configurable: true,
      value: { ...locationRef, assign: assignSpy }
    });

    component.conectarGitHub();

    expect(miPerfilService.getConnectUrl).toHaveBeenCalled();
    expect(assignSpy).toHaveBeenCalledWith('/api/profile/github/connect');
  });
});
