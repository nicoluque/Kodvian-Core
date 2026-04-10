import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { ApiResponse, PagedResult, ProyectoDetalle, ProyectoFiltros, ProyectoFormulario, ProyectoListado, ProyectoLookups } from '../models/proyectos.models';

@Injectable({
  providedIn: 'root'
})
export class ProyectosService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = '/api/projects';

  obtenerListado(filtros: ProyectoFiltros): Observable<PagedResult<ProyectoListado>> {
    let params = new HttpParams()
      .set('pageNumber', filtros.pageNumber)
      .set('pageSize', filtros.pageSize);

    if (filtros.search) {
      params = params.set('search', filtros.search);
    }

    if (filtros.clientId) {
      params = params.set('clientId', filtros.clientId);
    }

    if (filtros.status) {
      params = params.set('status', filtros.status);
    }

    if (filtros.priority) {
      params = params.set('priority', filtros.priority);
    }

    return this.http.get<ApiResponse<PagedResult<ProyectoListado>>>(this.endpoint, { params }).pipe(map((r) => r.data));
  }

  obtenerDetalle(id: string): Observable<ProyectoDetalle> {
    return this.http.get<ApiResponse<ProyectoDetalle>>(`${this.endpoint}/${id}`).pipe(map((r) => r.data));
  }

  obtenerLookups(): Observable<ProyectoLookups> {
    return this.http.get<ApiResponse<ProyectoLookups>>(`${this.endpoint}/lookups`).pipe(map((r) => r.data));
  }

  crear(payload: ProyectoFormulario): Observable<ProyectoDetalle> {
    return this.http.post<ApiResponse<ProyectoDetalle>>(this.endpoint, payload).pipe(map((r) => r.data));
  }

  actualizar(id: string, payload: ProyectoFormulario): Observable<ProyectoDetalle> {
    return this.http.put<ApiResponse<ProyectoDetalle>>(`${this.endpoint}/${id}`, payload).pipe(map((r) => r.data));
  }
}
