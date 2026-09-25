export type PaymentStatus = 'Pending' | 'Completed' | 'Failed';

export interface Payment {
  id: string;
  appointmentId: string;
  doctorName: string;
  patientName: string;
  appointmentTime: string;
  amount: number;
  status: PaymentStatus;
  method: string;
  paidAt?: string;
  createdAt: string;
}
