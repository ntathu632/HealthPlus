import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedResult } from '../../models/api.models';
import {
  AdminUser, Role, Permission, AuditLog, SystemSetting, DoctorPatient, AdminDashboardStats,
  UpdateUserRoleRequest, UpdateUserStatusRequest, CreateDoctorRequest, ResetPasswordRequest,
  UpdateRolePermissionsRequest, AssignPatientRequest, UpdateSystemSettingRequest,
} from '../../models/admin.models';
import { Appointment, AppointmentStatus } from '../../models/appointment.models';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly api = `${environment.apiUrl}/admin`;

  constructor(private http: HttpClient) {}

  getDashboard(): Observable<ApiResponse<AdminDashboardStats>> {
    return this.http.get<ApiResponse<AdminDashboardStats>>(`${this.api}/dashboard`);
  }

  getUsers(page = 1, pageSize = 20, search?: string, roleId?: number): Observable<ApiResponse<PagedResult<AdminUser>>> {
    let url = `${this.api}/users?page=${page}&pageSize=${pageSize}`;
    if (search) url += `&search=${encodeURIComponent(search)}`;
    if (roleId) url += `&roleId=${roleId}`;
    return this.http.get<ApiResponse<PagedResult<AdminUser>>>(url);
  }

  getUserById(id: string): Observable<ApiResponse<AdminUser>> {
    return this.http.get<ApiResponse<AdminUser>>(`${this.api}/users/${id}`);
  }

  updateUserRole(id: string, request: UpdateUserRoleRequest): Observable<ApiResponse<AdminUser>> {
    return this.http.put<ApiResponse<AdminUser>>(`${this.api}/users/${id}/role`, request);
  }

  updateUserStatus(id: string, request: UpdateUserStatusRequest): Observable<ApiResponse<AdminUser>> {
    return this.http.put<ApiResponse<AdminUser>>(`${this.api}/users/${id}/status`, request);
  }

  createDoctor(request: CreateDoctorRequest): Observable<ApiResponse<AdminUser>> {
    return this.http.post<ApiResponse<AdminUser>>(`${this.api}/doctors`, request);
  }

  resetPassword(id: string, request: ResetPasswordRequest): Observable<ApiResponse<null>> {
    return this.http.put<ApiResponse<null>>(`${this.api}/users/${id}/reset-password`, request);
  }

  getRoles(): Observable<ApiResponse<Role[]>> {
    return this.http.get<ApiResponse<Role[]>>(`${this.api}/roles`);
  }

  getPermissions(): Observable<ApiResponse<Permission[]>> {
    return this.http.get<ApiResponse<Permission[]>>(`${this.api}/permissions`);
  }

  getRolePermissionIds(roleId: number): Observable<ApiResponse<number[]>> {
    return this.http.get<ApiResponse<number[]>>(`${this.api}/roles/${roleId}/permissions`);
  }

  updateRolePermissions(roleId: number, request: UpdateRolePermissionsRequest): Observable<ApiResponse<null>> {
    return this.http.put<ApiResponse<null>>(`${this.api}/roles/${roleId}/permissions`, request);
  }

  getDoctorPatients(doctorId?: string): Observable<ApiResponse<DoctorPatient[]>> {
    const url = doctorId ? `${this.api}/doctor-patients?doctorId=${doctorId}` : `${this.api}/doctor-patients`;
    return this.http.get<ApiResponse<DoctorPatient[]>>(url);
  }

  assignPatient(request: AssignPatientRequest): Observable<ApiResponse<DoctorPatient>> {
    return this.http.post<ApiResponse<DoctorPatient>>(`${this.api}/doctor-patients`, request);
  }

  unassignPatient(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.api}/doctor-patients/${id}`);
  }

  getAuditLogs(page = 1, pageSize = 20, userId?: string, entity?: string): Observable<ApiResponse<PagedResult<AuditLog>>> {
    let url = `${this.api}/audit-logs?page=${page}&pageSize=${pageSize}`;
    if (userId) url += `&userId=${userId}`;
    if (entity) url += `&entity=${encodeURIComponent(entity)}`;
    return this.http.get<ApiResponse<PagedResult<AuditLog>>>(url);
  }

  getSettings(): Observable<ApiResponse<SystemSetting[]>> {
    return this.http.get<ApiResponse<SystemSetting[]>>(`${this.api}/settings`);
  }

  updateSetting(key: string, request: UpdateSystemSettingRequest): Observable<ApiResponse<SystemSetting>> {
    return this.http.put<ApiResponse<SystemSetting>>(`${this.api}/settings/${encodeURIComponent(key)}`, request);
  }

  getAllAppointments(page = 1, pageSize = 20, status?: AppointmentStatus): Observable<ApiResponse<PagedResult<Appointment>>> {
    let url = `${this.api}/appointments?page=${page}&pageSize=${pageSize}`;
    if (status) url += `&status=${status}`;
    return this.http.get<ApiResponse<PagedResult<Appointment>>>(url);
  }
}
