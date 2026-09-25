import { Component, OnInit, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { BaseChartDirective } from 'ng2-charts';
import {
  ChartConfiguration, ChartData, Chart, DoughnutController, ArcElement,
  BarController, BarElement, LinearScale, CategoryScale, Tooltip, Legend,
} from 'chart.js';
import { AdminService } from '../../../core/services/admin.service';
import { AdminDashboardStats, DoctorPatient } from '../../../models/admin.models';

Chart.register(DoughnutController, ArcElement, BarController, BarElement, LinearScale, CategoryScale, Tooltip, Legend);

interface StatCard { label: string; value: number; icon: string; color: string; }

@Component({
    templateUrl: './admin-dashboard.component.html',
    styleUrl: './admin-dashboard.component.scss',
    selector: 'app-admin-dashboard',
  standalone: true,
  imports: [MatCardModule, MatIconModule, MatProgressSpinnerModule, BaseChartDirective]
})
export class AdminDashboardComponent implements OnInit {
  loading = signal(true);
  stats = signal<AdminDashboardStats | null>(null);

  cards = () => {
    const s = this.stats();
    if (!s) return [];
    return [
      { label: 'Tổng người dùng', value: s.totalUsers, icon: 'group', color: '#1565C0' },
      { label: 'Bác sĩ', value: s.totalDoctors, icon: 'medical_services', color: '#AD1457' },
      { label: 'Bệnh nhân', value: s.totalPatients, icon: 'personal_injury', color: '#0D47A1' },
      { label: 'Phân công đang hoạt động', value: s.totalAssignments, icon: 'assignment_ind', color: '#6A1B9A' },
      { label: 'Nhật ký 7 ngày qua', value: s.recentAuditLogCount, icon: 'history', color: '#C62828' },
    ] as StatCard[];
  };

  // Bảng màu categorical đã validate (blue/red/pink — bỏ xanh lá theo yêu cầu thống nhất chỉ
  // dùng xanh dương/đỏ(hồng) cho toàn app). Xanh dương ở đây (--blue-500, tươi hơn xanh chủ đạo
  // UI) theo đúng quy ước riêng cho biểu đồ. Xem dataviz skill: cả 3 cặp liền kề đều đạt ngưỡng
  // phân biệt màu cho người mù màu (CVD ΔE ≥ 8 light mode) và tương phản thường (≥15).
  private readonly roleColors = ['#2196F3', '#C62828', '#e87ba4'];

  chartData: ChartData<'doughnut'> = { labels: [], datasets: [] };
  chartOptions: ChartConfiguration<'doughnut'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    cutout: '68%',
    plugins: {
      legend: {
        position: 'bottom',
        labels: {
          color: '#334155',
          font: { size: 12 },
          padding: 14,
          usePointStyle: true,
          pointStyle: 'circle',
          // Thêm số liệu trực tiếp vào nhãn legend (relief cho slot magenta có tương phản thấp hơn
          // 3:1 trên nền trắng — dataviz skill yêu cầu nhãn hiển thị rõ thay vì chỉ dựa vào màu).
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
        backgroundColor: 'white',
        titleColor: '#0F172A',
        bodyColor: '#546E7A',
        borderColor: '#E2E8F0',
        borderWidth: 1,
        padding: 10,
        boxPadding: 4,
      },
    },
  };

  private updateChart(s: AdminDashboardStats): void {
    const adminCount = Math.max(0, s.totalUsers - s.totalDoctors - s.totalPatients);
    this.chartData = {
      labels: ['Quản trị viên', 'Bác sĩ', 'Bệnh nhân'],
      datasets: [{
        data: [adminCount, s.totalDoctors, s.totalPatients],
        backgroundColor: this.roleColors,
        borderColor: '#FFFFFF',
        borderWidth: 2,
        hoverOffset: 6,
      }],
    };
  }

  // Biểu đồ cột 1 chuỗi (số bệnh nhân/bác sĩ) — theo dataviz skill: 1 series thì dùng 1 màu duy nhất
  // (màu không mang ý nghĩa xếp hạng), không cần legend riêng vì trục Y đã ghi rõ tên bác sĩ.
  // Xanh dương tươi (--blue-500), riêng cho biểu đồ, khác xanh chủ đạo của giao diện.
  loadingPatientsByDoctor = signal(true);
  patientsByDoctorData: ChartData<'bar'> = { labels: [], datasets: [] };
  patientsByDoctorOptions: ChartConfiguration<'bar'>['options'] = {
    indexAxis: 'y',
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      tooltip: {
        backgroundColor: 'white',
        titleColor: '#0F172A',
        bodyColor: '#546E7A',
        borderColor: '#E2E8F0',
        borderWidth: 1,
        padding: 10,
        boxPadding: 4,
        callbacks: { label: ctx => `${ctx.parsed.x} bệnh nhân` },
      },
    },
    scales: {
      x: {
        beginAtZero: true,
        ticks: { color: '#64748B', font: { size: 11 }, precision: 0 },
        grid: { color: '#F0F4F8' },
      },
      y: {
        ticks: { color: '#334155', font: { size: 12 } },
        grid: { display: false },
      },
    },
  };

  private loadPatientsByDoctor(): void {
    this.loadingPatientsByDoctor.set(true);
    this.svc.getDoctorPatients().subscribe({
      next: res => {
        const counts = new Map<string, number>();
        for (const a of res.data as DoctorPatient[]) {
          if (!a.isActive) continue;
          counts.set(a.doctorName, (counts.get(a.doctorName) ?? 0) + 1);
        }
        const entries = Array.from(counts.entries()).sort((a, b) => b[1] - a[1]);
        this.patientsByDoctorData = {
          labels: entries.map(([name]) => name),
          datasets: [{
            data: entries.map(([, count]) => count),
            backgroundColor: '#2196F3',
            borderRadius: 4,
            barThickness: 20,
          }],
        };
        this.loadingPatientsByDoctor.set(false);
      },
      error: () => this.loadingPatientsByDoctor.set(false),
    });
  }

  constructor(private svc: AdminService) {}

  ngOnInit(): void {
    this.svc.getDashboard().subscribe({
      next: res => {
        this.stats.set(res.data);
        this.updateChart(res.data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
    this.loadPatientsByDoctor();
  }
}
