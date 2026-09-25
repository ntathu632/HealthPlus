import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatExpansionModule } from '@angular/material/expansion';
import { InfoPageLayoutComponent } from '../../../layout/info-page-layout/info-page-layout.component';

interface FaqItem { q: string; a: string; }

@Component({
  selector: 'app-faq',
  standalone: true,
  imports: [RouterLink, MatExpansionModule, InfoPageLayoutComponent],
  templateUrl: './faq.component.html',
  styleUrl: './faq.component.scss',
})
export class FaqComponent {
  readonly items: FaqItem[] = [
    {
      q: 'Health+ có miễn phí không?',
      a: 'Có. Health+ hoàn toàn miễn phí sử dụng cho bệnh nhân, bác sĩ và quản trị viên.',
    },
    {
      q: 'Dữ liệu sức khỏe của tôi có an toàn không?',
      a: 'Dữ liệu được lưu trữ trên hệ thống có kiểm soát truy cập theo vai trò — bác sĩ chỉ xem được hồ sơ của bệnh nhân đã được phân công cho mình, không xem được toàn bộ hệ thống. Mật khẩu được mã hoá một chiều, không lưu ở dạng văn bản thuần.',
    },
    {
      q: 'Tôi có thể quản lý hồ sơ sức khỏe cho người thân không?',
      a: 'Có. Bạn có thể tạo nhiều hồ sơ sức khỏe khác nhau trong cùng một tài khoản, ví dụ hồ sơ cho bản thân và cho con nhỏ.',
    },
    {
      q: 'Làm sao để được bác sĩ theo dõi trên hệ thống?',
      a: 'Quản trị viên sẽ phân công bác sĩ phụ trách cho bạn. Sau khi được phân công, bác sĩ đó có thể xem hồ sơ sức khỏe, ghi nhận chẩn đoán và kê đơn thuốc cho bạn.',
    },
    {
      q: 'Tôi có thể huỷ lịch hẹn đã đặt không?',
      a: 'Có. Vào mục Lịch hẹn khám, chọn lịch hẹn cần huỷ và bấm Huỷ lịch — miễn là lịch hẹn chưa được đánh dấu hoàn thành.',
    },
    {
      q: 'Tính năng nhận diện đơn thuốc hoạt động thế nào?',
      a: 'Khi bạn chụp ảnh đơn thuốc giấy và tải lên, hệ thống dùng công nghệ OCR để tự động nhận diện tên thuốc và liều dùng, giúp bạn không phải nhập tay từng loại thuốc.',
    },
    {
      q: 'Tôi có nhận được nhắc nhở khi đến giờ uống thuốc không?',
      a: 'Có. Bạn có thể bật nhắc nhở qua thông báo đẩy (Push), Email hoặc SMS trong phần Cài đặt thông báo.',
    },
  ];
}
