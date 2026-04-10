import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { ApiResponse, DashboardOverview } from '../models/dashboard.models';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);

  obtenerResumen(year?: number, month?: number): Observable<DashboardOverview> {
    const params: Record<string, number> = {};
    if (year) params['year'] = year;
    if (month) params['month'] = month;

    return this.http
      .get<ApiResponse<DashboardOverview>>('/api/dashboard/overview', { params })
      .pipe(map((response) => response.data));
  }
}
