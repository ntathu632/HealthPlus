import { Component, OnInit, signal, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { DoctorService } from '../../../core/services/doctor.service';
import { PatientSummary } from '../../../models/doctor.models';

@Component({
    templateUrl: './doctor-patients-list.component.html',
    styleUrl: './doctor-patients-list.component.scss',
    selector: 'app-doctor-patients-list',
  standalone: true,
  imports: [
    RouterLink, FormsModule, DatePipe, MatCardModule, MatIconModule,
    MatFormFieldModule, MatInputModule, MatProgressSpinnerModule,
  ]
})
export class DoctorPatientsListComponent implements OnInit {
  patients = signal<PatientSummary[]>([]);
  loading = signal(true);
  search = '';

  filteredPatients = computed(() => {
    const term = this.search.trim().toLowerCase();
    if (!term) return this.patients();
    return this.patients().filter(p =>
      p.fullName.toLowerCase().includes(term) || p.email.toLowerCase().includes(term));
  });

  constructor(private svc: DoctorService) {}

  ngOnInit(): void {
    this.svc.getMyPatients().subscribe({
      next: res => { this.patients.set(res.data); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  initials(name: string): string {
    return name.trim().split(' ').filter(Boolean).map(w => w[0]).slice(0, 2).join('').toUpperCase();
  }
}
