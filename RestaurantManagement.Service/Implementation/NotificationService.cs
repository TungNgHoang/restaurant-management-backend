using RestaurantManagement.Service.Hubs;
namespace RestaurantManagement.Service.Implementation
{
    public class NotificationService : INotificationService
    {
        private readonly IRepository<TblNotification> _notificationRepository;
        private readonly ILogger<NotificationService> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IRepository<TblNotification> notificationRepository, ILogger<NotificationService> logger, IHubContext<NotificationHub> hubContext)
        {
            _notificationRepository = notificationRepository;
            _logger = logger;
            _hubContext = hubContext;
        }

        public async Task SendNotificationAsync(NotificationTypeEnum type, string message, Guid? resId = null, Guid? createdBy = null)
        {
            try
            {
                var notification = new TblNotification
                {
                    NotiId = Guid.NewGuid(),
                    ResId = resId,
                    Message = $"[{type.GetDescription()}] {message}",
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                    CreatedBy = createdBy
                };

                await _notificationRepository.InsertAsync(notification);

                // Gửi thông báo real-time đến tất cả clients
                var notificationDto = new NotificationResponseDto
                {
                    NotiId = notification.NotiId,
                    ResId = notification.ResId,
                    Message = notification.Message,
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt,
                    CreatedBy = notification.CreatedBy?.ToString(),
                    NotificationType = ExtractNotificationType(notification.Message),
                    TimeAgo = "Vừa xong"
                };

                await _hubContext.Clients.All.SendAsync("NewNotification", notificationDto);

                // Cập nhật số lượng thông báo chưa đọc
                var unreadCount = await GetNotificationCountAsync(false);
                await _hubContext.Clients.All.SendAsync("UnreadCountUpdated", unreadCount);

                _logger.LogInformation("Đã gửi thông báo real-time: Type={NotificationType}, Message={Message}, ResId={ResId}",
                    type, message, resId);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Handle concurrency exception specifically
                _logger.LogWarning(ex, "Concurrency conflict when inserting notification: Type={NotificationType}, Message={Message}", type, message);

                
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi thông báo: Type={NotificationType}, Message={Message}",
                    type, message);
                return;
            }
        }

        public async Task SendNewReservationNotificationAsync(Guid resId, string customerName, string tableName)
        {
            var message = $"Khách hàng {customerName} đã đặt bàn {tableName}";
            await SendNotificationAsync(NotificationTypeEnum.NewReservation, message, resId);
        }

        public async Task SendPaymentSuccessNotificationAsync(Guid resId, decimal amount, string customerName)
        {
            var message = $"Khách hàng {customerName} đã thanh toán thành công số tiền {amount.ToString("N0")} VND";
            await SendNotificationAsync(NotificationTypeEnum.PaymentSuccess, message, resId);
        }

        public async Task SendReservationExpiringSoonNotificationAsync(Guid resId, string customerName, DateTime expiryTime)
        {
            var message = $"Đặt bàn của khách hàng {customerName} sẽ hết hạn lúc {expiryTime:HH:mm dd/MM/yyyy}";
            await SendNotificationAsync(NotificationTypeEnum.ReservationExpiringSoon, message, resId);
        }

        public async Task SendMonthlyRevenueReportNotificationAsync(decimal totalRevenue, int month, int year)
        {
            var message = $"Báo cáo doanh thu tháng {month}/{year}: Tổng doanh thu {totalRevenue:C0} VND";
            await SendNotificationAsync(NotificationTypeEnum.MonthlyRevenueReport, message);
        }

        public async Task SendReservationAutoCancelledNotificationAsync(Guid resId, string customerName, bool hasPreOrder = false)
        {
            var message = hasPreOrder
                ? $"Đặt bàn và đơn đặt trước của khách hàng {customerName} đã được hủy tự động do quá hạn"
                : $"Đặt bàn của khách hàng {customerName} đã được hủy tự động do quá hạn";

            var type = hasPreOrder ? NotificationTypeEnum.PreOrderAutoCancelled : NotificationTypeEnum.ReservationAutoCancelled;
            await SendNotificationAsync(type, message, resId);
        }

        public async Task<IEnumerable<TblNotification>> GetUnreadNotificationsAsync()
        {
            try
            {
                var notifications = await _notificationRepository.FilterAsync(n => !n.IsRead);
                return notifications.OrderByDescending(n => n.CreatedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách thông báo chưa đọc");
                throw;
            }
        }

        public async Task<IEnumerable<TblNotification>> GetAllNotificationsAsync(int pageNumber = 1, int pageSize = 20, bool? isRead = null)
        {
            try
            {
                // Bắt đầu với query cơ bản
                var query = await _notificationRepository.AsNoTrackingAsync(); // Chỉ lấy những thông báo chưa bị xóa

                // Lọc theo trạng thái đọc nếu có
                if (isRead.HasValue)
                {
                    query = query.Where(n => n.IsRead == isRead.Value);
                }

                // Sắp xếp, phân trang và trả về
                var notifications = query
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);

                return notifications;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách tất cả thông báo với pageNumber={PageNumber}, pageSize={PageSize}, isRead={IsRead}",
                    pageNumber, pageSize, isRead);
                throw;
            }
        }

        public async Task MarkNotificationAsReadAsync(Guid notificationId)
        {
            try
            {
                var notification = await _notificationRepository.FindByIdAsync(notificationId);
                if (notification != null && !notification.IsRead)
                {
                    notification.IsRead = true;
                    await _notificationRepository.UpdateAsync(notification);
                    _logger.LogInformation("Đã đánh dấu thông báo đã đọc: NotificationId={NotificationId}", notificationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu thông báo đã đọc: NotificationId={NotificationId}", notificationId);
                throw;
            }
        }

        public async Task MarkAllNotificationsAsReadAsync()
        {
            try
            {
                var unreadNotifications = await _notificationRepository.FilterAsync(n => !n.IsRead);
                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    await _notificationRepository.UpdateAsync(notification);
                }
                _logger.LogInformation("Đã đánh dấu tất cả {Count} thông báo đã đọc", unreadNotifications.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu tất cả thông báo đã đọc");
                throw;
            }
        }

        public async Task DeleteNotificationAsync(Guid notificationId)
        {
            try
            {
                var notification = await _notificationRepository.FindByIdAsync(notificationId);
                if (notification != null)
                {
                    await _notificationRepository.DeleteAsync(notification);
                    _logger.LogInformation("Đã xóa thông báo: NotificationId={NotificationId}", notificationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa thông báo: NotificationId={NotificationId}", notificationId);
                throw;
            }
        }

        public async Task<int> DeleteAllReadNotificationsAsync()
        {
            try
            {
                var readNotifications = await _notificationRepository.FilterAsync(n => n.IsRead);
                var count = readNotifications.Count();

                foreach (var notification in readNotifications)
                {
                    await _notificationRepository.DeleteAsync(notification);
                }

                _logger.LogInformation("Đã xóa {Count} thông báo đã đọc", count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa tất cả thông báo đã đọc");
                throw;
            }
        }

        public async Task<int> GetNotificationCountAsync(bool? isRead = null)
        {
            try
            {
                if (isRead.HasValue)
                {
                    var notifications = await _notificationRepository.FilterAsync(n => n.IsRead == isRead.Value);
                    return notifications.Count();
                }
                else
                {
                    var notifications = await _notificationRepository.AsNoTrackingAsync();
                    return notifications.Count();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đếm thông báo");
                throw;
            }
        }
        private string ExtractNotificationType(string message)
        {
            if (message.StartsWith("[Đặt bàn mới]")) return NotificationTypeEnum.NewReservation.ToString();
            if (message.StartsWith("[Thanh toán thành công]")) return NotificationTypeEnum.PaymentSuccess.ToString();
            if (message.StartsWith("[Đơn đặt bàn sắp quá hạn]")) return NotificationTypeEnum.ReservationExpiringSoon.ToString();
            if (message.StartsWith("[Báo cáo doanh thu hàng tháng]")) return NotificationTypeEnum.MonthlyRevenueReport.ToString();
            if (message.StartsWith("[Đơn đặt bàn đã bị hủy tự động]")) return NotificationTypeEnum.ReservationAutoCancelled.ToString();
            if (message.StartsWith("[Đơn đặt trước hủy tự động]")) return NotificationTypeEnum.PreOrderAutoCancelled.ToString();
            return "General";
        }
    }

    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (System.ComponentModel.DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(System.ComponentModel.DescriptionAttribute));
            return attribute == null ? value.ToString() : attribute.Description;
        }
    }
    
}