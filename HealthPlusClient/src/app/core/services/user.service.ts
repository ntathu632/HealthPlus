import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models/api.models';
import { UserResponse } from '../../models/auth.models';
import { UpdateUserRequest, ChangePasswordRequest } from '../../models/user.models';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly api = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient) {}

  getMe(): Observable<ApiResponse<UserResponse>> {
    return this.http.get<ApiResponse<UserResponse>>(`${this.api}/me`);
  }

  updateProfile(request: UpdateUserRequest): Observable<ApiResponse<UserResponse>> {
    return this.http.put<ApiResponse<UserResponse>>(`${this.api}/me`, request);
  }

  changePassword(request: ChangePasswordRequest): Observable<ApiResponse<null>> {
    return this.http.put<ApiResponse<null>>(`${this.api}/me/password`, request);
  }

  uploadAvatar(file: File): Observable<ApiResponse<{ avatarUrl: string }>> {
    const form = new FormData();
    form.append('avatar', file);
    return this.http.post<ApiResponse<{ avatarUrl: string }>>(`${this.api}/me/avatar`, form);
  }
}
