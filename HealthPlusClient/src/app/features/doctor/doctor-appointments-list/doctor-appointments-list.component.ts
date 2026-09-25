import { Component, OnInit, signal, computed } from '@angular/core';
import { DatePipe, NgClass } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AppointmentService } from '../../../core/services/appointment.service';
import { Appointment } from '../../../models/appointment.models';

@Component({
    templateUrl: './doctor-appointments-list.component.html',
    styleUrl: './doctor-appointments-list.component.scss',
    selector: 'app-doctor-appointments-list',
  standalone: true,
  imports: [DatePipe, NgClass, MatCardModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule]
})
export class DoctorAppointmentsListComponent implements OnInit {
  allItems = signal<Appointment[]>([]);
  loading = signal(true);
  selectedStatus = signal('All');

  readonly statusTabs = [
    { value: 'All',       label: 'Tất cả' },
    { value: 'Pending',   label: 'Chờ xác nhận' },
    { value: 'Confirmed', label: 'Đã xác nhận' },
    { value: 'Completed', label: 'Hoàn thành' },
    { value: 'Cancelled', label: 'Đã huỷ' },
  ];

  filteredItems = computed(() => {
    const status = this.selectedStatus();
    if (status === 'All') return this.allItems();
    return this.allItems().filter(a => a.status === status);
  });

  constructor(private svc: AppointmentService, private snack: MatSnackBar) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.svc.getMyAppointmentsAsDoctor().subscribe({
      next: res => { this.allItems.set(res.data); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  countByStatus(status: string): number {
    if (status === 'All') return this.allItems().length;
    return this.allItems().filter(a => a.status === status).length;
  }

  updateStatus(a: Appointment, status: 'Confirmed' | 'Completed' | 'Cancelled'): void {
    this.svc.updateStatus(a.id, { status }).subscribe({
      next: res => {
        this.allItems.update(cur => cur.map(x => x.id === a.id ? res.data : x));
        this.snack.open('Đã cập nhật lịch hẹn', 'Đóng', { duration: 2500, panelClass: 'snack-success' });
      },
      error: () => this.snack.open('Cập nhật thất bại', 'Đóng', { duration: 3000 }),
    });
  }

  statusLabel(s: string): string {
    const map: Record<string, string> = {
      Pending: 'Chờ xác nhận', Confirmed: 'Đã xác nhận', Completed: 'Hoàn thành', Cancelled: 'Đã huỷ',
    };
    return map[s] ?? s;
  }
}
