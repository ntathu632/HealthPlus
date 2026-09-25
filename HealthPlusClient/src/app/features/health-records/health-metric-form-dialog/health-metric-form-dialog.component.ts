import { Component, Inject, signal, computed } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DecimalPipe } from '@angular/common';
import { HealthRecordService } from '../../../core/services/health-record.service';

@Component({
    templateUrl: './health-metric-form-dialog.component.html',
    styleUrl: './health-metric-form-dialog.component.scss',
    selector: 'app-health-metric-form-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule, DecimalPipe,
    MatDialogModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule, MatProgressSpinnerModule,
  ]
})
export class HealthMetricFormDialogComponent {
  form: FormGroup;
  loading = signal(false);

  bmiPreview = computed(() => {
    const h = this.form?.get('heightCm')?.value;
    const w = this.form?.get('weightKg')?.value;
    if (!h || !w || h <= 0 || w <= 0) return null;
    return Math.round((w / ((h / 100) ** 2)) * 10) / 10;
  });

  bmiPreviewClass = computed(() => {
    const bmi = this.bmiPreview();
    if (!bmi) return '';
    if (bmi < 18.5) return 'bmi-preview bmi-underweight';
    if (bmi < 25)   return 'bmi-preview bmi-normal';
    if (bmi < 30)   return 'bmi-preview bmi-overweight';
    return 'bmi-preview bmi-obese';
  });

  bmiStatus = computed(() => {
    const bmi = this.bmiPreview();
    if (!bmi) return '';
    if (bmi < 18.5) return 'Thiếu cân';
    if (bmi < 25)   return 'Bình thường ✓';
    if (bmi < 30)   return 'Thừa cân';
    return 'Béo phì';
  });

  hasAnyValue = computed(() => {
    if (!this.form) return false;
    const v = this.form.value;
    return !!(v.heightCm || v.weightKg || v.systolicBp || v.heartRate || v.bloodSugar || v.temperature);
  });

  constructor(
    private fb: FormBuilder,
    private svc: HealthRecordService,
    private dialogRef: MatDialogRef<HealthMetricFormDialogComponent>,
    private snack: MatSnackBar,
    @Inject(MAT_DIALOG_DATA) private recordId: string,
  ) {
    const now = new Date();
    now.setMinutes(now.getMinutes() - now.getTimezoneOffset());
    this.form = this.fb.group({
      heightCm:    [null], weightKg:   [null],
      systolicBp:  [null], diastolicBp:[null],
      heartRate:   [null], bloodSugar: [null], temperature:[null],
      measuredAt:  [now.toISOString().slice(0, 16)],
      notes:       [''],
    });
  }

  submit(): void {
    this.loading.set(true);
    const raw = this.form.value;
    const payload: Record<string, unknown> = {};
    if (raw.heightCm)    payload['heightCm']    = +raw.heightCm;
    if (raw.weightKg)    payload['weightKg']    = +raw.weightKg;
    if (raw.systolicBp)  payload['systolicBp']  = +raw.systolicBp;
    if (raw.diastolicBp) payload['diastolicBp'] = +raw.diastolicBp;
    if (raw.heartRate)   payload['heartRate']   = +raw.heartRate;
    if (raw.bloodSugar)  payload['bloodSugar']  = +raw.bloodSugar;
    if (raw.temperature) payload['temperature'] = +raw.temperature;
    if (raw.notes)       payload['notes']       = raw.notes;
    if (raw.measuredAt)  payload['measuredAt']  = new Date(raw.measuredAt).toISOString();

    this.svc.addMetric(this.recordId, payload as any).subscribe({
      next: () => {
        this.snack.open('Đã lưu chỉ số', 'Đóng', { duration: 3000, panelClass: 'snack-success' });
        this.dialogRef.close(true);
      },
      error: () => this.loading.set(false),
    });
  }
}
