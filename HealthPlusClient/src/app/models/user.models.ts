export interface UpdateUserRequest {
  fullName: string;
  phoneNumber?: string;
  hospitalId?: string;
  specialty?: string;
  consultationFee?: number;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}
