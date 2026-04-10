import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { ApiResponse, ClienteDetalle, ClienteFormulario, ClienteListado, ClientesFiltros, EstadoCliente, PagedResult } from '../models/clientes.models';

@Injectable({
  providedIn: 'root'
})
export class ClientesService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = '/api/clients';

  obtenerListado(filtros: ClientesFiltros): Observable<PagedResult<ClienteListado>> {
    let params = new HttpParams()
      .set('pageNumber', filtros.pageNumber)
      .set('pageSize', filtros.pageSize);

    if (filtros.search) {
      params = params.set('search', filtros.search);
    }

    if (filtros.status) {
      params = params.set('status', filtros.status);
    }

    return this.http
      .get<ApiResponse<PagedResult<ClienteListado>>>(this.endpoint, { params })
      .pipe(map((response) => response.data));
  }

  obtenerDetalle(id: string): Observable<ClienteDetalle> {
    return this.http
      .get<ApiResponse<ClienteDetalle>>(`${this.endpoint}/${id}`)
      .pipe(map((response) => response.data));
  }

  crear(cliente: ClienteFormulario): Observable<ClienteDetalle> {
    return this.http
      .post<ApiResponse<ClienteDetalle>>(this.endpoint, cliente)
      .pipe(map((response) => response.data));
  }

  actualizar(id: string, cliente: ClienteFormulario): Observable<ClienteDetalle> {
    return this.http
      .put<ApiResponse<ClienteDetalle>>(`${this.endpoint}/${id}`, cliente)
      .pipe(map((response) => response.data));
  }

  cambiarEstado(id: string, status: EstadoCliente): Observable<ClienteDetalle> {
    return this.http
      .patch<ApiResponse<ClienteDetalle>>(`${this.endpoint}/${id}/status`, { status })
      .pipe(map((response) => response.data));
  }
}
