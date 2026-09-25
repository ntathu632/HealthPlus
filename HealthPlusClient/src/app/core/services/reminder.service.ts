import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models/api.models';
import {
  Reminder, CreateReminderRequest, UpdateReminderRequest,
  NotificationSetting, UpdateNotificationSettingRequest,
} from '../../models/reminder.models';

@Injectable({ providedIn: 'root' })
export class ReminderService {
  private readonly api = `${environment.apiUrl}/reminders`;
  private readonly notifApi = `${environment.apiUrl}/notifications/settings`;

  constructor(private http: HttpClient) {}

  getAll(isEnabled?: boolean): Observable<ApiResponse<Reminder[]>> {
    const url = isEnabled !== undefined ? `${this.api}?isEnabled=${isEnabled}` : this.api;
    return this.http.get<ApiResponse<Reminder[]>>(url);
  }

  getUpcoming(hours = 24): Observable<ApiResponse<Reminder[]>> {
    return this.http.get<ApiResponse<Reminder[]>>(`${this.api}/upcoming?hours=${hours}`);
  }

  getById(id: string): Observable<ApiResponse<Reminder>> {
    return this.http.get<ApiResponse<Reminder>>(`${this.api}/${id}`);
  }

  create(request: CreateReminderRequest): Observable<ApiResponse<Reminder>> {
    return this.http.post<ApiResponse<Reminder>>(this.api, request);
  }

  update(id: string, request: UpdateReminderRequest): Observable<ApiResponse<Reminder>> {
    return this.http.put<ApiResponse<Reminder>>(`${this.api}/${id}`, request);
  }

  delete(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.api}/${id}`);
  }

  getSettings(): Observable<ApiResponse<NotificationSetting>> {
    return this.http.get<ApiResponse<NotificationSetting>>(this.notifApi);
  }

  updateSettings(request: UpdateNotificationSettingRequest): Observable<ApiResponse<NotificationSetting>> {
    return this.http.put<ApiResponse<NotificationSetting>>(this.notifApi, request);
  }
}
