import { Component, Inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ReactiveFormsModule as RF } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { VaccineService } from '../../../core/services/vaccine.service';
import { Vaccine, VaccineScheduleTemplate } from '../../../models/vaccine.models';
import { HealthRecord } from '../../../models/health-record.models';

interface DialogData {
  vaccine: Vaccine | null;
  healthRecords: HealthRecord[];
}

@Component({
    templateUrl: './vaccine-form-dialog.component.html',
    styleUrl: './vaccine-form-dialog.component.scss',
    selector: 'app-vaccine-form-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule,
    MatSelectModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule,
    MatAutocompleteModule,
  ]
})
export class VaccineFormDialogComponent implements OnInit {
  form!: FormGroup;
  loading = signal(false);
  isEdit: boolean;
  allTemplates = signal<VaccineScheduleTemplate[]>([]);
  filteredVaccineNames = signal<string[]>([]);
  matchedTemplates = signal<VaccineScheduleTemplate[]>([]);

  constructor(
    private fb: FormBuilder,
    private svc: VaccineService,
    private dialogRef: MatDialogRef<VaccineFormDialogComponent>,
    private snack: MatSnackBar,
    @Inject(MAT_DIALOG_DATA) public data: DialogData,
  ) {
    this.isEdit = !!data.vaccine;
  }

  ngOnInit(): void {
    const v = this.data.vaccine;
    this.form = this.fb.group({
      healthRecordId: [v?.healthRecordId ?? (this.data.healthRecords[0]?.id ?? ''), Validators.required],
      vaccineName:    [v?.vaccineName ?? '',   Validators.required],
      doseNumber:     [v?.doseNumber ?? 1],
      status:         [v?.status ?? 'Completed'],
      injectionDate:  [v?.injectionDate ?? new Date().toISOString().split('T')[0], Validators.required],
      nextDueDate:    [v?.nextDueDate ?? ''],
      manufacturer:   [v?.manufacturer ?? ''],
      lotNumber:      [v?.lotNumber ?? ''],
      location:       [v?.location ?? ''],
      administeredBy: [v?.administeredBy ?? ''],
      sideEffects:    [v?.sideEffects ?? ''],
      notes:          [v?.notes ?? ''],
    });

    // Load templates
    this.svc.getTemplates().subscribe(res => {
      this.allTemplates.set(res.data);
      this.updateVaccineFilter('');
    });

    // Watch vaccine name changes for autocomplete + template matching
    this.form.get('vaccineName')!.valueChanges.pipe(
      debounceTime(200), distinctUntilChanged()
    ).subscribe(val => {
      this.updateVaccineFilter(val);
      this.matchTemplates(val);
    });

    // Init template matching for edit mode
    if (v) this.matchTemplates(v.vaccineName);
  }

  private updateVaccineFilter(query: string): void {
    const names = [...new Set(this.allTemplates().map(t => t.vaccineName))];
    const q = (query ?? '').toLowerCase();
    this.filteredVaccineNames.set(q ? names.filter(n => n.toLowerCase().includes(q)) : names.slice(0, 10));
  }

  private matchTemplates(name: string): void {
    const matched = this.allTemplates().filter(t => t.vaccineName.toLowerCase() === (name ?? '').toLowerCase());
    this.matchedTemplates.set(matched);
  }

  onVaccineSelected(name: string): void {
    this.matchTemplates(name);
    this.autoFillNextDue();
  }

  onDateChange(): void { this.autoFillNextDue(); }

  private autoFillNextDue(): void {
    const dose = this.form.get('doseNumber')!.value;
    const dateStr = this.form.get('injectionDate')!.value;
    const name = this.form.get('vaccineName')!.value;
    if (!dateStr || !name) return;

    // Tìm template cho mũi kế tiếp
    const nextTemplate = this.allTemplates().find(
      t => t.vaccineName.toLowerCase() === name.toLowerCase() && t.doseNumber === dose + 1
    );
    if (nextTemplate?.intervalDays) {
      const d = new Date(dateStr);
      d.setDate(d.getDate() + nextTemplate.intervalDays);
      this.form.patchValue({ nextDueDate: d.toISOString().split('T')[0] });
    }
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
    if (val.nextDueDate)    payload['nextDueDate']    = val.nextDueDate;
    if (val.manufacturer)   payload['manufacturer']   = val.manufacturer;
    if (val.lotNumber)      payload['lotNumber']      = val.lotNumber;
    if (val.location)       payload['location']       = val.location;
    if (val.administeredBy) payload['administeredBy'] = val.administeredBy;
    if (val.sideEffects)    payload['sideEffects']    = val.sideEffects;
    if (val.notes)          payload['notes']          = val.notes;

    const call = this.isEdit
      ? this.svc.update(this.data.vaccine!.id, payload as any)
      : this.svc.create(payload as any);

    call.subscribe({
      next: () => {
        this.snack.open(this.isEdit ? 'Đã cập nhật' : 'Thêm thành công', 'Đóng',
          { duration: 3000, panelClass: 'snack-success' });
        this.dialogRef.close(true);
      },
      error: () => this.loading.set(false),
    });
  }
}
