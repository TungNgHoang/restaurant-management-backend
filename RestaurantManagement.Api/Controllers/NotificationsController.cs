namespace RestaurantManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : BaseApiController
    {
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly ILogger<NotificationsController> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;
        public NotificationsController(IServiceProvider serviceProvider, INotificationService notificationService, IMapper mapper, ILogger<NotificationsController> logger, IHubContext<NotificationHub> hubContext) : base(serviceProvider)
        {
            _notificationService = notificationService;
            _mapper = mapper;
            _logger = logger;
            _hubContext = hubContext;
        }

        [Authorize(Policy = "AccessAllPolicy")]
        [HttpGet]
        public async Task<ActionResult<PaginatedNotificationResponseDto>> GetNotifications(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool? isRead = null)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var notifications = await _notificationService.GetAllNotificationsAsync(page, pageSize, isRead);
                var totalCount = await _notificationService.GetNotificationCountAsync(isRead);

                var notificationDtos = notifications.Select(n => new NotificationResponseDto
                {
                    NotiId = n.NotiId,
                    ResId = n.ResId,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    CreatedBy = n.CreatedBy?.ToString(),
                    NotificationType = ExtractNotificationType(n.Message),
                    TimeAgo = CalculateTimeAgo(n.CreatedAt)
                }).ToList();

                var response = new PaginatedNotificationResponseDto
                {
                    Notifications = notificationDtos,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    HasPreviousPage = page > 1,
                    HasNextPage = page < Math.Ceiling((double)totalCount / pageSize)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách thông báo");
                return StatusCode(500, new { message = "Lỗi server khi lấy thông báo" });
            }
        }

        [Authorize(Policy = "AccessAllPolicy")]
        [HttpGet("get-unread")]
        public async Task<ActionResult<List<NotificationResponseDto>>> GetUnreadNotifications()
        {
            try
            {
                var notifications = await _notificationService.GetUnreadNotificationsAsync();
                var notificationDtos = notifications.Select(n => new NotificationResponseDto
                {
                    NotiId = n.NotiId,
                    ResId = n.ResId,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    CreatedBy = n.CreatedBy?.ToString(),
                    NotificationType = ExtractNotificationType(n.Message),
                    TimeAgo = CalculateTimeAgo(n.CreatedAt)
                }).OrderByDescending(n => n.CreatedAt).ToList();

                return Ok(notificationDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông báo chưa đọc");
                return StatusCode(500, new { message = "Lỗi server khi lấy thông báo chưa đọc" });
            }
        }

        [Authorize(Policy = "AccessAllPolicy")]
        [HttpGet("count-notifications")]
        public async Task<ActionResult<NotificationCountResponseDto>> GetNotificationCount()
        {
            try
            {
                var unreadCount = await _notificationService.GetNotificationCountAsync(false);
                var totalCount = await _notificationService.GetNotificationCountAsync();

                return Ok(new NotificationCountResponseDto
                {
                    UnreadCount = unreadCount,
                    TotalCount = totalCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy số lượng thông báo");
                return StatusCode(500, new { message = "Lỗi server khi lấy số lượng thông báo" });
            }
        }

        [Authorize(Policy = "AccessAllPolicy")]
        [HttpPut("{id}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            try
            {
                await _notificationService.MarkNotificationAsReadAsync(id);

                // Gửi cập nhật real-time về số lượng thông báo chưa đọc
                var unreadCount = await _notificationService.GetNotificationCountAsync(false);
                await _hubContext.Clients.All.SendAsync("UnreadCountUpdated", unreadCount);

                return Ok(new { message = "Đã đánh dấu thông báo đã đọc" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu thông báo đã đọc: {Id}", id);
                return StatusCode(500, new { message = "Lỗi server khi cập nhật thông báo" });
            }
        }

        [Authorize(Policy = "AccessAllPolicy")]
        [HttpPut("mark-multi-as-read")]
        public async Task<IActionResult> MarkMultipleAsRead([FromBody] MarkAsReadRequestDto request)
        {
            try
            {
                if (request.NotificationIds?.Any() == true)
                {
                    foreach (var id in request.NotificationIds)
                    {
                        await _notificationService.MarkNotificationAsReadAsync(id);
                    }
                }

                // Gửi cập nhật real-time
                var unreadCount = await _notificationService.GetNotificationCountAsync(false);
                await _hubContext.Clients.All.SendAsync("UnreadCountUpdated", unreadCount);

                return Ok(new
                {
                    message = $"Đã đánh dấu {request.NotificationIds?.Count ?? 0} thông báo đã đọc"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu nhiều thông báo đã đọc");
                return StatusCode(500, new { message = "Lỗi server khi cập nhật thông báo" });
            }
        }

        [Authorize(Policy = "AccessAllPolicy")]
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                await _notificationService.MarkAllNotificationsAsReadAsync();

                // Gửi cập nhật real-time
                await _hubContext.Clients.All.SendAsync("UnreadCountUpdated", 0);
                await _hubContext.Clients.All.SendAsync("AllNotificationsRead");

                return Ok(new { message = "Đã đánh dấu tất cả thông báo đã đọc" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu tất cả thông báo đã đọc");
                return StatusCode(500, new { message = "Lỗi server khi cập nhật thông báo" });
            }
        }

        [Authorize(Policy = "AccessAllPolicy")]
        [HttpDelete("{id}/Delete-one")]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            try
            {
                await _notificationService.DeleteNotificationAsync(id);

                // Gửi cập nhật real-time
                var unreadCount = await _notificationService.GetNotificationCountAsync(false);
                await _hubContext.Clients.All.SendAsync("UnreadCountUpdated", unreadCount);
                await _hubContext.Clients.All.SendAsync("NotificationDeleted", id);

                return Ok(new { message = "Đã xóa thông báo" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa thông báo: {Id}", id);
                return StatusCode(500, new { message = "Lỗi server khi xóa thông báo" });
            }
        }

        [Authorize(Policy = "AccessAllPolicy")]
        [HttpDelete("delete-all-had-read")]
        public async Task<IActionResult> DeleteAllReadNotifications()
        {
            try
            {
                var deletedCount = await _notificationService.DeleteAllReadNotificationsAsync();

                // Gửi cập nhật real-time
                await _hubContext.Clients.All.SendAsync("ReadNotificationsDeleted", deletedCount);

                return Ok(new
                {
                    message = $"Đã xóa {deletedCount} thông báo đã đọc"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa tất cả thông báo đã đọc");
                return StatusCode(500, new { message = "Lỗi server khi xóa thông báo" });
            }
        }

        private string ExtractNotificationType(string message)
        {
            if (message.StartsWith("[Đặt bàn mới]")) return "new_reservation";
            if (message.StartsWith("[Thanh toán thành công]")) return "payment_success";
            if (message.StartsWith("[Đặt bàn sắp quá hạn]")) return "reservation_expiring";
            if (message.StartsWith("[Báo cáo doanh thu hàng tháng]")) return "monthly_report";
            if (message.StartsWith("[Đặt bàn hủy tự động]")) return "reservation_cancelled";
            if (message.StartsWith("[Đơn đặt trước hủy tự động]")) return "preorder_cancelled";
            return "general";
        }

        private string CalculateTimeAgo(DateTime createdAt)
        {
            var timeSpan = DateTime.Now - createdAt;

            if (timeSpan.TotalMinutes < 1)
                return "Vừa xong";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} phút trước";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} giờ trước";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} ngày trước";

            return createdAt.ToString("dd/MM/yyyy HH:mm");
        }
    }
}
