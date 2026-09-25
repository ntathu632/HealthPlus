import { Component, OnInit, signal, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../core/auth/auth.service';
import { DoctorService } from '../../../core/services/doctor.service';
import { AppointmentService } from '../../../core/services/appointment.service';
import { PatientSummary } from '../../../models/doctor.models';
import { Appointment } from '../../../models/appointment.models';

interface StatCard { label: string; value: number; icon: string; fg: string; lightBg: string; route: string; }

// Cache ở module-level (không nằm trong instance) để dữ liệu vẫn còn đó khi bấm sang
// mục khác ở sidebar rồi quay lại — tránh chớp qua trạng thái "đang tải/trống" mỗi lần
// component bị huỷ-tạo lại do điều hướng. Chỉ hiện lại spinner ở lần tải đầu tiên của
// mỗi tài khoản (so khớp userId), các lần quay lại sau đó hiện ngay dữ liệu đã có rồi
// âm thầm làm mới ở nền.
let cachedUserId: string | null = null;
const patientsCache = signal<PatientSummary[]>([]);
const appointmentsCache = signal<Appointment[]>([]);
const patientsLoadedOnce = signal(false);
const appointmentsLoadedOnce = signal(false);

@Component({
  selector: 'app-doctor-dashboard',
  standalone: true,
  imports: [RouterLink, DatePipe, MatIconModule, MatButtonModule, MatProgressSpinnerModule],
  templateUrl: './doctor-dashboard.component.html',
  styleUrl: './doctor-dashboard.component.scss',
})
export class DoctorDashboardComponent implements OnInit {
  patients = patientsCache;
  appointments = appointmentsCache;
  patientsLoading = computed(() => !patientsLoadedOnce());
  appointmentsLoading = computed(() => !appointmentsLoadedOnce());
  readonly today = new Date();

  // Sắp tới (không giới hạn đúng ngày hôm nay) — để mục này luôn có nội dung hữu ích
  // thay vì trống trơn chỉ vì lịch hẹn gần nhất không rơi đúng vào hôm nay.
  upcomingAppointments = computed(() => {
    const now = Date.now();
    return this.appointments()
      .filter(a => (a.status === 'Pending' || a.status === 'Confirmed') && new Date(a.appointmentTime).getTime() >= now)
      .sort((a, b) => new Date(a.appointmentTime).getTime() - new Date(b.appointmentTime).getTime());
  });

  pendingAppointments = computed(() =>
    this.appointments().filter(a => a.status === 'Pending')
      .sort((a, b) => new Date(a.appointmentTime).getTime() - new Date(b.appointmentTime).getTime()));

  recentPatients = computed(() =>
    [...this.patients()].sort((a, b) => new Date(b.assignedAt).getTime() - new Date(a.assignedAt).getTime()));

  stats = computed<StatCard[]>(() => [
    { label: 'Tổng bệnh nhân', value: this.patients().length, icon: 'groups', fg: '#1565C0', lightBg: '#E3F2FD', route: '/doctor/patients' },
    { label: 'Lịch hẹn sắp tới', value: this.upcomingAppointments().length, icon: 'today', fg: '#0D47A1', lightBg: '#BBDEFB', route: '/doctor/appointments' },
    { label: 'Chờ xác nhận', value: this.pendingAppointments().length, icon: 'pending_actions', fg: '#C62828', lightBg: '#FFEBEE', route: '/doctor/appointments' },
    { label: 'Đã xác nhận (sắp tới)', value: this.upcomingConfirmedCount(), icon: 'event_available', fg: '#6A1B9A', lightBg: '#F3E5F5', route: '/doctor/appointments' },
  ]);

  constructor(
    public auth: AuthService,
    private doctorSvc: DoctorService,
    private appointmentSvc: AppointmentService,
  ) {}

  ngOnInit(): void {
    const userId = this.auth.user()?.id ?? null;
    if (userId !== cachedUserId) {
      cachedUserId = userId;
      patientsCache.set([]);
      appointmentsCache.set([]);
      patientsLoadedOnce.set(false);
      appointmentsLoadedOnce.set(false);
    }

    this.doctorSvc.getMyPatients().subscribe({
      next: res => { patientsCache.set(res.data); patientsLoadedOnce.set(true); },
      error: () => patientsLoadedOnce.set(true),
    });
    this.appointmentSvc.getMyAppointmentsAsDoctor().subscribe({
      next: res => { appointmentsCache.set(res.data); appointmentsLoadedOnce.set(true); },
      error: () => appointmentsLoadedOnce.set(true),
    });
  }

  private upcomingConfirmedCount(): number {
    const now = Date.now();
    return this.appointments().filter(a => a.status === 'Confirmed' && new Date(a.appointmentTime).getTime() >= now).length;
  }

  firstName(): string {
    const name = this.auth.currentUserName() ?? '';
    return name.trim().split(' ').pop() ?? name;
  }

  statusLabel(s: string): string {
    const map: Record<string, string> = {
      Pending: 'Chờ xác nhận', Confirmed: 'Đã xác nhận', Completed: 'Hoàn thành', Cancelled: 'Đã huỷ',
    };
    return map[s] ?? s;
  }
}
