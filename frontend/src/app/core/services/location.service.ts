import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { ApiResponse } from '../../shared/models/api.models';
import { CityOption, CountryOption, RegionOption } from '../models/location.models';

@Injectable({
  providedIn: 'root'
})
export class LocationService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = '/api/locations';

  getCountries(): Observable<CountryOption[]> {
    return this.http
      .get<ApiResponse<CountryOption[]>>(`${this.endpoint}/countries`)
      .pipe(map((response) => response.data));
  }

  getRegions(countryCode: string): Observable<RegionOption[]> {
    const params = new HttpParams().set('countryCode', countryCode);

    return this.http
      .get<ApiResponse<RegionOption[]>>(`${this.endpoint}/regions`, { params })
      .pipe(map((response) => response.data));
  }

  getCities(countryCode: string, regionCode: string): Observable<CityOption[]> {
    const params = new HttpParams()
      .set('countryCode', countryCode)
      .set('regionCode', regionCode);

    return this.http
      .get<ApiResponse<CityOption[]>>(`${this.endpoint}/cities`, { params })
      .pipe(map((response) => response.data));
  }
}
