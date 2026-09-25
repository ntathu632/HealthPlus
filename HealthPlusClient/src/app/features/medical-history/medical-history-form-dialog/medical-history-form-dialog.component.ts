import { Component, Inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MedicalHistoryService } from '../../../core/services/medical-history.service';
import { MedicalHistory } from '../../../models/medical-history.models';
import { HealthRecord } from '../../../models/health-record.models';

interface DialogData {
  record: MedicalHistory | null;
  healthRecords: HealthRecord[];
}

@Component({
    templateUrl: './medical-history-form-dialog.component.html',
    styleUrl: './medical-history-form-dialog.component.scss',
    selector: 'app-medical-history-form-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule, MatDialogModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatButtonModule, MatIconModule, MatDatepickerModule, MatProgressSpinnerModule,
  ]
})
export class MedicalHistoryFormDialogComponent implements OnInit {
  form!: FormGroup;
  loading = signal(false);
  isEdit: boolean;

  readonly specialties = [
    'Nội khoa', 'Ngoại khoa', 'Nhi khoa', 'Sản phụ khoa', 'Tim mạch',
    'Tiêu hóa', 'Thần kinh', 'Da liễu', 'Tai mũi họng', 'Mắt',
    'Răng hàm mặt', 'Ung bướu', 'Xương khớp', 'Hô hấp', 'Nội tiết',
    'Tâm thần', 'Phục hồi chức năng', 'Khám tổng quát',
  ];

  constructor(
    private fb: FormBuilder,
    private svc: MedicalHistoryService,
    private dialogRef: MatDialogRef<MedicalHistoryFormDialogComponent>,
    private snack: MatSnackBar,
    @Inject(MAT_DIALOG_DATA) public data: DialogData,
  ) {
    this.isEdit = !!data.record;
  }

  ngOnInit(): void {
    const r = this.data.record;
    this.form = this.fb.group({
      healthRecordId: [r?.healthRecordId ?? (this.data.healthRecords[0]?.id ?? ''), Validators.required],
      visitDate:      [r?.visitDate ? new Date(r.visitDate) : new Date(), Validators.required],
      followUpDate:   [r?.followUpDate ? new Date(r.followUpDate) : null],
      hospital:       [r?.hospital ?? ''],
      doctorName:     [r?.doctorName ?? ''],
      specialty:      [r?.specialty ?? ''],
      diagnosis:      [r?.diagnosis ?? ''],
      treatment:      [r?.treatment ?? ''],
      notes:          [r?.notes ?? ''],
    });
  }

  // mat-datepicker làm việc với đối tượng Date, còn backend nhận DateOnly dạng chuỗi
  // "yyyy-MM-dd" — tự ghép từ các phần ngày/tháng/năm cục bộ, tránh dùng toISOString()
  // (quy đổi sang UTC có thể lùi/tiến 1 ngày tuỳ múi giờ trình duyệt).
  private toDateOnlyString(d: Date): string {
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading.set(true);

    const val = this.form.value;
    const payload: Record<string, unknown> = {
      healthRecordId: val.healthRecordId,
      visitDate: this.toDateOnlyString(val.visitDate),
    };
    if (val.followUpDate) payload['followUpDate'] = this.toDateOnlyString(val.followUpDate);
    if (val.hospital)    payload['hospital']    = val.hospital;
    if (val.doctorName)  payload['doctorName']  = val.doctorName;
    if (val.specialty)   payload['specialty']   = val.specialty;
    if (val.diagnosis)   payload['diagnosis']   = val.diagnosis;
    if (val.treatment)   payload['treatment']   = val.treatment;
    if (val.notes)       payload['notes']       = val.notes;

    const call = this.isEdit
      ? this.svc.update(this.data.record!.id, payload as any)
      : this.svc.create(payload as any);

    call.subscribe({
      next: () => {
        this.snack.open(
          this.isEdit ? 'Đã cập nhật lần khám' : 'Thêm lần khám thành công',
          'Đóng', { duration: 3000, panelClass: 'snack-success' }
        );
        this.dialogRef.close(true);
      },
      error: () => this.loading.set(false),
    });
  }
}
