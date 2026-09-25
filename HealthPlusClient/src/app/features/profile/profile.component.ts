import { Component, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDividerModule } from '@angular/material/divider';
import { MatSelectModule } from '@angular/material/select';
import { UserService } from '../../core/services/user.service';
import { HospitalService } from '../../core/services/hospital.service';
import { AuthService } from '../../core/auth/auth.service';
import { UserResponse } from '../../models/auth.models';
import { Hospital } from '../../models/hospital.models';

function passwordsMatchValidator(control: AbstractControl): ValidationErrors | null {
  const newPassword = control.get('newPassword')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;
  return newPassword && confirmPassword && newPassword !== confirmPassword
    ? { passwordMismatch: true }
    : null;
}

@Component({
    templateUrl: './profile.component.html',
    styleUrl: './profile.component.scss',
    selector: 'app-profile',
  standalone: true,
  imports: [
    DatePipe, ReactiveFormsModule,
    MatCardModule, MatButtonModule, MatIconModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatProgressSpinnerModule, MatDividerModule,
  ]
})
export class ProfileComponent implements OnInit {
  loading = signal(true);
  savingProfile = signal(false);
  savingPassword = signal(false);
  uploadingAvatar = signal(false);
  user = signal<UserResponse | null>(null);
  hospitals = signal<Hospital[]>([]);

  readonly specialties = [
    'Nội khoa', 'Ngoại khoa', 'Nhi khoa', 'Sản phụ khoa', 'Tim mạch',
    'Tiêu hóa', 'Thần kinh', 'Da liễu', 'Tai mũi họng', 'Mắt',
    'Răng hàm mặt', 'Ung bướu', 'Xương khớp', 'Hô hấp', 'Nội tiết',
    'Tâm thần', 'Phục hồi chức năng', 'Khám tổng quát',
  ];

  profileForm: FormGroup;
  passwordForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private userSvc: UserService,
    private hospitalSvc: HospitalService,
    public auth: AuthService,
    private snack: MatSnackBar,
  ) {
    this.profileForm = this.fb.group({
      fullName: ['', Validators.required],
      phoneNumber: [''],
      hospitalId: [''],
      specialty: [''],
      consultationFee: [0],
    });
    this.passwordForm = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', Validators.required],
    }, { validators: passwordsMatchValidator });
  }

  ngOnInit(): void {
    this.userSvc.getMe().subscribe({
      next: res => {
        this.user.set(res.data);
        this.profileForm.patchValue({
          fullName: res.data.fullName,
          phoneNumber: res.data.phoneNumber ?? '',
          hospitalId: res.data.hospitalId ?? '',
          specialty: res.data.specialty ?? '',
          consultationFee: res.data.consultationFee ?? 0,
        });
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });

    if (this.auth.isDoctor()) {
      this.hospitalSvc.getAll().subscribe({ next: res => this.hospitals.set(res.data) });
    }
  }

  initials(): string {
    const name = this.user()?.fullName ?? '';
    return name.trim().split(' ').filter(Boolean).map(w => w[0]).slice(0, 2).join('').toUpperCase();
  }

  saveProfile(): void {
    if (this.profileForm.invalid) { this.profileForm.markAllAsTouched(); return; }
    this.savingProfile.set(true);
    const v = this.profileForm.value;
    const payload: Record<string, unknown> = { fullName: v.fullName, phoneNumber: v.phoneNumber || undefined };
    if (this.auth.isDoctor()) {
      payload['hospitalId'] = v.hospitalId || undefined;
      payload['specialty'] = v.specialty || undefined;
      payload['consultationFee'] = v.consultationFee || 0;
    }
    this.userSvc.updateProfile(payload as any).subscribe({
      next: res => {
        this.user.set(res.data);
        this.auth.updateUser({ fullName: res.data.fullName, avatarUrl: res.data.avatarUrl });
        this.savingProfile.set(false);
        this.snack.open('Đã lưu thông tin cá nhân', 'Đóng', { duration: 2500 });
      },
      error: () => this.savingProfile.set(false),
    });
  }

  changePassword(): void {
    if (this.passwordForm.invalid) { this.passwordForm.markAllAsTouched(); return; }
    this.savingPassword.set(true);
    const v = this.passwordForm.value;
    this.userSvc.changePassword({ currentPassword: v.currentPassword, newPassword: v.newPassword }).subscribe({
      next: () => {
        this.savingPassword.set(false);
        this.passwordForm.reset();
        this.snack.open('Đã đổi mật khẩu thành công', 'Đóng', { duration: 2500 });
      },
      error: err => {
        this.savingPassword.set(false);
        const msg = err?.error?.errors?.[0] ?? 'Đổi mật khẩu thất bại';
        this.snack.open(msg, 'Đóng', { duration: 3500 });
      },
    });
  }

  triggerAvatarUpload(): void {
    const input = document.querySelector<HTMLInputElement>('app-profile input[type="file"]');
    input?.click();
  }

  onAvatarSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    if (file.size > 2 * 1024 * 1024) {
      this.snack.open('Ảnh vượt quá 2MB', 'Đóng', { duration: 3000 });
      return;
    }
    this.uploadingAvatar.set(true);
    this.userSvc.uploadAvatar(file).subscribe({
      next: res => {
        this.user.update(u => u ? { ...u, avatarUrl: res.data.avatarUrl } : u);
        this.auth.updateUser({ avatarUrl: res.data.avatarUrl });
        this.uploadingAvatar.set(false);
        this.snack.open('Đã cập nhật ảnh đại diện', 'Đóng', { duration: 2500 });
      },
      error: () => {
        this.uploadingAvatar.set(false);
        this.snack.open('Tải ảnh thất bại', 'Đóng', { duration: 3000 });
      },
    });
  }
}
