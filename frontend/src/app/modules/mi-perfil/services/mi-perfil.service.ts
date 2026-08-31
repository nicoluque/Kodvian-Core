import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { ApiResponse } from '../../../shared/models/api.models';
import { UserProfile } from '../models/mi-perfil.models';

@Injectable({ providedIn: 'root' })
export class MiPerfilService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = '/api/profile';

  obtenerPerfil(): Observable<UserProfile> {
    return this.http.get<ApiResponse<UserProfile>>(this.endpoint).pipe(map((response) => response.data));
  }

  desconectarGitHub(): Observable<void> {
    return this.http.delete<ApiResponse<object>>(`${this.endpoint}/github/disconnect`).pipe(map(() => undefined));
  }

  getConnectUrl(): string {
    return `${this.endpoint}/github/connect`;
  }
}
