import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedResult } from '../../models/api.models';
import { HealthRecord, HealthMetric, CreateHealthRecordRequest, CreateHealthMetricRequest } from '../../models/health-record.models';

@Injectable({ providedIn: 'root' })
export class HealthRecordService {
  private readonly api = `${environment.apiUrl}/health-records`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<HealthRecord[]>> {
    return this.http.get<ApiResponse<HealthRecord[]>>(this.api);
  }

  getById(id: string): Observable<ApiResponse<HealthRecord>> {
    return this.http.get<ApiResponse<HealthRecord>>(`${this.api}/${id}`);
  }

  create(request: CreateHealthRecordRequest): Observable<ApiResponse<HealthRecord>> {
    return this.http.post<ApiResponse<HealthRecord>>(this.api, request);
  }

  update(id: string, request: CreateHealthRecordRequest): Observable<ApiResponse<HealthRecord>> {
    return this.http.put<ApiResponse<HealthRecord>>(`${this.api}/${id}`, request);
  }

  delete(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.api}/${id}`);
  }

  getMetrics(recordId: string, page = 1, pageSize = 20): Observable<ApiResponse<PagedResult<HealthMetric>>> {
    return this.http.get<ApiResponse<PagedResult<HealthMetric>>>(
      `${this.api}/${recordId}/metrics?page=${page}&pageSize=${pageSize}`
    );
  }

  addMetric(recordId: string, request: CreateHealthMetricRequest): Observable<ApiResponse<HealthMetric>> {
    return this.http.post<ApiResponse<HealthMetric>>(`${this.api}/${recordId}/metrics`, request);
  }

  deleteMetric(recordId: string, metricId: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.api}/${recordId}/metrics/${metricId}`);
  }
}
