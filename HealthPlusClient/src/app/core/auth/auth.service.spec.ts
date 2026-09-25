import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideRouter, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { TokenService } from './token.service';
import { environment } from '../../../environments/environment';
import { AuthResponse } from '../../models/auth.models';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  let tokenService: TokenService;
  let router: Router;

  const authResponse: AuthResponse = {
    accessToken: 'access-123',
    refreshToken: 'refresh-456',
    accessTokenExpiresAt: '2099-01-01T00:00:00Z',
    user: { id: 'user-1', email: 'user@example.com', fullName: 'Nguyen Van A', roles: ['User'] },
  };

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    tokenService = TestBed.inject(TokenService);
    router = TestBed.inject(Router);
  });

  afterEach(() => httpMock.verify());

  it('starts logged out when no token is stored', () => {
    expect(service.isLoggedIn()).toBe(false);
    expect(service.user()).toBeNull();
  });

  it('login() success stores tokens and updates the user signal', () => {
    service.login({ email: 'user@example.com', password: 'Password1' }).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/auth/login`);
    expect(req.request.method).toBe('POST');
    req.flush({ success: true, data: authResponse });

    expect(service.isLoggedIn()).toBe(true);
    expect(service.user()).toEqual(authResponse.user);
    expect(service.currentUserName()).toBe('Nguyen Van A');
    expect(tokenService.getAccessToken()).toBe('access-123');
  });

  it('logout() clears state and navigates to the home page', () => {
    service.login({ email: 'user@example.com', password: 'Password1' }).subscribe();
    httpMock.expectOne(`${environment.apiUrl}/auth/login`).flush({ success: true, data: authResponse });

    const navigateSpy = vi.spyOn(router, 'navigate');
    service.logout();

    httpMock.expectOne(`${environment.apiUrl}/auth/logout`).flush({ success: true, data: null });

    expect(service.isLoggedIn()).toBe(false);
    expect(service.user()).toBeNull();
    expect(tokenService.getAccessToken()).toBeNull();
    expect(navigateSpy).toHaveBeenCalledWith(['/']);
  });

  it('updateUser() merges a partial patch into the current user without touching tokens', () => {
    service.login({ email: 'user@example.com', password: 'Password1' }).subscribe();
    httpMock.expectOne(`${environment.apiUrl}/auth/login`).flush({ success: true, data: authResponse });

    service.updateUser({ fullName: 'Nguyen Van B', avatarUrl: '/uploads/avatars/x.png' });

    expect(service.user()?.fullName).toBe('Nguyen Van B');
    expect(service.user()?.avatarUrl).toBe('/uploads/avatars/x.png');
    expect(service.user()?.email).toBe(authResponse.user.email);
    expect(tokenService.getAccessToken()).toBe('access-123');
  });

  it('updateUser() is a no-op when nobody is logged in', () => {
    service.updateUser({ fullName: 'Should not apply' });
    expect(service.user()).toBeNull();
  });
});
