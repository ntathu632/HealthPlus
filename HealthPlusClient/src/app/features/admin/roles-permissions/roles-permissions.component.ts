import { Component, OnInit, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AdminService } from '../../../core/services/admin.service';
import { Role, Permission } from '../../../models/admin.models';

interface ResourceGroup { resource: string; permissions: Permission[]; }

@Component({
    templateUrl: './roles-permissions.component.html',
    styleUrl: './roles-permissions.component.scss',
    selector: 'app-roles-permissions',
  standalone: true,
  imports: [MatCardModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatCheckboxModule]
})
export class RolesPermissionsComponent implements OnInit {
  loading = signal(true);
  saving = signal(false);
  roles = signal<Role[]>([]);
  permissions = signal<Permission[]>([]);
  selectedRole = signal<Role | null>(null);
  selectedIds = signal<Set<number>>(new Set());

  groups = () => {
    const map = new Map<string, Permission[]>();
    for (const p of this.permissions()) {
      if (!map.has(p.resource)) map.set(p.resource, []);
      map.get(p.resource)!.push(p);
    }
    return Array.from(map.entries()).map(([resource, permissions]) => ({ resource, permissions } as ResourceGroup));
  };

  constructor(private svc: AdminService, private snack: MatSnackBar) {}

  ngOnInit(): void {
    this.loading.set(true);
    this.svc.getRoles().subscribe(res => this.roles.set(res.data));
    this.svc.getPermissions().subscribe({
      next: res => { this.permissions.set(res.data); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  selectRole(r: Role): void {
    this.selectedRole.set(r);
    this.svc.getRolePermissionIds(r.id).subscribe(res => this.selectedIds.set(new Set(res.data)));
  }

  toggle(permissionId: number, checked: boolean): void {
    this.selectedIds.update(cur => {
      const next = new Set(cur);
      if (checked) next.add(permissionId); else next.delete(permissionId);
      return next;
    });
  }

  save(): void {
    const role = this.selectedRole();
    if (!role) return;
    this.saving.set(true);
    this.svc.updateRolePermissions(role.id, { permissionIds: Array.from(this.selectedIds()) }).subscribe({
      next: () => {
        this.saving.set(false);
        this.snack.open('Cập nhật quyền thành công', 'Đóng', { duration: 2500, panelClass: 'snack-success' });
      },
      error: () => {
        this.saving.set(false);
        this.snack.open('Cập nhật quyền thất bại', 'Đóng', { duration: 3000 });
      },
    });
  }
}
