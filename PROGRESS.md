# HealthPlus — Tiến độ dự án

> Cập nhật: 2026-07-22  
> Stack: ASP.NET Core 8 API + Angular 21 · Clean Architecture · SQL Server

---

## Tổng quan nhanh

| Phần | Trạng thái | Ghi chú |
|---|---|---|
| Backend API | ✅ Hoàn chỉnh | 7/7 services, 8/8 controllers |
| Database | ✅ Hoàn chỉnh | 21 entities, migrations đã chạy |
| Angular — Auth | ✅ Xong | Login, Register, điều hướng theo vai trò sau đăng nhập |
| Angular — Dashboard (Bệnh nhân) | ✅ Xong | Stats, preview, cảnh báo |
| Angular — Health Records | ✅ Xong | CRUD + biểu đồ metrics + BMI |
| Angular — Medical History | ✅ Xong | CRUD + upload document |
| Angular — Vaccines | ✅ Xong | CRUD + overdue + templates |
| Angular — Prescriptions | ✅ Xong | List + detail dialog + thêm/sửa/xoá thuốc + upload ảnh |
| Angular — Reminders | ✅ Xong | List + tạo/sửa + toggle + cài đặt thông báo |
| Angular — Profile | ✅ Xong | Sửa thông tin, đổi mật khẩu, đổi avatar |
| **Angular — Bác sĩ** | ✅ Xong (2026-07-19) | Danh sách bệnh nhân được gán, hồ sơ bệnh nhân (tab hồ sơ/lịch sử khám/tiêm chủng/đơn thuốc), thêm chẩn đoán, tạo đơn thuốc + thêm/sửa/xoá thuốc, **lịch hẹn khám** — xem mục riêng bên dưới |
| **Angular — Admin** | ✅ Xong (2026-07-19) | Dashboard thống kê, quản lý người dùng (đổi role/khoá-mở), tạo tài khoản bác sĩ, gán bệnh nhân cho bác sĩ, vai trò & quyền, nhật ký hệ thống, cấu hình hệ thống — xem mục riêng bên dưới |
| **Lịch hẹn khám (Appointment)** | ✅ Xong (2026-07-19) | Tính năng mới hoàn toàn (trước đó cả hệ thống không có) — bệnh nhân đặt lịch với bác sĩ, bác sĩ xác nhận/từ chối/hoàn thành — xem mục riêng bên dưới |
| Notifications thật (FCM/Email/SMS) | ✅ Xong (Push+Email+SMS) | Cả 3 kênh — xem mục riêng bên dưới |
| OCR tích hợp | ✅ Xong | Tesseract (offline), test end-to-end qua API thật — xem lưu ý ARM64 bên dưới |
| Unit Tests | ✅ Xong | Backend 84 tests (xUnit+Moq, gồm cả Admin/Doctor/Payment/AiChat service), Frontend 11 tests (Vitest) — xem mục riêng bên dưới |
| **Tư vấn bác sĩ trả phí + Trợ lý AI** | ✅ Xong (2026-07-22) | Theo dõi sức khỏe vẫn miễn phí; đặt lịch tư vấn với bác sĩ từ bệnh viện (hư cấu) tính phí + thanh toán mô phỏng; trợ lý AI (tích hợp thật API AI) — xem mục riêng bên dưới |
| Docker / deploy config | ✅ Xong | docker-compose (API+Angular+SQL Server) — **config xong, chưa test `docker compose up` chạy thật** (Docker Desktop đã cài nhưng cần bật Nested Virtualization trong Parallels mới chạy được), xem mục riêng bên dưới |
| PWA / Service Worker | ✅ Xong (2026-07-21) | `@angular/service-worker`, cài được vào máy, cache offline cho phần tĩnh — xem mục riêng bên dưới |
| Git + CI/CD | ✅ Xong (2026-07-21) | Khởi tạo git repo (trước đó chưa từng có), GitHub Actions build+test cả 2 phía — xem mục riêng bên dưới |

---

## Tư vấn bác sĩ trực tuyến trả phí + Trợ lý AI sức khỏe (mới hoàn toàn, xong 2026-07-22)

Yêu cầu của user: theo dõi sức khỏe (hồ sơ, nhắc nhở, tiêm chủng, đơn thuốc...) vẫn **miễn phí hoàn toàn**; riêng **tư vấn trực tuyến với bác sĩ thì tính phí**, bác sĩ đến từ nhiều bệnh viện; thêm **trợ lý AI trò chuyện về sức khỏe**. Đã làm rõ trước khi code: thanh toán làm **đầy đủ flow nhưng mô phỏng** (chưa nối cổng thanh toán thật, theo đúng tiền lệ nút đăng nhập MXH "sắp ra mắt"); AI chat là **tích hợp thật** (gọi HTTP thật tới API AI, sẽ hoạt động ngay khi điền `ApiKey` thật vào `appsettings.json`); tên bệnh viện dùng **hư cấu** (không dùng tên bệnh viện Việt Nam có thật, tránh ngụ ý hợp tác/bảo chứng không có thật).

**Backend — Hospital + phí bác sĩ**
- Entity mới `Hospital` (`Name`, `Address`); `User` thêm `HospitalId`/`Specialty`/`ConsultationFee` (chỉ có ý nghĩa với role Bác sĩ); `Appointment` thêm `Fee` (snapshot phí tại thời điểm đặt lịch) và `IsPaid`.
- Migration `AddPaymentsHospitalsAiChat`. Seed 4 bệnh viện hư cấu (Đa khoa Quốc tế Thành Đô, Đa khoa Hồng Phúc, Đa khoa Việt Tâm, Quốc tế Sông Hàn) gán cho 3 bác sĩ demo kèm chuyên khoa + phí (150k/250k/300k).
- `AppointmentService.CreateAsync`: snapshot `Fee` từ `doctor.ConsultationFee`, `IsPaid = fee <= 0` (lịch miễn phí tự động coi như đã "thanh toán"). `GetActiveDoctorsAsync` trả kèm `Specialty`/`HospitalName`/`ConsultationFee` để bệnh nhân thấy trước khi đặt.
- `AdminService.CreateDoctorAsync`: hỗ trợ gán `HospitalId`/`Specialty`/`ConsultationFee` khi tạo tài khoản bác sĩ mới.

**Backend — Thanh toán mô phỏng**
- Entity `Payment` (`Amount`, `Status`: Pending/Completed/Failed, `Method="Demo"`, `PaidAt`). `IPaymentService`/`PaymentService`, `PaymentsController` (`/api/payments`): `POST /{appointmentId}/pay` (chặn thanh toán trùng, thanh toán hộ người khác, thanh toán lịch miễn phí — trả lỗi rõ ràng 404/403/409), `GET /my` (lịch sử thanh toán).

**Backend — Trợ lý AI (tích hợp thật)**
- Entity `AiConversation`/`AiMessage`. `IAiChatService`/`AiChatService`: quản lý hội thoại (tự đặt tiêu đề từ 60 ký tự đầu tin nhắn), ghép lịch sử hội thoại gửi kèm mỗi lượt hỏi. `AiChatController` (`/api/ai-chat` — lưu ý `KebabCaseControllerConvention` có sẵn tự chuyển `AiChatController` thành kebab-case, không phải `aichat`): `POST /messages`, `GET /conversations`, `GET /conversations/{id}/messages`.
- `AiCompletionClient` (`IAiCompletionClient`): gọi thật API của nhà cung cấp AI (`v1/messages`, model `model AI`). Theo đúng triết lý `TwilioSmsSender`/`SmtpEmailSender` có sẵn — **chưa điền `AiChat:ApiKey` trong `appsettings.json` thì không crash**, tự trả lời mặc định "chưa được cấu hình API key" thay vì gọi HTTP; mọi lỗi HTTP/parse đều bắt và trả câu trả lời thân thiện thay vì để lỗi 500 lộ ra ngoài.
- System prompt ép buộc: chỉ trả lời chủ đề sức khỏe, luôn nói rõ đây là thông tin tham khảo không thay thế bác sĩ, khuyến khích đặt lịch tư vấn thật nếu triệu chứng nghiêm trọng, không chẩn đoán chắc chắn/không kê đơn cụ thể.

**Frontend**
- `book-appointment-dialog`: chọn bác sĩ hiện luôn chuyên khoa + bệnh viện + phí (hoặc "Miễn phí"); nếu lịch có phí, sau khi đặt chuyển sang bước xác nhận thanh toán mô phỏng ngay trong dialog (không phải đóng dialog rồi mở lại).
- `appointments-list`: mỗi lịch hẹn có phí hiện chip "Đã thanh toán"/"Chưa thanh toán" + nút thanh toán ngay nếu chưa trả; link sang trang lịch sử thanh toán mới (`/payments`, `payments-list.component`).
- `ai-chat.component` (route `/ai-chat`, thêm vào sidebar bệnh nhân, nhãn "Trợ lý AI"): giao diện chat kiểu tin nhắn, banner cảnh báo "chỉ tham khảo, không thay thế bác sĩ", gợi ý câu hỏi mở đầu, gửi tin nhắn optimistic (hiện ngay tin của mình trước khi server phản hồi).
- Landing page, trang Tính năng (`/tinh-nang`), trang Giới thiệu (`/gioi-thieu`) đều cập nhật nội dung phản ánh đúng mô hình: các tính năng theo dõi sức khỏe ghi rõ "(miễn phí)", tính năng tư vấn bác sĩ ghi rõ "(trả phí)", trợ lý AI ghi rõ "(miễn phí)" — không thêm số liệu/lời hứa không có thật.

**Đã kiểm thử qua API thật**: đặt lịch với bác sĩ có phí → tạo đúng `Fee`/`IsPaid=false` → gọi `/payments/{id}/pay` thành công lần đầu, gọi lại lần 2 đúng bị chặn 409 "đã thanh toán"; đặt lịch bác sĩ miễn phí → `IsPaid=true` ngay, không cần thanh toán. AI chat: gọi khi chưa có `ApiKey` → nhận đúng câu trả lời "chưa cấu hình" thay vì lỗi; route đúng là `/api/ai-chat/messages` (kebab-case, không phải 404 như tưởng nhầm ban đầu khi test — xem lại route đăng ký qua `/swagger/v1/swagger.json` để xác nhận).

**Unit test mới**: `PaymentServiceTests` (6 test — not-found/unauthorized/đã thanh toán/miễn phí/thanh toán thành công/lịch sử sắp xếp đúng thứ tự), `AiChatServiceTests` (6 test — tin nhắn rỗng/tạo hội thoại mới với tiêu đề rút gọn/hội thoại của người khác bị chặn/không tìm thấy hội thoại/danh sách hội thoại sắp xếp đúng), `AiCompletionClientTests` (1 test — chưa cấu hình API key trả về đúng thông báo thân thiện, không gọi HTTP thật). Tổng backend: 71 → **84 test**, tất cả pass.

Đã build/test đầy đủ: `dotnet build` (0 lỗi), `dotnet test HealthPlus.Tests` (84/84 pass), `tsc --noEmit` (0 lỗi), `ng build --configuration production` (pass, không phát sinh cảnh báo ngân sách style mới ngoài các warning đã biết từ trước), `npm test` (11/11 pass).

---

## Admin & Bác sĩ (xong 2026-07-19)

Backend (`AdminController`/`AdminService`, `DoctorController`/`DoctorService`) đã được viết sẵn từ trước nhưng **chưa từng có giao diện Angular** và **chưa có tài khoản Admin nào để đăng nhập**. Đợt này bổ sung:

**Backend**
- `Program.cs`: seed 3 tài khoản demo cố định khi DB chưa có Admin nào — `admin`/`123` (Admin), `bacsi`/`123` (Bác sĩ), `benhnhan`/`123` (Bệnh nhân) — kèm sẵn 1 phân công `bacsi↔benhnhan` và 1 `HealthRecord` cơ bản cho `benhnhan` để demo được ngay luồng bác sĩ (thêm chẩn đoán/tạo đơn thuốc cần bệnh nhân đã có health-record).
- `LoginRequestValidator`: bỏ rule `.EmailAddress()` (chỉ giữ `NotEmpty`) để hỗ trợ đăng nhập bằng tên đăng nhập đơn giản như trên — **`RegisterRequestValidator` vẫn giữ nguyên rule email chuẩn** cho đăng ký công khai, không ảnh hưởng.

**Frontend — nền tảng role-based**
- `core/auth/role-routes.ts` (mới): `homeRouteForRoles()` — Admin→`/admin`, Doctor→`/doctor`, còn lại→`/dashboard`. Dùng chung ở `guestGuard`, `login.component`, và route `''`/wildcard.
- `core/auth/auth.guard.ts`: thêm `roleGuard(allowedRoles)` chặn nhầm khu vực theo role.
- `layout/sidebar/sidebar.component.ts`: nav item đổi theo role (`auth.isAdmin()`/`isDoctor()`).
- `app.routes.ts`: thêm 2 nhánh lazy-load `/admin` và `/doctor`, gated bởi `roleGuard`.

**Frontend — `features/doctor/`**: `doctor-patients-list` (danh sách bệnh nhân được gán) → `patient-profile` (4 tab: Hồ sơ sức khỏe/Lịch sử khám/Tiêm chủng/Đơn thuốc), `add-medical-history-dialog`, `doctor-prescription-dialog.component.ts` (chi tiết đơn + thêm/sửa/xoá thuốc, phỏng theo `PrescriptionDetailDialogComponent` phía bệnh nhân nhưng bỏ phần upload OCR). Service: `core/services/doctor.service.ts`. Model: `models/doctor.models.ts` (tái dùng `CreateMedicalHistoryRequest`/`CreatePrescriptionRequest`/... đã có).

**Frontend — `features/admin/`**: `admin-dashboard`, `admin-users-list` (search + filter role + đổi role/khoá-mở + `create-doctor-dialog`), `assignments-list` (+ `assign-patient-dialog`), `roles-permissions` (checkbox theo resource), `audit-logs-list`, `system-settings-list`. Service: `core/services/admin.service.ts`. Model: `models/admin.models.ts`.

**Đã kiểm thử qua API thật (curl, dữ liệu SQL thật, không mock)**: login cả 3 tài khoản, admin dashboard/users/doctor-patients/roles/settings, tạo tài khoản bác sĩ mới, khoá/mở tài khoản, đổi quyền role rồi khôi phục, bác sĩ thêm chẩn đoán + tạo đơn thuốc + thêm thuốc cho `benhnhan`, xác nhận `benhnhan` thấy đúng dữ liệu đó qua API bệnh nhân hiện có. `dotnet build` và `ng build --configuration production` đều pass không lỗi.

**Chưa kiểm thử**: click-through bằng trình duyệt thật (môi trường chạy công cụ AI dòng lệnh này không có công cụ điều khiển trình duyệt) — cần tự mở `http://localhost:4200` để xác nhận giao diện hiển thị đúng, dù API/logic phía sau đã xác nhận hoạt động đúng qua kiểm thử API.

---

## Đăng xuất về trang chủ + link "Về trang chủ" ở Login/Register (xong 2026-07-19)

- `AuthService.logout()`: đổi điều hướng sau đăng xuất từ `/auth/login` → `/` (trang chủ/landing) — áp dụng cho **mọi vai trò** (bệnh nhân/bác sĩ/admin) vì đều dùng chung `AuthService`.
- `login.component`/`register.component`: logo ở panel bên trái giờ là link về `/`; thêm link "← Về trang chủ" tường minh ở đầu form bên phải.
- Cập nhật `auth.service.spec.ts` cho khớp hành vi mới (test cũ assert điều hướng `/auth/login`, đã sửa thành `/`) — phát hiện qua `npm test` chạy lại sau khi đổi, không phải bỏ sót.

Đã build/test đầy đủ (`tsc --noEmit`, `ng build --configuration production`, `npm test` 11/11) — pass hết.

---

## Bác sĩ sửa/xoá được chẩn đoán, mũi tiêm, đơn thuốc (xong 2026-07-19)

Trước đó bác sĩ chỉ **tạo** được chẩn đoán/mũi tiêm/đơn thuốc, không sửa/xoá được nếu lỡ nhập sai. Đã bổ sung:

- Backend: `IDoctorService` thêm `UpdateMedicalHistoryAsync`/`DeleteMedicalHistoryAsync`, `UpdateVaccineAsync`/`DeleteVaccineAsync`, `DeletePrescriptionAsync` — đều chỉ là lớp delegate mỏng gọi sang `IMedicalHistoryService`/`IVaccineService`/`IPrescriptionService` đã có sẵn (dùng cho bệnh nhân), kèm `EnsureAssignedAsync` để chặn bác sĩ sửa/xoá dữ liệu bệnh nhân không thuộc mình phụ trách. 5 endpoint mới trong `DoctorController`.
- Frontend: `add-medical-history-dialog` và `add-vaccine-dialog` giờ hỗ trợ cả 2 chế độ thêm/sửa (nhận thêm `record`/`vaccine` optional qua dialog data, giống pattern `isEdit` đã dùng ở dialog phía bệnh nhân). Thêm nút sửa/xoá trên từng thẻ chẩn đoán, mũi tiêm, và nút xoá trên thẻ đơn thuốc trong `patient-profile.component`.
- Đã verify qua API thật: tạo → sửa → xoá cho cả 3 loại dữ liệu, dữ liệu cập nhật đúng; xác nhận chặn 403 khi thao tác trên bệnh nhân chưa được gán. Dọn sạch dữ liệu test sau khi xong.

Đã build/test đầy đủ (`dotnet build`, `ng build --configuration production`, `tsc --noEmit`, `npm test` 11/11) — pass hết, không phát sinh lỗi ngân sách style mới.

---

## 3 tính năng bổ sung cho khu vực Bác sĩ (xong 2026-07-19)

User phản hồi khu vực Bác sĩ (đăng nhập thật, không phải landing page) còn thiếu nhiều so với app thị trường. Đã bổ sung:

1. **Bác sĩ tự ghi được mũi tiêm cho bệnh nhân** (trước đây tab "Tiêm chủng" chỉ xem được):
   - Backend: `IDoctorService.AddVaccineAsync` (mới) — delegate sang `IVaccineService.CreateAsync` đã có sẵn, giống hệt pattern `AddMedicalHistoryAsync` (kiểm tra `EnsureAssignedAsync` trước). Endpoint mới `POST /api/doctor/patients/{patientId}/vaccines`.
   - Frontend: `add-vaccine-dialog.component` (mới, phỏng theo `vaccine-form-dialog` phía bệnh nhân nhưng bớt phần autocomplete tên vaccine theo template cho gọn), nút "Thêm mũi tiêm" trong tab Tiêm chủng của `patient-profile.component`.
   - Đã test qua API thật: bác sĩ thêm vaccine cho bệnh nhân đã gán → bệnh nhân thấy đúng dữ liệu ở trang của họ; thử thêm cho bệnh nhân chưa gán → đúng bị chặn 403.

2. **Tìm kiếm trong danh sách bệnh nhân**: thêm ô tìm theo tên/email (lọc phía client, danh sách bác sĩ thường không quá nhiều nên không cần API riêng) trong `doctor-patients-list.component`.

3. **Trang Tổng quan riêng cho Bác sĩ** (trước đây đăng nhập vào thẳng danh sách bệnh nhân, không có dashboard): `doctor-dashboard.component` (mới) — hero chào hỏi + 4 thẻ số liệu (tổng bệnh nhân, lịch hẹn hôm nay, chờ xác nhận, đã xác nhận sắp tới) + 3 khối xem nhanh (lịch hẹn hôm nay, lịch chờ xác nhận, bệnh nhân gần đây) — cùng phong cách với dashboard bệnh nhân đã có. **Không cần API mới** — chỉ ghép dữ liệu từ 2 API đã có sẵn (`/doctor/patients` + `/appointments/doctor`) tính toán phía client. Route mặc định `/doctor` giờ trỏ vào `dashboard` thay vì `patients`; sidebar bác sĩ thêm mục "Tổng quan" lên đầu.

Đã build/test đầy đủ (`dotnet build`, `ng build --configuration production`, `tsc --noEmit`, `npm test` 11/11) — pass hết, không phát sinh lỗi ngân sách style mới.

---

## Landing page — điều chỉnh sau phản hồi (xong 2026-07-19)

3 vòng chỉnh sửa nhỏ sau khi user xem trực tiếp:
1. **Navbar cố định**: đổi `position: sticky` → `fixed` (thêm `left/right:0`) để chắc chắn luôn dính trên cùng khi cuộn, kèm tăng `padding-top` của Hero để không bị navbar che nội dung.
2. **Màu sắc nhạt nhoà**: gradient Hero rõ hơn (xanh dương đậm → hồng nhạt thay vì trắng-xanh mờ), 3 thẻ vai trò + 6 ô tính năng mỗi cái có **màu icon riêng** (bind qua `[style.background]`/`[style.color]` từ field `color` trong data — giống pattern đã dùng ở admin dashboard — không tốn thêm CSS budget), blob trang trí đậm màu hơn, thẻ chỉ số trong hình minh hoạ dùng gradient thay vì màu phẳng.
3. **Footer sơ sài**: mở rộng từ 1 dòng (logo+tagline+copyright) thành bố cục nhiều cột (Thương hiệu / Sản phẩm / Tài khoản) với link thật (anchor tới section cùng trang, route đăng nhập/đăng ký) — không thêm thông tin liên hệ/mạng xã hội giả vì chưa có thật.

**Lưu ý ngân sách style**: mỗi lần thêm CSS đều phải kiểm tra lại `anyComponentStyle` budget (8kB error, xem mục tách file HTML/SCSS phía dưới) — đã cân bằng bằng cách gộp rule trùng lặp, chuyển giá trị định vị duy nhất sang inline style, và tận dụng lại class có sẵn (`.nav-links`) cho danh sách link ở footer thay vì viết rule mới.

Đã build/test lại đầy đủ sau mỗi thay đổi (`ng build`, `tsc --noEmit`, `npm test` 11/11) — đều pass.

---

## Trang giới thiệu (Landing page) cho khách chưa đăng nhập (xong 2026-07-19)

Trước đây người chưa đăng nhập bị điều hướng thẳng vào `/auth/login`, không có trang giới thiệu app nào. Đã thêm `features/landing/landing.component.ts`, gắn vào route gốc `''` (thay cho redirect thẳng `dashboard` trước đây), dùng lại `guestGuard` có sẵn — người đã đăng nhập vẫn tự động về đúng trang chủ theo vai trò, chỉ khách chưa đăng nhập mới thấy landing page.

**Nội dung** (tham khảo bố cục các app y tế phổ biến — hero, tính năng theo vai trò, cách hoạt động, CTA cuối trang):
- Nav sát trên cùng: logo + nút Đăng nhập/Đăng ký.
- Hero: tiêu đề + 2 nút CTA + minh hoạ "mock dashboard card" (tự vẽ bằng HTML/CSS/SVG, không dùng ảnh chụp thật — không có công cụ tạo/tải ảnh photo trong phiên làm việc) kèm các icon nổi (lịch hẹn, đơn thuốc, vaccine) có hiệu ứng bồng bềnh (`@keyframes float`).
- "Dành cho mọi đối tượng": 3 khối Bệnh nhân/Bác sĩ/Quản trị viên, liệt kê đúng tính năng thật đã xây (không thêm tính năng chưa có).
- "Tính năng nổi bật": 6 ô tính năng có icon.
- "Cách hoạt động": 3 bước.
- CTA band cuối trang + footer.
- Hiệu ứng: fade-in khi cuộn tới (IntersectionObserver, không cần thư viện animation ngoài), hover nổi khối, gradient nền.

**Chủ đích tránh nội dung sai sự thật**: không đưa số liệu kiểu "10.000+ người dùng" hay đánh giá/testimonial giả — vì đây có thể trở thành nội dung marketing thật khi app lên production, chỉ mô tả tính năng thực tế app đang có.

**Lưu ý kỹ thuật**: `landing.component.scss` ban đầu vượt ngân sách style/component (`anyComponentStyle` error tại 8kB, cấu hình sẵn trong `angular.json`) — đã tối ưu bằng cách gộp rule CSS trùng lặp (role-card/feature-tile dùng chung shell) và chuyển các giá trị định vị duy nhất (vị trí blob/badge nổi, độ rộng mock-line) sang `style` inline trong template thay vì tạo class CSS riêng cho từng cái — không đổi cấu hình ngân sách chung của dự án.

Đã build (`ng build --configuration production`, `tsc --noEmit`) và `npm test` (11/11) đều pass sau khi thêm.

---

## 3 tính năng bổ sung cho khu vực Admin (xong 2026-07-19)

User phản hồi khu vực Admin còn thiếu so với app thị trường. Đã bổ sung:

1. **Admin tạo được tài khoản bệnh nhân** (trước đây `POST /api/admin/doctors` chỉ tạo được bác sĩ, hardcode role Doctor):
   - Backend: `CreateDoctorRequest` thêm field `RoleId` (2=Bác sĩ hoặc 3=Bệnh nhân, validate bằng `CreateDoctorRequestValidator`); `AdminService.CreateDoctorAsync` dùng `request.RoleId` thay vì hardcode 2. Đổi tên thông báo thành công từ "Tạo tài khoản bác sĩ thành công." → "Tạo tài khoản thành công." (dùng chung cho cả 2 loại).
   - Frontend: đổi `create-doctor-dialog` → `create-user-dialog` (thêm `mat-select` chọn vai trò Bác sĩ/Bệnh nhân), đổi tên nút "Tạo tài khoản bác sĩ" → "Tạo tài khoản mới" trong `admin-users-list`.
   - Đã test qua API thật: tạo tài khoản bệnh nhân mới với `roleId:3` → login thành công, role trả về đúng "User"; thử `roleId:1` (Admin) → đúng bị chặn với thông báo lỗi rõ ràng.

2. **Admin đặt lại mật khẩu cho người dùng** (trước đây không có cách nào giúp người dùng quên mật khẩu ngoài tự đổi khi đã đăng nhập):
   - Backend: `ResetPasswordRequest`/`ResetPasswordRequestValidator` (mới, cùng rule độ mạnh mật khẩu như tạo tài khoản), `AdminService.ResetPasswordAsync`, endpoint `PUT /api/admin/users/{id}/reset-password`.
   - Frontend: `reset-password-dialog.component` (mới) — hiện tên người dùng đích, 1 ô mật khẩu mới; nút icon "Đặt lại mật khẩu" (khoá) trên từng dòng trong `admin-users-list`.
   - Đã test qua API thật: reset mật khẩu tài khoản `benhnhan2` → mật khẩu cũ bị từ chối, mật khẩu mới đăng nhập được.

3. **Admin xem được lịch hẹn toàn hệ thống** (trước đây chỉ bác sĩ/bệnh nhân xem lịch hẹn của riêng mình, Admin không có cái nhìn tổng thể):
   - Backend: `AdminService.GetAllAppointmentsAsync` (phân trang + lọc theo `AppointmentStatus`, join tên bác sĩ/bệnh nhân qua `_uow.Users` giống pattern `GetDoctorPatientsAsync`), endpoint `GET /api/admin/appointments`.
   - Frontend: `admin-appointments-list.component` (mới) — tab lọc theo trạng thái (Tất cả/Chờ xác nhận/Đã xác nhận/Hoàn thành/Đã huỷ), bảng ngày giờ/bác sĩ/bệnh nhân/lý do/trạng thái, phân trang. Thêm mục "Lịch hẹn khám" vào sidebar Admin.
   - Đã test qua API thật: lấy danh sách không lọc (5 lịch hẹn, đúng tên bác sĩ/bệnh nhân) và lọc `status=Pending` (đúng còn 1 kết quả).

Đã dọn dữ liệu test (khoá tài khoản `testpatient@healthplus.local` tạo lúc kiểm thử, đặt lại mật khẩu `benhnhan2` về giá trị hợp lệ khác do rule mật khẩu mạnh không cho phép "123").

Đã build/test đầy đủ (`dotnet build`, `ng build --configuration production`, `tsc --noEmit`, `npm test` 11/11) — pass hết, không phát sinh lỗi ngân sách style mới.

---

## Biểu đồ cột "Số bệnh nhân theo từng bác sĩ" (xong 2026-07-19)

Thêm biểu đồ thứ 2 vào Admin Dashboard (nằm cạnh biểu đồ tròn, `.charts-row` grid tự xuống hàng khi màn hẹp), lấy dữ liệu từ `/admin/doctor-patients` đã có sẵn (không cần API mới), tự nhóm theo `doctorName` phía client và đếm số bệnh nhân active. Biểu đồ cột ngang (`indexAxis: 'y'`), sắp xếp giảm dần theo số lượng.

Theo dataviz skill: đây là biểu đồ **1 chuỗi dữ liệu** (patients count) nên chỉ dùng **1 màu duy nhất** (xanh dương, không phải nhiều màu categorical) — màu không mang ý nghĩa xếp hạng, và không cần legend riêng vì trục Y đã ghi rõ tên bác sĩ. Có xử lý trạng thái rỗng (chưa bác sĩ nào có bệnh nhân) thay vì để trống trơn.

---

## Biểu đồ tròn phân bổ vai trò ở Admin Dashboard (xong 2026-07-19)

Thêm 1 biểu đồ donut nhỏ vào `admin-dashboard.component.ts` bên dưới 5 thẻ số liệu, thể hiện tỉ lệ Quản trị viên/Bác sĩ/Bệnh nhân (tính từ `AdminDashboardStats` đã có sẵn, không cần API mới). Dùng lại `chart.js`/`ng2-charts` đã có sẵn trong project (đúng thư viện đang dùng ở biểu đồ chỉ số sức khỏe), không thêm dependency mới.

Màu 3 lát cắt được chọn và **validate qua script của dataviz skill** (`validate_palette.js`) thay vì chọn theo cảm tính — bộ màu xanh dương/xanh lá/hồng magenta đạt ngưỡng phân biệt màu cho người mù màu (CVD ΔE ≥ 8) và độ tương phản bình thường (≥15); riêng lát magenta dưới 3:1 tương phản trên nền trắng nên đã thêm số liệu trực tiếp vào legend (vd. "Bệnh nhân (12)") thay vì chỉ dựa vào màu — đúng yêu cầu "relief" của bộ quy tắc màu.

---

## Dữ liệu mẫu đa dạng hơn cho demo (xong 2026-07-19)

Thêm 1 khối seed thứ 2 trong `Program.cs` (độc lập, tự kiểm tra qua tài khoản `bacsi2` để không chạy trùng lặp nếu restart nhiều lần):

- **Thêm tài khoản**: `bacsi2`/`123` (BS. Trần Thị Hương — Nhi khoa), `bacsi3`/`123` (BS. Lê Văn Minh — Tim mạch), `benhnhan2`/`123` (Nguyễn Thị Lan) — `benhnhan2` được gán cho `bacsi2`; `benhnhan` được gán thêm `bacsi2` (ngoài `bacsi` đã có) để demo 1 bệnh nhân có nhiều bác sĩ.
- **benhnhan** có thêm: 1 hồ sơ "Con trai" (bên cạnh "Bản thân"), tổng 3 lịch sử khám (hospital/chuyên khoa/chẩn đoán khác nhau), 3 vaccine đủ 3 trạng thái (Completed/Scheduled/Overdue), 1 đơn thuốc mới đã hoàn thành (2 thuốc), 3 lịch hẹn thêm (Pending/Confirmed/Cancelled) với 3 bác sĩ khác nhau, 3 nhắc nhở đủ loại (Medicine/Vaccine/FollowUp).
- **benhnhan2** có hồ sơ sức khỏe riêng (tăng huyết áp mạn tính) để đa dạng dữ liệu cho admin/bác sĩ xem.

Đã verify qua API thật: `admin/dashboard` giờ báo 17 users/4 doctors/12 patients/3 assignments; `bacsi2` thấy đúng 2 bệnh nhân trong "Bệnh nhân của tôi"; toàn bộ health-records/medical-history/vaccines/prescriptions/appointments/reminders của `benhnhan` trả về đúng dữ liệu đa dạng như thiết kế.

---

## Fix: 3 lỗi giao diện phát hiện khi user test trực tiếp (xong 2026-07-19)

Sau khi build xong, user tự test trên trình duyệt thật và phát hiện thêm 3 lỗi hiển thị (không tự thấy được vì môi trường dòng lệnh hiện tại không có trình duyệt):

1. **Ô tìm kiếm/lọc màu xám khó nhìn**: input/select kiểu `appearance="outline"` (dùng cho toàn bộ ô tìm kiếm/lọc) vốn trong suốt theo thiết kế Material — đặt trực tiếp lên nền trang (hơi xám-xanh) nên nhìn như bị chìm. Đã ép nền trắng rõ ràng cho `.mdc-text-field--outlined` trong `styles.scss`.

2. **Nút xanh dương (nút "primary") chữ và nền cùng màu**: rule có sẵn từ trước `.mat-mdc-raised-button.mat-primary { background-color: var(--blue-800) !important; }` chỉ ép nền, quên đổi màu chữ — mà theo mặc định M3, chữ trên loại nút này lại tự động là `--mat-sys-primary` (cũng xanh dương) → chữ xanh trên nền xanh, không đọc được. Đã thêm `color: #FFFFFF !important` vào cùng rule.

3. **Icon hiện chữ thô ("add" bị cắt còn "ad", v.v.)**: `index.html` load font Material Icons qua Google Fonts CDN (`fonts.googleapis.com`/`fonts.gstatic.com`) — máy dev ARM64 chạy Parallels tải font này không ổn định (curl từ trong VM tải được nhưng trình duyệt thật thì không, khả năng do proxy/adblock/mạng khác nhau giữa 2 tiến trình), khiến `<mat-icon>` hiện chữ ligature gốc ("add", "close"...) thay vì icon, bị cắt bởi khung cố định 24x24px của icon.
   - **Đã tự host font Material Icons** thay vì phụ thuộc CDN — tải sẵn `material-icons.woff2` (128KB) về `public/fonts/material-icons.woff2` (thư mục `public/` là static assets theo cấu hình `angular.json` hiện tại, không phải `src/assets`), khai báo `@font-face` trực tiếp trong `styles.scss`, xoá link Google Fonts icon khỏi `index.html` (giữ lại link Roboto cho chữ thường vì đã có fallback `sans-serif` an toàn, không critical bằng icon).
   - Icon set duy nhất đang dùng trong app là "Material Icons" (filled) — không dùng "Material Icons Outlined" nên chỉ cần host 1 file font.

Cả 3 fix đều đã build lại (`ng build --configuration production` pass) và verify trực tiếp trong CSS/output đã build ra (đúng giá trị, đúng đường dẫn font, dev server trả về `HTTP 200 font/woff2` khi gọi `/fonts/material-icons.woff2`). Dev server tự áp dụng qua hot-reload — user xác nhận trực tiếp trên trình duyệt của mình.

---

## Fix: khung/form/dialog bị "chìm" vào màu nền thay vì trắng (xong 2026-07-19)

**Nguyên nhân gốc** (không liên quan gì tới việc tách file HTML/SCSS ở trên): `src/styles.scss` cấu hình `@include mat.theme((color: (primary: $hp-primary, secondary: $hp-primary, tertiary: $hp-primary, error: $hp-error), ...))` nhưng **không truyền palette "neutral"** — hậu quả là Angular Material không tính được hầu hết các token hệ thống phái sinh từ neutral/secondary/error (`--mat-sys-surface*`, `--mat-sys-background`, `--mat-sys-outline`, `--mat-sys-secondary*`, `--mat-sys-error*`...), để lại giá trị **rỗng** (`light-dark(, )`). Riêng `primary`/`tertiary` thì tính đúng vì dùng trực tiếp palette được truyền vào.

Hậu quả: mọi `background-color: var(--mat-sys-surface...)` (dùng bởi `mat-card`, `mat-dialog`, `mat-menu`, `mat-select` panel...) nhận giá trị invalid → trình duyệt bỏ qua khai báo → `background-color` fallback về `transparent` mặc định → khung/form/dialog **trong suốt**, để lộ màu nền trang phía sau, nhìn như "chìm" vào nền thay vì có nền trắng riêng.

**Cách fix**: thêm 1 khối ghi đè trực tiếp ~30 CSS custom property `--mat-sys-*` bị rỗng ngay trong `html { }` (sau `@include mat.theme(...)`, để thắng theo thứ tự khai báo cùng selector) — surface/container các loại → trắng hoặc gần trắng, secondary/error → map lại đúng theo bảng màu xanh/đỏ sẵn có của app. Đồng thời thêm ghi đè tường minh cho `.mat-mdc-card`, `.mat-mdc-dialog-surface`/`.mdc-dialog__surface`, `.mat-mdc-menu-panel`, `.mat-mdc-select-panel` để chắc chắn có nền trắng dù sau này theme có thay đổi.

Đã verify: `ng build --configuration production` pass, kiểm tra trực tiếp trong CSS đã build ra thấy `--mat-sys-surface`, `--mat-sys-background`, `--mat-sys-outline`... giờ có giá trị hex thật thay vì `light-dark(, )` rỗng. **Chưa xác nhận bằng mắt trên trình duyệt thật** (môi trường dòng lệnh hiện tại không có công cụ browser) — cần user tự mở app kiểm tra lại các trang có `mat-card`/dialog xem đã có nền trắng rõ ràng chưa.

---

## Tách file HTML/SCSS riêng cho mọi component (xong 2026-07-19)

Trước đó **toàn bộ component Angular đều viết inline** (`template: \`...\`` và `styles: [\`...\`]` ngay trong file `.ts`, không có `.html`/`.scss` riêng) — đây là convention có sẵn từ đầu dự án, không phải lỗi. Theo yêu cầu của user, đã tách toàn bộ ~40 component (35 file `.ts`, kể cả 3 file có nhiều component trong 1 file: `prescriptions-list.component.ts`, `reminders-list.component.ts`, `doctor-prescription-dialog.component.ts`) sang đúng 1 file `.ts` + 1 `.html` + 1 `.scss` mỗi component, dùng `templateUrl`/`styleUrl` thay vì `template`/`styles` inline.

Thực hiện bằng script Node dùng `ts-morph` (cài tạm thời qua `npm install --no-save`, đã gỡ sạch sau khi xong — không còn trong `package.json`/`node_modules`) để parse AST chính xác thay vì regex, tránh vỡ template có ký tự đặc biệt/backtick. Với file nhiều component, tên file `.html`/`.scss` mới đặt theo tên class (kebab-case) thay vì tên file `.ts` gốc — vd `prescriptions-list.component.ts` chứa `ItemFormDialogComponent`, `PrescriptionDetailDialogComponent`, `PrescriptionsListComponent` → sinh ra `item-form-dialog.component.html/scss`, `prescription-detail-dialog.component.html/scss`, `prescriptions-list.component.html/scss` (đặt cùng thư mục với file `.ts` gốc).

Đã kiểm chứng an toàn: backup toàn bộ `src/app` trước khi chạy script (xoá sau khi xác nhận ổn), `tsc --noEmit` + `ng build --configuration production` + `npm test` (11/11 test) đều pass sau khi tách, không phát sinh warning/lỗi mới ngoài các warning ngân sách CSS đã có từ trước (chỉ đổi từ báo ở style inline sang báo ở file `.scss` mới, không phải regression).

---

## Lịch hẹn khám / Appointment (mới hoàn toàn, xong 2026-07-19)

Trước đợt này, **toàn bộ hệ thống không có tính năng đặt lịch hẹn khám nào** (chỉ có "Reminder" — nhắc nhở cá nhân, không phải lịch hẹn với bác sĩ). Đã bổ sung từ đầu:

**Backend**
- Entity mới `Appointment` (`HealthPlus.Domain/Entities/Appointment.cs`): `DoctorId`, `PatientId`, `AppointmentTime`, `Reason`, `Status` (enum `AppointmentStatus`: Pending/Confirmed/Completed/Cancelled), `Notes`.
- Migration `AddAppointments` (`HealthPlus.Persistence/Migrations/20260719145949_AddAppointments.cs`) — chỉ tạo bảng mới, không đụng dữ liệu cũ.
  - **Lưu ý khi tạo migration mới sau này trên máy ARM64 này**: `dotnet ef migrations add` sẽ báo lỗi `FileNotFoundException` do `HealthPlus.csproj` có `RuntimeIdentifier=win-x64` (xem mục OCR ARM64 phía trên) khiến EF tooling tìm sai thư mục output. Dự án đã có sẵn escape hatch: chạy với biến môi trường `SkipWinX64Rid=true dotnet ef migrations add ...` (xem comment trong `HealthPlus.csproj`).
- `IAppointmentService`/`AppointmentService`, `AppointmentsController` (`/api/appointments`):
  - `GET /doctors` — danh sách bác sĩ đang hoạt động (để bệnh nhân chọn khi đặt lịch, không giới hạn role).
  - `GET /` , `POST /`, `PUT /{id}/cancel` — bệnh nhân xem/đặt/huỷ lịch hẹn của chính mình.
  - `GET /doctor`, `PUT /{id}/status` (`[Authorize(Roles="Doctor")]`) — bác sĩ xem lịch hẹn của mình, xác nhận/từ chối/hoàn thành.
  - Không có validator FluentValidation riêng (theo đúng convention hiện có — nhiều DTO create request khác trong dự án cũng không có validator riêng), chỉ validate nghiệp vụ trong service (bác sĩ phải tồn tại/đang hoạt động/đúng role; chỉ chủ sở hữu mới huỷ/cập nhật được; không huỷ được lịch đã Completed/Cancelled).

**Frontend**
- `models/appointment.models.ts`, `core/services/appointment.service.ts`.
- **Bệnh nhân** (`features/appointments/`, route `/appointments`, đã thêm vào sidebar patient): `appointments-list` (tab theo trạng thái, giống UI `prescriptions-list`) + `book-appointment-dialog` (chọn bác sĩ từ danh sách + ngày giờ + lý do).
- **Bác sĩ** (`features/doctor/doctor-appointments-list/`, route `/doctor/appointments`, đã thêm vào sidebar bác sĩ): danh sách lịch hẹn theo trạng thái, nút Xác nhận/Từ chối cho lịch Pending, Hoàn thành/Huỷ cho lịch Confirmed.

**Đã kiểm thử qua API thật (dữ liệu SQL thật)**: `benhnhan` đặt lịch với `bacsi` → `bacsi` xác nhận → đánh dấu hoàn thành → `benhnhan` xác nhận thấy đúng trạng thái cuối. Đã test thêm các trường hợp biên: bệnh nhân huỷ lịch của chính mình (Pending/Confirmed), bệnh nhân gọi endpoint riêng của bác sĩ → đúng 403, đặt lịch với người không phải bác sĩ → đúng bị chặn, huỷ lịch đã Completed → đúng bị chặn (409). `dotnet build` và `ng build --configuration production` đều pass.

---

## ĐÃ LÀM XONG

### Backend (100%)
- **Domain**: 21 entities, 7 enums, IGenericRepository, IUnitOfWork
- **Application**: 7 services (Auth, HealthRecord, User, Prescription, Reminder, Vaccine, MedicalHistory), DTOs đầy đủ, FluentValidation, ApiResponse, PagedResult
- **Persistence**: AppDbContext, GenericRepository, UnitOfWork, EF Config cho 21 entities, Migration InitialCreate
- **Infrastructure**: JwtService, PasswordService, LocalFileStorageService, ReminderBackgroundService, NotificationService (dispatch theo Channel) ← **cập nhật 2026-07-13**: SmtpEmailSender (MailKit), FirebasePushNotificationSender (FirebaseAdmin SDK), TwilioSmsSender (Twilio SDK) — thay cho LogNotificationService cũ chỉ log console; TesseractOcrService (OCR offline, xem mục riêng bên dưới)
- **API Controllers**: Auth, HealthRecords, Users, Prescriptions, Reminders, Vaccines, MedicalHistory, BaseApiController ← **cập nhật 2026-07-13**: UsersController thêm `POST /me/avatar` (trước đây `IUserService.UpdateAvatarAsync` có sẵn nhưng không có endpoint gọi tới); thêm `ChangePasswordRequestValidator` (trước đây đổi mật khẩu không validate độ mạnh mật khẩu mới)
- **Middleware**: ExceptionHandlingMiddleware, KebabCaseControllerConvention
- **Config**: JWT, CORS (localhost:4200), Swagger Bearer, Serilog, port 5146

### Angular Frontend (100%)

#### Core
- AuthService (signal-based), TokenService, AuthGuard, AuthInterceptor
- HealthRecordService, MedicalHistoryService, VaccineService
- ReminderService ← **cập nhật**: thêm getById, update, getSettings, updateSettings
- PushNotificationService ← **mới (2026-07-13)**: xin quyền trình duyệt + lấy FCM token qua Firebase JS SDK (`firebase` npm package), lắng nghe message foreground
- PrescriptionService ← **mới**: getAll, getById, create, delete, addItem, updateItem, deleteItem, uploadImage

#### Models
- auth.models.ts, api.models.ts, health-record.models.ts
- medical-history.models.ts, vaccine.models.ts
- reminder.models.ts ← **cập nhật**: thêm UpdateReminderRequest, NotificationSetting, UpdateNotificationSettingRequest
- prescription.models.ts ← **mới**: Prescription, PrescriptionItem, CreatePrescriptionRequest, CreatePrescriptionItemRequest, UpdatePrescriptionItemRequest

#### Feature Pages
| Feature | File chính | Nội dung |
|---|---|---|
| Auth | features/auth/ | Login, Register |
| Dashboard | features/dashboard/ | Stats, preview records, follow-ups, overdue vaccines, upcoming reminders |
| Health Records | features/health-records/ | CRUD, filter, metrics chart, BMI |
| Medical History | features/medical-history/ | CRUD, follow-up tracking, upload document |
| Vaccines | features/vaccines/ | CRUD, overdue banner, schedule templates, filter theo profile/status |
| **Prescriptions** | features/prescriptions/ | List dạng card, filter theo status, tạo đơn, xoá đơn; Detail dialog: xem ảnh, upload ảnh OCR (kết quả hiện ngay trong dialog, không cần tải lại trang), thêm/sửa/xoá từng thuốc |
| **Reminders** | features/reminders/ | List dạng row, filter theo trạng thái + loại; Tạo/sửa qua dialog; Toggle bật/tắt inline; Upcoming banner 24h; Dialog cài đặt thông báo (channel, loại, giờ không làm phiền) |
| **Profile** | features/profile/ | Sửa họ tên/SĐT, đổi avatar (click vào ảnh), đổi mật khẩu (validate khớp mật khẩu xác nhận), xem trạng thái email/ngày tạo TK. Truy cập qua menu avatar ở header (`/profile`) |

---

## CHƯA LÀM

### Ưu tiên cao
(không còn mục nào)

### Ưu tiên thấp
(không còn mục nào — toàn bộ tính năng đã lên kế hoạch đều xong)

---

## Cấu trúc thư mục tham chiếu

```
C:\HealthPlus\
├── HealthPlus\                        # Backend solution
│   ├── HealthPlus.Domain\
│   ├── HealthPlus.Application\
│   ├── HealthPlus.Persistence\
│   ├── HealthPlus.Infrastructure\
│   └── HealthPlus\                    # ASP.NET Web API (port 5146)
└── HealthPlusClient\
    └── src\app\
        ├── core\services\             # auth, health-record, medical-history, vaccine, reminder, prescription
        ├── models\                    # TS interfaces
        ├── features\
        │   ├── auth\                  ✅
        │   ├── dashboard\             ✅
        │   ├── health-records\        ✅
        │   ├── medical-history\       ✅
        │   ├── vaccines\              ✅
        │   ├── prescriptions\         ✅
        │   └── reminders\             ✅
        └── layout\
```

---

## Cách chạy

```bash
# Backend
cd C:\HealthPlus\HealthPlus\HealthPlus
dotnet run --launch-profile http
# → http://localhost:5146
# → Swagger: http://localhost:5146/swagger

# Frontend
cd C:\HealthPlus\HealthPlusClient
npm start
# → http://localhost:4200

# Backend tests
cd C:\HealthPlus\HealthPlus
dotnet test HealthPlus.Tests

# Frontend tests
cd C:\HealthPlus\HealthPlusClient
npm test
```

---

## Unit Tests (xong 2026-07-13)

**Backend — `HealthPlus.Tests` (xUnit + Moq), 53 test, chạy `dotnet test` từ `C:\HealthPlus\HealthPlus`:**
- `Common/PrescriptionOcrParserTests.cs` — test kỹ nhất vì đây là logic tự viết dễ vỡ nhất: parse nhiều dòng đúng, skip header/metadata (họ tên, ngày sinh...), nhận diện các đơn vị liều dùng, và **regression test cho bug thật đã tìm thấy khi test tay** (mẫu `"x2/ngày"` từng để sót `/ngay` dính vào tên thuốc)
- `Validators/` — RegisterRequestValidator, LoginRequestValidator, ChangePasswordRequestValidator (validator mới thêm cùng đợt Profile page)
- `Services/PasswordServiceTests.cs` — BCrypt hash/verify roundtrip, sai mật khẩu, và test 2 lần hash cùng 1 mật khẩu phải ra hash khác nhau (chống ai đó lỡ tối ưu thành hàm hash không salt)
- `Services/UserServiceTests.cs` — mock `IUnitOfWork`/`IPasswordService` bằng Moq: GetById/UpdateProfile/ChangePassword (cả 2 nhánh lỗi: user không tồn tại, sai mật khẩu hiện tại)/UpdateAvatar
- `Services/TwilioSmsSenderTests.cs` — test `ToE164()` (chuyển số nội địa VN `"091..."` sang chuẩn quốc tế `"+8491..."` mà Twilio yêu cầu); method đổi từ `private` sang `internal` + `InternalsVisibleTo("HealthPlus.Tests")` trong `HealthPlus.Infrastructure/AssemblyInfo.cs` để test được mà không cần expose ra public API
- **Không dùng FluentAssertions** (v8+ đổi sang giấy phép trả phí cho dùng thương mại) — chỉ dùng `Assert` có sẵn của xUnit, hoàn toàn miễn phí

**Frontend — Vitest (builder `@angular/build:unit-test` có sẵn từ `ng new`), 11 test, chạy `npm test` từ `HealthPlusClient`:**
- `app.spec.ts` — đã sửa test cũ bị hỏng từ lúc scaffold (assert `<h1>Hello, HealthPlusClient</h1>` không tồn tại nữa vì `App` giờ chỉ render `<router-outlet>`) — đây là lý do CI/test suite trước đây coi như "chưa từng chạy được"
- `core/auth/token.service.spec.ts` — roundtrip localStorage: save/get/clear/updateUser
- `core/auth/auth.service.spec.ts` — mock HttpClient qua `HttpTestingController`: login cập nhật signal + lưu token, logout xoá state + điều hướng `/auth/login`, updateUser merge đúng và không tự chạy khi chưa đăng nhập

**Chưa test** (biết trước, không phải thiếu sót): các component UI (dialog, form) và các service HTTP thuần một-dòng (Prescription/Reminder/Vaccine/User service — chỉ wrap `http.get/post`, giá trị test thấp). Có thể bổ sung sau nếu cần coverage cao hơn, nhưng phần lõi có logic thật (parser, validator, business rule đổi mật khẩu, auth state) đã được test.

---

## Cấu hình Notification thật (Email + Push + SMS)

Chưa điền credentials thì app vẫn chạy bình thường — `SmtpEmailSender`/`FirebasePushNotificationSender`/`TwilioSmsSender` đều tự phát hiện chưa cấu hình và log cảnh báo thay vì crash.

### Email (SMTP qua Gmail)
1. Bật xác minh 2 bước cho Gmail: https://myaccount.google.com/security
2. Tạo **App Password**: https://myaccount.google.com/apppasswords → chọn app "Mail" → copy mật khẩu 16 ký tự
3. Điền vào `HealthPlus\HealthPlus\appsettings.Development.json` (không commit mật khẩu thật vào `appsettings.json` chung):
```json
"Email": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "UseSsl": true,
  "SenderEmail": "your@gmail.com",
  "SenderName": "HealthPlus",
  "Username": "your@gmail.com",
  "Password": "app-password-16-ky-tu"
}
```

### Push (Firebase Cloud Messaging)
1. Tạo project tại https://console.firebase.google.com
2. Project Settings → Service Accounts → **Generate new private key** → tải file JSON
3. Đặt file vào `HealthPlus\HealthPlus\firebase-service-account.json` (đúng path mặc định trong `appsettings.json` → `Firebase:CredentialsPath`)
4. Điền **Web app config** của project đó (Project Settings → General → Your apps → Web app, không phải service account JSON ở bước 2) vào 2 nơi:
   - `HealthPlusClient\src\environments\environment.ts` và `environment.prod.ts` → `firebaseConfig` (apiKey, authDomain, projectId, storageBucket, messagingSenderId, appId)
   - `HealthPlusClient\public\firebase-messaging-sw.js` → object `firebase.initializeApp({...})` (phải giống hệt `firebaseConfig` ở trên vì service worker không import được `environment.ts`)
5. Tạo **Web Push certificate** (VAPID key): Project Settings → Cloud Messaging → Web configuration → Generate key pair → điền vào `fcmVapidKey` trong `environment.ts`/`environment.prod.ts`

Phía Angular đã tích hợp xong (2026-07-13): `core/services/push-notification.service.ts` xin quyền trình duyệt + lấy `FcmToken` khi user bật toggle "Push notification" trong dialog Cài đặt thông báo (`features/reminders`), tự gửi lên `PUT /api/notifications/settings`. `App` component lắng nghe thông báo khi app đang mở (foreground); `public/firebase-messaging-sw.js` xử lý khi app ở background/đóng tab.

### SMS (Twilio) — xong 2026-07-13
1. Đăng ký tài khoản tại https://www.twilio.com/try-twilio (có gói dùng thử miễn phí kèm số credit, nhưng tài khoản trial chỉ gửi được tới số điện thoại đã verify thủ công trong Twilio Console — muốn gửi tự do cần nâng cấp tài khoản trả phí)
2. Console Twilio → **Account SID** và **Auth Token** ở trang Dashboard chính
3. Mua/lấy 1 số điện thoại gửi SMS: Phone Numbers → Buy a number (tài khoản trial có sẵn 1 số dùng thử miễn phí)
4. Điền vào `HealthPlus\HealthPlus\appsettings.Development.json`:
```json
"Twilio": {
  "AccountSid": "ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "AuthToken": "your-auth-token",
  "FromPhoneNumber": "+1xxxxxxxxxx"
}
```

**Lưu ý format số điện thoại**: DB lưu số nội địa VN (`RegisterRequestValidator` bắt buộc `^[0-9]{10,11}$`, vd `"0912345678"`), còn Twilio cần chuẩn quốc tế E.164 (`"+84912345678"`). `TwilioSmsSender.ToE164()` tự động chuyển đổi (thay số 0 đầu bằng `+84`) — đã unit test kỹ (xem mục Unit Tests).

**Lưu ý package version**: cài Twilio SDK phát sinh xung đột — Twilio yêu cầu `System.IdentityModel.Tokens.Jwt >= 8.3.1`, trong khi `HealthPlus.Infrastructure.csproj` đang ghim ở `7.1.2` (lịch sử: từng cố tình hạ xuống để né 1 xung đột khác với `JwtBearer`, xem mục "Bug fixes đáng nhớ"). Đã nâng lên `8.3.1`, build + toàn bộ test (kể cả auth/JWT) đều pass — không phát hiện breaking change. Nếu sau này auth có lỗi lạ liên quan JWT, đây là chỗ đầu tiên cần nghi ngờ.

### Lưu ý bảo mật
- File `firebase-service-account.json`, mật khẩu SMTP, và Twilio Auth Token thật **không nên** để trong `appsettings.json` (sẽ commit vào git nếu sau này init repo) — dùng `appsettings.Development.json` (đã gitignore theo convention ASP.NET) hoặc User Secrets (`dotnet user-secrets`).

---

## OCR đơn thuốc (xong 2026-07-13)

**Cách hoạt động**: Upload ảnh → `TesseractOcrService` (package `Tesseract` 5.2.0, chạy offline, không cần API key/tài khoản) đọc text (eng+vie) → `PrescriptionOcrParser` (`HealthPlus.Application/Common/`) tách từng dòng thành 1 thuốc bằng regex (nhận diện `mg/ml/g/viên/gói`, `xN/ngày`, `N ngày`) → tạo `PrescriptionItem` với `IsConfirmed=false` → `Prescription.Status=Completed`. Toàn bộ chạy đồng bộ ngay trong request upload-image (ảnh test 3 dòng thuốc mất ~1-2 giây, chấp nhận được).

**⚠️ QUAN TRỌNG — máy dev ARM64**: Thư viện native của Tesseract chỉ có bản x86/x64, không có bản ARM64. Máy dev hiện tại là **Windows ARM64**, nên `HealthPlus.csproj` (project API) đã được set `<RuntimeIdentifier>win-x64</RuntimeIdentifier>` + `<SelfContained>false</SelfContained>` để ép chạy qua lớp giả lập x64 có sẵn của Windows — nếu không, gọi `TesseractEngine` sẽ throw `DllNotFoundException` (Win32 error 193 — sai định dạng kiến trúc). Nếu deploy lên máy Windows x64 thật thì không cần lo, dòng RID này vẫn chạy bình thường. Nếu build lỗi/OCR không hoạt động sau này, kiểm tra lại dòng này trước.

**Độ chính xác — heuristic, không phải NLP thật**:
- Hoạt động tốt với đơn thuốc **in/đánh máy rõ ràng** (đã test end-to-end qua API thật: đọc đúng 100% tên thuốc/liều/số lần/số ngày, confidence ~92%)
- Đơn thuốc **viết tay** (thực tế phổ biến ở VN) sẽ có độ chính xác thấp hơn nhiều — Tesseract không phải model chuyên nhận diện chữ viết tay
- Parser chỉ regex đơn giản, không hiểu ngữ nghĩa — người dùng luôn cần xem lại/sửa qua UI thêm-sửa-xoá thuốc đã có sẵn (đây là lý do `IsConfirmed=false` mặc định)

**File liên quan**:
- `HealthPlus.Application/Interfaces/IOcrService.cs`, `HealthPlus.Application/Common/PrescriptionOcrParser.cs`
- `HealthPlus.Infrastructure/Services/TesseractOcrService.cs`
- `HealthPlus/HealthPlus/tessdata/` — chứa `eng.traineddata` + `vie.traineddata` (tải từ github.com/tesseract-ocr/tessdata_fast, ~4.6MB, copy vào output khi build)
- `PrescriptionService.UploadImageAsync` (Application) — orchestrate toàn bộ flow, controller giờ chỉ gọi service (đã dọn logic ra khỏi `PrescriptionsController` như code cũ)
- Frontend: `PrescriptionDetailDialogComponent.onFileSelected()` giờ nhận full `PrescriptionResponse` sau upload và hiển thị ngay trong dialog (trước đây chỉ nhận `{imageUrl, status}` rồi đóng dialog)

---

## User Profile page (xong 2026-07-13)

**Route**: `/profile` (`features/profile/profile.component.ts`) — truy cập qua menu avatar ở góc phải header (link `routerLink="/profile"` đã có sẵn từ trước nhưng route chưa tồn tại, giờ đã nối).

**3 khối chức năng trong 1 trang**:
1. Sửa họ tên + số điện thoại (email chỉ đọc), avatar click-to-upload (JPG/PNG, tối đa 2MB)
2. Đổi mật khẩu — có validate khớp mật khẩu xác nhận phía client, và validate độ mạnh (≥8 ký tự, có hoa + số) cả 2 phía
3. Thông tin chỉ đọc: trạng thái xác thực email, lần đăng nhập gần nhất, ngày tạo tài khoản

**Backend bổ sung** (trước đây thiếu):
- `POST /api/users/me/avatar` — nhận `IFormFile avatar`, lưu qua `IFileStorageService` vào `uploads/avatars/`, gọi `IUserService.UpdateAvatarAsync` (hàm này đã có sẵn nhưng mồ côi, không có route nào gọi tới)
- `ChangePasswordRequestValidator` (FluentValidation) — đổi mật khẩu trước đây không kiểm tra độ mạnh mật khẩu mới, giờ áp cùng rule với đăng ký (≥8 ký tự, 1 hoa, 1 số, khác mật khẩu cũ)

**Đồng bộ trạng thái sau khi đổi**: `AuthService.updateUser(patch)` (mới) cập nhật signal `_user` + `TokenService.updateUser()` ghi lại localStorage, để header/sidebar hiện tên/avatar mới ngay lập tức không cần reload. Header và sidebar cũng đã sửa để hiện `<img>` avatar thật nếu có, thay vì luôn hiện chữ cái đầu tên.

**Đã test end-to-end qua API thật** (không chỉ build): GetMe, UpdateProfile, ChangePassword (cả 2 trường hợp lỗi: mật khẩu yếu bị validator chặn đúng thông báo, sai mật khẩu hiện tại bị chặn — và trường hợp đúng), UploadAvatar (file lưu đúng vào `uploads/avatars/`, `avatarUrl` trả về và persist đúng qua GetMe lần 2). Đã dọn dữ liệu test sau khi xong.

---

## Docker / deploy config (xong 2026-07-13)

**⚠️ Chưa từng chạy `docker compose up` thật.** Máy dev là VM Windows ARM64 chạy trong Parallels Desktop trên MacBook Pro M1. Docker Desktop đã cài (qua winget), nhưng Docker cần WSL2, và WSL2 báo lỗi "virtualization is not enabled" — cần bật **Nested Virtualization** từ phía Parallels trên macOS (tắt VM → Parallels → Configure → Hardware → CPU & Memory → Nested Virtualization, hoặc `prlctl set "Windows 11" --nested-virt on` sau khi tắt VM). Việc này không sửa được từ bên trong Windows. Đã tạm dừng ở đây theo quyết định của người dùng (thấy phiền so với lợi ích chỉ để verify) — mọi thứ dưới đây đã được kiểm tra kỹ càng nhất có thể mà không cần Docker chạy thật (xem phần "Đã kiểm tra" bên dưới). Khi nào bật được virtualization, chạy `docker compose up --build` và báo lại nếu có lỗi.

### Chạy

```bash
cd C:\HealthPlus
copy .env.example .env    # đã có sẵn .env với giá trị mặc định, đổi nếu muốn
docker compose up --build
```
- Frontend: http://localhost:4200
- API + Swagger: http://localhost:5146/swagger
- SQL Server: `localhost,1433` (user `sa`, mật khẩu trong `.env`)

Lần đầu chạy sẽ tự tạo database + áp dụng migration (đã thêm `db.Database.Migrate()` vào `Program.cs`, chạy mỗi lần khởi động — an toàn, không làm gì nếu đã up-to-date).

### Kiến trúc 3 container
- `sqlserver` — `mcr.microsoft.com/mssql/server:2022-latest`, data lưu ở named volume `sqlserver_data`
- `api` — build từ `HealthPlus/HealthPlus/Dockerfile` (multi-stage: SDK build → aspnet runtime), map cổng `5146:8080`, mount `uploads/` + `logs/` ra ngoài để không mất dữ liệu khi container bị xoá
- `client` — build từ `HealthPlusClient/Dockerfile` (multi-stage: node build → nginx serve), map cổng `4200:80`

**nginx làm reverse proxy** — `/api/*` và `/uploads/*` được nginx forward sang container `api`, còn lại phục vụ Angular SPA (fallback về `index.html` cho client-side routing). Nhờ vậy browser chỉ thấy **1 origin duy nhất** (`localhost:4200`), khớp đúng với `environment.prod.ts` (`apiUrl: '/api'` — đường dẫn tương đối, rõ ràng được thiết kế sẵn cho kiểu proxy này dù trước đây chưa ai dùng tới) và tránh hoàn toàn vấn đề CORS.

### File liên quan
- `HealthPlus/HealthPlus/Dockerfile`, `HealthPlus/.dockerignore`
- `HealthPlusClient/Dockerfile`, `HealthPlusClient/nginx.conf`, `HealthPlusClient/.dockerignore`
- `docker-compose.yml`, `.env.example`, `.env` (đã tạo sẵn từ .env.example, **đừng commit file `.env`** nếu sau này init git — dùng `.env.example` làm mẫu)

### ⚠️ 2 giới hạn quan trọng cần biết

1. **OCR (Tesseract) sẽ KHÔNG hoạt động trong Docker.** Package `Tesseract` chỉ đóng gói native binary Windows (x86/x64), còn container là Linux (`mcr.microsoft.com/dotnet/aspnet:8.0` — Debian-based). Không crash — `TesseractOcrService` đã có sẵn try/catch nên sẽ tự fallback về "không nhận diện được" giống hệt trường hợp chưa cấu hình, chỉ là không đọc được ảnh đơn thuốc. Toàn bộ tính năng khác của app không bị ảnh hưởng. Muốn OCR chạy trong Docker thật thì cần đổi sang giải pháp OCR khác tương thích Linux (vd: cài `tesseract-ocr` qua `apt-get` + wrapper khác, hoặc gọi Google Vision API) — chưa làm, ngoài phạm vi việc dockerize.

2. **`RuntimeIdentifier=win-x64` (thêm để OCR chạy được trên máy dev ARM64) đã sửa thành có điều kiện** (`Condition="'$(OS)' == 'Windows_NT'"` trong `HealthPlus.csproj`) để không phá build khi Docker build trong container Linux. Đã verify bằng cách giả lập publish không RID (`dotnet publish -p:RuntimeIdentifier= -p:SelfContained=false`) — build thành công, output đúng cấu trúc.

3. **Máy dev là Windows ARM64** — image `mcr.microsoft.com/mssql/server` chỉ có bản amd64 (không có arm64 chính thức). Docker Desktop sẽ tự chạy qua lớp giả lập (tương tự QEMU) — vẫn chạy được nhưng khởi động chậm hơn và tốn CPU hơn bình thường, đừng hoảng nếu `sqlserver` container mất hơn chục giây mới "healthy".

### Đã kiểm tra (không cần Docker)
- `dotnet build` toàn bộ solution vẫn ra `win-x64` như cũ trên máy này sau khi sửa RID thành có điều kiện — không phá dev workflow hiện tại
- Giả lập publish không RID (đúng như trong container Linux) — build + publish thành công, `tessdata/` vẫn được copy đúng (dead weight vô hại, không lỗi)
- `npx ng build --configuration production` (lệnh dùng trong Dockerfile frontend) chạy thành công, output đúng ở `dist/HealthPlusClient/browser`
- `npm ci --legacy-peer-deps` (lệnh dùng trong Dockerfile frontend) chạy thành công với lockfile hiện tại
- `docker-compose.yml` parse hợp lệ (kiểm tra bằng `js-yaml`)
- App chạy local với `db.Database.Migrate()` mới thêm — không lỗi, không phá DB đã có sẵn migration

---

## Unit test cho Admin/Bác sĩ + PWA + Git & CI/CD (xong 2026-07-21)

Sau khi so sánh với một checklist tính năng đầy đủ hơn, phát hiện 3 khoảng trống thật sự (không tính phần tài liệu đồ án — SRS/UML/ERD/Test Plan/User Manual/PPT, việc đó để riêng khi cần):

### 1. Unit test cho `AdminService` và `DoctorService` (trước đây chưa có)
Hai service quan trọng nhất mới thêm (tạo tài khoản/đặt lại mật khẩu/lịch hẹn hệ thống phía Admin, và toàn bộ delegate có kiểm tra quyền `EnsureAssignedAsync` phía Bác sĩ) trước đó chỉ được test tay qua API — không có test tự động, dễ hồi quy khi sửa code sau này.
- `HealthPlus.Tests/Services/AdminServiceTests.cs` (mới): `CreateDoctorAsync` (chặn tạo Admin, chặn email trùng, tạo đúng role + tự tạo `UserNotificationSetting`), `ResetPasswordAsync` (unknown user, hash + lưu mật khẩu mới), `GetAllAppointmentsAsync` (map đúng tên bác sĩ/bệnh nhân, xử lý user đã xoá thành `"(đã xoá)"`).
- `HealthPlus.Tests/Services/DoctorServiceTests.cs` (mới): với mỗi hành động ghi (thêm/sửa/xoá vaccine, sửa/xoá chẩn đoán, xoá đơn thuốc) đều test 2 chiều — **chặn** khi bệnh nhân chưa được gán (`UnauthorizedAccessException`, không gọi service bên dưới), và **delegate đúng** khi đã được gán; thêm 1 test cho `GetMyPatientsAsync` xác nhận sắp xếp theo lần gán gần nhất.
- Tổng test backend: 53 → **71** (tất cả pass, cả ở Debug lẫn Release config).

### 2. PWA / Service Worker
Cài qua `ng add @angular/pwa` (schematic tự resolve nhầm về bản rất cũ `12.2.18` — không tương thích Angular 21, phải chỉ định tường minh `ng add @angular/pwa@21.2.16` khớp đúng `@angular/core` đang cài). Bước cài package qua npm bị lỗi ERESOLVE do version service-worker resolve lệch 1 bản patch (`21.2.18` vs core `21.2.16`) — xử lý bằng `npm install --legacy-peer-deps` (giống cách project này vẫn cài các gói khác từ trước).
- Sinh ra: `ngsw-config.json`, `public/manifest.webmanifest`, icon 8 kích cỡ, `provideServiceWorker(...)` trong `app.config.ts` (chỉ bật khi không phải dev mode).
- Chỉnh lại `manifest.webmanifest` (tên/mô tả đúng "HealthPlus" thay vì tên project mặc định, thêm `theme_color`/`background_color` khớp màu thương hiệu `#1565C0`) và thêm `<meta name="theme-color">` vào `index.html`.
- Đã verify: `ng build --configuration production` sinh đúng `ngsw-worker.js`/`ngsw.json`/`manifest.webmanifest` trong `dist/`, không phát sinh lỗi build mới.

### 3. Git repository + CI/CD cơ bản
Phát hiện **cả dự án từ trước đến giờ chưa từng có git repo** — toàn bộ lịch sử thay đổi không được version control. Đã:
- Tạo `.gitignore` ở root (loại trừ `.env` thật — chỉ giữ `.env.example`, `bin/`/`obj/`, `node_modules/`/`dist/`/`.angular/`, thư mục `uploads/` lưu file cục bộ) — **verify bằng `git add -A --dry-run`: chỉ có `.env.example` được thêm, `.env` thật không lọt vào**.
- `git init` tại `C:\HealthPlus`.
- `.github/workflows/ci.yml` (mới): 2 job song song — `backend` (`dotnet restore/build/test` ở Release config, cài cả .NET 8 & 9 SDK vì `HealthPlus.Tests` dùng net9.0 còn API dùng net8.0) và `frontend` (`npm install --legacy-peer-deps` → `tsc --noEmit` → `ng build --configuration production` → `npm test`), chạy khi push/PR vào nhánh `main`.
- Đã verify cục bộ tương đương CI: build + test Release config pass (71/71), `git add -A --dry-run` xác nhận không rò rỉ secret hay file build.
- **Chưa commit/push gì** — repo mới chỉ init, chưa có commit đầu tiên; CI thật sự chỉ chạy khi có remote GitHub và code được push lên.

---

## Landing page — giao diện đăng nhập/đăng ký + navbar/footer đầy đủ hơn (xong 2026-07-22)

Sau nhiều vòng chỉnh sửa theo phản hồi trực tiếp của user khi xem trên trình duyệt:

**Đăng nhập/Đăng ký**: thử qua 3 phương án — (1) ảnh thật phủ toàn bộ panel với overlay xanh đậm → user thấy quá đậm/ảnh bị cắt xấu, (2) ảnh thật thu nhỏ thành thẻ nổi → user thấy quá nhỏ, (3) **chốt cuối cùng**: bỏ hẳn ảnh thật, dùng lại khối minh hoạ mock-dashboard (SVG sóng nhịp tim, thẻ chỉ số, avatar) y hệt hiệu ứng ở hero trang chủ — nền chuyển sang gradient nhạt xanh-trắng-hồng thay vì xanh đậm đặc. Thêm 3 nút đăng nhập Google/Facebook/Apple (SVG logo thật) — chỉ làm giao diện, bấm vào hiện snackbar "đang phát triển" vì cần Client ID/API key riêng từ từng hãng mà chỉ user tự tạo được.

**Footer trang chủ**: đổi nền từ trắng sang xanh navy đậm (khớp màu khối CTA phía trên) để không bị ngắt đột ngột; mở rộng từ 2 cột (Sản phẩm/Tài khoản) thành 4 cột, thêm **Công ty** (Giới thiệu/Liên hệ) và **Pháp lý** (Điều khoản sử dụng/Chính sách bảo mật).

**4 trang nội dung mới** (`features/info/`): `about` (Giới thiệu — sứ mệnh, 3 vai trò, tính năng thật đã xây, không bịa số liệu/đội ngũ/giải thưởng), `contact` (Liên hệ — form tên/email/nội dung, nộp xong hiện thông báo rõ đây là bản demo nên tin nhắn chưa gửi thật, tránh gây hiểu lầm có kênh hỗ trợ thật), `terms` (Điều khoản sử dụng — 7 mục chuẩn), `privacy` (Chính sách bảo mật — 6 mục, mô tả đúng thực tế hệ thống: lưu SQL nội bộ, mật khẩu hash một chiều, bác sĩ chỉ xem bệnh nhân được phân công). Cả 4 dùng chung `layout/info-page-layout/` (header nhỏ + logo/back-home + khung nội dung + footer bản quyền) để tránh lặp code. Route mới: `/gioi-thieu`, `/lien-he`, `/dieu-khoan-su-dung`, `/chinh-sach-bao-mat` — không cần đăng nhập, truy cập tự do như trang chủ.

**Lưu ý ngân sách CSS**: `landing.component.scss` liên tục sát ngưỡng lỗi 8kB qua các vòng sửa — xử lý bằng cách gộp 3 khối `@media (max-width: 760px)` trùng lặp thành 1, và chuyển toàn bộ màu chữ riêng của footer (trắng/xám trên nền tối) sang inline style thay vì viết rule SCSS mới — về lại mức an toàn 7.93kB.

Đã build/test đầy đủ (`tsc --noEmit`, `ng build --configuration production`, `npm test` 11/11) sau mỗi vòng chỉnh — đều pass, không lỗi ngân sách mới.

---

## Thêm trang Tính năng + FAQ, bỏ nav dạng cuộn trang (xong 2026-07-22)

User phản hồi 3 mục "Tính năng"/"Dành cho ai"/"Cách hoạt động" trên navbar chỉ cuộn trang (anchor `#id`) chứ không thật sự điều hướng — muốn khi bấm phải **chuyển sang trang mới** như một website thật. Đã tái cấu trúc:

- **`features/info/features-page/`** (route `/tinh-nang`, mới): trang Tính năng riêng, liệt kê chi tiết cả 6 tính năng (mô tả + gạch đầu dòng cụ thể hơn bản teaser ở trang chủ), dữ liệu lấy đúng từ tính năng thật đã xây (không thêm tính năng chưa có).
- **`features/info/faq/`** (route `/cau-hoi-thuong-gap`, mới): trang Câu hỏi thường gặp dạng accordion (`mat-expansion-panel`, module có sẵn trong Angular Material, không thêm thư viện mới) — 7 câu hỏi thực tế dựa trên tính năng đã có (miễn phí không, dữ liệu an toàn không, OCR hoạt động thế nào...).
- **`features/info/about/`**: bổ sung mục "Cách hoạt động" (3 bước, lấy lại đúng nội dung từ trang chủ) — gộp nội dung "Dành cho ai" + "Cách hoạt động" vào chung trang Giới thiệu thay vì tách riêng, tránh có quá nhiều trang mỏng nội dung.
- **Navbar trang chủ**: đổi từ anchor `#features/#roles/#how-it-works` sang `routerLink` thật: Tính năng | Giới thiệu | Câu hỏi thường gặp | Liên hệ.
- **Footer**: cột "Sản phẩm" trỏ sang `/tinh-nang` thay vì anchor; gộp cột "Tài khoản" vào "Sản phẩm", thêm liên kết FAQ vào cột "Công ty".
- Các section cũ trên trang chủ (role-grid/feature-grid/steps-row) **vẫn giữ nguyên** làm nội dung xem trước ngay tại trang chủ — chỉ bỏ `id` neo vì không còn ai trỏ tới, không xoá nội dung.

**Phát hiện và sửa 1 lỗi thật khi làm phần này**: `layout/info-page-layout/` dùng `<ng-content>` để các trang con (About/Contact/Terms/Privacy/Tính năng/FAQ) chiếu nội dung riêng vào, nhưng style `.info-content h2/p/ul/li` viết trong `info-page-layout.component.scss` **không áp dụng được** cho nội dung chiếu vào — do Angular gắn thuộc tính encapsulation riêng cho từng component, nội dung `<ng-content>` mang thuộc tính của component khai báo ra nó (vd. `AboutComponent`) chứ không phải của `InfoPageLayoutComponent`, nên selector biên dịch ra `.info-content[_ngcontent-X] h2[_ngcontent-X]` không khớp. Nghĩa là **toàn bộ 4 trang trước đó (Giới thiệu/Liên hệ/Điều khoản/Chính sách) đang hiển thị chữ không có style** dù code nhìn đúng — phát hiện qua việc build và soi thẳng CSS đã biên dịch trong `dist/`, không phải qua báo lỗi biên dịch (Angular không cảnh báo trường hợp này). Đã sửa bằng `::ng-deep` bọc quanh khối style đó trong `info-page-layout.component.scss` — verify lại bằng cách grep CSS đã biên dịch, xác nhận selector không còn yêu cầu thuộc tính khớp ở phần tử con.

Đã build/test đầy đủ sau khi sửa (`tsc --noEmit`, `ng build --configuration production`, `npm test` 11/11) — pass hết, không lỗi ngân sách CSS mới.

---

## Navbar/footer dùng chung cho mọi trang công khai + layout đẹp hơn cho các trang phụ (xong 2026-07-22)

User phản hồi layout các trang phụ (Giới thiệu/Liên hệ/...) "chán", và navbar không giữ nguyên khi chuyển trang (vì `info-page-layout` trước đó tự vẽ 1 header rút gọn khác hẳn navbar trang chủ). Đã tái cấu trúc:

**Tách navbar/footer trang chủ thành component dùng chung**:
- `layout/public-nav/` (mới) — đúng navbar trang chủ (logo, 4 mục điều hướng, nút Đăng nhập/Đăng ký), thêm `routerLinkActive` để tô đậm mục đang đứng.
- `layout/public-footer/` (mới) — đúng footer 4 cột trang chủ.
- `landing.component` giờ dùng `<app-public-nav />` / `<app-public-footer />` thay vì HTML/CSS viết trực tiếp — gọn code, và giảm luôn kích thước `landing.component.scss` từ 7.93kB xuống 6.20kB (bớt hẳn nguy cơ chạm ngưỡng lỗi 8kB).
- `layout/info-page-layout/` (dùng cho 6 trang: Giới thiệu/Liên hệ/Điều khoản/Chính sách/Tính năng/FAQ) đổi sang dùng chung 2 component trên — **navbar/footer giờ giống hệt nhau dù đứng ở trang nào**.

**Layout các trang phụ đẹp hơn**: `info-page-layout` thêm 1 khối "page hero" (gradient nhạt xanh-hồng giống hero trang chủ, có icon tròn + tiêu đề + mô tả ngắn, 2 blob trang trí bay nhẹ) thay vì chỉ có `<h1>` trơn. Thêm `input subtitle`/`icon` để từng trang tự truyền nội dung phù hợp.
- **Giới thiệu**: đổi phần "Dành cho ai" thành lưới 3 thẻ vai trò (y hệt `role-card` ở trang chủ, có icon màu + viền trên theo màu), "Cách hoạt động" thành 3 bước có số thứ tự + đường nối (y hệt `steps-row` ở trang chủ) — thay cho danh sách gạch đầu dòng khô khan trước đó.
- **Tính năng**: đổi từ danh sách dọc thành lưới thẻ 2 cột, mỗi thẻ có bóng đổ + hover nổi lên.
- **Liên hệ**: thêm layout 2 cột — form bên trái, 3 thẻ thông tin nhanh bên phải (thời gian phản hồi, gợi ý xem FAQ, cam kết bảo mật).

**Phát hiện và sửa 1 lỗi thật khi làm phần này**: `layout/info-page-layout/` dùng `<ng-content>` để các trang con (About/Contact/Terms/Privacy/Tính năng/FAQ) chiếu nội dung riêng vào, nhưng style `.info-content h2/p/ul/li` viết trong `info-page-layout.component.scss` **không áp dụng được** cho nội dung chiếu vào — do Angular gắn thuộc tính encapsulation riêng cho từng component, nội dung `<ng-content>` mang thuộc tính của component khai báo ra nó (vd. `AboutComponent`) chứ không phải của `InfoPageLayoutComponent`, nên selector biên dịch ra `.info-content[_ngcontent-X] h2[_ngcontent-X]` không khớp. Nghĩa là **toàn bộ 4 trang trước đó (Giới thiệu/Liên hệ/Điều khoản/Chính sách) đang hiển thị chữ không có style** dù code nhìn đúng — phát hiện qua việc build và soi thẳng CSS đã biên dịch trong `dist/`, không phải qua báo lỗi biên dịch (Angular không cảnh báo trường hợp này). Đã sửa bằng `::ng-deep` bọc quanh khối style đó trong `info-page-layout.component.scss` — verify lại bằng cách grep CSS đã biên dịch, xác nhận selector không còn yêu cầu thuộc tính khớp ở phần tử con.

Đã build/test đầy đủ (`tsc --noEmit`, `ng build --configuration production`, `npm test` 11/11) — pass hết, không có trang mới nào vượt ngân sách CSS (kể cả cảnh báo).
