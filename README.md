# Hệ Thống Quản Lý Nhà Hàng

Hệ thống quản lý nhà hàng là một ứng dụng web API được xây dựng trên nền tảng **ASP.NET Core (.NET 8)** với cơ sở dữ liệu **SQL Server**. Ứng dụng hỗ trợ quản lý đặt bàn, đặt món, thanh toán, báo cáo doanh thu, cùng với các tính năng quản lý thực đơn, nhân viên và khách hàng.
### Đây là phần xử lý Backend cho hệ thống, để truy cập sourcecode Frontend, truy cập repo [sau](https://github.com/baonhi12/pizza-restaurant-system-frontend).
### Để theo dõi tiến trình của nhóm, truy cập link Jira [sau](https://nguyendminh025.atlassian.net/jira/software/projects/SCRUM/boards/1/backlog?selectedIssue=SCRUM-3&atlOrigin=eyJpIjoiNWNkYTRhMGVmZjBkNGQ2OGI3YmE5MzA0MWYzZDU1YWIiLCJwIjoiaiJ9).
### Để xem thêm các báo cáo yêu cầu, thiết kế và kiểm thử phần mềm, truy cập link [sau](https://drive.google.com/drive/folders/1oYuZufJPoggj7VS6KTqJn-v1M_nsZUhw).
## Mục Lục
- [Tính Năng](#tính-năng)
- [Kiến Trúc](#kiến-trúc)
- [Cài Đặt](#cài-đặt)
- [Cấu Hình](#cấu-hình)
- [Chạy Ứng Dụng](#chạy-ứng-dụng)
- [Kiểm Thử Phần Mềm](#kiểm-thử-phần-mềm)
- [Đóng Góp](#đóng-góp)
- [Giấy Phép](#giấy-phép)

## Tính Năng
- **Đặt bàn online**: Khách hàng nhập thông tin, ngày giờ đến, số lượng người và thời gian dùng bữa.
- **Kiểm tra bàn trống**: Tìm bàn còn trống trong khung giờ mong muốn.
- **Đặt món qua mã QR**: Quét mã QR tại bàn để đặt món trực tuyến.
- **Thanh toán hóa đơn**: Hỗ trợ tính toán, thanh toán, in hóa đơn.
- **Báo cáo thống kê**: Doanh thu theo ngày, tháng, quý, năm.
- **Quản lý thực đơn**: Thêm, sửa, xóa món ăn.

## Kiến Trúc
Hệ thống gồm 4 tầng:
- **API**: Chứa controller và endpoint RESTful.
- **Core**: Định nghĩa entity, DTO và interface.
- **DataAccess**: Quản lý cơ sở dữ liệu, repository.
- **Service**: Xử lý nghiệp vụ, sử dụng repository và AutoMapper.

## Cài Đặt
### Yêu Cầu Hệ Thống
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server hoặc MySQL
- [Git](https://git-scm.com/)

### Clone Repository
```bash
git clone https://github.com/yourusername/RestaurantManagement.git
cd RestaurantManagement
```

## Cấu Hình
### Cấu Hình Cơ Sở Dữ Liệu
Mở **appsettings.json** và cập nhật chuỗi kết nối:
```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=RestaurantDB;User Id=sa;Password=yourpassword;TrustServerCertificate=True;"
  }
}
```
### Migration Database
```bash
dotnet ef migrations add InitialCreate --project RestaurantManagement.DataAccess
dotnet ef database update --project RestaurantManagement.DataAccess
```

## Chạy Ứng Dụng
```bash
cd RestaurantManagement.Api
dotnet run
```
Ứng dụng chạy tại `https://localhost:5001/swagger`.

## Kiểm Thử Phần Mềm

Hệ thống áp dụng các phương thức kiểm thử phần mềm phổ biến để đảm bảo chất lượng:

### Kiểm Thử Đơn Vị (Unit Testing)

- Kiểm thử từng chức năng nhỏ, chẳng hạn như:

- Đặt bàn (ReservationService)

- Tính năng quản lý bàn ăn

- Sử dụng xUnit / NUnit để kiểm thử từng phương thức riêng lẻ.

### Kiểm Thử Tích Hợp (Integration Testing)

- Đảm bảo API có thể giao tiếp đúng với database.

- Kiểm tra tích hợp giữa ReservationService và TableRepository.

- Sử dụng Moq để mô phỏng database.

### Kiểm thử hệ thống (System Testing)

- Kiểm tra các tính năng từ đầu đến cuối như:

- Đặt bàn từ giao diện người dùng.

- Quản lý bàn và thực đơn.

- Sử dụng Selenium để kiểm thử giao diện.

### Kiểm thử hồi quy (Regression Testing)

- Đảm bảo các tính năng cũ không bị lỗi khi cập nhật hệ thống.

- Sử dụng Automated Test Suites để kiểm tra tự động sau mỗi lần cập nhật.

### Kiểm thử hiệu năng (Performance Testing)

- Kiểm tra thời gian phản hồi của API.

- Dùng JMeter để kiểm tra hệ thống khi có nhiều người dùng cùng lúc.

### Kiểm thử bảo mật (Security Testing)

- Kiểm tra SQL Injection, XSS, CSRF.

- Sử dụng OWASP ZAP để kiểm tra các lỗ hổng bảo mật.

## Đóng Góp
Nếu muốn đóng góp, vui lòng fork repository, tạo branch mới, commit thay đổi và gửi pull request.

## Giấy Phép
Dự án này được cấp phép theo [MIT License](LICENSE).
```

