import { Component, computed, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../core/auth/auth.service';

interface NavItem { label: string; icon: string; route: string; }

const PATIENT_NAV: NavItem[] = [
  { label: 'Tổng quan',        icon: 'grid_view',            route: '/dashboard' },
  { label: 'Hồ sơ sức khỏe',  icon: 'person',               route: '/health-records' },
  { label: 'Khám Online',      icon: 'event',                 route: '/appointments' },
  { label: 'Lịch sử khám',     icon: 'local_hospital',       route: '/medical-history' },
  { label: 'Tiêm chủng',       icon: 'vaccines',             route: '/vaccines' },
  { label: 'Đơn thuốc',        icon: 'medication',           route: '/prescriptions' },
  { label: 'Nhắc nhở',         icon: 'notifications_active', route: '/reminders' },
  { label: 'Trợ lý AI',        icon: 'smart_toy',            route: '/ai-chat' },
  { label: 'Thanh toán',       icon: 'receipt_long',         route: '/payments' },
];

const DOCTOR_NAV: NavItem[] = [
  { label: 'Tổng quan',         icon: 'grid_view', route: '/doctor/dashboard' },
  { label: 'Bệnh nhân của tôi', icon: 'groups', route: '/doctor/patients' },
  { label: 'Lịch hẹn khám',     icon: 'event',  route: '/doctor/appointments' },
  { label: 'Doanh thu',         icon: 'payments', route: '/doctor/earnings' },
];

const ADMIN_NAV: NavItem[] = [
  { label: 'Tổng quan',       icon: 'grid_view',      route: '/admin/dashboard' },
  { label: 'Người dùng',      icon: 'group',          route: '/admin/users' },
  { label: 'Gán bệnh nhân',   icon: 'assignment_ind', route: '/admin/assignments' },
  { label: 'Lịch hẹn khám',   icon: 'event',          route: '/admin/appointments' },
  { label: 'Vai trò & quyền', icon: 'admin_panel_settings', route: '/admin/roles' },
  { label: 'Nhật ký',         icon: 'history',        route: '/admin/audit-logs' },
  { label: 'Cấu hình',        icon: 'settings',        route: '/admin/settings' },
];

@Component({
    templateUrl: './sidebar.component.html',
    styleUrl: './sidebar.component.scss',
    selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, MatIconModule]
})
export class SidebarComponent {
  closeSidenav = output<void>();

  readonly navItems = computed<NavItem[]>(() => {
    if (this.auth.isAdmin()) return ADMIN_NAV;
    if (this.auth.isDoctor()) return DOCTOR_NAV;
    return PATIENT_NAV;
  });

  readonly roleLabel = computed(() => {
    if (this.auth.isAdmin()) return 'Quản trị viên';
    if (this.auth.isDoctor()) return 'Bác sĩ';
    return 'Người dùng';
  });

  constructor(public auth: AuthService) {}

  initials(): string {
    const name = this.auth.currentUserName() ?? '';
    return name.trim().split(' ').filter(Boolean).map(w => w[0]).slice(0, 2).join('').toUpperCase();
  }
}
