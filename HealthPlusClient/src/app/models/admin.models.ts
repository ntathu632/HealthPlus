export interface Role {
  id: number;
  name: string;
  description?: string;
}

export interface Permission {
  id: number;
  name: string;
  resource: string;
  action: string;
  description?: string;
}

export interface AdminUser {
  id: string;
  email: string;
  fullName: string;
  phoneNumber?: string;
  isActive: boolean;
  isEmailVerified: boolean;
  lastLoginAt?: string;
  createdAt: string;
  roles: Role[];
}

export interface AuditLog {
  id: number;
  userId?: string;
  userEmail?: string;
  action: string;
  entity?: string;
  entityId?: string;
  oldValues?: string;
  newValues?: string;
  ipAddress?: string;
  createdAt: string;
}

export interface SystemSetting {
  id: number;
  settingKey: string;
  settingValue?: string;
  description?: string;
  updatedAt: string;
}

export interface DoctorPatient {
  id: string;
  doctorId: string;
  doctorName: string;
  patientId: string;
  patientName: string;
  isActive: boolean;
  createdAt: string;
}

export interface AdminDashboardStats {
  totalUsers: number;
  totalDoctors: number;
  totalPatients: number;
  totalAssignments: number;
  recentAuditLogCount: number;
}

export interface UpdateUserRoleRequest {
  roleId: number;
}

export interface UpdateUserStatusRequest {
  isActive: boolean;
}

export interface CreateDoctorRequest {
  email: string;
  password: string;
  fullName: string;
  phoneNumber?: string;
  roleId: number; // 2 = Bác sĩ, 3 = Bệnh nhân
}

export interface ResetPasswordRequest {
  newPassword: string;
}

export interface UpdateRolePermissionsRequest {
  permissionIds: number[];
}

export interface AssignPatientRequest {
  doctorId: string;
  patientId: string;
}

export interface UpdateSystemSettingRequest {
  settingValue?: string;
}
