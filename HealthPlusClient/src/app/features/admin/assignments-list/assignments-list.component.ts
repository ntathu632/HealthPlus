import { Component, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AdminService } from '../../../core/services/admin.service';
import { DoctorPatient } from '../../../models/admin.models';
import { AssignPatientDialogComponent } from './assign-patient-dialog.component';

@Component({
    templateUrl: './assignments-list.component.html',
    styleUrl: './assignments-list.component.scss',
    selector: 'app-assignments-list',
  standalone: true,
  imports: [DatePipe, MatCardModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatTooltipModule]
})
export class AssignmentsListComponent implements OnInit {
  assignments = signal<DoctorPatient[]>([]);
  loading = signal(true);

  constructor(
    private svc: AdminService,
    private dialog: MatDialog,
    private snack: MatSnackBar,
  ) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.svc.getDoctorPatients().subscribe({
      next: res => { this.assignments.set(res.data); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  openAssign(): void {
    this.dialog.open(AssignPatientDialogComponent, { width: '480px' })
      .afterClosed().subscribe(ok => { if (ok) this.load(); });
  }

  unassign(a: DoctorPatient): void {
    if (!confirm(`Huỷ gán bệnh nhân "${a.patientName}" khỏi bác sĩ "${a.doctorName}"?`)) return;
    this.svc.unassignPatient(a.id).subscribe({
      next: () => {
        this.snack.open('Đã huỷ gán', 'Đóng', { duration: 2000 });
        this.assignments.update(cur => cur.filter(x => x.id !== a.id));
      },
    });
  }
}
