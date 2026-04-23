import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { ApiResponse, DesarrolladorExterno, DesarrolladorFormulario, ResumenContratoDesarrollador } from '../models/desarrolladores.models';

@Injectable({
  providedIn: 'root'
})
export class DesarrolladoresService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = '/api/developers';

  obtenerListado(): Observable<DesarrolladorExterno[]> {
    return this.http.get<ApiResponse<DesarrolladorExterno[]>>(this.endpoint).pipe(map((r) => r.data));
  }

  crear(payload: DesarrolladorFormulario): Observable<DesarrolladorExterno> {
    return this.http.post<ApiResponse<DesarrolladorExterno>>(this.endpoint, payload).pipe(map((r) => r.data));
  }

  actualizar(id: string, payload: DesarrolladorFormulario): Observable<DesarrolladorExterno> {
    return this.http.put<ApiResponse<DesarrolladorExterno>>(`${this.endpoint}/${id}`, payload).pipe(map((r) => r.data));
  }

  obtenerResumenContratos(developerId: string, year: number): Observable<ResumenContratoDesarrollador[]> {
    return this.http
      .get<ApiResponse<ResumenContratoDesarrollador[]>>(`${this.endpoint}/${developerId}/contracts-summary`, { params: { year } })
      .pipe(map((r) => r.data));
  }
}
