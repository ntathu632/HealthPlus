import { HealthRecord } from './health-record.models';
import { MedicalHistory } from './medical-history.models';
import { Vaccine } from './vaccine.models';
import { Prescription } from './prescription.models';

export interface PatientSummary {
  id: string;
  email: string;
  fullName: string;
  phoneNumber?: string;
  avatarUrl?: string;
  assignedAt: string;
}

export interface PatientProfile {
  patient: PatientSummary;
  healthRecords: HealthRecord[];
  medicalHistories: MedicalHistory[];
  vaccines: Vaccine[];
  prescriptions: Prescription[];
}
