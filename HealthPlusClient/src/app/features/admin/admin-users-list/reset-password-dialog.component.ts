import { Component, Inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AdminService } from '../../../core/services/admin.service';

interface DialogData {
  userId: string;
  fullName: string;
}

@Component({
    templateUrl: './reset-password-dialog.component.html',
    styleUrl: './reset-password-dialog.component.scss',
    selector: 'app-reset-password-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule, MatDialogModule, MatFormFieldModule,
    MatInputModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule,
  ]
})
export class ResetPasswordDialogComponent {
  form: FormGroup;
  loading = signal(false);

  constructor(
    fb: FormBuilder,
    private svc: AdminService,
    private dialogRef: MatDialogRef<ResetPasswordDialogComponent>,
    private snack: MatSnackBar,
    @Inject(MAT_DIALOG_DATA) public data: DialogData,
  ) {
    this.form = fb.group({
      newPassword: ['', [Validators.required, Validators.pattern(/^(?=.*[A-Z])(?=.*\d).{8,}$/)]],
    });
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading.set(true);
    this.svc.resetPassword(this.data.userId, { newPassword: this.form.value.newPassword }).subscribe({
      next: () => {
        this.snack.open('Đặt lại mật khẩu thành công', 'Đóng', { duration: 3000, panelClass: 'snack-success' });
        this.dialogRef.close(true);
      },
      error: err => {
        this.loading.set(false);
        this.snack.open(err.error?.errors?.[0] ?? err.error?.message ?? 'Đặt lại mật khẩu thất bại', 'Đóng', { duration: 3500 });
      },
    });
  }
}
