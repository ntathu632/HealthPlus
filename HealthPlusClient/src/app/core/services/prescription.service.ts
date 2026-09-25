import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models/api.models';
import {
  Prescription, PrescriptionItem,
  CreatePrescriptionRequest, CreatePrescriptionItemRequest, UpdatePrescriptionItemRequest
} from '../../models/prescription.models';

@Injectable({ providedIn: 'root' })
export class PrescriptionService {
  private readonly api = `${environment.apiUrl}/prescriptions`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<Prescription[]>> {
    return this.http.get<ApiResponse<Prescription[]>>(this.api);
  }

  getById(id: string): Observable<ApiResponse<Prescription>> {
    return this.http.get<ApiResponse<Prescription>>(`${this.api}/${id}`);
  }

  create(request: CreatePrescriptionRequest): Observable<ApiResponse<Prescription>> {
    return this.http.post<ApiResponse<Prescription>>(this.api, request);
  }

  delete(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.api}/${id}`);
  }

  addItem(prescriptionId: string, request: CreatePrescriptionItemRequest): Observable<ApiResponse<PrescriptionItem>> {
    return this.http.post<ApiResponse<PrescriptionItem>>(`${this.api}/${prescriptionId}/items`, request);
  }

  updateItem(prescriptionId: string, itemId: string, request: UpdatePrescriptionItemRequest): Observable<ApiResponse<PrescriptionItem>> {
    return this.http.put<ApiResponse<PrescriptionItem>>(`${this.api}/${prescriptionId}/items/${itemId}`, request);
  }

  deleteItem(prescriptionId: string, itemId: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.api}/${prescriptionId}/items/${itemId}`);
  }

  uploadImage(id: string, file: File): Observable<ApiResponse<Prescription>> {
    const form = new FormData();
    form.append('image', file);
    return this.http.post<ApiResponse<Prescription>>(`${this.api}/${id}/upload-image`, form);
  }
}
