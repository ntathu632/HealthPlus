import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DatePipe, DecimalPipe } from '@angular/common';
import { HealthRecordService } from '../../../core/services/health-record.service';
import { HealthRecord, HealthMetric, BLOOD_TYPE_LABELS } from '../../../models/health-record.models';
import { HealthMetricFormDialogComponent } from '../health-metric-form-dialog/health-metric-form-dialog.component';
import { HealthMetricsChartComponent } from '../health-metrics-chart/health-metrics-chart.component';

@Component({
    templateUrl: './health-record-detail.component.html',
    styleUrl: './health-record-detail.component.scss',
    selector: 'app-health-record-detail',
  standalone: true,
  imports: [
    RouterLink, DatePipe, DecimalPipe,
    MatCardModule, MatButtonModule, MatIconModule,
    MatProgressSpinnerModule,
    HealthMetricsChartComponent,
  ]
})
export class HealthRecordDetailComponent implements OnInit {
  record = signal<HealthRecord | null>(null);
  metrics = signal<HealthMetric[]>([]);
  loading = signal(true);
  metricsLoading = signal(true);
  private recordId!: string;

  constructor(
    private route: ActivatedRoute,
    private svc: HealthRecordService,
    private dialog: MatDialog,
    private snack: MatSnackBar,
  ) {}

  ngOnInit(): void {
    this.recordId = this.route.snapshot.paramMap.get('id')!;
    this.loadRecord();
    this.loadMetrics();
  }

  loadRecord(): void {
    this.svc.getById(this.recordId).subscribe({
      next: res => { this.record.set(res.data); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  loadMetrics(): void {
    this.metricsLoading.set(true);
    this.svc.getMetrics(this.recordId, 1, 50).subscribe({
      next: res => { this.metrics.set(res.data.items); this.metricsLoading.set(false); },
      error: () => this.metricsLoading.set(false),
    });
  }

  addMetric(): void {
    this.dialog.open(HealthMetricFormDialogComponent, { width: '480px', data: this.recordId })
      .afterClosed().subscribe(ok => { if (ok) { this.loadRecord(); this.loadMetrics(); } });
  }

  deleteMetric(metricId: string): void {
    if (!confirm('Xoá chỉ số này?')) return;
    this.svc.deleteMetric(this.recordId, metricId).subscribe({
      next: () => {
        this.snack.open('Đã xoá', 'Đóng', { duration: 2000 });
        this.loadRecord();
        this.loadMetrics();
      },
    });
  }

  calcAge(dob: string): number {
    const today = new Date(), birth = new Date(dob);
    let age = today.getFullYear() - birth.getFullYear();
    if (today.getMonth() - birth.getMonth() < 0 ||
       (today.getMonth() === birth.getMonth() && today.getDate() < birth.getDate())) age--;
    return age;
  }
  bloodTypeLabel(bt: string): string { return BLOOD_TYPE_LABELS[bt as keyof typeof BLOOD_TYPE_LABELS] ?? bt; }
  genderLabel(g: string): string { return g === 'Male' ? 'Nam' : g === 'Female' ? 'Nữ' : 'Khác'; }
  getAvatarColor(gender: string): string {
    return gender === 'Male' ? '#1976D2' : gender === 'Female' ? '#E91E63' : '#546E7A';
  }
  bmiBoxClass(bmi: number): string {
    if (bmi < 18.5) return 'bmi-underweight';
    if (bmi < 25)   return 'bmi-normal';
    if (bmi < 30)   return 'bmi-overweight';
    return 'bmi-obese';
  }
  bmiStatus(bmi: number): string {
    if (bmi < 18.5) return 'Thiếu cân';
    if (bmi < 25)   return 'Bình thường';
    if (bmi < 30)   return 'Thừa cân';
    return 'Béo phì';
  }
  bmiChipClass(bmi: number): string {
    if (bmi < 18.5) return 'bmi-underweight';
    if (bmi < 25)   return 'bmi-normal';
    if (bmi < 30)   return 'bmi-overweight';
    return 'bmi-obese';
  }
}
