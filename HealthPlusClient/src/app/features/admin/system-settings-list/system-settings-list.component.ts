import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AdminService } from '../../../core/services/admin.service';
import { SystemSetting } from '../../../models/admin.models';

@Component({
    templateUrl: './system-settings-list.component.html',
    styleUrl: './system-settings-list.component.scss',
    selector: 'app-system-settings-list',
  standalone: true,
  imports: [FormsModule, MatCardModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatFormFieldModule, MatInputModule]
})
export class SystemSettingsListComponent implements OnInit {
  settings = signal<SystemSetting[]>([]);
  loading = signal(true);
  saving = signal<string | null>(null);
  draft: Record<string, string> = {};

  constructor(private svc: AdminService, private snack: MatSnackBar) {}

  ngOnInit(): void {
    this.svc.getSettings().subscribe({
      next: res => {
        this.settings.set(res.data);
        for (const s of res.data) this.draft[s.settingKey] = s.settingValue ?? '';
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  save(s: SystemSetting): void {
    this.saving.set(s.settingKey);
    this.svc.updateSetting(s.settingKey, { settingValue: this.draft[s.settingKey] }).subscribe({
      next: res => {
        this.settings.update(cur => cur.map(x => x.id === s.id ? res.data : x));
        this.saving.set(null);
        this.snack.open('Đã lưu cấu hình', 'Đóng', { duration: 2500, panelClass: 'snack-success' });
      },
      error: () => {
        this.saving.set(null);
        this.snack.open('Lưu cấu hình thất bại', 'Đóng', { duration: 3000 });
      },
    });
  }
}
