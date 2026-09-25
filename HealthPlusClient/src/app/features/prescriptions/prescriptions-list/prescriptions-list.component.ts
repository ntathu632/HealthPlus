import { Component, OnInit, signal, computed, Inject } from '@angular/core';
import { DatePipe, NgClass } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { PrescriptionService } from '../../../core/services/prescription.service';
import { Prescription, PrescriptionItem, CreatePrescriptionItemRequest, UpdatePrescriptionItemRequest } from '../../../models/prescription.models';

// ─── Add/Edit Item Dialog ─────────────────────────────────────────────────────

interface ItemDialogData {
  item: PrescriptionItem | null;
  prescriptionId: string;
}

@Component({
    templateUrl: './item-form-dialog.component.html',
    styleUrl: './item-form-dialog.component.scss',
    selector: 'app-item-form-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatCheckboxModule,
  ]
})
export class ItemFormDialogComponent {
  form: FormGroup;
  loading = signal(false);
  isEdit: boolean;

  constructor(
    fb: FormBuilder,
    private svc: PrescriptionService,
    private dialogRef: MatDialogRef<ItemFormDialogComponent>,
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
      ? this.svc.updateItem(this.data.prescriptionId, this.data.item!.id, payload as UpdatePrescriptionItemRequest)
      : this.svc.addItem(this.data.prescriptionId, payload as CreatePrescriptionItemRequest);

    obs.subscribe({
      next: () => {
        this.snack.open(this.isEdit ? 'Đã cập nhật thuốc' : 'Thêm thuốc thành công', 'Đóng', { duration: 2500 });
        this.dialogRef.close(true);
      },
      error: () => this.loading.set(false),
    });
  }
}

// ─── Prescription Detail Dialog ───────────────────────────────────────────────

@Component({
    templateUrl: './prescription-detail-dialog.component.html',
    styleUrl: './prescription-detail-dialog.component.scss',
    selector: 'app-prescription-detail-dialog',
  standalone: true,
  imports: [
    DatePipe, NgClass,
    MatDialogModule, MatButtonModule, MatIconModule,
    MatProgressSpinnerModule, MatTooltipModule, MatDividerModule, MatChipsModule,
  ]
})
export class PrescriptionDetailDialogComponent {
  uploading = signal(false);

  constructor(
    private svc: PrescriptionService,
    private dialog: MatDialog,
    private dialogRef: MatDialogRef<PrescriptionDetailDialogComponent>,
    private snack: MatSnackBar,
    @Inject(MAT_DIALOG_DATA) public rx: Prescription,
  ) {}

  close(): void {
    this.dialogRef.close(true);
  }

  triggerUpload(): void {
    const input = document.querySelector<HTMLInputElement>('app-prescription-detail-dialog input[type="file"]');
    input?.click();
  }

  onFileSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    if (file.size > 5 * 1024 * 1024) {
      this.snack.open('Ảnh vượt quá 5MB', 'Đóng', { duration: 3000 });
      return;
    }
    this.uploading.set(true);
    this.svc.uploadImage(this.rx.id, file).subscribe({
      next: res => {
        Object.assign(this.rx, res.data);
        this.uploading.set(false);
        const msg = res.data.status === 'Completed'
          ? `Đã nhận diện ${res.data.items.length} thuốc từ ảnh. Vui lòng kiểm tra lại.`
          : 'Tải ảnh thành công nhưng nhận diện OCR thất bại. Hãy thêm thuốc thủ công.';
        this.snack.open(msg, 'Đóng', { duration: 4000 });
      },
      error: () => {
        this.uploading.set(false);
        this.snack.open('Tải ảnh thất bại', 'Đóng', { duration: 3000 });
      },
    });
  }

  openAddItem(): void {
    this.dialog.open(ItemFormDialogComponent, {
      width: '520px',
      data: { item: null, prescriptionId: this.rx.id } satisfies ItemDialogData,
    }).afterClosed().subscribe(ok => {
      if (ok) {
        this.svc.getById(this.rx.id).subscribe(res => {
          this.rx.items = res.data.items;
          this.dialogRef.close(true);
        });
      }
    });
  }

  openEditItem(item: PrescriptionItem): void {
    this.dialog.open(ItemFormDialogComponent, {
      width: '520px',
      data: { item, prescriptionId: this.rx.id } satisfies ItemDialogData,
    }).afterClosed().subscribe(ok => {
      if (ok) {
        this.svc.getById(this.rx.id).subscribe(res => {
          this.rx.items = res.data.items;
          this.dialogRef.close(true);
        });
      }
    });
  }

  deleteItem(item: PrescriptionItem): void {
    if (!confirm(`Xoá thuốc "${item.medicineName}"?`)) return;
    this.svc.deleteItem(this.rx.id, item.id).subscribe({
      next: () => {
        this.rx.items = this.rx.items.filter(i => i.id !== item.id);
        this.snack.open('Đã xoá thuốc', 'Đóng', { duration: 2000 });
        this.dialogRef.close(true);
      },
    });
  }

  statusLabel(s: string): string {
    const map: Record<string, string> = {
      Pending: 'Chờ xử lý', Processing: 'Đang xử lý', Completed: 'Hoàn thành', Failed: 'Lỗi',
    };
    return map[s] ?? s;
  }
}

// ─── Prescriptions List (main page) ──────────────────────────────────────────

@Component({
    templateUrl: './prescriptions-list.component.html',
    styleUrl: './prescriptions-list.component.scss',
    selector: 'app-prescriptions-list',
  standalone: true,
  imports: [
    DatePipe, NgClass,
    MatCardModule, MatButtonModule, MatIconModule,
    MatProgressSpinnerModule, MatTooltipModule,
  ]
})
export class PrescriptionsListComponent implements OnInit {
  allItems    = signal<Prescription[]>([]);
  loading     = signal(true);
  selectedStatus = signal('All');

  readonly statusTabs = [
    { value: 'All',        label: 'Tất cả' },
    { value: 'Pending',    label: 'Chờ xử lý' },
    { value: 'Processing', label: 'Đang xử lý' },
    { value: 'Completed',  label: 'Hoàn thành' },
    { value: 'Failed',     label: 'Lỗi' },
  ];

  filteredItems = computed(() => {
    const status = this.selectedStatus();
    if (status === 'All') return this.allItems();
    return this.allItems().filter(rx => rx.status === status);
  });

  constructor(
    private svc: PrescriptionService,
    private dialog: MatDialog,
    private snack: MatSnackBar,
  ) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.svc.getAll().subscribe({
      next: res => { this.allItems.set(res.data); this.loading.set(false); },
      error: ()  => this.loading.set(false),
    });
  }

  countByStatus(status: string): number {
    if (status === 'All') return this.allItems().length;
    return this.allItems().filter(rx => rx.status === status).length;
  }

  createNew(): void {
    this.svc.create({}).subscribe({
      next: res => {
        this.snack.open('Đã tạo đơn thuốc mới', 'Đóng', { duration: 2500 });
        this.allItems.update(cur => [res.data, ...cur]);
        this.openDetail(res.data);
      },
      error: () => this.snack.open('Tạo đơn thất bại', 'Đóng', { duration: 3000 }),
    });
  }

  openDetail(rx: Prescription): void {
    this.dialog.open(PrescriptionDetailDialogComponent, {
      width: '600px',
      maxHeight: '90vh',
      data: { ...rx },
    }).afterClosed().subscribe(changed => { if (changed) this.load(); });
  }

  delete(rx: Prescription): void {
    if (!confirm(`Xoá đơn thuốc ngày ${new Date(rx.createdAt).toLocaleDateString('vi-VN')}?`)) return;
    this.svc.delete(rx.id).subscribe({
      next: () => {
        this.snack.open('Đã xoá đơn thuốc', 'Đóng', { duration: 2000 });
        this.allItems.update(cur => cur.filter(r => r.id !== rx.id));
      },
    });
  }

  statusLabel(s: string): string {
    const map: Record<string, string> = {
      Pending: 'Chờ xử lý', Processing: 'Đang xử lý', Completed: 'Hoàn thành', Failed: 'Lỗi',
    };
    return map[s] ?? s;
  }
}
