import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DecimalPipe, DatePipe } from '@angular/common';
import { HealthRecordService } from '../../../core/services/health-record.service';
import { HealthRecord, BLOOD_TYPE_LABELS } from '../../../models/health-record.models';
import { HealthRecordFormDialogComponent } from '../health-record-form-dialog/health-record-form-dialog.component';

@Component({
    templateUrl: './health-records-list.component.html',
    styleUrl: './health-records-list.component.scss',
    selector: 'app-health-records-list',
  standalone: true,
  imports: [
    RouterLink, DecimalPipe, DatePipe,
    MatCardModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule,
  ]
})
export class HealthRecordsListComponent implements OnInit {
  records = signal<HealthRecord[]>([]);
  loading = signal(true);

  constructor(
    private svc: HealthRecordService,
    private dialog: MatDialog,
    private snack: MatSnackBar,
  ) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.svc.getAll().subscribe({
      next: res => { this.records.set(res.data); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  openCreate(): void {
    this.dialog.open(HealthRecordFormDialogComponent, { width: '520px', data: null })
      .afterClosed().subscribe(ok => { if (ok) this.load(); });
  }

  openEdit(record: HealthRecord, event: Event): void {
    event.stopPropagation();
    event.preventDefault();
    this.dialog.open(HealthRecordFormDialogComponent, { width: '520px', data: record })
      .afterClosed().subscribe(ok => { if (ok) this.load(); });
  }

  calcAge(dob: string): number {
    const today = new Date(), birth = new Date(dob);
    let age = today.getFullYear() - birth.getFullYear();
    if (today.getMonth() - birth.getMonth() < 0 ||
       (today.getMonth() === birth.getMonth() && today.getDate() < birth.getDate())) age--;
    return age;
  }

  bloodTypeLabel(bt: string): string {
    return BLOOD_TYPE_LABELS[bt as keyof typeof BLOOD_TYPE_LABELS] ?? bt;
  }

  getAvatarColor(gender: string): string {
    return gender === 'Male' ? '#1976D2' : gender === 'Female' ? '#E91E63' : '#546E7A';
  }

  bmiClass(bmi: number): string {
    if (bmi < 18.5) return 'metric-val bmi-underweight';
    if (bmi < 25)   return 'metric-val bmi-normal';
    if (bmi < 30)   return 'metric-val bmi-overweight';
    return 'metric-val bmi-obese';
  }
}
