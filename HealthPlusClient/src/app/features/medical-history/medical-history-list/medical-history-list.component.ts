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
import { DatePipe, SlicePipe } from '@angular/common';
import { MedicalHistoryService } from '../../../core/services/medical-history.service';
import { HealthRecordService } from '../../../core/services/health-record.service';
import { MedicalHistory } from '../../../models/medical-history.models';
import { HealthRecord } from '../../../models/health-record.models';
import { MedicalHistoryFormDialogComponent } from '../medical-history-form-dialog/medical-history-form-dialog.component';
import { MedicalHistoryDetailDialogComponent } from '../medical-history-detail-dialog/medical-history-detail-dialog.component';

@Component({
    templateUrl: './medical-history-list.component.html',
    styleUrl: './medical-history-list.component.scss',
    selector: 'app-medical-history-list',
  standalone: true,
  imports: [
    DatePipe, SlicePipe, FormsModule,
    MatCardModule, MatButtonModule, MatIconModule,
    MatProgressSpinnerModule, MatSelectModule, MatFormFieldModule, MatTooltipModule,
  ]
})
export class MedicalHistoryListComponent implements OnInit {
  items        = signal<MedicalHistory[]>([]);
  healthRecords= signal<HealthRecord[]>([]);
  loading      = signal(true);
  totalCount   = signal(0);
  upcomingCount= signal(0);
  selectedRecordId = '';
  page     = 1;
  pageSize = 10;

  hasMore = computed(() => this.items().length < this.totalCount());

  constructor(
    private svc: MedicalHistoryService,
    private recordSvc: HealthRecordService,
    private dialog: MatDialog,
    private snack: MatSnackBar,
  ) {}

  ngOnInit(): void {
    this.loadHealthRecords();
    this.load(true);
    this.loadUpcoming();
  }

  loadHealthRecords(): void {
    this.recordSvc.getAll().subscribe(res => this.healthRecords.set(res.data));
  }

  load(reset = false): void {
    if (reset) { this.page = 1; this.items.set([]); }
    this.loading.set(true);
    const rid = this.selectedRecordId || undefined;
    this.svc.getAll(rid, this.page, this.pageSize).subscribe({
      next: res => {
        this.items.update(cur => reset ? res.data.items : [...cur, ...res.data.items]);
        this.totalCount.set(res.data.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  loadMore(): void {
    this.page++;
    this.load(false);
  }

  loadUpcoming(): void {
    this.svc.getUpcomingFollowUps(30).subscribe(res => this.upcomingCount.set(res.data.length));
  }

  onFilterChange(): void { this.load(true); }

  openCreate(): void {
    this.dialog.open(MedicalHistoryFormDialogComponent, {
      width: '580px',
      data: { record: null, healthRecords: this.healthRecords() },
    }).afterClosed().subscribe(ok => { if (ok) { this.load(true); this.loadUpcoming(); } });
  }

  openEdit(item: MedicalHistory): void {
    this.dialog.open(MedicalHistoryFormDialogComponent, {
      width: '580px',
      data: { record: item, healthRecords: this.healthRecords() },
    }).afterClosed().subscribe(ok => { if (ok) { this.load(true); this.loadUpcoming(); } });
  }

  openDetail(item: MedicalHistory): void {
    this.dialog.open(MedicalHistoryDetailDialogComponent, {
      width: '640px', maxHeight: '90vh',
      data: item,
    });
  }

  delete(item: MedicalHistory): void {
    if (!confirm(`Xoá lần khám ngày ${item.visitDate}?`)) return;
    this.svc.delete(item.id).subscribe({
      next: () => {
        this.snack.open('Đã xoá', 'Đóng', { duration: 2000 });
        this.load(true);
        this.loadUpcoming();
      },
    });
  }

  isUpcoming(followUpDate?: string): boolean {
    if (!followUpDate) return false;
    const days = this.daysUntil(followUpDate);
    return days >= 0 && days <= 30;
  }

  daysUntil(date: string): number {
    const diff = new Date(date).getTime() - new Date().setHours(0, 0, 0, 0);
    return Math.ceil(diff / 86400000);
  }
}
