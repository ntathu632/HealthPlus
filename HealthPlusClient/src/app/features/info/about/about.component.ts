import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { InfoPageLayoutComponent } from '../../../layout/info-page-layout/info-page-layout.component';

interface RoleCard { icon: string; color: string; title: string; desc: string; }
interface Step { number: string; title: string; desc: string; }

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [RouterLink, MatIconModule, InfoPageLayoutComponent],
  templateUrl: './about.component.html',
  styleUrl: './about.component.scss',
})
export class AboutComponent {
  readonly roleCards: RoleCard[] = [
    { icon: 'person', color: '#1976D2', title: 'Bệnh nhân', desc: 'Quản lý hồ sơ sức khỏe, lịch sử khám, đơn thuốc, sổ tiêm chủng và đặt lịch hẹn với bác sĩ.' },
    { icon: 'medical_services', color: '#1976D2', title: 'Bác sĩ', desc: 'Theo dõi bệnh nhân được phân công, ghi nhận chẩn đoán, kê đơn thuốc và quản lý lịch hẹn khám.' },
    { icon: 'admin_panel_settings', color: '#1976D2', title: 'Quản trị viên', desc: 'Vận hành hệ thống, quản lý tài khoản và phân quyền một cách an toàn.' },
  ];

  readonly steps: Step[] = [
    { number: '1', title: 'Tạo tài khoản', desc: 'Đăng ký miễn phí chỉ với email và số điện thoại.' },
    { number: '2', title: 'Tạo hồ sơ & đặt lịch', desc: 'Khai báo hồ sơ sức khỏe, đặt lịch hẹn với bác sĩ phù hợp.' },
    { number: '3', title: 'Theo dõi & chăm sóc', desc: 'Nhận tư vấn từ bác sĩ, theo dõi tiến trình điều trị mọi lúc.' },
  ];
}
