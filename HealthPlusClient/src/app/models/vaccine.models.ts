export type VaccineStatus = 'Completed' | 'Scheduled' | 'Overdue';

export interface Vaccine {
  id: string;
  userId: string;
  healthRecordId: string;
  vaccineName: string;
  manufacturer?: string;
  lotNumber?: string;
  doseNumber: number;
  injectionDate: string;
  nextDueDate?: string;
  location?: string;
  administeredBy?: string;
  sideEffects?: string;
  status: VaccineStatus;
  notes?: string;
  createdAt: string;
}

export interface VaccineScheduleTemplate {
  id: number;
  vaccineName: string;
  doseNumber: number;
  intervalDays?: number;
  recommendedAge?: string;
  notes?: string;
}

export interface CreateVaccineRequest {
  healthRecordId: string;
  vaccineName: string;
  manufacturer?: string;
  lotNumber?: string;
  doseNumber: number;
  injectionDate: string;
  nextDueDate?: string;
  location?: string;
  administeredBy?: string;
  sideEffects?: string;
  status: VaccineStatus;
  notes?: string;
}
