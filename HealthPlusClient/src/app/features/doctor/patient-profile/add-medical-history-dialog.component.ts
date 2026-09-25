import { Component, Inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DoctorService } from '../../../core/services/doctor.service';
import { HealthRecord } from '../../../models/health-record.models';
import { MedicalHistory } from '../../../models/medical-history.models';

interface DialogData {
  patientId: string;
  healthRecords: HealthRecord[];
  record?: MedicalHistory | null;
}

@Component({
    templateUrl: './add-medical-history-dialog.component.html',
    styleUrl: './add-medical-history-dialog.component.scss',
    selector: 'app-add-medical-history-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule, MatDialogModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule,
  ]
})
export class AddMedicalHistoryDialogComponent implements OnInit {
  form!: FormGroup;
  loading = signal(false);
  isEdit = false;

  readonly specialties = [
    'Nội khoa', 'Ngoại khoa', 'Nhi khoa', 'Sản phụ khoa', 'Tim mạch',
    'Tiêu hóa', 'Thần kinh', 'Da liễu', 'Tai mũi họng', 'Mắt',
    'Răng hàm mặt', 'Ung bướu', 'Xương khớp', 'Hô hấp', 'Nội tiết',
    'Tâm thần', 'Phục hồi chức năng', 'Khám tổng quát',
  ];

  constructor(
    private fb: FormBuilder,
    private svc: DoctorService,
    private dialogRef: MatDialogRef<AddMedicalHistoryDialogComponent>,
    private snack: MatSnackBar,
    @Inject(MAT_DIALOG_DATA) public data: DialogData,
  ) {
    this.isEdit = !!data.record;
  }

  ngOnInit(): void {
    const r = this.data.record;
    this.form = this.fb.group({
      healthRecordId: [r?.healthRecordId ?? (this.data.healthRecords[0]?.id ?? ''), Validators.required],
      visitDate:      [r?.visitDate ?? new Date().toISOString().split('T')[0], Validators.required],
      followUpDate:   [r?.followUpDate ?? ''],
      hospital:       [r?.hospital ?? ''],
      doctorName:     [r?.doctorName ?? ''],
      specialty:      [r?.specialty ?? ''],
      diagnosis:      [r?.diagnosis ?? ''],
      treatment:      [r?.treatment ?? ''],
      notes:          [r?.notes ?? ''],
    });
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading.set(true);

    const val = this.form.value;
    const payload: Record<string, unknown> = {
      healthRecordId: val.healthRecordId,
      visitDate: val.visitDate,
    };
    if (val.followUpDate) payload['followUpDate'] = val.followUpDate;
    if (val.hospital)    payload['hospital']    = val.hospital;
    if (val.doctorName)  payload['doctorName']  = val.doctorName;
    if (val.specialty)   payload['specialty']   = val.specialty;
    if (val.diagnosis)   payload['diagnosis']   = val.diagnosis;
    if (val.treatment)   payload['treatment']   = val.treatment;
    if (val.notes)       payload['notes']       = val.notes;

    const call = this.isEdit
      ? this.svc.updateMedicalHistory(this.data.patientId, this.data.record!.id, payload as any)
      : this.svc.addMedicalHistory(this.data.patientId, payload as any);

    call.subscribe({
      next: () => {
        this.snack.open(this.isEdit ? 'Đã cập nhật chẩn đoán' : 'Thêm chẩn đoán thành công', 'Đóng',
          { duration: 3000, panelClass: 'snack-success' });
        this.dialogRef.close(true);
      },
      error: () => this.loading.set(false),
    });
  }
}
