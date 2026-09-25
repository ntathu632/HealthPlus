import { Component, OnInit, signal } from '@angular/core';
import { DatePipe, NgClass } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AdminService } from '../../../core/services/admin.service';
import { Appointment, AppointmentStatus } from '../../../models/appointment.models';

@Component({
    templateUrl: './admin-appointments-list.component.html',
    styleUrl: './admin-appointments-list.component.scss',
    selector: 'app-admin-appointments-list',
  standalone: true,
  imports: [DatePipe, NgClass, MatButtonModule, MatIconModule, MatProgressSpinnerModule]
})
export class AdminAppointmentsListComponent implements OnInit {
  appointments = signal<Appointment[]>([]);
  loading = signal(true);
  page = 1;
  pageSize = 20;
  totalCount = signal(0);
  selectedStatus: AppointmentStatus | 'All' = 'All';

  readonly statusTabs: { value: AppointmentStatus | 'All'; label: string }[] = [
    { value: 'All',       label: 'Tất cả' },
    { value: 'Pending',   label: 'Chờ xác nhận' },
    { value: 'Confirmed', label: 'Đã xác nhận' },
    { value: 'Completed', label: 'Hoàn thành' },
    { value: 'Cancelled', label: 'Đã huỷ' },
  ];

  totalPages = () => Math.max(1, Math.ceil(this.totalCount() / this.pageSize));

  constructor(private svc: AdminService) {}

  ngOnInit(): void { this.load(); }

  selectStatus(s: AppointmentStatus | 'All'): void {
    this.selectedStatus = s;
    this.page = 1;
    this.load();
  }

  load(): void {
    this.loading.set(true);
    const status = this.selectedStatus === 'All' ? undefined : this.selectedStatus;
    this.svc.getAllAppointments(this.page, this.pageSize, status).subscribe({
      next: res => {
        this.appointments.set(res.data.items);
        this.totalCount.set(res.data.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  prevPage(): void { if (this.page > 1) { this.page--; this.load(); } }
  nextPage(): void { if (this.page < this.totalPages()) { this.page++; this.load(); } }

  statusLabel(s: string): string {
    const map: Record<string, string> = {
      Pending: 'Chờ xác nhận', Confirmed: 'Đã xác nhận', Completed: 'Hoàn thành', Cancelled: 'Đã huỷ',
    };
    return map[s] ?? s;
  }
}
