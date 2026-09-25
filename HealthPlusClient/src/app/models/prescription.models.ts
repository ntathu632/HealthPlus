export type PrescriptionStatus = 'Pending' | 'Processing' | 'Completed' | 'Failed';

export interface PrescriptionItem {
  id: string;
  prescriptionId: string;
  medicineName: string;
  dosage?: string;
  frequencyPerDay?: number;
  durationDays?: number;
  timing?: string;
  instructions?: string;
  isConfirmed: boolean;
}

export interface Prescription {
  id: string;
  userId: string;
  medicalHistoryId?: string;
  imageUrl?: string;
  ocrRawText?: string;
  ocrConfidenceScore?: number;
  status: PrescriptionStatus;
  processedAt?: string;
  createdAt: string;
  items: PrescriptionItem[];
}

export interface CreatePrescriptionRequest {
  medicalHistoryId?: string;
  notes?: string;
}

export interface CreatePrescriptionItemRequest {
  medicineName: string;
  dosage?: string;
  frequencyPerDay?: number;
  durationDays?: number;
  timing?: string;
  instructions?: string;
}

export interface UpdatePrescriptionItemRequest {
  medicineName: string;
  dosage?: string;
  frequencyPerDay?: number;
  durationDays?: number;
  timing?: string;
  instructions?: string;
  isConfirmed: boolean;
}
