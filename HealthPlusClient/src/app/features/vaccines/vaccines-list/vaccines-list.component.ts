import { Component, OnInit, signal, computed } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { VaccineService } from '../../../core/services/vaccine.service';
import { HealthRecordService } from '../../../core/services/health-record.service';
import { Vaccine } from '../../../models/vaccine.models';
import { HealthRecord } from '../../../models/health-record.models';
import { VaccineFormDialogComponent } from '../vaccine-form-dialog/vaccine-form-dialog.component';

@Component({
    templateUrl: './vaccines-list.component.html',
    styleUrl: './vaccines-list.component.scss',
    selector: 'app-vaccines-list',
  standalone: true,
  imports: [
    DatePipe, FormsModule,
    MatCardModule, MatButtonModule, MatIconModule,
    MatProgressSpinnerModule, MatSelectModule, MatFormFieldModule, MatTooltipModule,
  ]
})
export class VaccinesListComponent implements OnInit {
  allItems       = signal<Vaccine[]>([]);
  healthRecords  = signal<HealthRecord[]>([]);
  overdueList    = signal<Vaccine[]>([]);
  loading        = signal(true);
  overdueLoading = signal(true);
  totalCount     = signal(0);
  selectedRecordId = '';
  selectedStatus   = 'All';
  page     = 1;
  pageSize = 20;

  readonly statusTabs = [
    { value: 'All',       label: 'Tất cả' },
    { value: 'Completed', label: 'Đã tiêm' },
    { value: 'Scheduled', label: 'Lịch tiêm' },
    { value: 'Overdue',   label: 'Quá hạn' },
  ];

  filteredItems = computed(() => {
    if (this.selectedStatus === 'All') return this.allItems();
    return this.allItems().filter(v => v.status === this.selectedStatus);
  });

  hasMore = computed(() => this.allItems().length < this.totalCount());

  constructor(
    private svc: VaccineService,
    private recordSvc: HealthRecordService,
    private dialog: MatDialog,
    private snack: MatSnackBar,
  ) {}

  ngOnInit(): void {
    this.recordSvc.getAll().subscribe(res => this.healthRecords.set(res.data));
    this.load(true);
    this.loadOverdue();
  }

  load(reset = false): void {
    if (reset) { this.page = 1; this.allItems.set([]); }
    this.loading.set(true);
    const rid = this.selectedRecordId || undefined;
    this.svc.getAll(rid, this.page, this.pageSize).subscribe({
      next: res => {
        this.allItems.update(cur => reset ? res.data.items : [...cur, ...res.data.items]);
        this.totalCount.set(res.data.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  loadMore(): void { this.page++; this.load(false); }
  loadOverdue(): void {
    this.overdueLoading.set(true);
    this.svc.getOverdue().subscribe({
      next: res => { this.overdueList.set(res.data); this.overdueLoading.set(false); },
      error: () => this.overdueLoading.set(false),
    });
  }

  onFilterChange(): void { this.load(true); }

  countByStatus(status: string): number {
    if (status === 'All') return this.allItems().length;
    return this.allItems().filter(v => v.status === status).length;
  }

  openCreate(): void {
    this.dialog.open(VaccineFormDialogComponent, {
      width: '560px',
      data: { vaccine: null, healthRecords: this.healthRecords() },
    }).afterClosed().subscribe(ok => { if (ok) { this.load(true); this.loadOverdue(); } });
  }

  openEdit(v: Vaccine): void {
    this.dialog.open(VaccineFormDialogComponent, {
      width: '560px',
      data: { vaccine: v, healthRecords: this.healthRecords() },
    }).afterClosed().subscribe(ok => { if (ok) { this.load(true); this.loadOverdue(); } });
  }

  delete(v: Vaccine): void {
    if (!confirm(`Xoá mũi tiêm "${v.vaccineName} mũi ${v.doseNumber}"?`)) return;
    this.svc.delete(v.id).subscribe({
      next: () => {
        this.snack.open('Đã xoá', 'Đóng', { duration: 2000 });
        this.load(true);
        this.loadOverdue();
      },
    });
  }

  statusLabel(s: string): string {
    return s === 'Completed' ? 'Đã tiêm' : s === 'Scheduled' ? 'Lịch tiêm' : 'Quá hạn';
  }

  isOverdue(date: string): boolean { return this.daysUntil(date) < 0; }

  daysUntil(date: string): number {
    return Math.ceil((new Date(date).getTime() - new Date().setHours(0,0,0,0)) / 86400000);
  }
}
