export type Gender = 'Male' | 'Female' | 'Other';
export type BloodType = 'APositive' | 'ANegative' | 'BPositive' | 'BNegative' | 'OPositive' | 'ONegative' | 'ABPositive' | 'ABNegative';

export interface HealthRecord {
  id: string;
  userId: string;
  profileName: string;
  fullName: string;
  dateOfBirth: string;
  gender: Gender;
  bloodType?: BloodType;
  allergies?: string;
  chronicDiseases?: string;
  insuranceNumber?: string;
  createdAt: string;
  updatedAt: string;
  latestMetric?: HealthMetric;
}

export interface HealthMetric {
  id: string;
  healthRecordId: string;
  measuredAt: string;
  heightCm?: number;
  weightKg?: number;
  bmi?: number;
  systolicBp?: number;
  diastolicBp?: number;
  heartRate?: number;
  bloodSugar?: number;
  temperature?: number;
  notes?: string;
  createdAt: string;
}

export interface CreateHealthRecordRequest {
  profileName: string;
  fullName: string;
  dateOfBirth: string;
  gender: Gender;
  bloodType?: BloodType;
  allergies?: string;
  chronicDiseases?: string;
  insuranceNumber?: string;
}

export interface CreateHealthMetricRequest {
  measuredAt?: string;
  heightCm?: number;
  weightKg?: number;
  systolicBp?: number;
  diastolicBp?: number;
  heartRate?: number;
  bloodSugar?: number;
  temperature?: number;
  notes?: string;
}

export const BLOOD_TYPE_LABELS: Record<BloodType, string> = {
  APositive: 'A+', ANegative: 'A−',
  BPositive: 'B+', BNegative: 'B−',
  OPositive: 'O+', ONegative: 'O−',
  ABPositive: 'AB+', ABNegative: 'AB−',
};
