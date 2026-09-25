import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AdminService } from '../../../core/services/admin.service';
import { AdminUser } from '../../../models/admin.models';

@Component({
    templateUrl: './assign-patient-dialog.component.html',
    styleUrl: './assign-patient-dialog.component.scss',
    selector: 'app-assign-patient-dialog',
  standalone: true,
  imports: [
    FormsModule, MatDialogModule, MatFormFieldModule, MatSelectModule,
    MatInputModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule,
  ]
})
export class AssignPatientDialogComponent implements OnInit {
  loadingLists = signal(true);
  submitting = signal(false);
  filterText = '';
  selectedDoctorId = '';
  selectedPatientId = '';

  doctors = signal<AdminUser[]>([]);
  patients = signal<AdminUser[]>([]);

  filteredDoctors = () => this.filterByText(this.doctors());
  filteredPatients = () => this.filterByText(this.patients());

  constructor(
    private svc: AdminService,
    private dialogRef: MatDialogRef<AssignPatientDialogComponent>,
    private snack: MatSnackBar,
  ) {}

  ngOnInit(): void {
    this.loadingLists.set(true);
    // roleId 2 = Doctor, 3 = User (patient)
    this.svc.getUsers(1, 200, undefined, 2).subscribe(res => this.doctors.set(res.data.items));
    this.svc.getUsers(1, 500, undefined, 3).subscribe({
      next: res => { this.patients.set(res.data.items); this.loadingLists.set(false); },
      error: () => this.loadingLists.set(false),
    });
  }

  private filterByText(list: AdminUser[]): AdminUser[] {
    const term = this.filterText.trim().toLowerCase();
    if (!term) return list;
    return list.filter(u => u.fullName.toLowerCase().includes(term) || u.email.toLowerCase().includes(term));
  }

  submit(): void {
    if (!this.selectedDoctorId || !this.selectedPatientId) return;
    this.submitting.set(true);
    this.svc.assignPatient({ doctorId: this.selectedDoctorId, patientId: this.selectedPatientId }).subscribe({
      next: () => {
        this.snack.open('Gán bệnh nhân thành công', 'Đóng', { duration: 3000, panelClass: 'snack-success' });
        this.dialogRef.close(true);
      },
      error: err => {
        this.submitting.set(false);
        this.snack.open(err.error?.message ?? 'Gán bệnh nhân thất bại', 'Đóng', { duration: 3500 });
      },
    });
  }
}
