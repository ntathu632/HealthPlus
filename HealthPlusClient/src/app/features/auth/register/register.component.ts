import { AfterViewInit, Component, ElementRef, signal, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthService } from '../../../core/auth/auth.service';
import { GoogleIdentityService } from '../../../core/services/google-identity.service';
import { homeRouteForRoles } from '../../../core/auth/role-routes';

@Component({
    templateUrl: './register.component.html',
    styleUrl: './register.component.scss',
    selector: 'app-register',
  standalone: true,
  imports: [
    ReactiveFormsModule, RouterLink,
    MatCardModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatSnackBarModule,
  ]
})
export class RegisterComponent implements AfterViewInit {
  @ViewChild('googleBtn') googleBtn?: ElementRef<HTMLElement>;

  form: FormGroup;
  loading = signal(false);
  showPassword = signal(false);
  errorMsg = signal('');

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar,
    private googleIdentity: GoogleIdentityService,
  ) {
    this.form = this.fb.group({
      fullName:    ['', Validators.required],
      email:       ['', [Validators.required, Validators.email]],
      phoneNumber: [''],
      password:    ['', [Validators.required, Validators.minLength(8)]],
    });
  }

  ngAfterViewInit(): void {
    if (this.googleBtn) {
      this.googleIdentity.renderButton(this.googleBtn.nativeElement, (idToken) => this.onGoogleCredential(idToken));
    }
  }

  private onGoogleCredential(idToken: string): void {
    this.loading.set(true);
    this.errorMsg.set('');
    this.authService.googleLogin(idToken).subscribe({
      next: (res) => this.router.navigate([homeRouteForRoles(res.data.user.roles)]),
      error: (err) => {
        this.loading.set(false);
        this.errorMsg.set(err.error?.errors?.[0] ?? 'Đăng ký bằng Google thất bại.');
      },
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.errorMsg.set('');

    this.authService.register(this.form.value).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: (err) => {
        this.loading.set(false);
        this.errorMsg.set(err.error?.errors?.[0] ?? 'Đăng ký thất bại. Vui lòng thử lại.');
      },
    });
  }

  socialLogin(provider: string): void {
    this.snackBar.open(`Đăng nhập bằng ${provider} đang được phát triển, sẽ sớm ra mắt!`, 'Đóng', { duration: 3000 });
  }
}
