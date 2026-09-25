import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models/api.models';
import {
  Appointment, DoctorListItem, AppointmentStatus,
  CreateAppointmentRequest, UpdateAppointmentStatusRequest,
} from '../../models/appointment.models';

@Injectable({ providedIn: 'root' })
export class AppointmentService {
  private readonly api = `${environment.apiUrl}/appointments`;

  constructor(private http: HttpClient) {}

  getActiveDoctors(): Observable<ApiResponse<DoctorListItem[]>> {
    return this.http.get<ApiResponse<DoctorListItem[]>>(`${this.api}/doctors`);
  }

  getMyAppointments(status?: AppointmentStatus): Observable<ApiResponse<Appointment[]>> {
    const url = status ? `${this.api}?status=${status}` : this.api;
    return this.http.get<ApiResponse<Appointment[]>>(url);
  }

  create(request: CreateAppointmentRequest): Observable<ApiResponse<Appointment>> {
    return this.http.post<ApiResponse<Appointment>>(this.api, request);
  }

  cancel(id: string): Observable<ApiResponse<null>> {
    return this.http.put<ApiResponse<null>>(`${this.api}/${id}/cancel`, {});
  }

  getMyAppointmentsAsDoctor(status?: AppointmentStatus): Observable<ApiResponse<Appointment[]>> {
    const url = status ? `${this.api}/doctor?status=${status}` : `${this.api}/doctor`;
    return this.http.get<ApiResponse<Appointment[]>>(url);
  }

  updateStatus(id: string, request: UpdateAppointmentStatusRequest): Observable<ApiResponse<Appointment>> {
    return this.http.put<ApiResponse<Appointment>>(`${this.api}/${id}/status`, request);
  }
}
