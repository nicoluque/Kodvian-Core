import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { ApiResponse } from '../../../shared/models/api.models';
import { EstadoTarea } from '../../tareas/models/tareas.models';
import {
  KanbanColumn,
  MiTrabajoCreateIssueRequest,
  MiTrabajoIssue,
  MiTrabajoIssueFiltros,
  MiTrabajoIssueStatus,
  MiTrabajoOverview,
  MiTrabajoRepositoriosPage,
  MiTrabajoSyncResult,
  PagedResult,
  ProyectoFiltros,
  ProyectoListado,
  TareaDetalle,
  TareaFiltros,
  TareaListado
} from '../models/mi-trabajo.models';

@Injectable({ providedIn: 'root' })
export class MiTrabajoService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = '/api/my-work';

  obtenerOverview(): Observable<MiTrabajoOverview> {
    return this.http.get<ApiResponse<MiTrabajoOverview>>(`${this.endpoint}/overview`).pipe(map((r) => r.data));
  }

  obtenerRepositorios(pageNumber = 1, pageSize = 20, search?: string): Observable<MiTrabajoRepositoriosPage> {
    let params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    if (search) {
      params = params.set('search', search);
    }
    return this.http
      .get<ApiResponse<MiTrabajoRepositoriosPage>>(`${this.endpoint}/repositories`, { params })
      .pipe(map((r) => r.data));
  }

  obtenerIssues(filtros: MiTrabajoIssueFiltros): Observable<PagedResult<MiTrabajoIssue>> {
    let params = new HttpParams().set('pageNumber', filtros.pageNumber).set('pageSize', filtros.pageSize);
    if (filtros.search) params = params.set('search', filtros.search);
    if (filtros.projectId) params = params.set('projectId', filtros.projectId);
    if (filtros.status) params = params.set('status', filtros.status);
    if (filtros.priority) params = params.set('priority', filtros.priority);
    return this.http.get<ApiResponse<PagedResult<MiTrabajoIssue>>>(`${this.endpoint}/issues`, { params }).pipe(map((r) => r.data));
  }

  sincronizarIssues(projectId?: string): Observable<MiTrabajoSyncResult> {
    return this.http
      .post<ApiResponse<MiTrabajoSyncResult>>(`${this.endpoint}/sync`, projectId ? { projectId } : {})
      .pipe(map((r) => r.data));
  }

  crearIssue(request: MiTrabajoCreateIssueRequest): Observable<MiTrabajoIssue> {
    return this.http
      .post<ApiResponse<MiTrabajoIssue>>(`${this.endpoint}/issues`, request)
      .pipe(map((r) => r.data));
  }

  actualizarEstadoIssue(id: string, status: MiTrabajoIssueStatus): Observable<MiTrabajoIssue> {
    return this.http
      .patch<ApiResponse<MiTrabajoIssue>>(`${this.endpoint}/issues/${id}/status`, { status })
      .pipe(map((r) => r.data));
  }

  obtenerProyectos(filtros: ProyectoFiltros): Observable<PagedResult<ProyectoListado>> {
    let params = new HttpParams().set('pageNumber', filtros.pageNumber).set('pageSize', filtros.pageSize);
    if (filtros.search) params = params.set('search', filtros.search);
    if (filtros.status) params = params.set('status', filtros.status);
    if (filtros.priority) params = params.set('priority', filtros.priority);
    return this.http.get<ApiResponse<PagedResult<ProyectoListado>>>(`${this.endpoint}/projects`, { params }).pipe(map((r) => r.data));
  }

  obtenerTareas(filtros: TareaFiltros): Observable<PagedResult<TareaListado>> {
    let params = new HttpParams().set('pageNumber', filtros.pageNumber).set('pageSize', filtros.pageSize);
    if (filtros.search) params = params.set('search', filtros.search);
    if (filtros.projectId) params = params.set('projectId', filtros.projectId);
    if (filtros.status) params = params.set('status', filtros.status);
    if (filtros.priority) params = params.set('priority', filtros.priority);
    if (filtros.dueDateFrom) params = params.set('dueDateFrom', filtros.dueDateFrom);
    if (filtros.dueDateTo) params = params.set('dueDateTo', filtros.dueDateTo);
    return this.http.get<ApiResponse<PagedResult<TareaListado>>>(`${this.endpoint}/tasks`, { params }).pipe(map((r) => r.data));
  }

  obtenerKanban(filtros: TareaFiltros): Observable<KanbanColumn[]> {
    let params = new HttpParams().set('pageNumber', filtros.pageNumber).set('pageSize', filtros.pageSize);
    if (filtros.search) params = params.set('search', filtros.search);
    if (filtros.projectId) params = params.set('projectId', filtros.projectId);
    if (filtros.status) params = params.set('status', filtros.status);
    if (filtros.priority) params = params.set('priority', filtros.priority);
    return this.http.get<ApiResponse<KanbanColumn[]>>(`${this.endpoint}/tasks/kanban`, { params }).pipe(map((r) => r.data));
  }

  obtenerDetalleTarea(id: string): Observable<TareaDetalle> {
    return this.http.get<ApiResponse<TareaDetalle>>(`${this.endpoint}/tasks/${id}`).pipe(map((r) => r.data));
  }

  actualizarEstadoTarea(id: string, status: EstadoTarea, kanbanOrder = 0): Observable<TareaDetalle> {
    return this.http.patch<ApiResponse<TareaDetalle>>(`${this.endpoint}/tasks/${id}/status`, { status, kanbanOrder }).pipe(map((r) => r.data));
  }
}
