import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedResult } from '../../models/api.models';
import { MedicalHistory, CreateMedicalHistoryRequest } from '../../models/medical-history.models';

@Injectable({ providedIn: 'root' })
export class MedicalHistoryService {
  private readonly api = `${environment.apiUrl}/medical-history`;

  constructor(private http: HttpClient) {}

  getAll(healthRecordId?: string, page = 1, pageSize = 20): Observable<ApiResponse<PagedResult<MedicalHistory>>> {
    let url = `${this.api}?page=${page}&pageSize=${pageSize}`;
    if (healthRecordId) url += `&healthRecordId=${healthRecordId}`;
    return this.http.get<ApiResponse<PagedResult<MedicalHistory>>>(url);
  }

  getById(id: string): Observable<ApiResponse<MedicalHistory>> {
    return this.http.get<ApiResponse<MedicalHistory>>(`${this.api}/${id}`);
  }

  getUpcomingFollowUps(days = 30): Observable<ApiResponse<MedicalHistory[]>> {
    return this.http.get<ApiResponse<MedicalHistory[]>>(`${this.api}/follow-ups?days=${days}`);
  }

  create(request: CreateMedicalHistoryRequest): Observable<ApiResponse<MedicalHistory>> {
    return this.http.post<ApiResponse<MedicalHistory>>(this.api, request);
  }

  update(id: string, request: CreateMedicalHistoryRequest): Observable<ApiResponse<MedicalHistory>> {
    return this.http.put<ApiResponse<MedicalHistory>>(`${this.api}/${id}`, request);
  }

  delete(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.api}/${id}`);
  }

  uploadDocument(historyId: string, file: File, documentType?: string): Observable<ApiResponse<any>> {
    const form = new FormData();
    form.append('file', file, file.name);
    if (documentType) form.append('documentType', documentType);
    return this.http.post<ApiResponse<any>>(`${this.api}/${historyId}/documents`, form);
  }
}
