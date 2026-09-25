import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { AdminService } from '../../../core/services/admin.service';
import { AuditLog } from '../../../models/admin.models';

@Component({
    templateUrl: './audit-logs-list.component.html',
    styleUrl: './audit-logs-list.component.scss',
    selector: 'app-audit-logs-list',
  standalone: true,
  imports: [FormsModule, DatePipe, MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatFormFieldModule, MatInputModule]
})
export class AuditLogsListComponent implements OnInit {
  logs = signal<AuditLog[]>([]);
  loading = signal(true);
  page = 1;
  pageSize = 20;
  totalCount = signal(0);
  entityFilter = '';
  private filterDebounce?: ReturnType<typeof setTimeout>;

  totalPages = () => Math.max(1, Math.ceil(this.totalCount() / this.pageSize));

  constructor(private svc: AdminService) {}

  ngOnInit(): void { this.load(); }

  onFilterChange(): void {
    clearTimeout(this.filterDebounce);
    this.filterDebounce = setTimeout(() => { this.page = 1; this.load(); }, 350);
  }

  load(): void {
    this.loading.set(true);
    this.svc.getAuditLogs(this.page, this.pageSize, undefined, this.entityFilter || undefined).subscribe({
      next: res => {
        this.logs.set(res.data.items);
        this.totalCount.set(res.data.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  prevPage(): void { if (this.page > 1) { this.page--; this.load(); } }
  nextPage(): void { if (this.page < this.totalPages()) { this.page++; this.load(); } }
}
