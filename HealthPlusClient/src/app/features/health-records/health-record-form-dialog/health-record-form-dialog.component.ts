import { Component, Inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { HealthRecordService } from '../../../core/services/health-record.service';
import { HealthRecord } from '../../../models/health-record.models';

@Component({
    templateUrl: './health-record-form-dialog.component.html',
    styleUrl: './health-record-form-dialog.component.scss',
    selector: 'app-health-record-form-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule,
    MatSelectModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatSnackBarModule,
  ]
})
export class HealthRecordFormDialogComponent implements OnInit {
  form!: FormGroup;
  loading = signal(false);
  isEdit: boolean;

  readonly bloodTypes = [
    { value: 'APositive', label: 'A+' }, { value: 'ANegative', label: 'A−' },
    { value: 'BPositive', label: 'B+' }, { value: 'BNegative', label: 'B−' },
    { value: 'OPositive', label: 'O+' }, { value: 'ONegative', label: 'O−' },
    { value: 'ABPositive', label: 'AB+' }, { value: 'ABNegative', label: 'AB−' },
  ];

  constructor(
    private fb: FormBuilder,
    private svc: HealthRecordService,
    private dialogRef: MatDialogRef<HealthRecordFormDialogComponent>,
    private snack: MatSnackBar,
    @Inject(MAT_DIALOG_DATA) public data: HealthRecord | null,
  ) {
    this.isEdit = !!data;
  }

  ngOnInit(): void {
    this.form = this.fb.group({
      profileName:     [this.data?.profileName ?? '',  Validators.required],
      fullName:        [this.data?.fullName ?? '',      Validators.required],
      dateOfBirth:     [this.data?.dateOfBirth ?? '',   Validators.required],
      gender:          [this.data?.gender ?? 'Male',    Validators.required],
      bloodType:       [this.data?.bloodType ?? ''],
      insuranceNumber: [this.data?.insuranceNumber ?? ''],
      allergies:       [this.data?.allergies ?? ''],
      chronicDiseases: [this.data?.chronicDiseases ?? ''],
    });
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading.set(true);

    const payload = { ...this.form.value };
    if (!payload.bloodType) delete payload.bloodType;

    const call = this.isEdit
      ? this.svc.update(this.data!.id, payload)
      : this.svc.create(payload);

    call.subscribe({
      next: () => {
        this.snack.open(this.isEdit ? 'Đã cập nhật hồ sơ' : 'Tạo hồ sơ thành công', 'Đóng',
          { duration: 3000, panelClass: 'snack-success' });
        this.dialogRef.close(true);
      },
      error: () => { this.loading.set(false); },
    });
  }
}
