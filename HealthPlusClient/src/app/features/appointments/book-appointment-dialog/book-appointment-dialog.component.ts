import { Component, OnInit, signal, computed } from '@angular/core';
import { DecimalPipe, DatePipe } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AppointmentService } from '../../../core/services/appointment.service';
import { PaymentService } from '../../../core/services/payment.service';
import { Appointment, DoctorListItem } from '../../../models/appointment.models';

@Component({
    templateUrl: './book-appointment-dialog.component.html',
    styleUrl: './book-appointment-dialog.component.scss',
    selector: 'app-book-appointment-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule, DecimalPipe, DatePipe, MatDialogModule, MatFormFieldModule, MatSelectModule,
    MatInputModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule,
  ]
})
export class BookAppointmentDialogComponent implements OnInit {
  form: FormGroup;
  loadingDoctors = signal(true);
  submitting = signal(false);
  paying = signal(false);
  doctors = signal<DoctorListItem[]>([]);
  step = signal<'form' | 'payment'>('form');
  createdAppointment = signal<Appointment | null>(null);

  selectedDoctor = computed(() => {
    const id = this.form?.get('doctorId')?.value;
    return this.doctors().find(d => d.id === id) ?? null;
  });

  constructor(
    fb: FormBuilder,
    private svc: AppointmentService,
    private paymentSvc: PaymentService,
    private dialogRef: MatDialogRef<BookAppointmentDialogComponent>,
    private snack: MatSnackBar,
  ) {
    this.form = fb.group({
      doctorId: ['', Validators.required],
      appointmentTime: ['', Validators.required],
      reason: [''],
    });
  }

  ngOnInit(): void {
    this.svc.getActiveDoctors().subscribe({
      next: res => {
        this.doctors.set(res.data);
        if (res.data.length > 0) this.form.patchValue({ doctorId: res.data[0].id });
        this.loadingDoctors.set(false);
      },
      error: () => this.loadingDoctors.set(false),
    });
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.submitting.set(true);
    const v = this.form.value;
    this.svc.create({
      doctorId: v.doctorId,
      appointmentTime: new Date(v.appointmentTime).toISOString(),
      reason: v.reason || undefined,
    }).subscribe({
      next: res => {
        this.submitting.set(false);
        if (res.data.fee > 0 && !res.data.isPaid) {
          this.createdAppointment.set(res.data);
          this.step.set('payment');
        } else {
          this.snack.open('Đặt lịch hẹn thành công', 'Đóng', { duration: 3000, panelClass: 'snack-success' });
          this.dialogRef.close(true);
        }
      },
      error: err => {
        this.submitting.set(false);
        this.snack.open(err.error?.message ?? 'Đặt lịch hẹn thất bại', 'Đóng', { duration: 3500 });
      },
    });
  }

  confirmPayment(): void {
    const appt = this.createdAppointment();
    if (!appt) return;
    this.paying.set(true);
    this.paymentSvc.pay(appt.id).subscribe({
      next: () => {
        this.snack.open('Thanh toán thành công (mô phỏng) — đã đặt lịch hẹn', 'Đóng', { duration: 3500, panelClass: 'snack-success' });
        this.dialogRef.close(true);
      },
      error: err => {
        this.paying.set(false);
        this.snack.open(err.error?.message ?? 'Thanh toán thất bại', 'Đóng', { duration: 3500 });
      },
    });
  }
}
