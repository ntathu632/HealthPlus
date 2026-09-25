import { Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { InfoPageLayoutComponent } from '../../../layout/info-page-layout/info-page-layout.component';

interface FeatureDetail {
  icon: string;
  color: string;
  title: string;
  desc: string;
  points: string[];
}

@Component({
  selector: 'app-features-page',
  standalone: true,
  imports: [MatIconModule, InfoPageLayoutComponent],
  templateUrl: './features-page.component.html',
  styleUrl: './features-page.component.scss',
})
export class FeaturesPageComponent {
  readonly features: FeatureDetail[] = [
    {
      icon: 'folder_shared', color: '#1976D2',
      title: 'Hồ sơ sức khỏe số hoá',
      desc: 'Lưu trữ toàn bộ chỉ số sức khỏe cho bản thân và từng thành viên trong gia đình.',
      points: ['Theo dõi chiều cao, cân nặng, BMI, huyết áp theo thời gian', 'Biểu đồ trực quan để thấy rõ xu hướng', 'Quản lý hồ sơ riêng cho từng thành viên gia đình'],
    },
    {
      icon: 'local_hospital', color: '#1976D2',
      title: 'Lịch sử khám & chẩn đoán',
      desc: 'Lưu lại đầy đủ mỗi lần khám bệnh — không lo thất lạc giấy tờ.',
      points: ['Ghi nhận chẩn đoán, điều trị, ngày tái khám', 'Bác sĩ được phân công có thể xem và cập nhật trực tiếp', 'Đính kèm tài liệu y tế liên quan'],
    },
    {
      icon: 'event_available', color: '#1976D2',
      title: 'Đặt lịch khám online',
      desc: 'Chọn bác sĩ, chọn khung giờ, đặt lịch chỉ trong một phút.',
      points: ['Xem danh sách bác sĩ đang hoạt động', 'Theo dõi trạng thái: chờ xác nhận, đã xác nhận, hoàn thành', 'Huỷ lịch dễ dàng khi cần đổi kế hoạch'],
    },
    {
      icon: 'local_hospital', color: '#1976D2',
      title: 'Tư vấn bác sĩ trực tuyến (trả phí)',
      desc: 'Kết nối với bác sĩ từ nhiều bệnh viện, chọn theo chuyên khoa — mỗi lượt tư vấn có mức phí riêng theo bác sĩ.',
      points: ['Xem rõ chuyên khoa, bệnh viện công tác và phí tư vấn trước khi đặt', 'Thanh toán ngay trong ứng dụng sau khi đặt lịch', 'Lịch sử thanh toán minh bạch, tra cứu lại bất cứ lúc nào'],
    },
    {
      icon: 'smart_toy', color: '#1976D2',
      title: 'Trợ lý AI sức khỏe (miễn phí)',
      desc: 'Trò chuyện cùng trợ lý AI để giải đáp nhanh các thắc mắc sức khỏe thường gặp, mọi lúc.',
      points: ['Hỏi đáp tức thì, không giới hạn số lần, hoàn toàn miễn phí', 'Chỉ mang tính tham khảo — không thay thế chẩn đoán của bác sĩ', 'Gợi ý đặt lịch tư vấn với bác sĩ khi vấn đề cần thăm khám thực tế'],
    },
    {
      icon: 'medication', color: '#1976D2',
      title: 'Đơn thuốc & nhận diện tự động',
      desc: 'Chụp ảnh đơn thuốc giấy, hệ thống tự nhận diện tên thuốc và liều dùng.',
      points: ['OCR đọc đơn thuốc từ ảnh chụp', 'Bác sĩ kê đơn trực tiếp trên hệ thống', 'Theo dõi liều dùng, số lần uống mỗi ngày'],
    },
    {
      icon: 'vaccines', color: '#1976D2',
      title: 'Theo dõi tiêm chủng',
      desc: 'Quản lý lịch tiêm theo từng mũi, không bỏ lỡ mũi nhắc lại.',
      points: ['Lưu lịch sử các mũi đã tiêm', 'Tự tính ngày đến hạn mũi tiếp theo', 'Cảnh báo khi quá hạn tiêm nhắc lại'],
    },
    {
      icon: 'notifications_active', color: '#1976D2',
      title: 'Nhắc nhở thông minh',
      desc: 'Không bỏ lỡ lịch uống thuốc, lịch tiêm chủng hay lịch tái khám.',
      points: ['Nhắc qua thông báo đẩy (Push), Email hoặc SMS', 'Tuỳ chỉnh giờ nhắc theo nhu cầu', 'Đánh dấu hoàn thành ngay trong thông báo'],
    },
  ];
}
