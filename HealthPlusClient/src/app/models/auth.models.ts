export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
  phoneNumber?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  user: UserInfo;
}

export interface UserInfo {
  id: string;
  email: string;
  fullName: string;
  avatarUrl?: string;
  roles: string[];
}

export interface UserResponse {
  id: string;
  email: string;
  fullName: string;
  phoneNumber?: string;
  avatarUrl?: string;
  isEmailVerified: boolean;
  lastLoginAt?: string;
  createdAt: string;
  roles: string[];
  // Chỉ có giá trị với tài khoản vai trò Bác sĩ.
  hospitalId?: string;
  hospitalName?: string;
  specialty?: string;
  consultationFee?: number;
}
