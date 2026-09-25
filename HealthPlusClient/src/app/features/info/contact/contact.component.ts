import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { InfoPageLayoutComponent } from '../../../layout/info-page-layout/info-page-layout.component';
import { ContactService } from '../../../core/services/contact.service';

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [
    RouterLink, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule,
    MatSnackBarModule, InfoPageLayoutComponent,
  ],
  templateUrl: './contact.component.html',
  styleUrl: './contact.component.scss',
})
export class ContactComponent {
  form: FormGroup;
  sent = signal(false);
  sending = signal(false);

  constructor(
    private fb: FormBuilder,
    private contactService: ContactService,
    private snackBar: MatSnackBar,
  ) {
    this.form = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      message: ['', Validators.required],
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.sending.set(true);
    this.contactService.send(this.form.value).subscribe({
      next: () => {
        this.sending.set(false);
        this.sent.set(true);
        this.form.reset();
      },
      error: () => {
        this.sending.set(false);
        this.snackBar.open('Gửi liên hệ thất bại, vui lòng thử lại sau.', 'Đóng', { duration: 3000 });
      },
    });
  }
}
