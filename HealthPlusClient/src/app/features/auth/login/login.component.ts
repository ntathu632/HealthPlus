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
import { homeRouteForRoles } from '../../../core/auth/role-routes';
import { GoogleIdentityService } from '../../../core/services/google-identity.service';

@Component({
    templateUrl: './login.component.html',
    styleUrl: './login.component.scss',
    selector: 'app-login',
  standalone: true,
  imports: [
    ReactiveFormsModule, RouterLink,
    MatCardModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatSnackBarModule,
  ]
})
export class LoginComponent implements AfterViewInit {
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
      email: ['', Validators.required],
      password: ['', Validators.required],
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
        this.errorMsg.set(err.error?.errors?.[0] ?? 'Đăng nhập bằng Google thất bại.');
      },
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.errorMsg.set('');

    this.authService.login(this.form.value).subscribe({
      next: (res) => this.router.navigate([homeRouteForRoles(res.data.user.roles)]),
      error: (err) => {
        this.loading.set(false);
        this.errorMsg.set(err.error?.errors?.[0] ?? 'Email hoặc mật khẩu không đúng.');
      },
    });
  }

  socialLogin(provider: string): void {
    this.snackBar.open(`Đăng nhập bằng ${provider} đang được phát triển, sẽ sớm ra mắt!`, 'Đóng', { duration: 3000 });
  }
}
