import { Component, OnInit, signal } from '@angular/core';
import { DatePipe, DecimalPipe, NgClass } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { PaymentService } from '../../../core/services/payment.service';
import { Payment } from '../../../models/payment.models';

@Component({
    templateUrl: './payments-list.component.html',
    styleUrl: './payments-list.component.scss',
    selector: 'app-payments-list',
  standalone: true,
  imports: [DatePipe, DecimalPipe, NgClass, MatCardModule, MatIconModule, MatProgressSpinnerModule],
})
export class PaymentsListComponent implements OnInit {
  items = signal<Payment[]>([]);
  loading = signal(true);

  constructor(private svc: PaymentService) {}

  ngOnInit(): void {
    this.svc.getMyPayments().subscribe({
      next: res => { this.items.set(res.data); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  statusLabel(s: string): string {
    const map: Record<string, string> = { Pending: 'Đang xử lý', Completed: 'Thành công', Failed: 'Thất bại' };
    return map[s] ?? s;
  }
}
