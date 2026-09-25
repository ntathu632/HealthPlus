import { Component, OnInit, signal, computed } from '@angular/core';
import { DatePipe, DecimalPipe, NgClass } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { PaymentService } from '../../../core/services/payment.service';
import { Payment } from '../../../models/payment.models';

@Component({
    templateUrl: './doctor-earnings.component.html',
    styleUrl: './doctor-earnings.component.scss',
    selector: 'app-doctor-earnings',
  standalone: true,
  imports: [DatePipe, DecimalPipe, NgClass, MatCardModule, MatIconModule, MatProgressSpinnerModule],
})
export class DoctorEarningsComponent implements OnInit {
  items = signal<Payment[]>([]);
  loading = signal(true);

  completedItems = computed(() => this.items().filter(p => p.status === 'Completed'));
  totalEarned = computed(() => this.completedItems().reduce((sum, p) => sum + p.amount, 0));
  totalConsultations = computed(() => this.completedItems().length);

  constructor(private svc: PaymentService) {}

  ngOnInit(): void {
    this.svc.getMyEarnings().subscribe({
      next: res => { this.items.set(res.data); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  statusLabel(s: string): string {
    const map: Record<string, string> = { Pending: 'Đang xử lý', Completed: 'Thành công', Failed: 'Thất bại' };
    return map[s] ?? s;
  }
}
