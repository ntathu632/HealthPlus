import { Component, OnInit, signal, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { DatePipe } from '@angular/common';
import { BaseChartDirective } from 'ng2-charts';
import {
  ChartConfiguration, ChartData, Chart, DoughnutController, ArcElement,
  BarController, BarElement, LineController, LineElement, PointElement,
  LinearScale, CategoryScale, Tooltip, Legend, Filler,
} from 'chart.js';
import { AuthService } from '../../core/auth/auth.service';
import { HealthRecordService } from '../../core/services/health-record.service';
import { MedicalHistoryService } from '../../core/services/medical-history.service';
import { VaccineService } from '../../core/services/vaccine.service';
import { ReminderService } from '../../core/services/reminder.service';
import { HealthRecord, HealthMetric } from '../../models/health-record.models';
import { MedicalHistory } from '../../models/medical-history.models';
import { Vaccine } from '../../models/vaccine.models';
import { Reminder } from '../../models/reminder.models';

Chart.register(
  DoughnutController, ArcElement, BarController, BarElement,
  LineController, LineElement, PointElement,
  LinearScale, CategoryScale, Tooltip, Legend, Filler,
);

interface StatCard { label: string; value: number; icon: string; bg: string; fg: string; lightBg: string; route: string; }

// Cache ở module-level (không nằm trong instance) để dữ liệu vẫn còn đó khi bấm sang
// mục khác ở sidebar rồi quay lại — tránh chớp qua trạng thái "đang tải/trống" mỗi lần
// component bị huỷ-tạo lại do điều hướng. Chỉ hiện lại spinner ở lần tải đầu tiên của
// mỗi tài khoản (so khớp userId), các lần quay lại sau đó hiện ngay dữ liệu đã có rồi
// âm thầm làm mới ở nền.
let cachedUserId: string | null = null;
const recordsCache           = signal<HealthRecord[]>([]);
const followUpsCache         = signal<MedicalHistory[]>([]);
const overdueVaccinesCache   = signal<Vaccine[]>([]);
const upcomingRemindersCache = signal<Reminder[]>([]);
const allVaccinesCache       = signal<Vaccine[]>([]);
const metricsCache           = signal<HealthMetric[]>([]);
const recordsLoadedOnce      = signal(false);
const followUpsLoadedOnce    = signal(false);
const vaccinesLoadedOnce     = signal(false);
const remindersLoadedOnce    = signal(false);
const allVaccinesLoadedOnce  = signal(false);
const metricsLoadedOnce      = signal(false);

@Component({
    templateUrl: './dashboard.component.html',
    styleUrl: './dashboard.component.scss',
    selector: 'app-dashboard',
  standalone: true,
  imports: [
    RouterLink, MatIconModule, MatButtonModule, MatCardModule, MatProgressSpinnerModule, DatePipe,
    BaseChartDirective,
  ]
})
export class DashboardComponent implements OnInit {
  records           = recordsCache;
  followUps         = followUpsCache;
  overdueVaccines   = overdueVaccinesCache;
  upcomingReminders = upcomingRemindersCache;
  allVaccines       = allVaccinesCache;
  metrics           = metricsCache;
  recordsLoading    = computed(() => !recordsLoadedOnce());
  followUpsLoading  = computed(() => !followUpsLoadedOnce());
  vaccinesLoading   = computed(() => !vaccinesLoadedOnce());
  remindersLoading  = computed(() => !remindersLoadedOnce());
  allVaccinesLoading = computed(() => !allVaccinesLoadedOnce());
  metricsLoading    = computed(() => !metricsLoadedOnce());
  readonly today    = new Date();

  stats = computed<StatCard[]>(() => [
    { label: 'Hồ sơ sức khỏe',  value: this.records().length,          icon: 'person',        bg: '#1565C0', fg: '#1565C0', lightBg: '#E3F2FD', route: '/health-records' },
    { label: 'Lịch tái khám',    value: this.followUps().length,         icon: 'event_note',    bg: '#1976D2', fg: '#1976D2', lightBg: '#EFF6FF', route: '/medical-history' },
    { label: 'Vaccine quá hạn',  value: this.overdueVaccines().length,   icon: 'vaccines',      bg: '#D32F2F', fg: '#D32F2F', lightBg: '#FFEBEE', route: '/vaccines' },
    { label: 'Nhắc nhở sắp tới', value: this.upcomingReminders().length, icon: 'notifications', bg: '#1565C0', fg: '#0D47A1', lightBg: '#DBEAFE', route: '/reminders' },
  ]);

  // Biểu đồ cột 1 chuỗi (số lượng theo từng mục) — theo dataviz skill: 1 series thì dùng 1 màu
  // duy nhất (màu không mang ý nghĩa xếp hạng), không cần legend vì trục đã ghi rõ nhãn.
  // Dùng xanh dương tươi hơn (--blue-500) cho biểu đồ, phân biệt với xanh chủ đạo của giao diện.
  countsChartData = computed<ChartData<'bar'>>(() => ({
    labels: this.stats().map(s => s.label),
    datasets: [{
      data: this.stats().map(s => s.value),
      backgroundColor: '#2196F3',
      borderRadius: 4,
      barThickness: 24,
    }],
  }));
  countsChartOptions: ChartConfiguration<'bar'>['options'] = {
    indexAxis: 'y',
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      tooltip: {
        backgroundColor: 'white', titleColor: '#0F172A', bodyColor: '#546E7A',
        borderColor: '#E2E8F0', borderWidth: 1, padding: 10, boxPadding: 4,
      },
    },
    scales: {
      x: { beginAtZero: true, ticks: { color: '#64748B', font: { size: 11 }, precision: 0 }, grid: { color: '#F0F4F8' } },
      y: { ticks: { color: '#334155', font: { size: 12 } }, grid: { display: false } },
    },
  };

  // 3 trạng thái cần 3 màu phân biệt rõ — biểu đồ được phép dùng nhiều màu hơn giao diện khi
  // thật sự cần, miễn xanh dương dùng ở đây tươi hơn xanh chủ đạo của UI. Đã validate qua
  // dataviz skill (CVD ΔE 19.3-20.3, đạt hết ngưỡng phân biệt màu).
  private readonly vaccineStatusColors: Record<string, string> = {
    Completed: '#2196F3', Scheduled: '#e87ba4', Overdue: '#C62828',
  };
  vaccineStatusData = computed<ChartData<'doughnut'>>(() => {
    const counts = { Completed: 0, Scheduled: 0, Overdue: 0 };
    for (const v of this.allVaccines()) counts[v.status]++;
    return {
      labels: ['Đã tiêm', 'Sắp tiêm', 'Quá hạn'],
      datasets: [{
        data: [counts.Completed, counts.Scheduled, counts.Overdue],
        backgroundColor: [this.vaccineStatusColors['Completed'], this.vaccineStatusColors['Scheduled'], this.vaccineStatusColors['Overdue']],
        borderColor: '#FFFFFF',
        borderWidth: 2,
        hoverOffset: 6,
      }],
    };
  });
  vaccineStatusOptions: ChartConfiguration<'doughnut'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    cutout: '68%',
    plugins: {
      legend: {
        position: 'bottom',
        labels: {
          color: '#334155', font: { size: 12 }, padding: 14, usePointStyle: true, pointStyle: 'circle',
          // Thêm số liệu vào nhãn legend — relief cho khả năng phân biệt màu, giống Admin Dashboard.
          generateLabels: chart => {
            const data = chart.data.datasets[0]?.data as number[] ?? [];
            const labels = chart.data.labels as string[] ?? [];
            return labels.map((label, i) => ({
              text: `${label} (${data[i]})`,
              fillStyle: (chart.data.datasets[0]?.backgroundColor as string[])[i],
              strokeStyle: 'transparent',
              pointStyle: 'circle' as const,
              index: i,
            }));
          },
        },
      },
      tooltip: {
        backgroundColor: 'white', titleColor: '#0F172A', bodyColor: '#546E7A',
        borderColor: '#E2E8F0', borderWidth: 1, padding: 10, boxPadding: 4,
      },
    },
  };

  // Biểu đồ đường 1 chuỗi — dùng xanh dương tươi (--blue-500, giống biểu đồ cột) thay vì bộ màu
  // teal/hồng/cam gốc của HealthMetricsChartComponent (thiết kế cho trang chi tiết hồ sơ, lệch
  // tông so với dashboard). Ưu tiên BMI nếu có, không thì cân nặng.
  private readonly sortedMetrics = computed(() =>
    [...this.metrics()].sort((a, b) => new Date(a.measuredAt).getTime() - new Date(b.measuredAt).getTime()));
  private readonly metricField = computed<'bmi' | 'weightKg'>(() =>
    this.sortedMetrics().some(m => m.bmi != null) ? 'bmi' : 'weightKg');
  metricChartTitle = computed(() =>
    this.metricField() === 'bmi' ? 'Chỉ số BMI theo thời gian' : 'Cân nặng theo thời gian (kg)');
  metricChartData = computed<ChartData<'line'>>(() => {
    const field = this.metricField();
    return {
      labels: this.sortedMetrics().map(m => {
        const d = new Date(m.measuredAt);
        return `${d.getDate()}/${d.getMonth() + 1}`;
      }),
      datasets: [{
        data: this.sortedMetrics().map(m => m[field] ?? null),
        borderColor: '#2196F3',
        backgroundColor: 'rgba(33,150,243,0.08)',
        fill: true,
      }],
    };
  });
  metricChartOptions: ChartConfiguration<'line'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      tooltip: {
        backgroundColor: 'white', titleColor: '#0F172A', bodyColor: '#546E7A',
        borderColor: '#E2E8F0', borderWidth: 1, padding: 10, boxPadding: 4,
      },
    },
    scales: {
      x: { grid: { color: '#F0F4F8' }, ticks: { color: '#64748B', font: { size: 11 } } },
      y: { grid: { color: '#F0F4F8' }, ticks: { color: '#64748B', font: { size: 11 } } },
    },
    elements: {
      line:  { tension: 0.4, borderWidth: 2 },
      point: { radius: 3, hoverRadius: 5 },
    },
  };

  constructor(
    public auth: AuthService,
    private healthSvc: HealthRecordService,
    private medSvc:    MedicalHistoryService,
    private vacSvc:    VaccineService,
    private remSvc:    ReminderService,
  ) {}

  ngOnInit(): void {
    const userId = this.auth.user()?.id ?? null;
    if (userId !== cachedUserId) {
      cachedUserId = userId;
      recordsCache.set([]);
      followUpsCache.set([]);
      overdueVaccinesCache.set([]);
      upcomingRemindersCache.set([]);
      allVaccinesCache.set([]);
      metricsCache.set([]);
      recordsLoadedOnce.set(false);
      followUpsLoadedOnce.set(false);
      vaccinesLoadedOnce.set(false);
      remindersLoadedOnce.set(false);
      allVaccinesLoadedOnce.set(false);
      metricsLoadedOnce.set(false);
    }

    this.healthSvc.getAll().subscribe({
      next: r => {
        recordsCache.set(r.data);
        recordsLoadedOnce.set(true);
        const firstRecord = r.data[0];
        if (!firstRecord) { metricsLoadedOnce.set(true); return; }
        this.healthSvc.getMetrics(firstRecord.id, 1, 20).subscribe({
          next: m => { metricsCache.set(m.data.items); metricsLoadedOnce.set(true); },
          error: () => metricsLoadedOnce.set(true),
        });
      },
      error: () => { recordsLoadedOnce.set(true); metricsLoadedOnce.set(true); },
    });
    this.medSvc.getUpcomingFollowUps(30).subscribe({
      next: r => { followUpsCache.set(r.data); followUpsLoadedOnce.set(true); },
      error: () => followUpsLoadedOnce.set(true),
    });
    this.vacSvc.getOverdue().subscribe({
      next: r => { overdueVaccinesCache.set(r.data); vaccinesLoadedOnce.set(true); },
      error: () => vaccinesLoadedOnce.set(true),
    });
    this.vacSvc.getAll(undefined, 1, 100).subscribe({
      next: r => { allVaccinesCache.set(r.data.items); allVaccinesLoadedOnce.set(true); },
      error: () => allVaccinesLoadedOnce.set(true),
    });
    // 7 ngày thay vì 24h — để mục này luôn có nội dung hữu ích thay vì trống trơn
    // chỉ vì không có nhắc nhở nào rơi đúng vào ~24 giờ tới.
    this.remSvc.getUpcoming(24 * 7).subscribe({
      next: r => { upcomingRemindersCache.set(r.data); remindersLoadedOnce.set(true); },
      error: () => remindersLoadedOnce.set(true),
    });
  }

  firstName(): string {
    const name = this.auth.currentUserName() ?? '';
    return name.trim().split(' ').pop() ?? name;
  }
}
