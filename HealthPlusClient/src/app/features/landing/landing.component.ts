import { Component, AfterViewInit, ElementRef, QueryList, ViewChildren } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { PublicNavComponent } from '../../layout/public-nav/public-nav.component';
import { PublicFooterComponent } from '../../layout/public-footer/public-footer.component';

interface RoleCard {
  icon: string;
  color: string;
  title: string;
  desc: string;
  features: string[];
}

interface FeatureTile {
  icon: string;
  color: string;
  title: string;
  desc: string;
}

interface Step {
  number: string;
  title: string;
  desc: string;
}

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [RouterLink, MatIconModule, MatButtonModule, PublicNavComponent, PublicFooterComponent],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.scss',
})
export class LandingComponent implements AfterViewInit {
  @ViewChildren('revealEl') revealEls!: QueryList<ElementRef<HTMLElement>>;

  readonly roleCards: RoleCard[] = [
    {
      icon: 'person', color: '#1976D2',
      title: 'Bệnh nhân',
      desc: 'Chủ động quản lý sức khỏe cho bản thân và gia đình',
      features: [
        'Hồ sơ sức khỏe cho nhiều thành viên trong gia đình',
        'Đặt lịch hẹn khám với bác sĩ chỉ trong vài bước',
        'Lưu trữ lịch sử khám, đơn thuốc, sổ tiêm chủng',
        'Nhắc nhở uống thuốc, tái khám, tiêm nhắc lại',
      ],
    },
    {
      icon: 'medical_services', color: '#1976D2',
      title: 'Bác sĩ',
      desc: 'Theo dõi và chăm sóc bệnh nhân hiệu quả hơn',
      features: [
        'Xem toàn bộ hồ sơ sức khỏe của bệnh nhân được phân công',
        'Ghi nhận chẩn đoán và kê đơn thuốc trực tiếp trên hệ thống',
        'Quản lý lịch hẹn: xác nhận, từ chối, đánh dấu hoàn thành',
        'Không cần giấy tờ, mọi thông tin đều số hoá',
      ],
    },
    {
      icon: 'admin_panel_settings', color: '#1976D2',
      title: 'Quản trị viên',
      desc: 'Vận hành hệ thống an toàn và linh hoạt',
      features: [
        'Quản lý tài khoản, phân quyền theo từng vai trò',
        'Gán bác sĩ phụ trách cho từng bệnh nhân',
        'Theo dõi nhật ký hoạt động toàn hệ thống',
        'Tuỳ chỉnh cấu hình hệ thống theo nhu cầu',
      ],
    },
  ];

  readonly featureTiles: FeatureTile[] = [
    { icon: 'folder_shared', color: '#1976D2', title: 'Hồ sơ sức khỏe số hoá — Miễn phí', desc: 'Lưu trữ đầy đủ chỉ số sức khỏe, bệnh sử, dễ dàng theo dõi qua biểu đồ. Hoàn toàn miễn phí.' },
    { icon: 'local_hospital', color: '#1976D2', title: 'Tư vấn bác sĩ trực tuyến', desc: 'Đặt lịch tư vấn với bác sĩ từ nhiều bệnh viện, theo chuyên khoa — tính phí theo từng lượt tư vấn.' },
    { icon: 'smart_toy', color: '#1976D2', title: 'Trợ lý AI sức khỏe', desc: 'Trò chuyện cùng AI để giải đáp nhanh thắc mắc về sức khỏe, mọi lúc — miễn phí sử dụng.' },
    { icon: 'medication', color: '#1976D2', title: 'Đơn thuốc & nhận diện tự động', desc: 'Chụp ảnh đơn thuốc, hệ thống tự nhận diện tên thuốc và liều dùng.' },
    { icon: 'vaccines', color: '#1976D2', title: 'Theo dõi tiêm chủng', desc: 'Quản lý lịch tiêm theo từng mũi, cảnh báo khi quá hạn.' },
    { icon: 'notifications_active', color: '#1976D2', title: 'Nhắc nhở thông minh', desc: 'Không bỏ lỡ lịch uống thuốc, lịch tiêm chủng hay lịch tái khám.' },
  ];

  readonly steps: Step[] = [
    { number: '1', title: 'Tạo tài khoản', desc: 'Đăng ký miễn phí chỉ với email và số điện thoại.' },
    { number: '2', title: 'Tạo hồ sơ & đặt lịch', desc: 'Khai báo hồ sơ sức khỏe, đặt lịch hẹn với bác sĩ phù hợp.' },
    { number: '3', title: 'Theo dõi & chăm sóc', desc: 'Nhận tư vấn từ bác sĩ, theo dõi tiến trình điều trị mọi lúc.' },
  ];

  ngAfterViewInit(): void {
    const observer = new IntersectionObserver(
      entries => {
        for (const entry of entries) {
          if (entry.isIntersecting) {
            entry.target.classList.add('is-visible');
            observer.unobserve(entry.target);
          }
        }
      },
      { threshold: 0.15 },
    );
    this.revealEls.forEach(el => observer.observe(el.nativeElement));
  }
}
