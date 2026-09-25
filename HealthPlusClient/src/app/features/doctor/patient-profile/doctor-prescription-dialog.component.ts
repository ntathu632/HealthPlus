import { Component, Inject, signal } from '@angular/core';
import { DatePipe, NgClass } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDividerModule } from '@angular/material/divider';
import { DoctorService } from '../../../core/services/doctor.service';
import { Prescription, PrescriptionItem, CreatePrescriptionItemRequest, UpdatePrescriptionItemRequest } from '../../../models/prescription.models';

// ─── Add/Edit Item Dialog ─────────────────────────────────────────────────────

interface ItemDialogData {
  item: PrescriptionItem | null;
  patientId: string;
  prescriptionId: string;
}

@Component({
    templateUrl: './doctor-item-form-dialog.component.html',
    styleUrl: './doctor-item-form-dialog.component.scss',
    selector: 'app-doctor-item-form-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatCheckboxModule,
  ]
})
export class DoctorItemFormDialogComponent {
  form: FormGroup;
  loading = signal(false);
  isEdit: boolean;

  constructor(
    fb: FormBuilder,
    private svc: DoctorService,
    private dialogRef: MatDialogRef<DoctorItemFormDialogComponent>,
    private snack: MatSnackBar,
    @Inject(MAT_DIALOG_DATA) public data: ItemDialogData,
  ) {
    this.isEdit = !!data.item;
    const it = data.item;
    this.form = fb.group({
      medicineName:    [it?.medicineName ?? '',  Validators.required],
      dosage:          [it?.dosage ?? ''],
      frequencyPerDay: [it?.frequencyPerDay ?? null],
      durationDays:    [it?.durationDays ?? null],
      timing:          [it?.timing ?? ''],
      instructions:    [it?.instructions ?? ''],
      isConfirmed:     [it?.isConfirmed ?? false],
    });
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading.set(true);
    const v = this.form.value;
    const payload = {
      medicineName:    v.medicineName,
      dosage:          v.dosage || undefined,
      frequencyPerDay: v.frequencyPerDay || undefined,
      durationDays:    v.durationDays || undefined,
      timing:          v.timing || undefined,
      instructions:    v.instructions || undefined,
      isConfirmed:     v.isConfirmed,
    };

    const obs = this.isEdit
      ? this.svc.updatePrescriptionItem(this.data.patientId, this.data.item!.id, payload as UpdatePrescriptionItemRequest)
      : this.svc.addPrescriptionItem(this.data.patientId, this.data.prescriptionId, payload as CreatePrescriptionItemRequest);

    obs.subscribe({
      next: () => {
        this.snack.open(this.isEdit ? 'Đã cập nhật thuốc' : 'Thêm thuốc thành công', 'Đóng', { duration: 2500 });
        this.dialogRef.close(true);
      },
      error: () => this.loading.set(false),
    });
  }
}

// ─── Prescription Detail Dialog (bác sĩ) ──────────────────────────────────────

interface DetailDialogData {
  prescription: Prescription;
  patientId: string;
}

@Component({
    templateUrl: './doctor-prescription-detail-dialog.component.html',
    styleUrl: './doctor-prescription-detail-dialog.component.scss',
    selector: 'app-doctor-prescription-detail-dialog',
  standalone: true,
  imports: [
    DatePipe, NgClass,
    MatDialogModule, MatButtonModule, MatIconModule,
    MatProgressSpinnerModule, MatTooltipModule, MatDividerModule,
  ]
})
export class DoctorPrescriptionDetailDialogComponent {
  rx: Prescription;
  patientId: string;

  constructor(
    private svc: DoctorService,
    private dialog: MatDialog,
    private dialogRef: MatDialogRef<DoctorPrescriptionDetailDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DetailDialogData,
  ) {
    this.rx = data.prescription;
    this.patientId = data.patientId;
  }

  close(): void {
    this.dialogRef.close(true);
  }

  openAddItem(): void {
    this.dialog.open(DoctorItemFormDialogComponent, {
      width: '520px',
      data: { item: null, patientId: this.patientId, prescriptionId: this.rx.id } satisfies ItemDialogData,
    }).afterClosed().subscribe(ok => {
      if (ok) this.dialogRef.close(true);
    });
  }

  openEditItem(item: PrescriptionItem): void {
    this.dialog.open(DoctorItemFormDialogComponent, {
      width: '520px',
      data: { item, patientId: this.patientId, prescriptionId: this.rx.id } satisfies ItemDialogData,
    }).afterClosed().subscribe(ok => {
      if (ok) this.dialogRef.close(true);
    });
  }

  deleteItem(item: PrescriptionItem): void {
    if (!confirm(`Xoá thuốc "${item.medicineName}"?`)) return;
    this.svc.deletePrescriptionItem(this.patientId, item.id).subscribe({
      next: () => this.dialogRef.close(true),
    });
  }

  statusLabel(s: string): string {
    const map: Record<string, string> = {
      Pending: 'Chờ xử lý', Processing: 'Đang xử lý', Completed: 'Hoàn thành', Failed: 'Lỗi',
    };
    return map[s] ?? s;
  }
}
