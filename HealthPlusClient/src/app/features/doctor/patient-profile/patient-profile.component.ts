import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DoctorService } from '../../../core/services/doctor.service';
import { PatientProfile } from '../../../models/doctor.models';
import { Prescription } from '../../../models/prescription.models';
import { MedicalHistory } from '../../../models/medical-history.models';
import { Vaccine } from '../../../models/vaccine.models';
import { BLOOD_TYPE_LABELS } from '../../../models/health-record.models';
import { AddMedicalHistoryDialogComponent } from './add-medical-history-dialog.component';
import { AddVaccineDialogComponent } from './add-vaccine-dialog.component';
import { DoctorPrescriptionDetailDialogComponent } from './doctor-prescription-dialog.component';

@Component({
    templateUrl: './patient-profile.component.html',
    styleUrl: './patient-profile.component.scss',
    selector: 'app-patient-profile',
  standalone: true,
  imports: [
    RouterLink, DatePipe,
    MatCardModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatTabsModule, MatTooltipModule,
  ]
})
export class PatientProfileComponent implements OnInit {
  profile = signal<PatientProfile | null>(null);
  loading = signal(true);
  patientId = '';

  constructor(
    private route: ActivatedRoute,
    private svc: DoctorService,
    private dialog: MatDialog,
    private snack: MatSnackBar,
  ) {}

  ngOnInit(): void {
    this.patientId = this.route.snapshot.paramMap.get('id')!;
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.svc.getPatientProfile(this.patientId).subscribe({
      next: res => { this.profile.set(res.data); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  initials(name: string): string {
    return name.trim().split(' ').filter(Boolean).map(w => w[0]).slice(0, 2).join('').toUpperCase();
  }

  bloodTypeLabel(bt: string): string {
    return BLOOD_TYPE_LABELS[bt as keyof typeof BLOOD_TYPE_LABELS] ?? bt;
  }

  statusLabel(s: string): string {
    const map: Record<string, string> = {
      Pending: 'Chờ xử lý', Processing: 'Đang xử lý', Completed: 'Hoàn thành', Failed: 'Lỗi',
    };
    return map[s] ?? s;
  }

  openAddMedicalHistory(p: PatientProfile, record?: MedicalHistory): void {
    this.dialog.open(AddMedicalHistoryDialogComponent, {
      width: '640px',
      data: { patientId: this.patientId, healthRecords: p.healthRecords, record: record ?? null },
    }).afterClosed().subscribe(ok => { if (ok) this.load(); });
  }

  deleteMedicalHistory(m: MedicalHistory): void {
    if (!confirm(`Xoá chẩn đoán ngày ${new Date(m.visitDate).toLocaleDateString('vi-VN')}?`)) return;
    this.svc.deleteMedicalHistory(this.patientId, m.id).subscribe({
      next: () => { this.snack.open('Đã xoá chẩn đoán', 'Đóng', { duration: 2000 }); this.load(); },
      error: () => this.snack.open('Xoá thất bại', 'Đóng', { duration: 3000 }),
    });
  }

  openAddVaccine(p: PatientProfile, vaccine?: Vaccine): void {
    this.dialog.open(AddVaccineDialogComponent, {
      width: '640px',
      data: { patientId: this.patientId, healthRecords: p.healthRecords, vaccine: vaccine ?? null },
    }).afterClosed().subscribe(ok => { if (ok) this.load(); });
  }

  deleteVaccine(v: Vaccine): void {
    if (!confirm(`Xoá mũi tiêm "${v.vaccineName}"?`)) return;
    this.svc.deleteVaccine(this.patientId, v.id).subscribe({
      next: () => { this.snack.open('Đã xoá mũi tiêm', 'Đóng', { duration: 2000 }); this.load(); },
      error: () => this.snack.open('Xoá thất bại', 'Đóng', { duration: 3000 }),
    });
  }

  createPrescription(p: PatientProfile): void {
    this.svc.createPrescription(this.patientId, {}).subscribe({
      next: res => {
        this.snack.open('Đã tạo đơn thuốc mới', 'Đóng', { duration: 2500 });
        this.openPrescription(res.data);
      },
      error: () => this.snack.open('Tạo đơn thất bại', 'Đóng', { duration: 3000 }),
    });
  }

  deletePrescription(rx: Prescription): void {
    if (!confirm(`Xoá đơn thuốc ngày ${new Date(rx.createdAt).toLocaleDateString('vi-VN')}?`)) return;
    this.svc.deletePrescription(this.patientId, rx.id).subscribe({
      next: () => { this.snack.open('Đã xoá đơn thuốc', 'Đóng', { duration: 2000 }); this.load(); },
      error: () => this.snack.open('Xoá thất bại', 'Đóng', { duration: 3000 }),
    });
  }

  openPrescription(rx: Prescription): void {
    this.dialog.open(DoctorPrescriptionDetailDialogComponent, {
      width: '600px',
      maxHeight: '90vh',
      data: { prescription: rx, patientId: this.patientId },
    }).afterClosed().subscribe(changed => { if (changed) this.load(); });
  }
}
