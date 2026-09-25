export type AppointmentStatus = 'Pending' | 'Confirmed' | 'Completed' | 'Cancelled';

export interface DoctorListItem {
  id: string;
  fullName: string;
  email: string;
  phoneNumber?: string;
  avatarUrl?: string;
  specialty?: string;
  hospitalName?: string;
  consultationFee: number;
}

export interface Appointment {
  id: string;
  doctorId: string;
  doctorName: string;
  patientId: string;
  patientName: string;
  appointmentTime: string;
  reason?: string;
  status: AppointmentStatus;
  notes?: string;
  fee: number;
  isPaid: boolean;
  createdAt: string;
  videoRoomUrl: string;
}

export interface CreateAppointmentRequest {
  doctorId: string;
  appointmentTime: string;
  reason?: string;
}

export interface UpdateAppointmentStatusRequest {
  status: AppointmentStatus;
  notes?: string;
}
