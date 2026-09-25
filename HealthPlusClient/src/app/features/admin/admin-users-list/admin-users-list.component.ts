import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AdminService } from '../../../core/services/admin.service';
import { AdminUser } from '../../../models/admin.models';
import { CreateUserDialogComponent } from './create-user-dialog.component';
import { ResetPasswordDialogComponent } from './reset-password-dialog.component';

const ROLE_OPTIONS = [
  { id: 1, name: 'Admin' },
  { id: 2, name: 'Doctor' },
  { id: 3, name: 'User' },
];

@Component({
    templateUrl: './admin-users-list.component.html',
    styleUrl: './admin-users-list.component.scss',
    selector: 'app-admin-users-list',
  standalone: true,
  imports: [
    FormsModule, DatePipe,
    MatCardModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule,
    MatFormFieldModule, MatInputModule, MatSelectModule, MatSlideToggleModule, MatTooltipModule,
  ]
})
export class AdminUsersListComponent implements OnInit {
  users = signal<AdminUser[]>([]);
  loading = signal(true);
  page = 1;
  pageSize = 20;
  totalCount = signal(0);
  search = '';
  roleFilter: number | undefined = undefined;
  private searchDebounce?: ReturnType<typeof setTimeout>;

  readonly roleOptions = ROLE_OPTIONS;

  totalPages = () => Math.max(1, Math.ceil(this.totalCount() / this.pageSize));

  constructor(
    private svc: AdminService,
    private dialog: MatDialog,
    private snack: MatSnackBar,
  ) {}

  ngOnInit(): void { this.load(); }

  onSearchChange(): void {
    clearTimeout(this.searchDebounce);
    this.searchDebounce = setTimeout(() => this.reload(), 350);
  }

  reload(): void {
    this.page = 1;
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.svc.getUsers(this.page, this.pageSize, this.search || undefined, this.roleFilter).subscribe({
      next: res => {
        this.users.set(res.data.items);
        this.totalCount.set(res.data.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  prevPage(): void { if (this.page > 1) { this.page--; this.load(); } }
  nextPage(): void { if (this.page < this.totalPages()) { this.page++; this.load(); } }

  primaryRoleId(u: AdminUser): number | undefined {
    return u.roles[0]?.id;
  }

  changeRole(u: AdminUser, roleId: number): void {
    if (roleId === this.primaryRoleId(u)) return;
    this.svc.updateUserRole(u.id, { roleId }).subscribe({
      next: res => {
        this.users.update(cur => cur.map(x => x.id === u.id ? res.data : x));
        this.snack.open('Cập nhật vai trò thành công', 'Đóng', { duration: 2500 });
      },
      error: () => this.snack.open('Cập nhật vai trò thất bại', 'Đóng', { duration: 3000 }),
    });
  }

  toggleStatus(u: AdminUser, isActive: boolean): void {
    this.svc.updateUserStatus(u.id, { isActive }).subscribe({
      next: res => {
        this.users.update(cur => cur.map(x => x.id === u.id ? res.data : x));
        this.snack.open(isActive ? 'Đã mở khoá tài khoản' : 'Đã khoá tài khoản', 'Đóng', { duration: 2500 });
      },
      error: () => this.snack.open('Cập nhật trạng thái thất bại', 'Đóng', { duration: 3000 }),
    });
  }

  openCreateUser(): void {
    this.dialog.open(CreateUserDialogComponent, { width: '480px' })
      .afterClosed().subscribe(ok => { if (ok) this.reload(); });
  }

  openResetPassword(u: AdminUser): void {
    this.dialog.open(ResetPasswordDialogComponent, {
      width: '440px',
      data: { userId: u.id, fullName: u.fullName },
    });
  }
}
