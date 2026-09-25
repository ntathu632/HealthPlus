import { TokenService } from './token.service';
import { UserInfo } from '../../models/auth.models';

describe('TokenService', () => {
  let service: TokenService;

  const user: UserInfo = {
    id: 'user-1',
    email: 'user@example.com',
    fullName: 'Nguyen Van A',
    roles: ['User'],
  };

  beforeEach(() => {
    localStorage.clear();
    service = new TokenService();
  });

  it('returns null for all fields when nothing has been saved', () => {
    expect(service.getAccessToken()).toBeNull();
    expect(service.getRefreshToken()).toBeNull();
    expect(service.getUser()).toBeNull();
    expect(service.isLoggedIn()).toBe(false);
  });

  it('save() persists access token, refresh token and user', () => {
    service.save('access-123', 'refresh-456', user);

    expect(service.getAccessToken()).toBe('access-123');
    expect(service.getRefreshToken()).toBe('refresh-456');
    expect(service.getUser()).toEqual(user);
    expect(service.isLoggedIn()).toBe(true);
  });

  it('clear() removes all persisted auth state', () => {
    service.save('access-123', 'refresh-456', user);
    service.clear();

    expect(service.getAccessToken()).toBeNull();
    expect(service.getRefreshToken()).toBeNull();
    expect(service.getUser()).toBeNull();
    expect(service.isLoggedIn()).toBe(false);
  });

  it('updateUser() overwrites only the stored user, not the tokens', () => {
    service.save('access-123', 'refresh-456', user);

    const updated: UserInfo = { ...user, fullName: 'Nguyen Van B', avatarUrl: '/uploads/avatars/x.png' };
    service.updateUser(updated);

    expect(service.getUser()).toEqual(updated);
    expect(service.getAccessToken()).toBe('access-123');
    expect(service.getRefreshToken()).toBe('refresh-456');
  });
});
