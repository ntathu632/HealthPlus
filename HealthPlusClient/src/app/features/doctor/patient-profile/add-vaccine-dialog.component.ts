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
import { Vaccine } from '../../../models/vaccine.models';

interface DialogData {
  patientId: string;
  healthRecords: HealthRecord[];
  vaccine?: Vaccine | null;
}

@Component({
  selector: 'app-add-vaccine-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule,
    MatSelectModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule,
  ],
  templateUrl: './add-vaccine-dialog.component.html',
  styleUrl: './add-vaccine-dialog.component.scss',
})
export class AddVaccineDialogComponent implements OnInit {
  form!: FormGroup;
  loading = signal(false);
  isEdit = false;

  constructor(
    private fb: FormBuilder,
    private svc: DoctorService,
    private dialogRef: MatDialogRef<AddVaccineDialogComponent>,
    private snack: MatSnackBar,
    @Inject(MAT_DIALOG_DATA) public data: DialogData,
  ) {
    this.isEdit = !!data.vaccine;
  }

  ngOnInit(): void {
    const v = this.data.vaccine;
    this.form = this.fb.group({
      healthRecordId: [v?.healthRecordId ?? (this.data.healthRecords[0]?.id ?? ''), Validators.required],
      vaccineName:    [v?.vaccineName ?? '', Validators.required],
      doseNumber:     [v?.doseNumber ?? 1],
      status:         [v?.status ?? 'Completed'],
      injectionDate:  [v?.injectionDate ?? new Date().toISOString().split('T')[0], Validators.required],
      nextDueDate:    [v?.nextDueDate ?? ''],
      manufacturer:   [v?.manufacturer ?? ''],
      lotNumber:      [v?.lotNumber ?? ''],
      location:       [v?.location ?? ''],
      sideEffects:    [v?.sideEffects ?? ''],
      notes:          [v?.notes ?? ''],
    });
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading.set(true);
    const val = this.form.value;

    const payload: Record<string, unknown> = {
      healthRecordId: val.healthRecordId,
      vaccineName: val.vaccineName,
      doseNumber: +val.doseNumber,
      status: val.status,
      injectionDate: val.injectionDate,
    };
    if (val.nextDueDate)  payload['nextDueDate']  = val.nextDueDate;
    if (val.manufacturer) payload['manufacturer'] = val.manufacturer;
    if (val.lotNumber)    payload['lotNumber']    = val.lotNumber;
    if (val.location)     payload['location']     = val.location;
    if (val.sideEffects)  payload['sideEffects']  = val.sideEffects;
    if (val.notes)        payload['notes']        = val.notes;

    const call = this.isEdit
      ? this.svc.updateVaccine(this.data.patientId, this.data.vaccine!.id, payload as any)
      : this.svc.addVaccine(this.data.patientId, payload as any);

    call.subscribe({
      next: () => {
        this.snack.open(this.isEdit ? 'Đã cập nhật mũi tiêm' : 'Thêm mũi tiêm thành công', 'Đóng',
          { duration: 3000, panelClass: 'snack-success' });
        this.dialogRef.close(true);
      },
      error: () => this.loading.set(false),
    });
  }
}
