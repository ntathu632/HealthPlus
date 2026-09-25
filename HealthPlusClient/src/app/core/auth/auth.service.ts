import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models/api.models';
import { AuthResponse, LoginRequest, RegisterRequest, UserInfo } from '../../models/auth.models';
import { TokenService } from './token.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenService = inject(TokenService);
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly api = `${environment.apiUrl}/auth`;

  private _user = signal<UserInfo | null>(this.tokenService.getUser());
  readonly user = this._user.asReadonly();
  readonly isLoggedIn = computed(() => !!this._user());
  readonly currentUserName = computed(() => this._user()?.fullName ?? '');
  readonly isAdmin = computed(() => this._user()?.roles.includes('Admin') ?? false);
  readonly isDoctor = computed(() => this._user()?.roles.includes('Doctor') ?? false);
  readonly isPatient = computed(() => !this.isAdmin() && !this.isDoctor());

  register(request: RegisterRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.api}/register`, request).pipe(
      tap(res => this.handleAuthSuccess(res.data))
    );
  }

  login(request: LoginRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.api}/login`, request).pipe(
      tap(res => this.handleAuthSuccess(res.data))
    );
  }

  googleLogin(idToken: string): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.api}/google-login`, { idToken }).pipe(
      tap(res => this.handleAuthSuccess(res.data))
    );
  }

  refresh(): Observable<ApiResponse<AuthResponse>> {
    const refreshToken = this.tokenService.getRefreshToken();
    return this.http.post<ApiResponse<AuthResponse>>(`${this.api}/refresh`, { refreshToken }).pipe(
      tap(res => this.handleAuthSuccess(res.data))
    );
  }

  logout(): void {
    const refreshToken = this.tokenService.getRefreshToken();
    if (refreshToken) {
      this.http.post(`${this.api}/logout`, { refreshToken }).subscribe();
    }
    this.tokenService.clear();
    this._user.set(null);
    this.router.navigate(['/']);
  }

  updateUser(patch: Partial<UserInfo>): void {
    const current = this._user();
    if (!current) return;
    const updated = { ...current, ...patch };
    this.tokenService.updateUser(updated);
    this._user.set(updated);
  }

  private handleAuthSuccess(data: AuthResponse): void {
    this.tokenService.save(data.accessToken, data.refreshToken, data.user);
    this._user.set(data.user);
  }
}
