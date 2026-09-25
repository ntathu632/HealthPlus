import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedResult } from '../../models/api.models';
import { Vaccine, VaccineScheduleTemplate, CreateVaccineRequest } from '../../models/vaccine.models';

@Injectable({ providedIn: 'root' })
export class VaccineService {
  private readonly api = `${environment.apiUrl}/vaccines`;

  constructor(private http: HttpClient) {}

  getAll(healthRecordId?: string, page = 1, pageSize = 20): Observable<ApiResponse<PagedResult<Vaccine>>> {
    let url = `${this.api}?page=${page}&pageSize=${pageSize}`;
    if (healthRecordId) url += `&healthRecordId=${healthRecordId}`;
    return this.http.get<ApiResponse<PagedResult<Vaccine>>>(url);
  }

  getOverdue(): Observable<ApiResponse<Vaccine[]>> {
    return this.http.get<ApiResponse<Vaccine[]>>(`${this.api}/overdue`);
  }

  getTemplates(vaccineName?: string): Observable<ApiResponse<VaccineScheduleTemplate[]>> {
    const url = vaccineName ? `${this.api}/templates?vaccineName=${vaccineName}` : `${this.api}/templates`;
    return this.http.get<ApiResponse<VaccineScheduleTemplate[]>>(url);
  }

  getById(id: string): Observable<ApiResponse<Vaccine>> {
    return this.http.get<ApiResponse<Vaccine>>(`${this.api}/${id}`);
  }

  create(request: CreateVaccineRequest): Observable<ApiResponse<Vaccine>> {
    return this.http.post<ApiResponse<Vaccine>>(this.api, request);
  }

  update(id: string, request: CreateVaccineRequest): Observable<ApiResponse<Vaccine>> {
    return this.http.put<ApiResponse<Vaccine>>(`${this.api}/${id}`, request);
  }

  delete(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.api}/${id}`);
  }
}
