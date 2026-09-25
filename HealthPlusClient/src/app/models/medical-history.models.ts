export interface MedicalHistory {
  id: string;
  userId: string;
  healthRecordId: string;
  visitDate: string;
  doctorName?: string;
  hospital?: string;
  specialty?: string;
  diagnosis?: string;
  treatment?: string;
  followUpDate?: string;
  notes?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateMedicalHistoryRequest {
  healthRecordId: string;
  visitDate: string;
  doctorName?: string;
  hospital?: string;
  specialty?: string;
  diagnosis?: string;
  treatment?: string;
  followUpDate?: string;
  notes?: string;
}
