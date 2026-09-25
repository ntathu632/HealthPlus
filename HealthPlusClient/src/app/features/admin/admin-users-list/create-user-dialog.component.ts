import { Component, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AdminService } from '../../../core/services/admin.service';

@Component({
    templateUrl: './create-user-dialog.component.html',
    styleUrl: './create-user-dialog.component.scss',
    selector: 'app-create-user-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatSelectModule,
    MatInputModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule,
  ]
})
export class CreateUserDialogComponent {
  form: FormGroup;
  loading = signal(false);

  constructor(
    fb: FormBuilder,
    private svc: AdminService,
    private dialogRef: MatDialogRef<CreateUserDialogComponent>,
    private snack: MatSnackBar,
  ) {
    this.form = fb.group({
      roleId: [2, Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.pattern(/^(?=.*[A-Z])(?=.*\d).{8,}$/)]],
      fullName: ['', Validators.required],
      phoneNumber: [''],
    });
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading.set(true);
    const v = this.form.value;
    this.svc.createDoctor({
      roleId: +v.roleId,
      email: v.email,
      password: v.password,
      fullName: v.fullName,
      phoneNumber: v.phoneNumber || undefined,
    }).subscribe({
      next: () => {
        this.snack.open('Tạo tài khoản thành công', 'Đóng', { duration: 3000, panelClass: 'snack-success' });
        this.dialogRef.close(true);
      },
      error: err => {
        this.loading.set(false);
        this.snack.open(err.error?.errors?.[0] ?? err.error?.message ?? 'Tạo tài khoản thất bại', 'Đóng', { duration: 3500 });
      },
    });
  }
}
